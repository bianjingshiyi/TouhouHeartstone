using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        [TranslateMsg("房间-卡池操作失败-标题")]
        public const string ROOM_CARD_POOL_MODIFY_FAILED = "卡池操作失败";
        [TranslateMsg("房间-卡池操作失败-卡池依赖不存在（{0}为丢失的必需卡池）")]
        public const string ROOM_CARD_POOL_DEPENDENCY_MISSING = "修改后的卡池内不包含以下必须包含的卡池：\n{0}";
        [TranslateMsg("房间-卡池版本后缀（{0}为版本号）")]
        public const string ROOM_CARD_POOL_VERSION = "（版本{0}）";


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
                    game.network.AnwserCardPoolsSuggestion(playerId, suggestion, true);
                    await cardPoolManager.CheckUpdate(addPools.ToArray());
                }
                else
                {
                    ShowDependencyMissingDialog(missingDeps);
                }
            }

            void CancelSuggestion()
            {
                game.network.AnwserCardPoolsSuggestion(playerId, suggestion, false);
            }
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
        public Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _currentRoomData = info.roomData;
                _isLocalGame = info.isLocalGame;

                _isDisplay = true;
                _gameStarted = false;

                _ui.display();

                if (_isLocalGame)
                {
                    _ui.displayIPText(false);
                }
                if (isRoomOwner)
                {
                    CardPoolInfoPack[] roomPools = new CardPoolInfoPack[] { CardPoolHelper.BuiltinCardPoolInfoPack };
                    _currentRoomData.setOption(Option.CardPools, roomPools);
                    setNotifyRoomSetting(Option.CardPools, roomPools);
                }
                _ui.setRandomSeed(_currentRoomData.getRandomSeed());
                _ui.setShuffle(_currentRoomData.getShuffle());
                _ui.setInitReplaceTime(_currentRoomData.getTimeoutForInitReplace());
                _ui.setTurnTime(_currentRoomData.getTimeoutForTurn());
                _ui.setDisableRandomPlayerActOrder(_currentRoomData.getSortedPlayers() != null);

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
            return Task.CompletedTask;
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
                    _cardPoolPanelDirty = true;
                    await cardPoolManager.CheckUpdate(pools);
                }
            }
        }
        public void UpdateRoomData(RoomData roomData)
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
            _ui.onViewDeckClick += onViewDeckClick;
            _ui.onViewCardPoolsClick += onViewCardPoolsClick;
            _ui.onAddCardPoolsClick += onAddCardPoolsClick;
            _ui.onClickPlayerItemViewDeckButton += onClickPlayerListItemViewDeckButton;
            _ui.onClickPlayerItemSetDeckButton += onClickPlayerListItemSetDeckButton;
            _ui.onClickPlayerItemRemoveButton += onClickPlayerListItemRemoveButton;
            _ui.onClickAddAIPlayerButton += onClickAddAIPlayerButton;

            _ui.onClickSelectDeckButton += onClickSelectDeckButton;
            _ui.onClickDeckSelectPanelItem += onClickDeckSelectPanelItem;
            _ui.onClickCardPoolsSelectPanelItem += onClickCardPoolSelectPanelItem;
            _ui.onAddCardPoolsSelectItemToggle += onAddCardPoolSelectPanelItemToggle;

            _ui.onConfirmAddCardPools += onConfirmAddCardPools;
            _ui.onCloseCardPoolsPanel += onCloseCardPoolsPanel;

            _ui.onClickStartButton += onClickStartButton;
            _ui.onClickReadyButton += onClickReadyButton;
            _ui.onClickCancleReadyButton += onClickCancleReadyButton;
            _ui.onClickQuitButton += onClickQuitButton;

            _ui.onUpdateButtons += onUpdateButtons;
            _ui.onPlayerRemove += onPlayerRemove;
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
        private async void onViewDeckClick(DefineReference[] cards)
        {
            var cardManager = game.card;
            //_cardViewPanel.setItems(cardManager,cards);
            async Task<(string name, Sprite image)> getCardData(DefineReference cardRef)
            {
                var cardPoolRef = cardManager.getCardPoolReferenceInSet(cardRef.cardPoolId, _currentRoomData.getCardPools());
                var card = cardManager.getCardDefine(cardPoolRef, cardRef.defineId);
                var skin = cardManager.getCardSkin(cardPoolRef, cardRef.defineId);
                Sprite image = null;
                if (card != null)
                {
                    var path = skin?.imagePath;
                    image = await cardManager.loadCardImage(cardPoolRef, cardRef.defineId);
                }
                return (skin?.name, image);
            }
            List<(string name, Sprite image)> list = new List<(string name, Sprite image)>();
            foreach (var cardRef in cards)
            {
                list.Add(await getCardData(cardRef));
            }
            _ui.updateViewCardPanel(list.ToArray());
        }
        private void onViewCardPoolsClick()
        {
            viewRoomCardPools();
        }
        private async void onAddCardPoolsClick()
        {
            await UpdateEditingCardPools();
            _selectedEditCardPools?.Clear();
            AddSelectedEditingCardPools(_currentRoomData.getCardPools());
            openEditCardPoolsPanel();
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
                    if (isRoomOwner)
                    {
                        string poolsString = PoolsToString(packs);
                        string desc = game.lang._(ROOM_DECK_HAS_NOT_SHARED_POOL, poolsString);
                        desc += "\n" + game.lang._(ROOM_CARD_POOL_WARNING_FOR_OWNER);

                        List<(CardPoolInfoPack roomPool, CardPoolInfoPack comingPool)> conflictPools;
                        List<CardPoolInfoPack> addPools;
                        void ConfirmSuggestion()
                        {
                            if (CheckRoomPoolsValidation(addPools, null, out var missingDeps))
                            {
                                AddCardPools(addPools);
                                SetPlayerDeck(_selectDeckPlayerId, deck);
                                _ = cardPoolManager.CheckUpdate(_currentRoomData.getCardPools());
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
                        desc += "\n" + game.lang._(ROOM_CARD_POOL_WARNING_FOR_CUSTOMER);

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
        private void onClickCardPoolSelectPanelItem(int index)
        {
            if (IsEditingCardPools())
            {
                var pool = _editingCardPools[index];
                onAddCardPoolSelectPanelItemToggle(index, !_selectedEditCardPools.Contains(pool));
            }
        }
        private void onAddCardPoolSelectPanelItemToggle(int index, bool value)
        {
            var pool = _editingCardPools[index];
            // 不能操作内置卡池。
            if (game.card.isBuiltinCardPool(pool.cardPoolId))
            {
                _ui.openAddCardPoolsPanel(_editingCardPools, _selectedEditCardPools.Select(p => p.GetReference()));
                return;
            }

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
            _ui.openAddCardPoolsPanel(_editingCardPools, _selectedEditCardPools.Select(p => p.GetReference()));
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
                            _ = cardPoolManager.CheckUpdate(_currentRoomData.getCardPools());
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
        private async void onClickStartButton()
        {
            if (_gameStarted)
                return;

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
                var cardPoolRef = game.card.getCardPoolReferenceInSet(deck.masterCardRef.cardPoolId, _currentRoomData.getCardPools());
                _ui.updateLocalPlayerDeck(deck.name, await game.card.loadCardImage(cardPoolRef, deck.masterCardRef.defineId));
            }
        }
        #region 卡池
        private CardPoolInfoPack[] getDeckCardPools(Deck deck, out long[] missingPools)
        {
            List<long> missingPoolList = new List<long>();
            long[] poolsId = deck.getFullCardPools(game.card);

            // 根据卡池ID获取卡池信息
            List<CardPoolInfoPack> packs = new List<CardPoolInfoPack>();
            for (int i = 0; i < poolsId.Length; i++)
            {
                long id = poolsId[i];
                if (game.card.isBuiltinCardPool(id))
                {
                    packs.Add(CardPoolHelper.BuiltinCardPoolInfoPack);
                    continue;
                }
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
            CardPoolInfoPack[] cardPools = newList.ToArray();
            setNotifyRoomSetting(Option.CardPools, cardPools);
            _cardPoolPanelDirty = true;
        }
        private void RemoveCardPools(IEnumerable<CardPoolInfoPack> packs)
        {
            var roomPools = _currentRoomData.getCardPools();
            var newList = new List<CardPoolInfoPack>(roomPools);
            foreach (var pack in packs)
            {
                newList.Remove(pack);
            }
            CardPoolInfoPack[] cardPools = newList.ToArray();
            setNotifyRoomSetting(Option.CardPools, cardPools);
            _cardPoolPanelDirty = true;
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
        private void SuggestCardPools(CardPoolSuggestion suggestion)
        {
            if (CanUploadCardPools(suggestion.cardPools, out string errorMessage))
            {
                game.network.SuggestCardPools(suggestion);
            }
            else
            {
                string title = game.lang._(StringTable.ERROR);
                game.ui.dialog.ShowConfirm(title, errorMessage);
            }
        }
        /// <summary>
        /// 获取房间中依赖目标卡池的卡池。
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        private CardPoolInfoPack[] GetDependedCardPools(CardPoolInfoPack pool)
        {
            var packs = _currentRoomData.getCardPools();
            return pool.GetDependedCardPools(packs);
        }

        private async Task<CardPoolInfoPack[]> GetLocalCardPools()
        {
            List<CardPoolInfoPack> cardPools = new List<CardPoolInfoPack>() { CardPoolHelper.BuiltinCardPoolInfoPack };
            // 本地游戏，将玩家的本地的所有卡池加进去.
            var result = await game.card.loadCardPoolInfosAsync();
            CardPoolInfoPack[] localPools = result.Values.Select(p => p.ToCardPoolInfoPack()).ToArray();
            cardPools.AddRange(localPools);
            return cardPools.ToArray();
        }

        #region 编辑卡池界面
        private bool IsEditingCardPools()
        {
            return _editingCardPools != null;
        }
        private async Task<(IEnumerable<CardPoolInfoPack> addedPools, IEnumerable<CardPoolInfoPack> removedPools)> UpdateEditingCardPools()
        {
            var roomPools = _currentRoomData.getCardPools();
            var localPools = await GetLocalCardPools();

            var newCardPools = localPools.Union(roomPools);

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
                    openEditCardPoolsPanel();
                }
                else
                {
                    viewRoomCardPools();
                }
            }
        }
        private void viewRoomCardPools()
        {
            _viewingCardPools = _currentRoomData.getCardPools();
            _ui.viewCardPools(_viewingCardPools, isRoomOwner);
        }
        private void openEditCardPoolsPanel()
        {
            _ui.openAddCardPoolsPanel(_editingCardPools, _selectedEditCardPools.Select(p => p.GetReference()));
        }
        private void closeEditCardPoolsPanel()
        {
            _editingCardPools = null;
            _selectedEditCardPools?.Clear();
            viewRoomCardPools();
        }
        #endregion
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
        #endregion
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
                else
                {
                    var cardPoolRef = game.card.getCardPoolReferenceInSet(deck.masterCardRef.cardPoolId, _currentRoomData.getCardPools());
                    if (game.card.tryGetCardSkin(cardPoolRef, deck.masterCardRef.defineId, out CardSkinData skin))
                    {
                        viewData[i].characterName = skin.name;
                    }
                    else
                    {
                        viewData[i].characterName = game.langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME);
                    }
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

            for (int i = 0; i < viewData.Length; i++)
            {
                Deck deck = deckList[i];
                CardPoolInfoPack[] cardPoolList = getDeckCardPools(deck, out _);

                viewData[i].name = deck.name;

                // 如果卡组内有不存在的卡池，加载卡池。
                foreach (CardPoolInfoPack cardPoolPack in cardPoolList)
                {
                    var cardPoolRef = cardPoolPack.GetReference();
                    if (!game.card.tryGetCardPool(cardPoolRef, out _))
                    {
                        var path = game.card.getCardPoolPath(cardPoolRef);
                        if (path is string)
                        {
                            await game.card.loadCardPool(path);
                        }
                    }
                }
                var masterPoolRef = game.card.getCardPoolReferenceInSet(deck.masterCardRef.cardPoolId, _currentRoomData.getCardPools());
                viewData[i].image = await game.card.loadCardImage(masterPoolRef, deck.masterCardRef.defineId);

                bool warning;
                // 判断牌组是否合法。
                if (_isLocalGame)
                {
                    // 本地模式下，如果卡组内有不允许的卡池的卡牌，不能选择该卡组。
                    warning = !game.deck.isValidDeck(deck) || HasNotSharedPools(getDeckCardPools(deck, out _), out _);
                }
                else
                {
                    warning = !game.deck.isValidDeckForOnlineGame(deck);
                }
                viewData[i].isWarning = warning;

                viewData[i].hasPrivatePools = HasNotSharedPools(cardPoolList, out _);
            }
            _ui.displayDeckSelectPanel(viewData);
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
