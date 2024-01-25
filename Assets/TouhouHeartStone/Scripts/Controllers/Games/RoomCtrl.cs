using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Option = Game.RoomDataPropName.Option;

namespace Controllers
{
    class RoomCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 公有方法
        public Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _displayInfo = info;
                _currentRoomData = info.roomData;
                _isLocalGame = info.roomMode == RoomMode.Local;
                _gameStarted = false;

                if (cardPoolManager == null)
                {
                    cardPoolManager = new RoomCardPoolManager(game);
                    cardPoolManager.onProgressChanged += onCardPoolProgressChanged;
                    cardPoolManager.onLoadStateChanged += onCardPoolStateChanged;
                    cardPoolManager.onOutOfRetry += onOutOfRetry;
                }
                cardPoolManager.Reset();

                _ui.setIPTextVisible(info.roomMode == RoomMode.NetworkingHost);
                _ui.setIPText(info.ipText);
                _ui.setLocalPlayerDeck(game.lang._(ROOM_SELECT_YOUR_DECK), null);
                _ui.setEditingRoomName(false);
                updateRoomData(_currentRoomData);
                _ui.display();

                bool needCheckCardPools = true;
                if (isRoomOwner)
                {
                    var classicPool = game.card.getLocalCardPoolInfo(CardManager.STANDARD_POOL_ID);
                    CardPoolInfoPack[] roomPools = new CardPoolInfoPack[] { classicPool.ToCardPoolInfoPack() };
                    _currentRoomData.setOption(Option.CardPools, roomPools);
                    setNotifyRoomSetting(Option.CardPools, roomPools);
                    needCheckCardPools = false; // 设置远端卡池会有回调，在里面检测卡池，所以这里不检测
                }
                if (needCheckCardPools)
                {
                    _ = CheckCardPoolsUpdate(_currentRoomData.getCardPools());
                }
                updateStartAndReadyButton();
                updatePlayerListUI();
            }
            return Task.CompletedTask;
        }
        public Task display(RoomData roomData, string ipText, RoomMode roomMode)
        {
            return display(new DisplayInfo(roomData, ipText, roomMode));
        }
        public Task hide()
        {
            _ui.gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            return _displayInfo;
        }

        public RoomPlayerData getLocalPlayerData()
        {
            if (_currentRoomData == null || _currentRoomData.playerDataList == null)
            {
                return null;
            }
            return _currentRoomData.playerDataList.Find(p => p.id == GetSelfID());
        }
        /// <summary>
        /// 获取自己的玩家ID
        /// </summary>
        public int GetSelfID()
        {
            return game.network.logic.localPlayer.id;
        }
        public void updateRoomData(RoomData roomData)
        {
            _currentRoomData = roomData;
            if (_currentRoomData == null)
                return;

            // 局域网中，如果本地玩家是房主，玩家状态更新后，读取缓存中的卡池。
            // 局域网的房主不会下载卡池，也就不会在下载完毕后读取缓存中的卡池，需要手动更新。
            if (game.network.logic.isLanHost)
            {
                if (roomData.playerDataList.All(p => p.getLoadState() == PlayerLoadState.None))
                {
                    LoadCardPoolsByFromCache(roomData.getCardPools());
                }
            }
            updateRoomOptions();
            updateStartAndReadyButton();
            updatePlayerListUI();
        }
        public void resetEvents()
        {
            onQuitClick = null;
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
            _ui.onPasswordInputFieldValueChanged += onPasswordInputFieldValueChanged;
            _ui.onRandomSeedInputFieldValueChanged += onRandomSeedInputFieldValueChanged;
            _ui.onShuffleToggleValueChanged += onShuffleToggleValueChanged;
            _ui.onInitReplaceTimeInputFieldValueChanged += onInitReplaceTimeInputFieldValueChanged;
            _ui.onTurnTimeInputFieldValueChanged += onTurnTimeInputFieldValueChanged;
            _ui.onDisableRandomPlayerActOrderToggleValueChanged += onDisableRandomPlayerActOrderToggleValueChanged;
            _ui.onClickTitleEditButton += onClickRoomNameEditButtonCallback;
            _ui.onClickTitleConfirmEditButton += onClickRoomNameConfirmEditButtonCallback;

            _ui.onViewCardPoolsClick += onViewCardPoolsClick;
            _ui.onClickPlayerItemViewDeckButton += onClickPlayerListItemViewDeckButton;
            _ui.onClickPlayerItemSetDeckButton += onClickPlayerListItemSetDeckButton;
            _ui.onClickPlayerItemRemoveButton += onClickPlayerListItemRemoveButton;
            _ui.onClickAddAIPlayerButton += onClickAddAIPlayerButton;

            _ui.onClickSelectDeckButton += onClickSelectDeckButton;
            _ui.onClickDeckSelectPanelItem += onClickDeckSelectPanelItem;
            _ui.onClickCardPoolsSelectPanelItem += onClickCardPoolSelectPanelItem;
            _ui.onAddCardPoolsSelectItemToggle += onAddCardPoolSelectPanelItemToggle;

            _ui.onCloseViewDeckButtonClick += onCloseViewDeckButtonClickCallback;
            _ui.onViewDeckItemPointerEnter += onViewDeckItemPointerEnterCallback;
            _ui.onViewDeckItemPointerExit += onViewDeckItemPointerExitCallback;

            _ui.onConfirmAddCardPools += onConfirmAddCardPools;
            _ui.onCloseCardPoolsPanel += onCloseCardPoolsPanel;

            _ui.onClickStartButton += onClickStartButton;
            _ui.onClickReadyButton += onClickReadyButton;
            _ui.onClickCancelReadyButton += onClickCancelReadyButton;
            _ui.onClickQuitButton += onClickQuitButton;

            game.network.onReceiveCardPoolSuggestion += onReceiveCardPoolsSuggestionCallback;
            game.network.onRoomDataChanged += onReceiveRoomDataChangedCallback;
            game.network.onRoomPropChanged += onReceiveRoomPropChangedCallback;
            game.network.onReceiveCardPoolSuggestionAnwser += onReceiveCardPoolsSuggestionAnwserCallback;
        }

        private async void Update()
        {
            if (_cardPoolPanelDirty)
            {
                _cardPoolPanelDirty = false;
                await RefreshRoomCardPoolPanels();
            }
        }
        #endregion

        #region 事件回调

        #region 房间设置
        private void onPasswordInputFieldValueChanged(string value)
        {
            _currentRoomData?.setRoomPassword(value);
            updateRoomOptions();
            game.network.roomClient.SetRoomProp(RoomData.PROP_ROOM_PASSWORD, value);
        }
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
                updateRoomOptions();
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
                updateRoomOptions();
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
                updateRoomOptions();
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
        private void onClickRoomNameEditButtonCallback()
        {
            _ui.setRoomNameInputFieldText(_currentRoomData.getRoomName());
            _ui.setEditingRoomName(true);
        }
        private void onClickRoomNameConfirmEditButtonCallback(string name)
        {
            _ui.setEditingRoomName(false);
            _currentRoomData.setRoomName(name);
            updateRoomOptions();
            game.network.roomClient.SetRoomProp(RoomData.PROP_ROOM_NAME, name);
        }
        #endregion

        #region 查看卡组
        private void onCloseViewDeckButtonClickCallback()
        {
            _viewingDeck = null;
            _ui.setViewingCard(false);
        }
        private async void onViewDeckItemPointerEnterCallback(int index)
        {
            await _ui.deckCardList.showPreviewCardByPoolSet(_viewingDeck, index, game.card, getCardPoolReferenceSet());
        }
        private void onViewDeckItemPointerExitCallback(int index)
        {
            _ui.deckCardList.hidePreviewCard();
        }
        #endregion

        #region 查看卡池
        private async void onViewCardPoolsClick()
        {
            await viewRoomCardPools();
        }
        private async void onClickCardPoolSelectPanelItem(int index)
        {
            if (IsEditingCardPools())
            {
                var pool = _editingCardPools[index];
                onAddCardPoolSelectPanelItemToggle(index, !_selectedEditCardPools.Contains(pool));
            }
            else if (_viewingCardPools != null && isRoomOwner && index >= _viewingCardPools.Length)
            {
                await UpdateEditingCardPools();
                _selectedEditCardPools?.Clear();
                AddSelectedEditingCardPools(_currentRoomData.getCardPools());
                await openEditCardPoolsPanel();
            }
        }
        private async void onAddCardPoolSelectPanelItemToggle(int index, bool value)
        {
            var pool = _editingCardPools[index];
            if (value)
            {
                // 未选中，选择
                _selectedEditCardPools.Add(pool);
                // 选中一个卡池后，要把它的所有依赖也选中。
                foreach (var dep in pool.GetDependenciesInSet(_editingCardPools))
                {
                    if (dep == null)
                        continue;
                    _selectedEditCardPools.Add(dep);
                }
            }
            else if (_selectedEditCardPools.Contains(pool))
            {
                // 已选中，取消选择。
                _selectedEditCardPools.Remove(pool);
                // 取消选中一个卡池后，要把所有依赖它的卡池也取消选中。
                foreach (var dep in pool.GetDependedCardPools(_editingCardPools))
                {
                    if (dep == null)
                        continue;
                    _selectedEditCardPools.Remove(dep);
                }
            }
            await openEditCardPoolsPanel();
        }
        private void onConfirmAddCardPools()
        {
            var roomPools = _currentRoomData.getCardPools();
            var addedCardPools = _selectedEditCardPools.Except(roomPools);
            var removedCardPools = roomPools.Except(_selectedEditCardPools);

            // 没有选择任何卡池。
            if (addedCardPools.Count() + removedCardPools.Count() <= 0)
            {
                closeEditCardPoolsPanel();
                return;
            }

            if (_isLocalGame)
            {
                // 添加卡池
                var conflictPools = GetConflictCardPools(addedCardPools.ToArray(), out var addPools);
                ShowRoomConflictDialogs(conflictPools, addPools, ConfirmSuggestion);
                void ConfirmSuggestion()
                {
                    if (CheckRoomPoolsValidation(addPools, removedCardPools, out var missingDeps))
                    {
                        AddCardPools(addPools);
                        RemoveCardPools(removedCardPools);
                        ApplyCardPools();
                        closeEditCardPoolsPanel();
                        updateStartAndReadyButton();
                    }
                    else
                    {
                        ShowDependencyMissingDialog(missingDeps);
                    }

                }
            }
            else
            {
                string addString = string.Empty;
                string removeString = string.Empty;
                var addedArray = addedCardPools.ToArray();
                if (addedCardPools.Count() > 0)
                {
                    string addPoolsString = PoolsToString(addedArray, false, 5);
                    addString = game.lang._(ROOM_EDIT_CARD_POOL_ADD, addPoolsString);
                }
                if (removedCardPools.Count() > 0)
                {
                    string removePoolsString = PoolsToString(removedCardPools.ToArray(), false, 5);
                    removeString = game.lang._(ROOM_EDIT_CARD_POOL_REMOVE, removePoolsString);
                }
                string desc = game.lang._(ROOM_EDIT_CARD_POOL_TEMPLATE, addString, removeString, ROOM_EDIT_CARD_POOL_CONTINUE);
                if (isRoomOwner)
                {
                    List<(CardPoolInfoPack roomPool, CardPoolInfoPack comingPool)> conflictPools;
                    List<CardPoolInfoPack> addPools;
                    void ConfirmSuggestion()
                    {
                        if (CheckRoomPoolsValidation(addPools, removedCardPools, out var missingDeps))
                        {
                            AddCardPools(addPools);
                            RemoveCardPools(removedCardPools);
                            ApplyCardPools();
                            closeEditCardPoolsPanel();
                        }
                        else
                        {
                            ShowDependencyMissingDialog(missingDeps);
                        }
                    }

                    game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), desc, () =>
                    {
                        if (CanUploadCardPools(addedArray, out string errorMessage))
                        {
                            conflictPools = GetConflictCardPools(addedArray, out addPools);
                            ShowRoomConflictDialogs(conflictPools, addPools, ConfirmSuggestion);
                        }
                        else
                        {
                            string title = game.lang._(StringTable.ERROR);
                            game.ui.dialog.ShowConfirm(title, errorMessage);
                        }
                    });
                }
            }
        }
        private void onCloseCardPoolsPanel()
        {
            if (IsEditingCardPools())
            {
                closeEditCardPoolsPanel();
            }
            else
            {
                _ui.closeViewCardPoolsPanel();
                _viewingCardPools = null;
            }
        }
        #endregion

        #region 玩家列表
        private async void onClickPlayerListItemViewDeckButton(int index)
        {
            var player = _currentRoomData.playerDataList[index];
            var deck = player.getDeck();
            _viewingDeck = deck;
            var viewDatas = await DeckCardViewData.fromDeck(deck, game, getCardPoolReferenceSet());
            _ui.setViewingCard(true);
            _ui.updateViewingDeckCards(viewDatas);
        }
        private async void onClickPlayerListItemSetDeckButton(int index)
        {
            _selectDeckPlayerId = _currentRoomData.playerDataList[index].id;
            await displaySelectDeckPanel();
        }
        private void onClickPlayerListItemRemoveButton(int index)
        {
            //var logic = game.network.logic;
            //if (isRoomOwner)
            //{
            //    logic.RemovePlayer(id);
            //}
            //_currentRoomData.playerDataList.RemoveAt(index);
            //updatePlayerListUI();
            //updateStartAndReadyButton();
        }
        private async void onClickAddAIPlayerButton()
        {
            await game.network.logic.AddAIPlayer();
            updatePlayerListUI();
            updateStartAndReadyButton();
        }
        #endregion

        #region 选择卡组
        /// <summary>
        /// 当点击选择卡组按钮
        /// </summary>
        private async void onClickSelectDeckButton()
        {
            _selectDeckPlayerId = GetSelfID();
            await displaySelectDeckPanel();
        }
        private void onClickDeckSelectPanelItem(int index)
        {
            //选择卡组
            var deck = game.deck.getOrLoadDeckList()[index];

            // 检测deck是否有房间没有的卡池。
            CardPoolInfoPack[] pools = getDeckCardPools(deck, out var missingPools);
            // 如果卡组内的卡牌属于或引用了丢失的卡池，弹出错误。
            if (missingPools.Length > 0)
            {
                string title = game.lang._(StringTable.ERROR);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(game.lang._(ROOM_ERROR_CANNOT_UPLOAD_CARD_POOLS));
                sb.AppendLine(game.lang._(ROOM_ERROR_MISSING_CARD_POOL));

                game.ui.dialog.ShowConfirm(title, sb.ToString());
                return;
            }
            // 如果有房间没有的卡池，提议加入该卡池。
            if (HasNotSharedPools(pools, out var privatePools))
            {
                CardPoolInfoPack[] packs = privatePools;
                // 如果是房主，弹出对话框提示是否设置卡池属性。
                if (isRoomOwner || _isLocalGame)
                {
                    string poolsString = PoolsToString(packs);
                    string desc = game.lang._(ROOM_DECK_HAS_NOT_SHARED_POOL, poolsString);
                    desc += $"\n{game.lang._(ROOM_CARD_POOL_WARNING_FOR_OWNER)}";

                    List<(CardPoolInfoPack roomPool, CardPoolInfoPack comingPool)> conflictPools;
                    List<CardPoolInfoPack> addPools;
                    void ConfirmSuggestion()
                    {
                        if (CheckRoomPoolsValidation(addPools, null, out var missingDeps))
                        {
                            AddCardPools(addPools);
                            ApplyCardPools();
                            setPlayerDeck(_selectDeckPlayerId, deck);
                        }
                        else
                        {
                            ShowDependencyMissingDialog(missingDeps);
                        }
                    }

                    game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), desc, () =>
                    {
                        if (CanUploadCardPools(packs, out string errorMessage))
                        {
                            conflictPools = GetConflictCardPools(packs, out addPools);
                            ShowRoomConflictDialogs(conflictPools, addPools, ConfirmSuggestion);
                        }
                        else
                        {
                            string title = game.lang._(StringTable.ERROR);
                            game.ui.dialog.ShowConfirm(title, errorMessage);
                        }
                    });
                }
                // 否则，向房主提议加入卡池。
                else
                {
                    string poolsString = PoolsToString(packs);
                    string desc = game.lang._(ROOM_DECK_HAS_NOT_SHARED_POOL, poolsString);
                    desc += $"\n{game.lang._(ROOM_CARD_POOL_WARNING_FOR_CUSTOMER)}";

                    game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), desc, () =>
                    {
                        SuggestCardPools(new CardPoolSuggestion(packs));
                        //隐藏选择卡组界面
                        _ui.hideDeckSelectPanel();
                    });
                }
                return;
            }
            setPlayerDeck(_selectDeckPlayerId, deck);
        }
        #endregion

        #region 底部按钮
        private async void onClickStartButton()
        {
            if (_gameStarted)
                return;

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

            _gameStarted = true;
            updateStartAndReadyButton();
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
            setLocalPlayerProp(THHRoomPlayerDataHelper.PROP_READY, true);
            updateStartAndReadyButton();
        }
        private void onClickCancelReadyButton()
        {
            var localPlayer = getLocalPlayerData();
            localPlayer.setReady(false);
            setLocalPlayerProp(THHRoomPlayerDataHelper.PROP_READY, false);
            updateStartAndReadyButton();
        }
        private void onClickQuitButton()
        {
            _currentRoomData = null;
            game.network.logic.QuitRoom();

            _ui.gameObject.SetActive(false);
            scene.menu.display();


            if (cardPoolManager != null && cardPoolManager.running)
            {
                cardPoolManager.Cancel();
            }

            onQuitClick?.Invoke();
        }
        #endregion

        #region 卡池管理器回调
        private void onCardPoolProgressChanged(float value)
        {
            var player = _currentRoomData.getPlayer(GetSelfID());
            var index = _currentRoomData.playerDataList.IndexOf(player);
            _ui.setCardPoolLoadProgress(index, value);
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
                setLocalPlayerProp(RoomPlayerDataHelper.PROP_LOAD_STATE, (int)state);
            }
            if (localPlayer.getReady())
            {
                localPlayer.setReady(false);
                setLocalPlayerProp(THHRoomPlayerDataHelper.PROP_READY, false);
            }
            updateStartAndReadyButton();
            updatePlayerListUI();
        }
        private void onOutOfRetry(Exception e)
        {
            string title = game.lang._(StringTable.ERROR);

            game.ui.dialog.ShowConfirm(title, e.Message);
        }
        #endregion

        #region 网络回调
        private void onReceiveRoomDataChangedCallback(RoomData roomData)
        {
            updateRoomData(roomData);
        }
        private void onReceiveCardPoolsSuggestionCallback(int playerId, CardPoolSuggestion suggestion)
        {
            string poolsString = PoolsToString(suggestion.cardPools);
            var suggester = _currentRoomData.getPlayer(playerId);
            string playerName = game.lang._(ROOM_UNKNOWN_PLAYER);
            if (suggester != null)
            {
                playerName = suggester.name;
            }
            string desc = game.lang._(ROOM_CARD_POOLS_SUGGESTED, playerName, poolsString);

            List<(CardPoolInfoPack roomPool, CardPoolInfoPack comingPool)> conflictPools;
            List<CardPoolInfoPack> addPools;

            game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), desc, () =>
            {
                conflictPools = GetConflictCardPools(suggestion.cardPools, out addPools);
                ShowRoomConflictDialogs(conflictPools, addPools, ConfirmSuggestion, CancelSuggestion);
            },
            CancelSuggestion);


            async void ConfirmSuggestion()
            {
                if (CheckRoomPoolsValidation(addPools, null, out var missingDeps))
                {
                    AddCardPools(addPools);
                    ApplyCardPools();
                    await game.network.roomClient.AnwserCardPoolsSuggestion(playerId, suggestion, true);
                }
                else
                {
                    ShowDependencyMissingDialog(missingDeps);
                }
            }

            void CancelSuggestion()
            {
                game.network.roomClient.AnwserCardPoolsSuggestion(playerId, suggestion, false);
            }
        }
        private async void onReceiveRoomPropChangedCallback(string key, object value)
        {
            if (key == Option.CardPools.ToString())
            {
                if (!_isLocalGame && value is CardPoolInfoPack[] pools)
                {
                    _cardPoolPanelDirty = true;
                    await CheckCardPoolsUpdate(pools);
                }
            }
        }
        private async void onReceiveCardPoolsSuggestionAnwserCallback(CardPoolSuggestion suggestion, bool agree)
        {
            string desc = game.lang._(agree ? ROOM_CARD_POOLS_SUGGESTION_AGREED : ROOM_CARD_POOLS_SUGGESTION_REFUSED);
            game.ui.dialog.ShowConfirm(game.lang._(StringTable.HINT_TITLE), desc);

            if (agree)
            {
                await CheckCardPoolsUpdate(suggestion.cardPools);
            }
        }
        #endregion

        #endregion

        #region 玩家属性
        /// <summary>
        /// 设置本地的房间值，并发送房间值改变的通知
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        private void setNotifyRoomSetting(Option prop, object value)
        {
            _currentRoomData?.setOption(prop, value);
            updateRoomOptions();
            var roomClient = game.network.logic.roomClient;
            roomClient.SetRoomProp(prop.ToString(), value);
        }
        /// <summary>
        /// 设置本地玩家的属性，并将其发送到远端服务器
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="value">属性</param>
        private void setLocalPlayerProp(string name, object value)
        {
            var roomClient = game.network.logic.roomClient;
            roomClient.SetPlayerProp(name, value);
        }
        private async void setPlayerDeck(int playerId, Deck deck)
        {
            RoomPlayerData player = _currentRoomData.playerDataList.Find(p => p.id == playerId);
            if (player != null)
            {
                player.setDeck(deck);

                if (player.type == RoomPlayerType.ai)
                {
                    player.setReady(true);
                }
                if (playerId == GetSelfID())
                {
                    setLocalPlayerProp(THHRoomPlayerDataHelper.PROP_DECK, deck.toDefRefArray(true));
                    var cardPoolRef = getCardPoolReference(deck.masterCardRef.cardPoolId);
                    _ui.setLocalPlayerDeck(deck.name, await game.card.loadCardImage(cardPoolRef, deck.masterCardRef.defineId));
                }
            }
            //隐藏选择卡组界面
            _ui.hideDeckSelectPanel();
            //更新界面
            updatePlayerListUI();
            updateStartAndReadyButton();
        }
        #endregion

        #region 卡池

        #region 卡池判断
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
            privatePools = pools.Where(pool => !roomCardPools.Contains(pool)).ToArray();
            return privatePools.Length > 0;
        }
        private string PoolsToString(CardPoolInfoPack[] packs, bool showVersion = false, int maxCount = 10)
        {
            int count = Mathf.Min(packs.Length, maxCount);
            string poolsString = string.Join(", ", packs.Select(p =>
            {
                string str = p == null ? game.lang._(ROOM_UNKNOWN_DECK) : p.cardPoolName;
                if (showVersion)
                {
                    str += game.lang._(ROOM_CARD_POOL_VERSION, p.version);
                }
                return str;
            }).ToArray(), 0, count);
            if (packs.Length > maxCount)
            {
                poolsString += "...";
            }
            return poolsString;
        }
        /// <summary>
        /// 检查卡池是否能上传。
        /// </summary>
        /// <param name="cardPools">卡池列表。</param>
        /// <param name="errorMessage">错误信息。</param>
        /// <returns>能否上传。</returns>
        private bool CanUploadCardPools(CardPoolInfoPack[] cardPools, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool valid = true;
            List<CardSkinData> missingCardSkins = new List<CardSkinData>();
            List<BuffSkin> missingBuffSkins = new List<BuffSkin>();
            foreach (var poolInfoPack in cardPools)
            {
                if (game.card.isInnateCardPool(poolInfoPack.cardPoolId) || _isLocalGame)
                {
                    continue;
                }
                if (game.card.tryGetCardPool(poolInfoPack.GetReference(), out CardPool pool))
                {
                    foreach (var skin in pool.cardSkinList)
                    {
                        if (skin == null || (skin.imagePath != null && !game.card.isExternalImageExists(skin.imagePath)))
                        {
                            missingCardSkins.Add(skin);
                            valid = false;
                        }
                    }
                    foreach (var skin in pool.buffSkinList)
                    {
                        if (skin == null || (skin.imagePath != null && !game.card.isExternalImageExists(skin.imagePath)))
                        {
                            missingBuffSkins.Add(skin);
                            valid = false;
                        }
                    }
                }
            }
            if (!valid)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(game.lang._(ROOM_ERROR_CANNOT_UPLOAD_CARD_POOLS));
                const int MAX_DISPLAY_COUNT = 5;
                if (missingCardSkins.Count > 0)
                {
                    var cardNames = missingCardSkins.Select(s => s.name).ToArray();
                    var cardNamesStr = string.Join(",", cardNames, 0, Mathf.Min(MAX_DISPLAY_COUNT, cardNames.Length));
                    if (missingCardSkins.Count > MAX_DISPLAY_COUNT)
                        cardNamesStr += "...";

                    sb.AppendLine(game.lang._(ROOM_ERROR_MISSING_CARD_IMAGE, cardNamesStr));
                }
                if (missingBuffSkins.Count > 0)
                {
                    var buffNames = missingBuffSkins.Select(s => s.name).ToArray();
                    var buffNamesStr = string.Join(",", buffNames, 0, Mathf.Min(MAX_DISPLAY_COUNT, buffNames.Length));
                    if (missingBuffSkins.Count > MAX_DISPLAY_COUNT)
                        buffNamesStr += "...";

                    sb.AppendLine(game.lang._(ROOM_ERROR_MISSING_BUFF_IMAGE, buffNamesStr));
                }
                errorMessage = sb.ToString();
            }
            return valid;
        }
        /// <summary>
        /// 检测房间在添加、删除某些卡池后，卡池的合法性。
        /// </summary>
        /// <param name="addCardPools">添加的卡池。</param>
        /// <param name="removeCardPools">要删除的卡池。</param>
        /// <param name="missingDeps">丢失的依赖卡池。</param>
        /// <returns>是否合法。</returns>
        private bool CheckRoomPoolsValidation(IEnumerable<CardPoolInfoPack> addCardPools, IEnumerable<CardPoolInfoPack> removeCardPools, out CardPoolInfoPack[] missingDeps)
        {
            IEnumerable<CardPoolInfoPack> finalPools = _currentRoomData.getCardPools();
            if (removeCardPools != null)
                finalPools = finalPools.Except(removeCardPools);
            if (addCardPools != null)
                finalPools = finalPools.Union(addCardPools);
            CardPoolInfoPack[] dependencies = finalPools.GetDependencies(game.card);
            List<CardPoolInfoPack> missing = new List<CardPoolInfoPack>();
            foreach (var dep in dependencies)
            {
                if (!finalPools.Contains(dep))
                {
                    missing.Add(dep);
                }
            }
            missingDeps = missing.ToArray();
            return missing.Count <= 0;
        }
        private List<(CardPoolInfoPack roomPool, CardPoolInfoPack comingPool)> GetConflictCardPools(CardPoolInfoPack[] comingPools, out List<CardPoolInfoPack> addPools)
        {
            addPools = new List<CardPoolInfoPack>();
            List<(CardPoolInfoPack roomPool, CardPoolInfoPack comingPool)> conflictPools = new List<(CardPoolInfoPack roomPool, CardPoolInfoPack comingPool)>();

            // 如果当前房间中有ID相同，版本不同的卡池，弹出新的询问框。
            var roomPools = _currentRoomData.getCardPools();
            foreach (CardPoolInfoPack pack in comingPools)
            {
                CardPoolInfoPack sameIdPack = roomPools.FirstOrDefault(p => p.cardPoolId == pack.cardPoolId && p.version != pack.version);
                if (sameIdPack != null)
                {
                    conflictPools.Add((sameIdPack, pack));
                }
                else
                {
                    addPools.Add(pack);
                }
            }
            return conflictPools;
        }
        private async void LoadCardPoolsByFromCache(CardPoolInfoPack[] pools)
        {
            foreach (var cardPool in pools.Where(p => game.card.getCardPoolInfo(p) == null))
            {
                await game.card.loadCachedCardPoolAsync(cardPool.cardPoolId, cardPool.version);
            }
        }
        #endregion

        #region 卡池获取
        private ICardPoolReference getCardPoolReference(long cardPoolId)
        {
            return game.card.getCardPoolReferenceInEnv(cardPoolId, getCardPoolReferenceSet());
        }
        private IEnumerable<ICardPoolReference> getCardPoolReferenceSet()
        {
            return _currentRoomData.getCardPools();
        }
        private CardPoolInfoPack[] getDeckCardPools(Deck deck, out long[] missingPools)
        {
            List<long> missingPoolList = new List<long>();
            long[] poolsId = deck.getFullCardPools(game.card);

            // 根据卡池ID获取卡池信息
            List<CardPoolInfoPack> packs = new List<CardPoolInfoPack>();
            for (int i = 0; i < poolsId.Length; i++)
            {
                long id = poolsId[i];
                var poolRef = game.card.getLocalCardPoolReference(id);
                var poolInfo = game.card.getCardPoolInfo(poolRef);
                if (poolInfo != null)
                {
                    packs.Add(poolInfo.ToCardPoolInfoPack());
                }
                else
                {
                    // 未知卡组。
                    missingPoolList.Add(id);
                }
            }
            missingPools = missingPoolList.ToArray();
            return packs.ToArray();
        }
        private CardPoolInfoPack[] GetLocalCardPools()
        {
            List<CardPoolInfoPack> cardPools = new List<CardPoolInfoPack>();
            // 本地游戏，将玩家的本地的所有卡池加进去.
            var result = game.card.getLocalCardPoolInfos();
            CardPoolInfoPack[] localPools = result.Select(p => p.ToCardPoolInfoPack()).ToArray();
            cardPools.AddRange(localPools);
            return cardPools.ToArray();
        }
        #endregion

        #region 卡池修改
        private void AddCardPools(IEnumerable<CardPoolInfoPack> packs)
        {
            var roomPools = _currentRoomData.getCardPools();
            var newList = new List<CardPoolInfoPack>(roomPools);
            foreach (var pack in packs)
            {
                var duplicatePack = newList.Find(p => p.cardPoolId == pack.cardPoolId);
                if (duplicatePack != null)
                {
                    newList.Remove(duplicatePack);
                }
                newList.Add(pack);
            }
            _currentRoomData?.setOption(Option.CardPools, newList.ToArray());
        }
        private void RemoveCardPools(IEnumerable<CardPoolInfoPack> packs)
        {
            var roomPools = _currentRoomData.getCardPools();
            var newList = new List<CardPoolInfoPack>(roomPools);
            foreach (var pack in packs)
            {
                newList.Remove(pack);
            }
            _currentRoomData?.setOption(Option.CardPools, newList.ToArray());
        }
        private void ApplyCardPools()
        {
            setNotifyRoomSetting(Option.CardPools, _currentRoomData.getCardPools());
            _cardPoolPanelDirty = true;
        }
        private void SuggestCardPools(CardPoolSuggestion suggestion)
        {
            if (CanUploadCardPools(suggestion.cardPools, out string errorMessage))
            {
                game.network.roomClient.SuggestCardPools(suggestion);
            }
            else
            {
                string title = game.lang._(StringTable.ERROR);
                game.ui.dialog.ShowConfirm(title, errorMessage);
            }
        }
        private async Task CheckCardPoolsUpdate(CardPoolInfoPack[] pools)
        {
            if (_isLocalGame)
                return;
            await cardPoolManager.StartPlan(pools);
        }
        #endregion

        #endregion

        #region 编辑卡池界面
        private bool IsEditingCardPools()
        {
            return _editingCardPools != null;
        }
        private async Task<(IEnumerable<CardPoolInfoPack> addedPools, IEnumerable<CardPoolInfoPack> removedPools)> UpdateEditingCardPools()
        {
            var roomPools = _currentRoomData.getCardPools();
            var localPools = GetLocalCardPools();

            var newCardPools = localPools.Union(roomPools);
            // 忽略无法加载的卡池。
            List<CardPoolInfoPack> ignoredCardPools = new List<CardPoolInfoPack>();
            foreach (var pool in newCardPools)
            {
                var poolRef = game.card.getLocalCardPoolReference(pool.cardPoolId);
                var (_, cardPool) = await game.card.getOrLoadCardPool(poolRef, true);
                if (cardPool == null)
                {
                    Debug.LogError($"无法在添加房间卡池时加载卡池，存在被忽略的卡池：{pool.cardPoolName}");
                    ignoredCardPools.Add(pool);
                }
            }
            newCardPools = newCardPools.Except(ignoredCardPools);

            // 计算添加的卡池和移除的卡池。
            IEnumerable<CardPoolInfoPack> addedPools, removedPools;
            if (_editingCardPools == null)
            {
                addedPools = newCardPools;
                removedPools = Array.Empty<CardPoolInfoPack>();
            }
            else
            {
                addedPools = newCardPools.Except(_editingCardPools);
                removedPools = _editingCardPools.Except(newCardPools);
            }



            _editingCardPools = newCardPools.ToArray();

            return (addedPools, removedPools);
        }
        private void AddSelectedEditingCardPools(IEnumerable<CardPoolInfoPack> addPools)
        {
            if (addPools == null)
            {
                return;
            }

            if (_selectedEditCardPools == null)
            {
                _selectedEditCardPools = new HashSet<CardPoolInfoPack>();
            }
            foreach (var pool in addPools)
            {
                _selectedEditCardPools.Add(pool);
            }
        }
        private void RemoveSelectedEditingCardPools(IEnumerable<CardPoolInfoPack> removePools)
        {
            if (removePools == null || _selectedEditCardPools == null)
            {
                return;
            }

            foreach (var pool in removePools)
            {
                _selectedEditCardPools.Remove(pool);
            }
        }
        private async Task RefreshRoomCardPoolPanels()
        {
            if (_ui.isViewingCardPools())
            {
                if (IsEditingCardPools())
                {
                    var modifies = await UpdateEditingCardPools();
                    AddSelectedEditingCardPools(modifies.addedPools);
                    RemoveSelectedEditingCardPools(modifies.removedPools);
                    await openEditCardPoolsPanel();
                }
                else
                {
                    await viewRoomCardPools();
                }
            }
        }
        private async Task viewRoomCardPools()
        {
            _viewingCardPools = _currentRoomData.getCardPools();
            var count = _viewingCardPools.Length;
            if (isRoomOwner)
                count++;
            var viewDatas = new DeckListItemViewData[count];
            for (int i = 0; i < count; i++)
            {
                if (isRoomOwner && i == count - 1)
                {
                    viewDatas[i] = new DeckListItemViewData()
                    {
                        mode = DeckListItem.Mode.Button,
                        buttonText = game.lang._(ROOM_BUTTON_EDIT_CARD_POOL)
                    };
                }
                else
                {
                    var pool = _viewingCardPools[i];
                    viewDatas[i] = new DeckListItemViewData()
                    {
                        name = pool.cardPoolName,
                        image = await game.card.loadCardPoolImage(pool),
                        mode = DeckListItem.Mode.Default
                    };
                }
            }
            var title = game.lang._(ROOM_TITLE_VIEW_CARD_POOLS);
            _ui.viewCardPools(viewDatas, title);
        }
        private async Task openEditCardPoolsPanel()
        {
            var viewDatas = new DeckListItemViewData[_editingCardPools.Length];
            for (int i = 0; i < viewDatas.Length; i++)
            {
                var pool = _editingCardPools[i];
                bool selected = _selectedEditCardPools.Any(pack => pack.CompareCardPool(pool));
                Color color = selected ? Color.grey : Color.white;
                string name = pool.cardPoolName;
                if (_editingCardPools.Count(p => p.cardPoolId == pool.cardPoolId) >= 2)
                {
                    name = game.lang._(ROOM_CARD_POOL_VERSION_TEMPLATE, name, pool.version);
                }
                viewDatas[i] = new DeckListItemViewData()
                {
                    name = name, 
                    image = await game.card.loadCardPoolImage(pool), 
                    selected = selected,
                    mode = DeckListItem.Mode.CanSelect
                };
            }
            var title = game.lang._(ROOM_TITLE_EDIT_CARD_POOLS);
            _ui.openAddCardPoolsPanel(viewDatas, title);
        }
        private void closeEditCardPoolsPanel()
        {
            _editingCardPools = null;
            _selectedEditCardPools?.Clear();
            _ = viewRoomCardPools();
        }
        #endregion

        #region 底部按钮
        private void updateStartAndReadyButton()
        {
            if (_currentRoomData == null)
                return;
            if (isRoomOwner)
            {
                _ui.displayStartButton(true);
                _ui.displayReadyButton(false);
                _ui.displayCancelReadyButton(false);
                _ui.enableStartButton(isAbleToStartGame());
            }
            else
            {
                _ui.displayStartButton(false);
                RoomPlayerData localPlayer = getLocalPlayerData();
                _ui.displayReadyButton(isAbleToReady(localPlayer) && !localPlayer.getReady());
                _ui.displayCancelReadyButton(localPlayer.getReady());
            }

            // AI自动准备
            foreach (var ai in _currentRoomData.playerDataList.Where(a => a.type == RoomPlayerType.ai))
            {
                ai.setReady(true);
            }
        }
        private bool isAbleToStartGame()
        {
            if (_currentRoomData.playerDataList.Count < 2)//必须有对手才能开始游戏
                return false;
            if (!isAbleToReady(getLocalPlayerData()))//房主自己必须准备好
                return false;
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
                // 卡组内的卡牌必须存在于房间内的卡池。
                return deckValid && !HasNotSharedPools(getDeckCardPools(deck, out _), out _);
            }
            return false;
        }
        #endregion

        #region UI
        private void updatePlayerListUI()
        {
            if (_currentRoomData == null)
                return;

            RoomPlayerItemViewData[] viewDatas = new RoomPlayerItemViewData[_currentRoomData.playerDataList.Count];
            for (int i = 0; i < viewDatas.Length; i++)
            {
                RoomPlayerData player = _currentRoomData.playerDataList[i];
                var playerName = player.name;
                Deck deck = player.getDeck();
                var characterName = string.Empty;
                if (deck != null)
                {
                    var cardPoolRef = getCardPoolReference(deck.masterCardRef.cardPoolId);
                    if (game.card.tryGetCardSkin(cardPoolRef, deck.masterCardRef.defineId, out CardSkinData skin))
                    {
                        characterName = skin.name;
                    }
                    else
                    {
                        characterName = game.langManager.getString(ROOM_UNKNOWN_CHARACTER_NAME);
                    }
                }
                var loadState = player.getLoadState();
                var isReady = player.getReady();
                var isAI = player.type == RoomPlayerType.ai;
                var canSetDeck = isAI;
                var canViewDeck = player.type == RoomPlayerType.human && deck != null;
                var canRemove = GetSelfID() == _currentRoomData.ownerId && player.id != _currentRoomData.ownerId;
                viewDatas[i] = new RoomPlayerItemViewData()
                {
                    name = playerName,
                    characterName = characterName,
                    loadState = loadState,
                    isReady = isReady,
                    canViewDeck = canViewDeck,
                    canSetDeck = canSetDeck,
                    canRemove = canRemove,
                    isAI = isAI
                };
            }
            _ui.updatePlayerList(viewDatas);
            _ui.displayAddAIButton(_currentRoomData.playerDataList.Count < _currentRoomData.maxPlayerCount && _isLocalGame);
        }
        private void updateRoomOptions()
        {
            var room = _currentRoomData;
            var seed = room.getRandomSeed();
            var isLocalGame = _isLocalGame;
            var isNetOwner = isRoomOwner && !isLocalGame;
            var roomName = isLocalGame ? game.lang._(ROOM_LOCAL_GAME_NAME) : room.getRoomName();
            var password = string.IsNullOrEmpty(room.getRoomPassword()) ? "" : "******";
            var randomSeed = seed == 0 ? "" : seed.ToString();
            var shuffle = room.getShuffle();
            var initReplaceTime = room.getTimeoutForInitReplace().ToString();
            var turnTime = room.getTimeoutForTurn().ToString();
            var disableRandomPlayerOrder = room.getSortedPlayers() != null;
            var viewData = new RoomOptionsViewData()
            {
                roomName = roomName,
                roomNameEditable = isNetOwner,
                password = password,
                passwordEditable = isNetOwner,
                seed = randomSeed,
                seedEditable = isRoomOwner,
                shuffle = shuffle,
                shuffleEditable = isRoomOwner,
                initReplaceTime = initReplaceTime,
                initReplaceTimeEditable = isRoomOwner,
                turnTime = turnTime,
                turnTimeEditable = isRoomOwner,
                disableRandomPlayerOrder = disableRandomPlayerOrder,
                disableRandomPlayerOrderEditable = isRoomOwner,
            };
            _ui.updateRoomOptions(viewData);
        }
        private async Task displaySelectDeckPanel()
        {
            List<Deck> deckList = game.deck.getOrLoadDeckList();
            DeckListItemViewData[] viewData = new DeckListItemViewData[deckList.Count];

            for (int i = 0; i < viewData.Length; i++)
            {
                Deck deck = deckList[i];
                CardPoolInfoPack[] cardPoolList = getDeckCardPools(deck, out _);

                var name = deck.name;

                // 如果卡组内有不存在的卡池，加载卡池。
                foreach (CardPoolInfoPack cardPoolPack in cardPoolList)
                {
                    var cardPoolRef = cardPoolPack.GetReference();
                    await game.card.getOrLoadCardPool(cardPoolRef, true);
                }
                var masterPoolRef = game.card.getLocalCardPoolReference(deck.masterCardRef.cardPoolId);
                var image = await game.card.loadCardImage(masterPoolRef, deck.masterCardRef.defineId);

                bool warning;
                // 判断牌组是否合法。
                if (_isLocalGame)
                    warning = !game.deck.isValidDeck(deck);
                else
                    warning = !game.deck.isValidDeckForOnlineGame(deck);

                viewData[i] = new DeckListItemViewData()
                {
                    name = name,
                    image = image,
                    isWarning = warning,
                    hasPrivatePools = HasNotSharedPools(cardPoolList, out _),
                    mode = DeckListItem.Mode.Default,
                    banned = warning
                };
            }
            _ui.displayDeckSelectPanel(viewData);
        }        
        /// <summary>
        /// 显示卡池依赖丢失的窗口。
        /// </summary>
        /// <param name="missingDeps">丢失的依赖卡池。</param>
        private void ShowDependencyMissingDialog(CardPoolInfoPack[] missingDeps)
        {
            string title = game.lang._(ROOM_CARD_POOL_MODIFY_FAILED);
            string desc = game.lang._(ROOM_CARD_POOL_DEPENDENCY_MISSING, PoolsToString(missingDeps, true));
            game.ui.dialog.ShowConfirm(title, desc);
        }
        private void ShowRoomConflictDialogs(List<(CardPoolInfoPack roomPool, CardPoolInfoPack comingPool)> conflictPools,
            List<CardPoolInfoPack> addingPools,
            Action confirmAction = null,
            Action cancelAction = null)
        {
            if (conflictPools.Count > 0)
            {
                var pools = conflictPools[0];
                CardPoolInfoPack roomPool = pools.roomPool;
                CardPoolInfoPack comingPool = pools.comingPool;
                string conflictDesc = game.lang._(ROOM_CARD_POOLS_CONFLICTED, roomPool.cardPoolName, comingPool.version, roomPool.version);
                string[] options = new string[]
                {
                    game.lang._(ROOM_CARD_POOLS_USE_POOL_BUTTON, roomPool.version),
                    game.lang._(ROOM_CARD_POOL_CANCEL),
                    game.lang._(ROOM_CARD_POOLS_USE_POOL_BUTTON, comingPool.version)
                };
                game.ui.dialog.ShowOptions(game.lang._(ROOM_CARD_POOLS_CONFLICTED_TITLE), conflictDesc, options, (index) =>
                {
                    switch (index)
                    {
                        case 0:
                            addingPools.Add(roomPool);
                            conflictPools.RemoveAt(0);
                            ShowRoomConflictDialogs(conflictPools, addingPools, confirmAction, cancelAction);
                            break;
                        case 1:
                            cancelAction?.Invoke();
                            break;
                        case 2:
                            addingPools.Add(comingPool);
                            conflictPools.RemoveAt(0);
                            ShowRoomConflictDialogs(conflictPools, addingPools, confirmAction, cancelAction);
                            break;
                    }
                });
            }
            else
            {
                confirmAction?.Invoke();
            }
        }
        #endregion

        #endregion

        #region 事件
        /// <summary>
        /// 点击退出房间后触发
        /// </summary>
        public event Action onQuitClick;
        #endregion

        #region 属性字段
        private DisplayInfo _displayInfo;
        /// <summary>
        /// 是否为房主
        /// </summary>
        bool isRoomOwner => GetSelfID() == _currentRoomData.ownerId;
        [SerializeField]
        RoomPanel _ui;
        [SerializeField]
        RoomData _currentRoomData;
        [SerializeField]
        bool _isLocalGame = true;
        [SerializeField]
        int _selectDeckPlayerId = 0;

        private Deck _viewingDeck;
        private RoomCardPoolManager cardPoolManager;
        /// <summary>
        /// 正在查看的卡池列表。
        /// </summary>
        CardPoolInfoPack[] _viewingCardPools;
        /// <summary>
        /// 编辑卡池界面中待选的卡池。
        /// </summary>
        CardPoolInfoPack[] _editingCardPools;
        /// <summary>
        /// 编辑卡池界面中已被选中的卡池。
        /// </summary>
        HashSet<CardPoolInfoPack> _selectedEditCardPools;
        /// <summary>
        /// 游戏是否已经开始，用于阻止玩家再次点击开始游戏。
        /// </summary>
        bool _gameStarted;
        /// <summary>
        /// 卡池显示面板是否需要更新。
        /// </summary>
        bool _cardPoolPanelDirty;

        #region 常量
        [TranslateMsg("房间显示-卡池显示界面-名称版本模板（{0}为卡池名称，{1}为版本号）")]
        private const string ROOM_CARD_POOL_VERSION_TEMPLATE = "{0}({1})";
        [TranslateMsg]
        private const string ROOM_SELECT_YOUR_DECK = "选择你的卡组";
        [TranslateMsg("在房间页面中，当玩家角色获取失败时显示的角色名称")]
        private const string ROOM_UNKNOWN_CHARACTER_NAME = "未知人物";
        [TranslateMsg("房间-查看卡池面板的标题")]
        private const string ROOM_TITLE_VIEW_CARD_POOLS = "查看本房间卡池";
        [TranslateMsg("房间-编辑可用卡池面板的标题")]
        private const string ROOM_TITLE_EDIT_CARD_POOLS = "编辑可用卡池";
        [TranslateMsg("房间界面_人机对战房间名称")]
        private const string ROOM_LOCAL_GAME_NAME = "人机对战";

        [TranslateMsg("房间-尝试选择包含未共享卡池的卡组（{0}：卡池名称列表）")]
        public const string ROOM_DECK_HAS_NOT_SHARED_POOL = "该套牌中包含房间设置之外的卡池：\n{0}";
        [TranslateMsg("房间-尝试添加未共享的卡池（{0}：卡池名称列表）")]
        public const string ROOM_ADD_SELECTED_CARD_POOL = "你选中了这些卡池：\n{0}";
        [TranslateMsg("房间-房主-尝试添加未共享的卡池警告")]
        public const string ROOM_CARD_POOL_WARNING_FOR_OWNER = "是否使用这些卡池？";
        [TranslateMsg("房间-玩家-尝试添加未共享的卡池警告")]
        public const string ROOM_CARD_POOL_WARNING_FOR_CUSTOMER = "是否向房主提议使用这些卡池？";


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
        [TranslateMsg("房间-玩家-无法上传卡组的提示")]
        public const string ROOM_ERROR_CANNOT_UPLOAD_CARD_POOLS = "无法上传该卡池：";
        [TranslateMsg("房间-玩家-无法上传卡组的提示")]
        public const string ROOM_ERROR_MISSING_CARD_POOL = "卡组内的卡牌属于不存在的卡池，或引用了不存在的卡池。";
        [TranslateMsg("房间-玩家-无法上传卡组的提示（{0}：卡牌皮肤名称列表）")]
        public const string ROOM_ERROR_MISSING_CARD_IMAGE = "卡图丢失：{0}";
        [TranslateMsg("房间-玩家-无法上传卡组的提示（{0}：增益皮肤名称列表）")]
        public const string ROOM_ERROR_MISSING_BUFF_IMAGE = "增益图片丢失：{0}";
        [TranslateMsg("房间-移除卡池时的警告（{0}为卡池名）")]
        public const string ROOM_REMOVE_CARD_POOL_WARNING = "你确定要在本房间内移除卡池“{0}”吗？";
        [TranslateMsg("房间-移除卡池时的警告-需要连带移除其他卡池时的附加信息（{0}为卡池名列表）")]
        public const string ROOM_REMOVE_CARD_POOL_WARNING_DEPENDENCIES = "以下卡池会一起被移除！\n{0}";

        [TranslateMsg("房间-卡池冲突警告-标题")]
        public const string ROOM_CARD_POOLS_CONFLICTED_TITLE = "卡池版本冲突";
        [TranslateMsg("房间-卡池冲突警告-窗口描述（{0}：卡池名，{1}：尝试加入的卡池版本，{2}：已设置的卡池版本）")]
        public const string ROOM_CARD_POOLS_CONFLICTED = "尝试加入卡池“{0}”的版本{1}，与当前已设置的卡池{0}版本{2}冲突，请选择要使用的版本。";
        [TranslateMsg("房间-卡池冲突警告-选择使用的版本的按钮文字（{0}：要使用的版本号）")]
        public const string ROOM_CARD_POOLS_USE_POOL_BUTTON = "使用版本{0}";
        [TranslateMsg("房间-卡池冲突警告-取消按钮")]
        public const string ROOM_CARD_POOL_CANCEL = "取消";

        [TranslateMsg("房间-卡池编辑-添加卡池提示模板（{0}：添加卡池提醒，{1}：移除卡池提醒，{2}：是否继续提醒）")]
        public const string ROOM_EDIT_CARD_POOL_TEMPLATE = "{0}{1}{2}";
        [TranslateMsg("房间-卡池编辑-添加卡池提示（{0}：卡池名列表）")]
        public const string ROOM_EDIT_CARD_POOL_ADD = "即将添加卡池：{0}\n";
        [TranslateMsg("房间-卡池编辑-移除卡池提示（{0}：卡池名列表）")]
        public const string ROOM_EDIT_CARD_POOL_REMOVE = "即将移除卡池：{0}\n";
        [TranslateMsg("房间-卡池编辑-是否继续提醒")]
        public const string ROOM_EDIT_CARD_POOL_CONTINUE = "是否继续？";
        [TranslateMsg("房间-卡池编辑-编辑卡池按钮的文字")]
        public const string ROOM_BUTTON_EDIT_CARD_POOL = "编辑卡池";

        [TranslateMsg("房间-卡池操作失败-标题")]
        public const string ROOM_CARD_POOL_MODIFY_FAILED = "卡池操作失败";
        [TranslateMsg("房间-卡池操作失败-卡池依赖不存在（{0}为丢失的必需卡池）")]
        public const string ROOM_CARD_POOL_DEPENDENCY_MISSING = "修改后的卡池内不包含以下必须包含的卡池：\n{0}";
        [TranslateMsg("房间-卡池版本后缀（{0}为版本号）")]
        public const string ROOM_CARD_POOL_VERSION = "（版本{0}）";
        #endregion

        #endregion

        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(RoomData roomData, string ipText, RoomMode roomMode)
            {
                this.roomData = roomData;
                this.ipText = ipText;
                this.roomMode = roomMode;
            }
            public RoomData roomData;
            public string ipText;
            public RoomMode roomMode;
        }
        #endregion
    }
}
