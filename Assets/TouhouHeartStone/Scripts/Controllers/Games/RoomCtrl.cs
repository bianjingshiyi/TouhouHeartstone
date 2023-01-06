using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Deck = TouhouHeartstone.Deck;
using Newtonsoft.Json.Linq;
using System;

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

            game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), desc, () =>
            {
                AddCardPools(suggestion.cardPools);
                game.network.AnwserCardPoolsSuggestion(playerId, suggestion, true);
            }, () =>
            {
                game.network.AnwserCardPoolsSuggestion(playerId, suggestion, false);
            });
        }
        public void ReceiveCardPoolsSuggestionAnwsered(CardPoolSuggestion suggestion, bool agree)
        {
            string desc = game.lang._(agree ? ROOM_CARD_POOLS_SUGGESTION_AGREED : ROOM_CARD_POOLS_SUGGESTION_REFUSED);
            game.ui.dialog.ShowConfirm(game.lang._(StringTable.HINT_TITLE), desc);

            if (agree)
            {
                UploadCardPools(suggestion.cardPools);
            }
        }
        public Task display(UIDisplayInfo displayInfo)
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
                updateUIStateForRoomOwner();
                updateStartAndReadyButton();
                SetRoomData(_currentRoomData);
                updatePlayerListUI();
            }
            return Task.CompletedTask;
        }

        public void display(RoomData roomData, bool isLocalGame)
        {
            display(new DisplayInfo(roomData, isLocalGame));
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
        public void UpdateRoomData(RoomData roomData)
        {
            _currentRoomData = roomData;
            if (!_isLocalGame)
            {
                CheckCardPoolsUpdate(out _);
            }
            LoadCachedCardPools();
            updateStartAndReadyButton();
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

        private void Update()
        {
            // 如果卡池有更改，并且没有下载卡池，重试下载卡池。
            if (cardPoolChanged && !downloadingCardPools)
            {
                if (cardPoolDownloadRetryCountdown > 0)
                {
                    cardPoolDownloadRetryCountdown -= Time.deltaTime;
                }
                else
                {
                    if (CheckCardPoolsUpdate(out var pools))
                    {
                        DownloadCardPools(pools);
                        LoadCachedCardPools();
                    }
                }
            }
        }
        #endregion
        #region 事件回调
        private void onRandomSeedInputFieldValueChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                setNotifyRoomSetting(RoomDataPropName.Option.RandomSeed, 0);
                return;
            }

            if (int.TryParse(value, out int i))
            {
                setNotifyRoomSetting(RoomDataPropName.Option.RandomSeed, i);
            }
            else
            {
                _ui.setRandomSeed(_currentRoomData.getRandomSeed());
            }
        }
        private void onShuffleToggleValueChanged(bool value)
        {
            setNotifyRoomSetting(RoomDataPropName.Option.Shuffle, value);
        }
        private void onInitReplaceTimeInputFieldValueChanged(string value)
        {
            if (int.TryParse(value, out int i) && 5 < i)
            {
                setNotifyRoomSetting(RoomDataPropName.Option.TimeoutForInitReplace, i);
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
                setNotifyRoomSetting(RoomDataPropName.Option.TimeoutForTurn, i);
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
                setNotifyRoomSetting(RoomDataPropName.Option.SortedPlayers, _currentRoomData.playerDataList.Select(p => p.id).ToArray());
            }
            else
            {
                setNotifyRoomSetting(RoomDataPropName.Option.SortedPlayers, null);
            }
        }

        private void onCustomCardsToggleValueChanged(bool value)
        {
            setNotifyRoomSetting(RoomDataPropName.Option.AllowCustomCards, value);
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
                long[] pools = deck.getCardPools();
                // 如果有房间没有的卡池，提议加入该卡池。
                if (HasNotSharedPools(pools, out var privatePools))
                {
                    List<CardPoolInfoPack> packsList = new List<CardPoolInfoPack>();
                    foreach (long pool in privatePools)
                    {
                        CardPoolInfo info = game.card.getCardPoolInfo(pool);
                        if (info != null)
                        {
                            CardPoolInfoPack pack = new CardPoolInfoPack(info.cardPoolId, info.cardPoolName, info.editTime);
                            packsList.Add(pack);
                        }
                    }
                    CardPoolInfoPack[] packs = packsList.ToArray();
                    // 如果是房主，弹出对话框提示是否设置卡池属性。
                    if (isRoomOwner)
                    {
                        string poolsString = PoolsToString(packs);
                        string desc = game.lang._(ROOM_DECK_HAS_NOT_SHARED_POOL_HOST, poolsString);

                        game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), desc, () =>
                        {
                            AddCardPools(packs);
                            SetPlayerDeck(_selectDeckPlayerId, deck);
                            if (!game.network.logic.isLanHost)
                            {
                                UploadCardPools(packs);
                            }
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
                _currentRoomData.setOption(RoomDataPropName.Option.SortedPlayers, _currentRoomData.playerDataList.Select(p => p.id).ToArray());
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
        #endregion

        /// <summary>
        /// 设置本地的房间值，并发送房间值改变的通知
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        void setNotifyRoomSetting(RoomDataPropName.Option prop, object value)
        {
            _currentRoomData?.setOption(prop, value);
            onRoomPropChanged?.Invoke(prop.ToString(), value);
        }
        /// <summary>
        /// 指定的卡池列表是否不在房间卡池列表中
        /// </summary>
        /// <param name="pools"></param>
        /// <param name="privatePools"></param>
        /// <returns></returns>
        private bool HasNotSharedPools(long[] pools, out long[] privatePools)
        {
            var roomCardPools = _currentRoomData.getCardPools();
            if (roomCardPools.Length == 0)
            {
                // 修正没有设置的情况下无法使用任何卡的问题
                roomCardPools = new CardPoolInfoPack[0];
            }

            List<long> privates = new List<long>();
            bool hasNotShared = false;
            foreach (long pool in pools)
            {
                if (!roomCardPools.Any(p => p.cardPoolId == pool))
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
            var cardPools = _currentRoomData.getCardPools().Union(packs).ToArray();
            _currentRoomData.setOption(RoomDataPropName.Option.CardPools, cardPools);
            game.network.SetRoomProp(RoomDataPropName.Option.CardPools.ToString(), cardPools);
        }
        private bool CheckCardPoolsUpdate(out CardPoolInfoPack[] pools)
        {
            pools = null;
            var cardPools = _currentRoomData.getCardPools();
            List<CardPoolInfoPack> infoPacks = new List<CardPoolInfoPack>();
            foreach (CardPoolInfoPack poolPack in cardPools)
            {
                string id = poolPack.GetResourceID();
                // 如果本地不存在该卡池，并且本地没有缓存，加入到列表中
                if (!game.card.isBuiltinCardPool(poolPack.cardPoolId) && game.card.getCardPoolInfo(poolPack.cardPoolId) == null && !game.cache.FileExists(game.fileManager, ResourceType.CardDefine, id))
                {
                    infoPacks.Add(poolPack);
                }
                else // 检测卡池内的图片是否被下载。
                {
                    // 获取卡池。
                    if (game.card.tryGetCardPool(poolPack.cardPoolId, out CardPool cardPool))
                    {
                        int skinCount = cardPool.cardSkinList.Count;
                        // 遍历卡池内的所有卡牌皮肤，如果有不存在的卡图，将该卡池标记为需要下载。
                        for (int s = 0; s < skinCount; s++)
                        {
                            var skin = cardPool.cardSkinList[s];
                            string picId = Path.GetFileNameWithoutExtension(skin.imagePath);
                            if (!string.IsNullOrEmpty(picId) && 
                                !game.cache.FileExists(game.fileManager, ResourceType.Picture, picId) &&
                                !game.card.isExternalImageExists(skin.imagePath))
                            {
                                infoPacks.Add(poolPack);
                                break;
                            }
                        }
                    }
                }
            }

            if (infoPacks.Count > 0)
            {
                cardPoolChanged = true;
                cardPoolDownloadRetryCountdown = 3;
                pools = infoPacks.ToArray();
                return true;
            }
            else
            {
                cardPoolChanged = false;
                cardPoolDownloadRetryCountdown = 0;
                return false;
            }
        }
        private async void LoadCachedCardPools()
        {
            var cardPools = _currentRoomData.getCardPools();
            foreach (var cardPool in cardPools)
            {
                if (!game.card.tryGetCardPool(cardPool.cardPoolId, out _))
                {
                    await game.card.loadCachedCardDefinesAsync(cardPool.cardPoolId, cardPool.version);
                }
            }
            updatePlayerListUI();
        }
        private async void UploadCardPools(CardPoolInfoPack[] cardPools)
        {
            var localPlayer = getLocalPlayerData();
            localPlayer.setLoadState(PlayerLoadState.Upload);
            localPlayer.setReady(false);
            updateStartAndReadyButton();
            game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_LOAD_STATE, (int)PlayerLoadState.Upload);
            game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_READY, false);

            int count = cardPools.Length;
            for (int i = 0; i < count; i++)
            {
                float progress = 0;
                _ui.SetCardPoolLoadProgress((i + progress) / count);
                var poolPack = cardPools[i];
                string cardPoolId = poolPack.GetResourceID();
                if (!await game.network.ResourceExistsAsync(ResourceType.CardDefine, cardPoolId))
                {
                    byte[] bytes = await game.fileManager.readBinaryFile(game.card.getCardPoolPath(poolPack.cardPoolId));
                    await game.network.UploadResourceAsync(ResourceType.CardDefine, cardPoolId, bytes);
                    progress = progress + 0.5f;
                    _ui.SetCardPoolLoadProgress((i + progress) / count);
                }
                // 读取卡池。
                if (game.card.tryGetCardPool(poolPack.cardPoolId, out CardPool cardPool))
                {
                    // 遍历卡池内的皮肤列表。
                    int skinCount = cardPool.cardSkinList.Count;
                    for (int s = 0; s < skinCount; s++)
                    {
                        // 读取皮肤。
                        var skin = cardPool.cardSkinList[s];
                        // 读取皮肤的图像字节数组。
                        Sprite spr = await game.card.loadImage(skin.imagePath, poolPack.cardPoolId);
                        if (spr != null)
                        {
                            byte[] bytes = spr.texture.EncodeToJPG();
                            // 图像资源的资源ID。
                            string pictureId = Path.GetFileNameWithoutExtension(skin.imagePath);
                            // 上传图片资源。
                            await game.network.UploadResourceAsync(ResourceType.Picture, pictureId, bytes);
                        }
                        // 设置进度条进度。
                        progress = progress + 0.5f * ((float)s / skinCount);
                        _ui.SetCardPoolLoadProgress((i + progress) / count);
                    }
                }
            }


            localPlayer.setLoadState(PlayerLoadState.None);
            localPlayer.setReady(false);
            updateStartAndReadyButton();
            game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_LOAD_STATE, (int)PlayerLoadState.None);
            game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_READY, false);
        }
        private async void DownloadCardPools(CardPoolInfoPack[] cardPools)
        {
            if (!downloadingCardPools)
            {
                var localPlayer = getLocalPlayerData();
                localPlayer.setLoadState(PlayerLoadState.Download);
                localPlayer.setReady(false);
                updateStartAndReadyButton();
                game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_LOAD_STATE, (int)PlayerLoadState.Download);
                game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_READY, false);
                downloadingCardPools = true;


                int count = cardPools.Length;
                for (int i = 0; i < count; i++)
                {
                    float progress = 0;
                    _ui.SetCardPoolLoadProgress((i + progress) / count);
                    var poolPack = cardPools[i];
                    string cardPoolResId = poolPack.GetResourceID();
                    // todo: 资源下载失败或没有资源时候的异常处理
                    if (!game.cache.FileExists(game.fileManager, ResourceType.CardDefine, cardPoolResId) && await game.network.ResourceExistsAsync(ResourceType.CardDefine, cardPoolResId))
                    {
                        byte[] bytes = await game.network.GetResourceAsync(ResourceType.CardDefine, cardPoolResId);
                        if (bytes != null)
                        {
                            await game.cache.SaveCache(game.fileManager, ResourceType.CardDefine, cardPoolResId, bytes);
                        }
                    }
                    progress += 0.5f;
                    _ui.SetCardPoolLoadProgress((i + progress) / count);


                    // 下载卡图资源。
                    if (!game.card.tryGetCardPool(poolPack.cardPoolId, out _))
                    {
                        await game.card.loadCachedCardDefinesAsync(poolPack.cardPoolId, poolPack.version);
                    }
                    if (game.card.tryGetCardPool(poolPack.cardPoolId, out CardPool pool))
                    {
                        int skinCount = pool.cardSkinList.Count;
                        for (int s = 0; s < skinCount; s++) 
                        {
                            var skin = pool.cardSkinList[s];
                            string picId = Path.GetFileNameWithoutExtension(skin.imagePath);
                            // todo: 资源下载失败或没有资源时候的异常处理
                            if (await game.network.ResourceExistsAsync(ResourceType.Picture, picId) && 
                                !game.cache.FileExists(game.fileManager, ResourceType.Picture, picId) &&
                                !game.card.isExternalImageExists(skin.imagePath))
                            {
                                byte[] bytes = await game.network.GetResourceAsync(ResourceType.Picture, picId);
                                if (bytes != null)
                                {
                                    await game.cache.SaveCache(game.fileManager, ResourceType.Picture, picId, bytes);
                                }
                            }
                            progress += 0.5f * ((float)s / skinCount);
                            _ui.SetCardPoolLoadProgress((i + progress) / count);
                        }
                    }
                }
                downloadingCardPools = false;


                localPlayer.setLoadState(PlayerLoadState.None);
                localPlayer.setReady(false);
                updateStartAndReadyButton();
                game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_LOAD_STATE, (int)PlayerLoadState.None);
                game.network.SetPlayerProp(THHRoomPlayerDataHelper.PROP_READY, false);
            }
        }
        private string PoolsToString(CardPoolInfoPack[] packs)
        {
            const int MAX_DISPLAY_POOL_COUNT = 10;
            int count = Mathf.Min(packs.Length, MAX_DISPLAY_POOL_COUNT);
            string poolsString = string.Join(", ", packs.Select(p => p.cardPoolName).ToArray(), 0, count);
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
            if (cardPoolChanged)
            {
                return false;
            }

            Deck deck = player.getDeck();
            if (deck != null)
            {
                bool deckValid = true;
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
            return _currentRoomData.playerDataList.Find(p => p.id == game.network.logic.localPlayer.id);
        }
        private void updatePlayerListUI()
        {
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
                long[] cardPoolList = deck.getCardPools();

                viewData[i].name = deck.name;
                viewData[i].image = await game.card.loadCardImage(deck.masterCardRef.cardPoolId, deck.masterCardRef.defineId);

                bool warning = false;
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

        /// <summary>
        /// 房间卡池是否被更改。
        /// </summary>
        bool cardPoolChanged = false;
        /// <summary>
        /// 下载卡池的重试倒计时。
        /// </summary>
        float cardPoolDownloadRetryCountdown = 0;
        /// <summary>
        /// 是否正在下载卡池。
        /// </summary>
        bool downloadingCardPools = false;
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
