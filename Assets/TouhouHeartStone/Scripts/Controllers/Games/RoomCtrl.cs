using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Deck = TouhouHeartstone.Deck;
using Option = Game.RoomDataPropName.Option;

namespace Controllers
{
    class RoomCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 字符串
        [TranslateMsg("房间-玩家-尝试选择包含未共享卡池的卡组（{0}：卡池名称列表）")]
        public const string ROOM_DECK_HAS_NOT_SHARED_POOL = "该套牌中包含房间设置之外的卡池：\n{0}\n是否向房主提议使用该卡池？";
        [TranslateMsg("房间-房主-尝试选择包含未共享卡池的卡组（{0}：卡池名称列表）")]
        public const string ROOM_DECK_HAS_NOT_SHARED_POOL_HOST = "该套牌中包含房间设置之外的卡池：\n{0}\n是否使用该卡池？";
        [TranslateMsg("房间-房主-收到来自玩家的上传卡池提议（{0}：玩家名；{1}：卡池名称列表）")]
        public const string ROOM_CARD_POOLS_SUGGESTED = "玩家{0}提议使用卡池：\n{1}\n是否使用？";
        [TranslateMsg("房间-玩家-房主同意上传卡池提议")]
        public const string ROOM_CARD_POOLS_SUGGESTION_AGREED = "房主同意了你的提议。";
        [TranslateMsg("房间-玩家-房主拒绝上传卡池提议")]
        public const string ROOM_CARD_POOLS_SUGGESTION_REFUSED = "房主拒绝了你的提议。";
        [TranslateMsg("房间-未知玩家")]
        public const string ROOM_UNKNOWN_PLAYER = "未知玩家";
        [TranslateMsg("房间-未知卡组")]
        public const string ROOM_UNKNOWN_DECK = "未知卡组";
        #endregion
        #region 公有方法
        public void SetRoomData(RoomData room)
        {
             _ui.UpdateRoomData(room, _currentMode, GetSelfID());
        }
        public void SetMode(RoomMode mode)
        {
            _currentMode = mode;
            _ui.SetMode(mode);
        }
        public void SetIPText(string text)
        {
            _ui.SetIPText(text);
        }

