using System;
using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using BJSYGameCore.UI;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    partial class RoomPanel
    {
        #region 常量
        [TranslateMsg]
        private const string Cards_Name = "选择你的卡组";
        [TranslateMsg("房间显示-卡池显示界面-名称版本模板（{0}为卡池名称，{1}为版本号）")]
        private const string ROOM_CARD_POOL_VERSION_TEMPLATE = "{0}({1})";
        [TranslateMsg("在房间页面中，当玩家角色获取失败时显示的角色名称")]
        private const string ROOM_UNKNOWN_CHARACTER_NAME = "未知角色";
        [TranslateMsg("房间-查看卡池面板的标题")]
        private const string ROOM_TITLE_VIEW_CARD_POOLS = "查看本房间卡池";
        [TranslateMsg("房间-编辑可用卡池面板的标题")]
        private const string ROOM_TITLE_EDIT_CARD_POOLS = "编辑可用卡池";
        #endregion
        #region 公有方法
        public void SetCardPoolLoadProgress(float progress)
        {
            _selfPlayerItem?.SetProgress(progress);
        }
        public void UpdateRoomData(RoomData room, RoomMode mode, int selfPlayerID, bool clearAIPlayerData = false)
        {
            if (clearAIPlayerData)
            {
                aiPlayerData = null;
            }

            RoomPlayerData selfPlayer = null;

            bool isRoomOwner = mode == RoomMode.Local || room?.ownerId == selfPlayerID;
            // 如果不修改_playerList的元素，会导致_playerList在调用updatePlayerList时包含已经被销毁的游戏对象，然后报错
            _playerList.Clear();
            RoomPlayerList.clearItems();
            foreach (var player in room.playerDataList)
            {


                var item = RoomPlayerList.addItem();
                _playerList.Add(item.rectTransform);

                // 设置套牌的英雄名称。
                string characterText = string.Empty;
                DefineReference[] deck = player.getDeck()?.toDefRefArray(true);
                if (deck != null)
                {
                    var cardManager = this.findInstance<CardManager>();
                    DefineReference masterRef = deck[0];
                    var cardPoolRef = cardManager.getCardPoolReferenceInSet(masterRef.cardPoolId, room.getCardPools());
                    var skin = cardManager.getCardSkin(cardPoolRef, masterRef.defineId);
                    if (skin != null)
                    {
                        characterText = skin.name;
                    }
                    else
                    {
                        characterText = ROOM_UNKNOWN_CHARACTER_NAME;
                    }
                }
                item.SetPlayerInfo(player, characterText);


                item.OnPlayerViewDeck += onViewDeck;

                // 顺便获取自己的引用
                if (player.id == selfPlayerID)
                {
                    selfPlayer = player;
                    _selfPlayerItem = item;
                }
                // 仅当自己是主机且此玩家不是自己时，设置为可以移除
                item.SetRemoveable(isRoomOwner && selfPlayerID != player.id);
                item.ViewButtonBlack.asButton.interactable = player.getDeck() != null;
                item.OnPlayerRemove += (id) =>
                {
                    if (id == aiPlayerData?.id)
                        aiPlayerData = null;
                    onPlayerRemove?.Invoke(id);
                };

                if (player.type == RoomPlayerType.ai)
                {
                    aiPlayerData = player;

                    item.SetMode(RoomPlayerListItem.PlayerListMode.AIPlayer);
                }
                else
                {
                    item.SetMode(RoomPlayerListItem.PlayerListMode.Player);
                }
            }


            if (selfPlayer == null)
            {
                Debug.LogWarning($"SelfPlayer是空的。PlayerID：{selfPlayerID}, players: {room.playerDataList.Select(e => e.id.ToString())}");
            }


            //设置房间选项
            try
            {
                var seed = room.getRandomSeed();
                _title.text = room.getRoomName();
                _passwordInputField.text = string.IsNullOrEmpty(room.getRoomPassword()) ? "" : "******";
                RandomSeedContainerGameOptionItemText.InputField.text = seed == 0 ? "" : seed.ToString();
                ShuffleContainerGameOptionItemToggle.Toggle.isOn = room.getShuffle();
                InitReplacementContainerGameOptionItemText.InputField.text = room.getTimeoutForInitReplace().ToString();
                TurnTimeContainerGameOptionItemText.InputField.text = room.getTimeoutForTurn().ToString();
                IsSortedToggleGameOptionItemToggle.Toggle.isOn = room.getSortedPlayers() != null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            updateButtonStates();

        }
        /// <summary>
        /// 设置房间模式，是本地、客户端还是远程
        /// </summary>
        /// <param name="mode"></param>
        public void SetMode(RoomMode mode)
        {
            IPText.gameObject.SetActive(mode == RoomMode.NetworkingHost);
            updateButtonStates();
        }
        /// <summary>
        /// 设置右上角的（IP地址）文本
        /// </summary>
        /// <param name="text"></param>
        public void SetIPText(string text)
        {
            IPText.text = text;
        }

        public void ResetEvents()
        {
            aiPlayerData = null;

            onPlayerRemove = null;
            onAIPlayerAdd = null;
            onUpdateButtons = null;
        }
        public void updatePlayerList((string playerName, string characterName, bool canViewDeck, bool canSetDeck, bool canRemove)[] viewData)
        {
            BJSYGameCore.UI.UIHelper.updateList(_playerRoot, _playerTemplate, _playerList, viewData.Length, (i, itemRect) =>
            {
                RoomPlayerListItem item = itemRect.GetComponent<RoomPlayerListItem>();
                item.NameText.text = viewData[i].playerName;
                item.CharacterText.text = viewData[i].characterName;
                if (viewData[i].canSetDeck)
                {
                    item.ViewButtonBlack.gameObject.SetActive(false);
                    item.SetButtonBlack.gameObject.SetActive(true);
                }
                else if (viewData[i].canViewDeck)
                {
                    item.ViewButtonBlack.gameObject.SetActive(true);
                    item.SetButtonBlack.gameObject.SetActive(false);
                }
                item.RemoveButtonBlack.asButton.interactable = viewData[i].canRemove;
            }, itemRect =>
            {
                RoomPlayerListItem item = itemRect.GetComponent<RoomPlayerListItem>();
                item.onClickViewDeckButton += onClickPlayerListItemViewDeckButtonCallback;
                item.onClickSetDeckButton += onClickPlayerListItemSetDeckButtonCallback;
                item.onClickRemoveButton += onClickPlayerListItemRemoveButtonCallback;
            }, itemRect =>
            {
                RoomPlayerListItem item = itemRect.GetComponent<RoomPlayerListItem>();
                item.onClickViewDeckButton -= onClickPlayerListItemViewDeckButtonCallback;
                item.onClickSetDeckButton -= onClickPlayerListItemSetDeckButtonCallback;
                item.onClickRemoveButton -= onClickPlayerListItemRemoveButtonCallback;
            });
        }
        public void displayAddAIButton(bool value)
        {
            _addAIPlayerButton.gameObject.SetActive(value);
        }
        public void displayIPText(bool value)
        {
            _ipText.gameObject.SetActive(value);
        }


        // 房间名编辑
        public void setRoomName(string value)
        {
            _title.text = value;
        }
        public void enableRoomNameEdit(bool value)
        {
            _titleEditButton.interactable = value;
            _confirmTitleEditButton.interactable = value;
            _titleEditInputField.interactable = value;
            if (!value)
            {
                _titleObj.SetActive(true);
                _editRoomNameObj.SetActive(false);
            }
        }


        public void setIPText(string text)
        {
            _ipText.text = text;
        }

        public void setRoomPassword(string value)
        {
            if (_passwordInputField.isFocused)
                return;
            _passwordInputField.SetTextWithoutNotify(string.IsNullOrEmpty(value) ? string.Empty : "******");
        }
        public void enableRoomPassword(bool value)
        {
            _passwordInputField.interactable = value;
        }
        public void setRandomSeed(int value)
        {
            if (_randomSeedInputField.isFocused)
                return;
            _randomSeedInputField.SetTextWithoutNotify(value.ToString());
        }
        public void enableRandomSeed(bool value)
        {
            _randomSeedInputField.interactable = value;
        }
        public void setShuffle(bool value)
        {
            _shuffleToggle.SetIsOnWithoutNotify(value);
        }
        public void enableShuffle(bool value)
        {
            _shuffleToggle.interactable = value;
        }
        public void setInitReplaceTime(float value)
        {
            if (_initReplaceTimeInputField.isFocused)
                return;
            _initReplaceTimeInputField.SetTextWithoutNotify(value.ToString());
        }
        public void enableInitReplaceTime(bool value)
        {
            _initReplaceTimeInputField.interactable = value;
        }
        public void setTurnTime(float value)
        {
            if (_turnTimeInputField.isFocused)
                return;
            _turnTimeInputField.SetTextWithoutNotify(value.ToString());
        }
        public void enableTurnTime(bool value)
        {
            _turnTimeInputField.interactable = value;
        }
        public void setDisableRandomPlayerActOrder(bool value)
        {
            _disableRandomPlayerActOrderToggle.SetIsOnWithoutNotify(value);
        }
        public void enableDisableRandomPlayerActOrder(bool value)
        {
            _disableRandomPlayerActOrderToggle.interactable = value;
        }
        public void updateLocalPlayerDeck(string deckName, Sprite deckImage)
        {
            _localPlayerDeckNameText.text = deckName;
            _localPlayerDeckImage.sprite = deckImage;
        }
        public void displayDeckSelectPanel((string name, Sprite image, bool isWarning, bool hasPrivatePools)[] viewData)
        {
            _deckViewPanel.update(viewData);
            _deckPanel.SetActive(true);
            //DeckPanelMasterCardSelection.updateItemList(viewData);
        }
        public void hideDeckSelectPanel()
        {
            _deckPanel.SetActive(false);
        }
        public void displayStartButton(bool value)
        {
            _startButton.gameObject.SetActive(value);
        }
        public void enableStartButton(bool value)
        {
            _startButton.interactable = value;
        }
        public void displayReadyButton(bool value)
        {
            _readyButton.gameObject.SetActive(value);
        }
        public void displayCancleReadyButton(bool value)
        {
            _cancleReadyButton.gameObject.SetActive(value);
        }

        public void updateViewCardPanel((string name, Sprite image)[] viewData)
        {
            _cardViewPanel.update(viewData);
            _cardPanel.SetActive(true);
        }
        /// <summary>
        /// 查看房间卡池
        /// </summary>
        /// <param name="cards"></param>
        public void viewCardPools(CardPoolInfoPack[] pools, bool showEditButton)
        {
            (string name, Sprite image) getPoolData(CardPoolInfoPack pool)
            {
                return (pool.cardPoolName, null);
            }
            List<(string name, Sprite image)> list = new List<(string name, Sprite image)>();

            foreach (var pool in pools)
            {
                list.Add(getPoolData(pool));
            }
            var viewData = list.ToArray();
            _cardPoolsViewPanel.updateCardPools(viewData, showEditButton);
            _cardPoolsPanel.SetActive(true);
            _confirmEditCardPoolButtonObj.SetActive(false);
            _cardPoolsPanelTitle.text = lang._(ROOM_TITLE_VIEW_CARD_POOLS);
        }
        public bool isViewingCardPools()
        {
            return _cardPoolsPanel.activeSelf;
        }
        public void closeViewCardPoolsPanel()
        {
            _cardPoolsPanel.SetActive(false);
        }
        /// <summary>
        /// 打开添加房间卡池界面
        /// </summary>
        /// <param name="cards"></param>
        public void openAddCardPoolsPanel(IEnumerable<CardPoolInfoPack> pools, IEnumerable<CardPoolReference> selectedCardPools)
        {
            (string name, Sprite image, bool selected) getPoolData(CardPoolInfoPack pool)
            {
                bool selected = selectedCardPools.Contains(pool.GetReference());
                Color color = selected ? Color.grey : Color.white;
                string name = pool.cardPoolName;
                if (pools.Count(p => p.cardPoolId == pool.cardPoolId) >= 2)
                {
                    name = lang._(ROOM_CARD_POOL_VERSION_TEMPLATE, name, pool.version);
                }
                return (name, null, selected);
            }
            List<(string name, Sprite image, bool selected)> list = new List<(string name, Sprite image, bool selected)>();

            foreach (var pool in pools)
            {
                list.Add(getPoolData(pool));
            }
            var viewData = list.ToArray();
            _cardPoolsViewPanel.updateAddCardPools(viewData);
            _cardPoolsPanel.SetActive(true);
            _confirmEditCardPoolButtonObj.SetActive(true);
            _cardPoolsPanelTitle.text = lang._(ROOM_TITLE_EDIT_CARD_POOLS);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        partial void onAwake()
        {
            StartButtonButtonBlack.asButton.setSelectable(false);
            DeckSelectionDeckListItem.SetMode(DeckListItem.Mode.DeckSelect);

            _closeDeckPanelButton.asButton.onClick.AddListener(onClickCloseDeckPanelButtonCallback);
            _closeCardPanelButton.asButton.onClick.AddListener(onClickCloseCardPanelButtonCallback);
            _closeCardPoolsPanelButton.onClick.AddListener(onClickCloseCardPoolsPanelButtonCallback);

            // 房间名编辑
            _titleEditButton.onClick.AddListener(onClickTitleEditButton);
            _confirmTitleEditButton.onClick.AddListener(onClickConfirmTitleEditButton);

            _deckPanel.SetActive(false);
            _cardPanel.SetActive(false);
            _cardPoolsPanel.SetActive(false);
            if (_playerTemplate.transform.parent == _playerRoot)
            {
                _playerTemplate.gameObject.SetActive(false);
            }
            _addAIPlayerButton.onClick.AddListener(onClickAddAIPlayerButtonCallback);
            _selectDeckButton.onClick.AddListener(onClickSelectDeckButtonCallback);
            _deckViewPanel.onClickDeckListItem += onClickDeckSelectItemCallback;
            _cardPoolsViewPanel.onClickDeckListItem += onClickCardPoolsSelectItemCallback;
            _cardPoolsViewPanel.onClickEditCardPools += onClickEditCardPoolsCallback;
            _cardPoolsViewPanel.onDeckListItemToggleValueChanged += onAddCardPoolsSelectItemToggleCallback;

            // 选项
            _passwordInputField.onEndEdit.AddListener(onPasswordInputFieldValueChangedCallback);
            _randomSeedInputField.onEndEdit.AddListener(onRandomSeedInputFieldValueChangedCallback);
            _shuffleToggle.onValueChanged.AddListener(onShuffleToggleValueChangedCallback);
            _initReplaceTimeInputField.onEndEdit.AddListener(onInitReplaceTimeInputFieldValueChangedCallback);
            _turnTimeInputField.onEndEdit.AddListener(onTurnTimeInputFieldValueChangedCallback);
            _disableRandomPlayerActOrderToggle.onValueChanged.AddListener(onDisableRandomPlayerActOrderValueChangedCallback);
            _viewCardPoolsButton.onClick.AddListener(onViewCardPoolsClickCallback);
            _confirmEditCardPoolsPanelButton.onClick.AddListener(onClickConfirmAddCardPoolsPanelButtonCallback);

            // 按钮
            _startButton.onClick.AddListener(onClickStartButtonCallback);
            _readyButton.onClick.AddListener(onClickReadyButtonCallback);
            _cancleReadyButton.onClick.AddListener(onClickCancleReadyButtonCallback);
            _quitButton.onClick.AddListener(onClickQuitButtonCallback);
        }
        private void OnEnable()
        {
            DeckSelectionDeckListItem.SetContent(0, lang._(Cards_Name), null);
        }
        #endregion
        #region 事件回调

        // 房间名编辑
        private void onClickTitleEditButton()
        {
            _titleEditInputField.text = _title.text;
            _titleObj.SetActive(false);
            _editRoomNameObj.SetActive(true);
        }
        private void onClickConfirmTitleEditButton()
        {
            _titleObj.SetActive(true);
            _editRoomNameObj.SetActive(false);
            var value = _titleEditInputField.text;
            OnRoomNameEdited?.Invoke(value);
        }
        private void onClickPlayerListItemViewDeckButtonCallback(RoomPlayerListItem item)
        {
            onClickPlayerItemViewDeckButton?.Invoke(_playerList.IndexOf(item.transform as RectTransform));
        }
        private void onClickPlayerListItemSetDeckButtonCallback(RoomPlayerListItem item)
        {
            onClickPlayerItemSetDeckButton?.Invoke(_playerList.IndexOf(item.transform as RectTransform));
        }
        private void onClickPlayerListItemRemoveButtonCallback(RoomPlayerListItem item)
        {
            onClickPlayerItemRemoveButton?.Invoke(_playerList.IndexOf(item.transform as RectTransform));
        }
        private void onClickAddAIPlayerButtonCallback()
        {
            onClickAddAIPlayerButton?.Invoke();
        }
        private void onClickSelectDeckButtonCallback()
        {
            onClickSelectDeckButton?.Invoke();
        }
        private void onClickDeckSelectItemCallback(int index)
        {
            onClickDeckSelectPanelItem?.Invoke(index);
        }
        private void onClickCardPoolsSelectItemCallback(int index)
        {
            onClickCardPoolsSelectPanelItem?.Invoke(index);
        }
        private void onAddCardPoolsSelectItemToggleCallback(int index, bool value)
        {
            onAddCardPoolsSelectItemToggle?.Invoke(index, value);
        }
        private void onPasswordInputFieldValueChangedCallback(string value)
        {
            onPasswordInputFieldValueChanged?.Invoke(value);
        }
        private void onRandomSeedInputFieldValueChangedCallback(string value)
        {
            onRandomSeedInputFieldValueChanged?.Invoke(value);
        }
        private void onShuffleToggleValueChangedCallback(bool value)
        {
            onShuffleToggleValueChanged?.Invoke(value);
        }
        private void onInitReplaceTimeInputFieldValueChangedCallback(string text)
        {
            onInitReplaceTimeInputFieldValueChanged?.Invoke(text);
        }
        private void onTurnTimeInputFieldValueChangedCallback(string text)
        {
            onTurnTimeInputFieldValueChanged?.Invoke(text);
        }
        private void onDisableRandomPlayerActOrderValueChangedCallback(bool value)
        {
            onDisableRandomPlayerActOrderToggleValueChanged?.Invoke(value);
        }
        private void onViewCardPoolsClickCallback()
        {
            onViewCardPoolsClick?.Invoke();
        }
        private void onClickEditCardPoolsCallback()
        {
            onAddCardPoolsClick?.Invoke();
        }
        private void onClickStartButtonCallback()
        {
            onClickStartButton?.Invoke();
        }
        private void onClickReadyButtonCallback()
        {
            onClickReadyButton?.Invoke();
        }
        private void onClickCancleReadyButtonCallback()
        {
            onClickCancleReadyButton?.Invoke();
        }

        private void onClickQuitButtonCallback()
        {
            onClickQuitButton?.Invoke();
        }

        private void onClickCloseDeckPanelButtonCallback()
        {
            _deckPanel.SetActive(false);
        }

        private void onClickCloseCardPanelButtonCallback()
        {
            _cardPanel.SetActive(false);
        }
        private void onClickCloseCardPoolsPanelButtonCallback()
        {
            onCloseCardPoolsPanel?.Invoke();
        }
        private void onClickConfirmAddCardPoolsPanelButtonCallback()
        {
            onConfirmAddCardPools?.Invoke();
        }


        /// <summary>
        /// 查看卡组
        /// </summary>
        /// <param name="cards"></param>
        private void onViewDeck(DefineReference[] cards)
        {
            onViewDeckClick?.Invoke(cards);
        }
        #endregion

        protected override void onDisplay()
        {
            base.onDisplay();

            // 重置房间名编辑
            _titleObj.SetActive(true);
            _editRoomNameObj.SetActive(false);
        }
        void updateButtonStates()
        {
            onUpdateButtons?.Invoke();
        }
        #endregion
        #region 事件
        /// <summary>
        /// 在房间名被编辑后触发的事件。
        /// </summary>
        public event Action<string> OnRoomNameEdited;
        /// <summary>
        /// 当点击玩家查看卡组按钮，参数为玩家索引
        /// </summary>
        public event Action<int> onClickPlayerItemViewDeckButton;
        /// <summary>
        /// 当点击玩家设置卡组按钮，参数为玩家索引
        /// </summary>
        public event Action<int> onClickPlayerItemSetDeckButton;
        /// <summary>
        /// 当点击玩家移除按钮，参数为玩家索引
        /// </summary>
        public event Action<int> onClickPlayerItemRemoveButton;
        /// <summary>
        /// 当点击添加AI玩家按钮
        /// </summary>
        public event Action onClickAddAIPlayerButton;
        public event Action onClickSelectDeckButton;
        public event Action<int> onClickDeckSelectPanelItem;
        public event Action<int> onClickCardPoolsSelectPanelItem;
        public event Action<int, bool> onAddCardPoolsSelectItemToggle;
        /// <summary>
        /// 当房主更改房间的密码
        /// </summary>
        public event Action<string> onPasswordInputFieldValueChanged;
        /// <summary>
        /// 当房主更改房间随机种子，参数为更改后的随机种子值
        /// </summary>
        public event Action<string> onRandomSeedInputFieldValueChanged;
        public event Action<bool> onShuffleToggleValueChanged;
        public event Action<string> onInitReplaceTimeInputFieldValueChanged;
        public event Action<string> onTurnTimeInputFieldValueChanged;
        public event Action<bool> onDisableRandomPlayerActOrderToggleValueChanged;
        public event Action onViewCardPoolsClick;
        public event Action<DefineReference[]> onViewDeckClick;
        public event Action onAddCardPoolsClick;
        public event Action onConfirmAddCardPools;
        public event Action onCloseCardPoolsPanel;
        public event Action onClickStartButton;
        public event Action onClickReadyButton;
        public event Action onClickCancleReadyButton;
        public event Action onClickQuitButton;
        /// <summary>
        /// 点击删除玩家按钮后触发
        /// </summary>
        public event Action<int> onPlayerRemove;
        /// <summary>
        /// 添加了AI玩家
        /// </summary>
        public event Action onAIPlayerAdd;
        /// <summary>
        /// 更新按钮状态
        /// </summary>
        public event Action onUpdateButtons;
        #endregion
        #region 属性字段
        RoomPlayerData aiPlayerData;

        [SerializeField]
        RectTransform _playerRoot;
        [SerializeField]
        RectTransform _playerTemplate;
        [SerializeField]
        List<RectTransform> _playerList = new List<RectTransform>();
        [SerializeField]
        Button _addAIPlayerButton;
        [SerializeField]
        Text _ipText;

        [Header("房间名编辑")]
        [SerializeField]
        GameObject _titleObj;
        [SerializeField]
        GameObject _editRoomNameObj;
        [SerializeField]
        Text _title;
        [SerializeField]
        Button _titleEditButton;
        [SerializeField]
        InputField _titleEditInputField;
        [SerializeField]
        Button _confirmTitleEditButton;


        private RoomPlayerListItem _selfPlayerItem;

        [Header("选项")]
        [SerializeField]
        InputField _passwordInputField;
        [SerializeField]
        InputField _randomSeedInputField;
        [SerializeField]
        Toggle _shuffleToggle;
        [SerializeField]
        InputField _initReplaceTimeInputField;
        [SerializeField]
        InputField _turnTimeInputField;
        [SerializeField]
        Toggle _disableRandomPlayerActOrderToggle;
        [SerializeField]
        Button _viewCardPoolsButton;

        [Header("卡组")]
        [SerializeField]
        Text _localPlayerDeckNameText;
        [SerializeField]
        Image _localPlayerDeckImage;
        [SerializeField]
        Button _selectDeckButton;

        [Header("按钮")]
        [SerializeField]
        Button _startButton;
        [SerializeField]
        Button _readyButton;
        [SerializeField]
        Button _cancleReadyButton;
        [SerializeField]
        Button _quitButton;


        [Header("ViewPanels")]
        [Header("Deck Selection")]
        [SerializeField]
        private GameObject _deckPanel;
        [SerializeField]
        private DeckViewPanel _deckViewPanel;
        [SerializeField]
        private ButtonBlack _closeDeckPanelButton;

        [Header("View Cards")]
        [SerializeField]
        private GameObject _cardPanel;
        [SerializeField]
        private DeckViewPanel _cardViewPanel;
        [SerializeField]
        private ButtonBlack _closeCardPanelButton;

        [Header("Card Pools")]
        [SerializeField]
        private Text _cardPoolsPanelTitle;
        [SerializeField]
        private GameObject _cardPoolsPanel;
        [SerializeField]
        private DeckViewPanel _cardPoolsViewPanel;
        [SerializeField]
        private Button _closeCardPoolsPanelButton;
        [SerializeField]
        private GameObject _confirmEditCardPoolButtonObj;
        [SerializeField]
        private Button _confirmEditCardPoolsPanelButton;

        DeckManager deckManager => this.findInstance<DeckManager>();
        CardManager cardManager => this.findInstance<CardManager>();

        LangManager lang => this.findInstance<LangManager>();
        #endregion
    }
    partial class RoomPlayerListItem
    {
        partial void onAwake()
        {
            asButton.onClick.AddListener(() =>
            {
                if (currentMode == PlayerListMode.AddAI)
                    OnAddAIPlayer?.Invoke();
            });
            ViewButtonBlack.asButton.onClick.AddListener(onClickViewDeckButtonCallback);
            SetButtonBlack.asButton.onClick.AddListener(onClickSetDeckButtonCallback);
            RemoveButtonBlack.asButton.onClick.AddListener(onClickRemoveButtonCallback);
        }

        RoomPlayerData playerData;
        /// <summary>
        /// 设置当前的玩家信息
        /// </summary>
        /// <param name="data"></param>
        public void SetPlayerInfo(RoomPlayerData data, string characterText)
        {
            playerData = data;
            NameText.text = data.name;


            _uploadIconImage.gameObject.SetActive(false);
            _downloadIconImage.gameObject.SetActive(false);
            _readyIconImage.gameObject.SetActive(false);
            _checkingIconImage.gameObject.SetActive(false);
            PlayerLoadState loadState = data.getLoadState();
            switch (loadState)
            {
                case PlayerLoadState.Upload:
                    _uploadIconImage.gameObject.SetActive(true);
                    break;
                case PlayerLoadState.Download:
                    _downloadIconImage.gameObject.SetActive(true);
                    break;
                case PlayerLoadState.Checking:
                    _checkingIconImage.gameObject.SetActive(true);
                    break;
                default:
                    _readyIconImage.gameObject.SetActive(data.getReady());
                    break;
            }
            _progressBar.gameObject.SetActive(loadState != PlayerLoadState.None);
            CharacterText.text = characterText;
        }

        /// <summary>
        /// 设置此玩家是否可以移除
        /// </summary>
        /// <param name="val"></param>
        public void SetRemoveable(bool val)
        {
            RemoveButtonBlack.asButton.interactable = val;
        }

        public enum PlayerListMode
        {
            Player,
            AIPlayer,
            AddAI
        }
        PlayerListMode currentMode;

        /// <summary>
        /// 设置显示模式
        /// </summary>
        /// <param name="mode"></param>
        public void SetMode(PlayerListMode mode)
        {
            currentMode = mode;

            PlayerImage.gameObject.SetActive(mode != PlayerListMode.AddAI);
            ViewButtonBlack.gameObject.SetActive(mode == PlayerListMode.Player);
            SetButtonBlack.gameObject.SetActive(mode == PlayerListMode.AIPlayer);
            AddImage.gameObject.SetActive(mode == PlayerListMode.AddAI);
        }
        public void SetProgress(float progress)
        {
            _progressBar.transform.localScale = new Vector3(Mathf.Clamp01(progress), 1, 1);
        }
        #region 私有方法
        #region 事件回调
        private void onClickViewDeckButtonCallback()
        {
            onClickViewDeckButton?.Invoke(this);
            if (playerData == null)
                return;
            var deck = playerData.getDeck();
            if (deck != null)
            {
                DefineReference[] idArray = playerData.getDeck().toDefRefArray(true);
                if (idArray != null)
                    OnPlayerViewDeck?.Invoke(idArray);
            }
        }
        private void onClickSetDeckButtonCallback()
        {
            OnSetAIDeck?.Invoke();
            onClickSetDeckButton?.Invoke(this);
        }
        private void onClickRemoveButtonCallback()
        {
            OnPlayerRemove?.Invoke(playerData.id);
            onClickRemoveButton?.Invoke(this);
        }
        #endregion
        #endregion
        #region 事件
        /// <summary>
        /// 点击移除玩家后的事件
        /// </summary>
        public event Action<int> OnPlayerRemove;
        /// <summary>
        /// 点击设置AI牌组后的事件
        /// </summary>
        public event Action OnSetAIDeck;
        /// <summary>
        /// 点击查看玩家牌组后的事件
        /// </summary>
        public event Action<DefineReference[]> OnPlayerViewDeck;
        /// <summary>
        /// 点击添加AI玩家的事件
        /// </summary>
        public event Action OnAddAIPlayer;
        public event Action<RoomPlayerListItem> onClickViewDeckButton;
        public event Action<RoomPlayerListItem> onClickSetDeckButton;
        public event Action<RoomPlayerListItem> onClickRemoveButton;
        #endregion
        #region 属性字段
        [SerializeField]
        private Image _readyIconImage;
        [SerializeField]
        private Image _uploadIconImage;
        [SerializeField]
        private Image _downloadIconImage;
        [SerializeField]
        private Image _checkingIconImage;
        [SerializeField]
        private Image _progressBar;
        #endregion
    }

    public enum RoomMode
    {
        /// <summary>
        /// 本地游戏
        /// </summary>
        Local,
        /// <summary>
        /// 局域网中的主机
        /// </summary>
        NetworkingHost,
        /// <summary>
        /// 局域网客户端或大厅客户端
        /// </summary>
        NetworkingClient
    }
}
