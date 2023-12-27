using System;
using BJSYGameCore.UI;
using MukioI18n;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class RoomPanel : UIObject
    {
        #region 公有方法

        #region IP地址
        public void setIPTextVisible(bool value)
        {
            _ipText.gameObject.SetActive(value);
        }
        /// <summary>
        /// 设置右上角的（IP地址）文本
        /// </summary>
        /// <param name="text"></param>
        public void setIPText(string text)
        {
            _ipText.text = text;
        }
        #endregion

        #region 玩家列表
        public void setCardPoolLoadProgress(int playerIndex, float progress)
        {
            var item = _playerElementList.getElement<RoomPlayerListItem>(playerIndex);
            item?.SetProgress(progress);
        }
        public void updatePlayerList(RoomPlayerItemViewData[] viewDatas)
        {
            _playerElementList.updateList(viewDatas.Length, (i, itemRect) =>
            {
                RoomPlayerListItem item = itemRect.GetComponent<RoomPlayerListItem>();
                var viewData = viewDatas[i];
                item.update(viewData);
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
        #endregion

        #region 选择卡组界面
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
        #endregion

        #region 底部按钮
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
        public void displayCancelReadyButton(bool value)
        {
            _cancelReadyButton.gameObject.SetActive(value);
        }
        #endregion

        #region 房间卡池
        /// <summary>
        /// 查看房间卡池
        /// </summary>
        /// <param name="cards"></param>
        public void viewCardPools((string name, Sprite image)[] viewDatas, bool showEditButton, string title)
        {
            _cardPoolsViewPanel.updateCardPools(viewDatas, showEditButton);
            _cardPoolsPanel.SetActive(true);
            _confirmEditCardPoolButtonObj.SetActive(false);
            _cardPoolsPanelTitle.text = title;
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
        public void openAddCardPoolsPanel((string name, Sprite image, bool selected)[] viewDatas, string title)
        {
            _cardPoolsViewPanel.updateAddCardPools(viewDatas);
            _cardPoolsPanel.SetActive(true);
            _confirmEditCardPoolButtonObj.SetActive(true);
            _cardPoolsPanelTitle.text = title;
        }
        #endregion

        #region 查看卡组
        public void setViewingCard(bool viewingCard)
        {
            _rightRegionObj.SetActive(!viewingCard);
            _cardViewRegionObj.SetActive(viewingCard);
        }
        public void updateViewingDeckCards(DeckCardViewData[] viewDatas)
        {
            _deckViewCardList.update(viewDatas);
        }
        #endregion

        public void updateRoomOptions(RoomOptionsViewData viewData)
        {
            //设置房间选项
            try
            {
                _titleEditButton.interactable = viewData.roomNameEditable;
                _confirmTitleEditButton.interactable = viewData.roomNameEditable;
                _titleEditInputField.interactable = viewData.roomNameEditable;
                if (!viewData.roomNameEditable)
                {
                    _titleObj.SetActive(true);
                    _editRoomNameObj.SetActive(false);
                }

                _passwordInputField.interactable = viewData.passwordEditable;
                _randomSeedInputField.interactable = viewData.seedEditable;
                _shuffleToggle.interactable = viewData.shuffleEditable;
                _initReplaceTimeInputField.interactable = viewData.initReplaceTimeEditable;
                _turnTimeInputField.interactable = viewData.turnTimeEditable;
                _disableRandomPlayerActOrderToggle.interactable = viewData.disableRandomPlayerOrderEditable;

                _title.text = viewData.roomName;
                _passwordInputField.text = viewData.password;
                _randomSeedInputField.text = viewData.seed;
                _shuffleToggle.isOn = viewData.shuffle;
                _initReplaceTimeInputField.text = viewData.initReplaceTime;
                _turnTimeInputField.text = viewData.turnTime;
                _disableRandomPlayerActOrderToggle.isOn = viewData.disableRandomPlayerOrder;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }
        public void setEditingRoomName(bool editing)
        {
            _titleObj.SetActive(!editing);
            _editRoomNameObj.SetActive(editing);
        }
        public void setRoomNameInputFieldText(string text)
        {
            _titleEditInputField.text = text;
        }
        public void setLocalPlayerDeck(string deckName, Sprite deckImage)
        {
            _localPlayerDeckNameText.text = deckName;
            _localPlayerDeckImage.sprite = deckImage;
        }

        #endregion

        #region 私有方法

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _startButton.setSelectable(false);
            _deckSelectionItem.SetMode(DeckListItem.Mode.DeckSelect);

            _closeDeckPanelButton.asButton.onClick.AddListener(onClickCloseDeckPanelButtonCallback);
            _closeViewDeckCardsButton.onClick.AddListener(onClickCloseCardPanelButtonCallback);
            _closeCardPoolsPanelButton.onClick.AddListener(onClickCloseCardPoolsPanelButtonCallback);

            // 房间名编辑
            _titleEditButton.onClick.AddListener(onClickTitleEditButtonCallback);
            _confirmTitleEditButton.onClick.AddListener(onClickConfirmTitleEditButtonCallback);

            _deckPanel.SetActive(false);
            _rightRegionObj.SetActive(true);
            _cardViewRegionObj.SetActive(false);
            _cardPoolsPanel.SetActive(false);
            _addAIPlayerButton.onClick.AddListener(onClickAddAIPlayerButtonCallback);
            _selectDeckButton.onClick.AddListener(onClickSelectDeckButtonCallback);
            _deckViewPanel.onClickDeckListItem += onClickDeckSelectItemCallback;
            _cardPoolsViewPanel.onClickDeckListItem += onClickCardPoolsSelectItemCallback;
            _cardPoolsViewPanel.onClickEditCardPools += onClickEditCardPoolsCallback;
            _cardPoolsViewPanel.onDeckListItemToggleValueChanged += onAddCardPoolsSelectItemToggleCallback;

            _deckViewCardList.onDeckItemPointerEnter += onDeckItemPointerEnterCallback;
            _deckViewCardList.onDeckItemPointerExit += onDeckItemPointerExitCallback;

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
            _cancelReadyButton.onClick.AddListener(onClickCancelReadyButtonCallback);
            _quitButton.onClick.AddListener(onClickQuitButtonCallback);
        }
        #endregion

        #region 事件回调

        #region 房间设置
        private void onClickTitleEditButtonCallback()
        {
            onClickTitleEditButton?.Invoke();
        }
        private void onClickConfirmTitleEditButtonCallback()
        {
            onClickTitleConfirmEditButton?.Invoke(_titleEditInputField.text);
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
        #endregion

        #region 玩家列表
        private void onClickPlayerListItemViewDeckButtonCallback(RoomPlayerListItem item)
        {
            onClickPlayerItemViewDeckButton?.Invoke(_playerElementList.indexOf(item));
        }
        private void onClickPlayerListItemSetDeckButtonCallback(RoomPlayerListItem item)
        {
            onClickPlayerItemSetDeckButton?.Invoke(_playerElementList.indexOf(item));
        }
        private void onClickPlayerListItemRemoveButtonCallback(RoomPlayerListItem item)
        {
            onClickPlayerItemRemoveButton?.Invoke(_playerElementList.indexOf(item));
        }
        private void onClickAddAIPlayerButtonCallback()
        {
            onClickAddAIPlayerButton?.Invoke();
        }
        #endregion

        #region 选择卡组
        private void onClickSelectDeckButtonCallback()
        {
            onClickSelectDeckButton?.Invoke();
        }
        private void onClickDeckSelectItemCallback(int index)
        {
            onClickDeckSelectPanelItem?.Invoke(index);
        }
        private void onClickCloseDeckPanelButtonCallback()
        {
            _deckPanel.SetActive(false);
        }
        #endregion

        #region 卡池查看
        private void onClickCardPoolsSelectItemCallback(int index)
        {
            onClickCardPoolsSelectPanelItem?.Invoke(index);
        }
        private void onAddCardPoolsSelectItemToggleCallback(int index, bool value)
        {
            onAddCardPoolsSelectItemToggle?.Invoke(index, value);
        }
        private void onViewCardPoolsClickCallback()
        {
            onViewCardPoolsClick?.Invoke();
        }
        private void onClickEditCardPoolsCallback()
        {
            onAddCardPoolsClick?.Invoke();
        }
        private void onClickCloseCardPoolsPanelButtonCallback()
        {
            onCloseCardPoolsPanel?.Invoke();
        }
        private void onClickConfirmAddCardPoolsPanelButtonCallback()
        {
            onConfirmAddCardPools?.Invoke();
        }
        #endregion

        #region 底部按钮
        private void onClickStartButtonCallback()
        {
            onClickStartButton?.Invoke();
        }
        private void onClickReadyButtonCallback()
        {
            onClickReadyButton?.Invoke();
        }
        private void onClickCancelReadyButtonCallback()
        {
            onClickCancelReadyButton?.Invoke();
        }
        private void onClickQuitButtonCallback()
        {
            onClickQuitButton?.Invoke();
        }
        #endregion

        #region 查看卡组
        private void onClickCloseCardPanelButtonCallback()
        {
            onCloseViewDeckButtonClick?.Invoke();
        }
        private void onDeckItemPointerEnterCallback(int index, PointerEventData pointerData)
        {
            onViewDeckItemPointerEnter?.Invoke(index);
        }
        private void onDeckItemPointerExitCallback(int index, PointerEventData pointerData)
        {
            onViewDeckItemPointerExit?.Invoke(index);
        }
        #endregion

        #endregion

        protected override void onDisplay()
        {
            base.onDisplay();

            // 重置房间名编辑
            _titleObj.SetActive(true);
            _editRoomNameObj.SetActive(false);
        }
        #endregion

        #region 事件

        #region 玩家列表
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
        #endregion

        #region 选择卡组
        public event Action onClickSelectDeckButton;
        public event Action<int> onClickDeckSelectPanelItem;
        #endregion

        #region 房间设置
        /// <summary>
        /// 在房间名被编辑后触发的事件。
        /// </summary>
        public event Action<string> OnRoomNameEdited;
        /// <summary>
        /// 当房主更改房间的密码
        /// </summary>
        public event Action<string> onPasswordInputFieldValueChanged;
        public event Action onClickTitleEditButton;
        public event Action<string> onClickTitleConfirmEditButton;
        /// <summary>
        /// 当房主更改房间随机种子，参数为更改后的随机种子值
        /// </summary>
        public event Action<string> onRandomSeedInputFieldValueChanged;
        public event Action<bool> onShuffleToggleValueChanged;
        public event Action<string> onInitReplaceTimeInputFieldValueChanged;
        public event Action<string> onTurnTimeInputFieldValueChanged;
        public event Action<bool> onDisableRandomPlayerActOrderToggleValueChanged;
        #endregion

        #region 查看卡组
        public event Action onCloseViewDeckButtonClick;
        public event Action<int> onViewDeckItemPointerEnter;
        public event Action<int> onViewDeckItemPointerExit;
        #endregion

        #region 查看/添加卡池
        public event Action onViewCardPoolsClick;
        public event Action onAddCardPoolsClick;
        public event Action onConfirmAddCardPools;
        public event Action onCloseCardPoolsPanel;
        public event Action<int> onClickCardPoolsSelectPanelItem;
        public event Action<int, bool> onAddCardPoolsSelectItemToggle;
        #endregion

        #region 底部按钮
        public event Action onClickStartButton;
        public event Action onClickReadyButton;
        public event Action onClickCancelReadyButton;
        public event Action onClickQuitButton;
        #endregion

        #endregion

        #region 属性字段
        public DeckCardList deckCardList => _deckViewCardList;

        [SerializeField]
        private GameObject _rightRegionObj;
        [SerializeField]
        private GameObject _cardViewRegionObj;
        [SerializeField]
        Text _ipText;
        [SerializeField]
        DeckListItem _deckSelectionItem;

        [Header("玩家列表")]
        [SerializeField]
        ElementList _playerElementList;
        [SerializeField]
        Button _addAIPlayerButton;

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

        [Header("查看卡组")]
        [SerializeField]
        private DeckCardList _deckViewCardList;
        [SerializeField]
        private Button _closeViewDeckCardsButton;


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
        Button _cancelReadyButton;
        [SerializeField]
        Button _quitButton;

        [Header("房间设置")]
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


        [Header("ViewPanels")]
        [Header("Deck Selection")]
        [SerializeField]
        private GameObject _deckPanel;
        [SerializeField]
        private DeckViewPanel _deckViewPanel;
        [SerializeField]
        private ButtonBlack _closeDeckPanelButton;

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
    public struct RoomOptionsViewData
    {
        public string roomName;
        public string password;
        public string seed;
        public bool shuffle;
        public string initReplaceTime;
        public string turnTime;
        public bool disableRandomPlayerOrder;

        public bool roomNameEditable;
        public bool passwordEditable;
        public bool seedEditable;
        public bool shuffleEditable;
        public bool initReplaceTimeEditable;
        public bool turnTimeEditable;
        public bool disableRandomPlayerOrderEditable;
    }
}