        public void ReceiveCardPoolsSuggested(int playerId, CardPoolSuggestion suggestion)
        {
            string poolsString = PoolsToString(suggestion.cardPools);
            var suggester = _currentRoomData.getPlayer(playerId);
            string playerName = game.lang._(ROOM_UNKNOWN_PLAYER);
            if (suggester != null)
            {
                playerName = suggester.name;
            }
            string desc = game.lang._(ROOM_CARD_POOLS_SUGGESTED, playerName, poolsString);

            game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), desc, async () =>
            {
                AddCardPools(suggestion.cardPools);
                game.network.AnwserCardPoolsSuggestion(playerId, suggestion, true);
                await cardPoolManager.CheckUpdate(suggestion.cardPools);
            }, () =>
            {
                game.network.AnwserCardPoolsSuggestion(playerId, suggestion, false);
            });
        }
        public async void ReceiveCardPoolsSuggestionAnwsered(CardPoolSuggestion suggestion, bool agree)
        {
            string desc = game.lang._(agree ? ROOM_CARD_POOLS_SUGGESTION_AGREED : ROOM_CARD_POOLS_SUGGESTION_REFUSED);
            game.ui.dialog.ShowConfirm(game.lang._(StringTable.HINT_TITLE), desc);

            if (agree)
            {
                await cardPoolManager.CheckUpdate(suggestion.cardPools);
            }
        }
        public async Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _currentRoomData = info.roomData;
                _isLocalGame = info.isLocalGame;

                _isDisplay = true;

                _ui.display();
                //_ui.SetMode(RoomPanel.Mode.Local);
                //_ui.SetSelfID(scene.game.network.logic.localPlayer.id);
                //_ui.SetRightText("");
                //_ui.SetInfo(roomData, true);
                if (_isLocalGame)
                {
                    _ui.displayIPText(false);
                }
                _ui.setRandomSeed(_currentRoomData.getRandomSeed());
                _ui.setShuffle(_currentRoomData.getShuffle());
                _ui.setInitReplaceTime(_currentRoomData.getTimeoutForInitReplace());
                _ui.setTurnTime(_currentRoomData.getTimeoutForTurn());
                _ui.setDisableRandomPlayerActOrder(_currentRoomData.getSortedPlayers() != null);
                _ui.setAllowCustomCards(_currentRoomData.getAllowCustomCards());

                if (cardPoolManager == null)
                {
                    cardPoolManager = new RoomCardPoolManager(game);
                    cardPoolManager.onProgressChanged += onCardPoolProgressChanged;
                    cardPoolManager.onLoadStateChanged += onCardPoolStateChanged;
                    cardPoolManager.onOutOfRetry += onOutOfRetry;
                }
                cardPoolManager.Reset();

                SetRoomData(_currentRoomData);
                updateUIStateForRoomOwner();

                if (!_isLocalGame)
                {
                    _ = cardPoolManager.CheckUpdate(_currentRoomData.getCardPools());
                }
                updateStartAndReadyButton();
                updatePlayerListUI();
            }
        }

        public Task display(RoomData roomData, bool isLocalGame)
        {
            return display(new DisplayInfo(roomData, isLocalGame));
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            return new DisplayInfo(_currentRoomData, _isLocalGame);
        }

        /// <summary>
        /// 获取自己的玩家ID
        /// </summary>
        public int GetSelfID()
        {
            return game.network.logic.localPlayer.id;
        }
        public Task hide()
        {
            _isDisplay = false;
            _ui.gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        public async void ReceiveRoomPropChanged(string key, object value)
        {
            if (key == Option.CardPools.ToString())
            {
                if (!_isLocalGame && value is CardPoolInfoPack[] pools)
                {
                    await cardPoolManager.CheckUpdate(pools);
                }
            }
        }
        public async void UpdateRoomData(RoomData roomData)
        {
            _currentRoomData = roomData;
            if (_currentRoomData == null)
            {
                return;
            }
            updateStartAndReadyButton();
            updatePlayerListUI();
        }
        public void ResetEvents()
        {
            onRoomPropChanged = null;

            onStartClick = null;
            onQuitClick = null;
            onPlayerReadyChange = null;
            onPlayerRemove = null;
            onSetPlayerDeck = null;
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<RoomPanel>();
            }
            //_ui.onQuitClick += onQuitButtonClick;
            //_ui.onRoomPropUpdate += onRoomPropUpdate;
            _ui.onRandomSeedInputFieldValueChanged += onRandomSeedInputFieldValueChanged;
            _ui.onShuffleToggleValueChanged += onShuffleToggleValueChanged;
            _ui.onInitReplaceTimeInputFieldValueChanged += onInitReplaceTimeInputFieldValueChanged;
            _ui.onTurnTimeInputFieldValueChanged += onTurnTimeInputFieldValueChanged;
            _ui.onDisableRandomPlayerActOrderToggleValueChanged += onDisableRandomPlayerActOrderToggleValueChanged;
            _ui.onCustomCardsToggleValueChanged += onCustomCardsToggleValueChanged;
            _ui.onClickPlayerItemViewDeckButton += onClickPlayerListItemViewDeckButton;
            _ui.onClickPlayerItemSetDeckButton += onClickPlayerListItemSetDeckButton;
            _ui.onClickPlayerItemRemoveButton += onClickPlayerListItemRemoveButton;
            _ui.onClickAddAIPlayerButton += onClickAddAIPlayerButton;
            _ui.onClickSelectDeckButton += onClickSelectDeckButton;
            _ui.onClickDeckSelectPanelItem += onClickDeckSelectPanelItem;
            _ui.onClickStartButton += onClickStartButton;
            _ui.onClickReadyButton += onClickReadyButton;
            _ui.onClickCancleReadyButton += onClickCancleReadyButton;
            _ui.onClickQuitButton += onClickQuitButton;
            _ui.onUpdateButtons += onUpdateButtons;
            _ui.onPlayerRemove += onPlayerRemove;
        }

        #endregion
        #region 事件回调
        private void onRandomSeedInputFieldValueChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                setNotifyRoomSetting(Option.RandomSeed, 0);
                return;
            }

            if (int.TryParse(value, out int i))
            {
                setNotifyRoomSetting(Option.RandomSeed, i);
            }
            else
            {
                _ui.setRandomSeed(_currentRoomData.getRandomSeed());
            }
        }
        private void onShuffleToggleValueChanged(bool value)
        {
            setNotifyRoomSetting(Option.Shuffle, value);
        }
        private void onInitReplaceTimeInputFieldValueChanged(string value)
        {
            if (int.TryParse(value, out int i) && 5 < i)
            {
                setNotifyRoomSetting(Option.TimeoutForInitReplace, i);
            }
            else
            {
                _ui.setInitReplaceTime(_currentRoomData.getTimeoutForInitReplace());
            }
        }
        private void onTurnTimeInputFieldValueChanged(string value)
        {
            if (int.TryParse(value, out int i) && 5 < i)
            {
                setNotifyRoomSetting(Option.TimeoutForTurn, i);
            }
            else
            {
                _ui.setTurnTime(_currentRoomData.getTimeoutForTurn());
            }
        }
        private void onDisableRandomPlayerActOrderToggleValueChanged(bool value)
        {
            if (value)
            {
                setNotifyRoomSetting(Option.SortedPlayers, _currentRoomData.playerDataList.Select(p => p.id).ToArray());
            }
            else
            {
                setNotifyRoomSetting(Option.SortedPlayers, null);
            }
        }

        private void onCustomCardsToggleValueChanged(bool value)
        {
            setNotifyRoomSetting(Option.AllowCustomCards, value);
            updateStartAndReadyButton();
        }
        private void onClickPlayerListItemViewDeckButton(int index)
        {
            Deck deck = _currentRoomData.playerDataList[index].getDeck();
            displayPlayerDeckList(deck);
        }
        private async void onClickPlayerListItemSetDeckButton(int index)
        {
            _selectDeckPlayerId = _currentRoomData.playerDataList[index].id;
            await displaySelectDeckPanel();
        }
        private void onClickPlayerListItemRemoveButton(int index)
        {
            _currentRoomData.playerDataList.RemoveAt(index);
            updatePlayerListUI();
            updateStartAndReadyButton();
        }
        private async void onClickAddAIPlayerButton()
        {
            await game.network.logic.AddAIPlayer();
            updatePlayerListUI();
            updateStartAndReadyButton();
        }
        /// <summary>
        /// 当点击选择卡组按钮
        /// </summary>
        private async void onClickSelectDeckButton()
        {
            _selectDeckPlayerId = scene.game.network.logic.localPlayer.id;
            await displaySelectDeckPanel();
        }
        private void onClickDeckSelectPanelItem(int index)
        {
            //选择卡组
            var deck = game.deck.getOrLoadDeckList()[index];

            // 如果不是本地游戏，进行卡牌的卡池检测。
            if (!_isLocalGame)
            {
                // 检测deck是否有房间没有的卡池。
                CardPoolInfoPack[] pools = getDeckCardPools(deck);
                // 如果有房间没有的卡池，提议加入该卡池。
                if (HasNotSharedPools(pools, out var privatePools))
                {
                    CardPoolInfoPack[] packs = privatePools;
                    // 如果是房主，弹出对话框提示是否设置卡池属性。
                    if (isRoomOwner)
                    {
                        string poolsString = PoolsToString(packs);
                        string desc = game.lang._(ROOM_DECK_HAS_NOT_SHARED_POOL_HOST, poolsString);

                        game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), desc, async () =>
                        {
                            AddCardPools(packs);
                            SetPlayerDeck(_selectDeckPlayerId, deck);
                            await cardPoolManager.CheckUpdate(_currentRoomData.getCardPools());
                        });
                    }
                    // 否则，向房主提议加入卡池。
                    else
                    {
                        string poolsString = PoolsToString(packs);
                        string desc = game.lang._(ROOM_DECK_HAS_NOT_SHARED_POOL, poolsString);

                        game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), desc, () =>
                        {
                            SuggestCardPools(new CardPoolSuggestion(packs));
                            //隐藏选择卡组界面
                            _ui.hideDeckSelectPanel();
                        });
                    }
                    return;
                }
            }
            SetPlayerDeck(_selectDeckPlayerId, deck);
        }
        private async void onClickStartButton()
        {
            if (isRoomOwner)
            {
                onStartClick?.Invoke();
            }
            var players = _currentRoomData.getSortedPlayers();
            if (players != null &&
                players.Length != _currentRoomData.playerDataList.Count)
            {
                setNotifyRoomSetting(Option.SortedPlayers, _currentRoomData.playerDataList.Select(p => p.id).ToArray());
            }

            // 处理数值为0的种子。
            if (_currentRoomData.getRandomSeed() == 0)
            {
                var rand = new byte[4];
                System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(rand);
                setNotifyRoomSetting(Option.RandomSeed, BitConverter.ToInt32(rand, 0));
            }

            if (_isLocalGame)
            {
                await game.game.startLocalGame(_currentRoomData);
            }
            else
            {
                game.network.StartRemoteGame();
            }
        }
        private void onClickReadyButton()
        {
            var localPlayer = getLocalPlayerData();
            localPlayer.setReady(true);
            onPlayerReadyChange?.Invoke(GetSelfID(), true);
            updateStartAndReadyButton();
            //TODO:多人再说
        }
        private void onClickCancleReadyButton()
        {
            var localPlayer = getLocalPlayerData();
            localPlayer.setReady(false);
            onPlayerReadyChange?.Invoke(GetSelfID(), false);
            updateStartAndReadyButton();
            //TODO:多人再说
        }
        private void onClickQuitButton()
        {
            _currentRoomData = null;
            game.network.quitRoom();

            _ui.gameObject.SetActive(false);
            scene.menu.display();


            if (cardPoolManager != null && cardPoolManager.running)
            {
                cardPoolManager.Cancel();
            }

            onQuitClick?.Invoke();
        }
        private void onUpdateButtons()
        {
            _ui.enableRandomSeed(isRoomOwner);
            _ui.enableShuffle(isRoomOwner);
            _ui.enableInitReplaceTime(isRoomOwner);
            _ui.enableTurnTime(isRoomOwner);
            _ui.enableDisableRandomPlayerActOrder(isRoomOwner);
            updateStartAndReadyButton();
        }
        private void onCardPoolProgressChanged(float value)
        {
            _ui.SetCardPoolLoadProgress(value);
        }
        private void onCardPoolStateChanged(PlayerLoadState state)
        {
            var localPlayer = getLocalPlayerData();
            if (localPlayer == null)
            {
                return;
            }
            if (state != localPlayer.getLoadState())
            {
                localPlayer.setLoadState(state);
                game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_LOAD_STATE, (int)state);
            }
            if (localPlayer.getReady())
            {
                localPlayer.setReady(false);
                game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_READY, false);
            }
            // 玩家状态更新后，读取缓存中的卡池。
            // 用于局域网联机，比如客场玩家向房主上传完卡池后，不需要下载卡池的场合。
            cardPoolManager.LoadRoomCardPoolsFromCache(_currentRoomData.getCardPools());
            updateStartAndReadyButton();
            updatePlayerListUI();
        }
        private void onOutOfRetry(Exception e)
        {
            string title = game.lang._(StringTable.ERROR);

            game.ui.dialog.ShowConfirm(title, e.Message);
        }
        #endregion

        /// <summary>
        /// 设置本地的房间值，并发送房间值改变的通知
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        void setNotifyRoomSetting(Option prop, object value)
        {
            _currentRoomData?.setOption(prop, value);
            onRoomPropChanged?.Invoke(prop.ToString(), value);
        }
        private CardPoolInfoPack[] getDeckCardPools(Deck deck)
        {
            long[] poolsId = deck.getCardPools();
            List<CardPoolInfoPack> packs = new List<CardPoolInfoPack>();
            for (int i = 0; i < poolsId.Length; i++)
            {
                long id = poolsId[i];
                var poolInfo = game.card.getCardPoolInfo(id);
                if (poolInfo != null)
                {
                    packs.Add(new CardPoolInfoPack(id, poolInfo.cardPoolName, poolInfo.editTime));
                }
            }
            return packs.ToArray();
        }
        /// <summary>
        /// 指定的卡池列表是否不在房间卡池列表中
        /// </summary>
        /// <param name="pools"></param>
        /// <param name="privatePools"></param>
        /// <returns></returns>
        private bool HasNotSharedPools(CardPoolInfoPack[] pools, out CardPoolInfoPack[] privatePools)
        {
            var roomCardPools = _currentRoomData.getCardPools();
            if (roomCardPools.Length == 0)
            {
                // 修正没有设置的情况下无法使用任何卡的问题
                roomCardPools = new CardPoolInfoPack[0];
            }

            List<CardPoolInfoPack> privates = new List<CardPoolInfoPack>();
            bool hasNotShared = false;
            foreach (CardPoolInfoPack pool in pools)
            {
                if (!roomCardPools.Any(p => p.cardPoolId == pool.cardPoolId && p.version == pool.version))
                {
                    hasNotShared = true;
                    privates.Add(pool);
                }
            }
            privatePools = privates.ToArray();
            return hasNotShared;
        }
        private async void SetPlayerDeck(int playerId, Deck deck)
        {
            for (int i = 0; i < _currentRoomData.playerDataList.Count; i++)
            {
                RoomPlayerData player = _currentRoomData.playerDataList[i];
                if (player.id == playerId)
                {
                    player.setDeck(deck);
                    onSetPlayerDeck?.Invoke(playerId, deck.toDefRefArray(true));

                    if (player.type == RoomPlayerType.ai)
                    {
                        player.setReady(true);
                    }
                }
            }
            //隐藏选择卡组界面
            _ui.hideDeckSelectPanel();
            //更新界面
            updatePlayerListUI();
            updateStartAndReadyButton();
            if (playerId == game.network.logic.localPlayer.id)
            {
                _ui.updateLocalPlayerDeck(deck.name, await game.card.loadCardImage(deck.masterCardRef));
            }
        }
        private void AddCardPools(CardPoolInfoPack[] packs)
        {
            var roomPools = _currentRoomData.getCardPools();
            var newList = new List<CardPoolInfoPack>(roomPools);
            // 如果当前房间中有版本旧的卡池，替换成版本更新的卡池。
            foreach (CardPoolInfoPack pack in packs)
            {
                CardPoolInfoPack sameIdPack = newList.FirstOrDefault(p => p.cardPoolId == pack.cardPoolId && p.version != pack.version);
                if (sameIdPack != null)
                {
                    if (pack.version > sameIdPack.version)
                    {
                        newList.Remove(sameIdPack);
                        newList.Add(pack);
                    }
                }
                else
                {
                    newList.Add(pack);
                }
            }
            CardPoolInfoPack[] cardPools = newList.ToArray();
            setNotifyRoomSetting(Option.CardPools, cardPools);
        }
        private string PoolsToString(CardPoolInfoPack[] packs)
        {
            const int MAX_DISPLAY_POOL_COUNT = 10;
            int count = Mathf.Min(packs.Length, MAX_DISPLAY_POOL_COUNT);
            string poolsString = string.Join(", ", packs.Select(p => 
            {
                if (p == null)
                {
                    return game.lang._(ROOM_UNKNOWN_DECK);
                }
                return p.cardPoolName;
            }).ToArray(), 0, count);
            if (packs.Length > MAX_DISPLAY_POOL_COUNT)
            {
                poolsString += "...";
            }
            return poolsString;
        }

        /// <summary>
        /// 是否为房主
        /// </summary>
        bool isRoomOwner => GetSelfID() == _currentRoomData.ownerId;
        private void updateUIStateForRoomOwner()
        {
            _ui.enableRandomSeed(isRoomOwner);
            _ui.enableShuffle(isRoomOwner);
            _ui.enableInitReplaceTime(isRoomOwner);
            _ui.enableTurnTime(isRoomOwner);
            _ui.enableDisableRandomPlayerActOrder(isRoomOwner);
            _ui.enableAllowCustomCards(isRoomOwner);
        }
        private void updateStartAndReadyButton()
        {
            if (_currentRoomData == null)
                return;
            if (isRoomOwner)
            {
                _ui.displayStartButton(true);
                _ui.displayReadyButton(false);
                _ui.displayCancleReadyButton(false);
                _ui.enableStartButton(isAbleToStartGame());
            }
            else
            {
                _ui.displayStartButton(false);
                RoomPlayerData localPlayer = getLocalPlayerData();
                _ui.displayReadyButton(isAbleToReady(localPlayer) && !localPlayer.getReady());
                _ui.displayCancleReadyButton(localPlayer.getReady());
            }
        }
        private bool isAbleToStartGame()
        {
            if (_currentRoomData.playerDataList.Count < 2)//必须有对手才能开始游戏
                return false;
            if (!isAbleToReady(getLocalPlayerData()))//房主自己必须准备好
                return false;
            if (!_currentRoomData.getAllowCustomCards() && isAnyHasCustomCard())//如果不允许使用自定义卡牌，必须所有玩家都没有包含自定义卡牌的卡组。
            {
                return false;
            }
            return _currentRoomData.playerDataList.Where(p => p.id != _currentRoomData.ownerId).All(p => p.getReady());//其他玩家必须准备好
        }
        private bool isAbleToReady(RoomPlayerData player)
        {
            if (cardPoolManager.state != PlayerLoadState.None)
            {
                return false;
            }

            Deck deck = player.getDeck();
            if (deck != null)
            {
                bool deckValid;
                if (_isLocalGame)
                {
                    deckValid = game.deck.isValidDeck(deck);
                }
                else
                {
                    deckValid = game.deck.isValidDeckForOnlineGame(deck);
                }
                return deckValid;
            }
            return false;
        }
        /// <summary>
        /// 是否有任意玩家中的卡组中包含自定义卡牌。
        /// </summary>
        /// <returns></returns>
        private bool isAnyHasCustomCard()
        {
            var players = _currentRoomData.playerDataList;
            foreach (var player in players)
            {
                Deck deck = player.getDeck();
                if (deckHasCustomCard(deck))
                {
                    return true;
                }
            }
            return false;
        }
        public bool deckHasCustomCard(Deck deck)
        {
            if (deck == null)
            {
                return false;
            }
            //检查是否有自定义卡牌，
            for (int i = 0; i < deck.cardList.Count; i++)
            {
                if (game.card.isExternalCard(deck.cardList[i].cardRef.cardPoolId, deck.cardList[i].cardRef.defineId))
                {
                    return true;
                }
            }
            return false;
        }
        private RoomPlayerData getLocalPlayerData()
        {
            if (_currentRoomData == null || _currentRoomData.playerDataList == null)
            {
                return null;
            }
            return _currentRoomData.playerDataList.Find(p => p.id == game.network.logic.localPlayer.id);
        }
        private void updatePlayerListUI()
        {
            if (_currentRoomData == null)
                return;

            (string playerName, string characterName, bool canViewDeck, bool canSetDeck, bool canRemove)[] viewData =
                new (string playerName, string characterName, bool canViewDeck, bool canSetDeck, bool canRemove)[_currentRoomData.playerDataList.Count];
            for (int i = 0; i < viewData.Length; i++)
            {
                RoomPlayerData player = _currentRoomData.playerDataList[i];
                viewData[i].playerName = player.name;
                Deck deck = player.getDeck();
                if (deck == null)
                {
                    viewData[i].characterName = string.Empty;
                }
                else if (game.card.tryGetCardSkin(deck.masterCardRef.cardPoolId, deck.masterCardRef.defineId, out CardSkinData skin))
                {
                    viewData[i].characterName = skin.name;
                }
                else
                {
                    viewData[i].characterName = game.langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME);
                }
                viewData[i].canViewDeck = player.type == RoomPlayerType.human && deck != null;
                viewData[i].canSetDeck = player.type == RoomPlayerType.ai;
                viewData[i].canRemove = game.network.logic.localPlayer.id == _currentRoomData.ownerId && player.id != _currentRoomData.ownerId;
            }
            _ui.updatePlayerList(viewData);
            _ui.displayAddAIButton(_currentRoomData.playerDataList.Count < _currentRoomData.maxPlayerCount && _isLocalGame);
        }
        private void displayPlayerDeckList(Deck deck)
        {
            //TODO:把编辑器界面里面右边那个挪过来
        }
        private async Task displaySelectDeckPanel()
        {
            List<Deck> deckList = game.deck.getOrLoadDeckList();
            (string name, Sprite image, bool isWarning, bool hasPrivatePools)[] viewData = new (string name, Sprite image, bool isWarning, bool hasPrivatePools)[deckList.Count];
            bool allowCustomCards = _currentRoomData.getAllowCustomCards();

            for (int i = 0; i < viewData.Length; i++)
            {
                Deck deck = deckList[i];
                CardPoolInfoPack[] cardPoolList = getDeckCardPools(deck);

                viewData[i].name = deck.name;
                viewData[i].image = await game.card.loadCardImage(deck.masterCardRef.cardPoolId, deck.masterCardRef.defineId);

                // 如果卡组内有不存在的卡池，加载卡池。
                foreach (long cardPoolId in deck.getCardPools())
                {
                    if (!game.card.tryGetCardPool(cardPoolId, out _))
                    {
                        var poolInfo = game.card.getCardPoolInfo(cardPoolId);
                        if (poolInfo != null)
                        {
                            var path = game.card.getCardPoolPath(poolInfo);
                            if (path is string)
                            {
                                await game.card.loadCardPool(path);
                            }
                        }
                    }
                }

                bool warning;
                // 判断牌组是否合法。
                if (_isLocalGame)
                {
                    warning = !game.deck.isValidDeck(deck);
                }
                else
                {
                    warning = !game.deck.isValidDeckForOnlineGame(deck);
                }
                // 如果不允许使用自定义卡牌，则屏蔽拥有自定义卡牌的卡组。
                if (!allowCustomCards && deckHasCustomCard(deck))
                {
                    warning = true;
                }
                viewData[i].isWarning = warning;

                viewData[i].hasPrivatePools = HasNotSharedPools(cardPoolList, out _);
            }
            _ui.displayDeckSelectPanel(viewData);
        }

        private void SuggestCardPools(CardPoolSuggestion suggestion)
        {
            game.network.SuggestCardPools(suggestion);
        }
        #endregion
        #region 事件
        /// <summary>
        /// 点击开始游戏后触发
        /// </summary>
        public event Action onStartClick;
        /// <summary>
        /// 点击退出房间后触发
        /// </summary>
        public event Action onQuitClick;
        /// <summary>
        /// 房间信息更新（主要是设置）更新后触发
        /// </summary>
        public event Action<string, object> onRoomPropChanged;
        /// <summary>
        /// 玩家准备或取消准备
        /// </summary>
        public event Action<int, bool> onPlayerReadyChange;
        /// <summary>
        /// 删除玩家后触发
        /// </summary>
        public event Action<int> onPlayerRemove;
        /// <summary>
        /// 玩家牌组改变
        /// </summary>
        public event Action<int, DefineReference[]> onSetPlayerDeck;
        #endregion
        #region 属性字段
        RoomMode _currentMode;
        public bool isDisplay => _isDisplay;
        [SerializeField]
        RoomPanel _ui;
        [SerializeField]
        bool _isDisplay = false;
        [SerializeField]
        RoomData _currentRoomData;
        [SerializeField]
        bool _isLocalGame = true;
        [SerializeField]
        int _selectDeckPlayerId = 0;

        private RoomCardPoolManager cardPoolManager;
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(RoomData roomData, bool isLocalGame)
            {
                this.roomData = roomData;
                this.isLocalGame = isLocalGame;
            }
            public RoomData roomData;
            public bool isLocalGame;
        }
        #endregion
    }
}
