
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using BJSYGameCore.UI;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
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
        [TranslateMsg]
        const string STR_START_GAME = "开始游戏";
        [TranslateMsg]
        const string STR_PREPARE = "准备";
        [TranslateMsg]
        private const String STR_REQUEST_PREPRAE = "取消准备";
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
                item.SetPlayerInfo(player);
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
                RandomSeedContainerGameOptionItemText.InputField.text = seed == 0 ? "" : seed.ToString();
                ShuffleContainerGameOptionItemToggle.Toggle.isOn = room.getShuffle();
                InitReplacementContainerGameOptionItemText.InputField.text = room.getTimeoutForInitReplace().ToString();
                TurnTimeContainerGameOptionItemText.InputField.text = room.getTimeoutForTurn().ToString();
                IsSortedToggleGameOptionItemToggle.Toggle.isOn = room.getSortedPlayers() != null;
                _allowCustomCardsToggle.isOn = room.getAllowCustomCards();
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
        public void setIPText(string text)
        {
            _ipText.text = text;
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
        public void setAllowCustomCards(bool value)
        {
            _allowCustomCardsToggle.SetIsOnWithoutNotify(value);
        }
        public void enableAllowCustomCards(bool value)
        {
            _allowCustomCardsToggle.interactable = value;
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
        #endregion
        #region 私有方法
        #region 生命周期
        partial void onAwake()
        {
            _closeDeckPanelButton.asButton.onClick.AddListener(onClickCloseDeckPanelButtonCallback);
            _closeCardPanelButton.asButton.onClick.AddListener(onClickCloseCardPanelButtonCallback);
            StartButtonButtonBlack.asButton.setSelectable(false);
            DeckSelectionDeckListItem.SetMode(DeckListItem.Mode.DeckSelect);

            _deckPanel.SetActive(false);
            _cardPanel.SetActive(false);
            if (_playerTemplate.transform.parent == _playerRoot)
            {
                _playerTemplate.gameObject.SetActive(false);
            }
            _addAIPlayerButton.onClick.AddListener(onClickAddAIPlayerButtonCallback);
            _selectDeckButton.onClick.AddListener(onClickSelectDeckButtonCallback);
            //DeckPanelMasterCardSelection.onClickItem += onClickDeckSelectItemCallback;
            _deckViewPanel.onClickDeckListItem += onClickDeckSelectItemCallback;
            //_randomSeedInputField.onValueChanged.AddListener(onRandomSeedInputFieldValueChangedCallback);

            // 选项
            _randomSeedInputField.onEndEdit.AddListener(onRandomSeedInputFieldValueChangedCallback);
            _shuffleToggle.onValueChanged.AddListener(onShuffleToggleValueChangedCallback);
            _initReplaceTimeInputField.onEndEdit.AddListener(onInitReplaceTimeInputFieldValueChangedCallback);
            _turnTimeInputField.onEndEdit.AddListener(onTurnTimeInputFieldValueChangedCallback);
            _disableRandomPlayerActOrderToggle.onValueChanged.AddListener(onDisableRandomPlayerActOrderValueChangedCallback);
            _allowCustomCardsToggle.onValueChanged.AddListener(onCustomCardsValueChangedCallback);

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

        private void onCustomCardsValueChangedCallback(bool value)
        {
            onCustomCardsToggleValueChanged?.Invoke(value);
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


        /// <summary>
        /// 查看卡组
        /// </summary>
        /// <param name="cards"></param>
        private async void onViewDeck(DefineReference[] cards)
        {
            //_cardViewPanel.setItems(cardManager,cards);
            async Task<(string name, Sprite image)> getCardData(DefineReference cardRef)
            {
                var card = cardManager.getCardDefine(cardRef.cardPoolId, cardRef.defineId);
                var skin = cardManager.getCardSkin(cardRef.cardPoolId, cardRef.defineId);
                Sprite image = null;
                if (card != null)
                {
                    var path = skin?.imagePath;
                    image = await cardManager.loadCardImage(cardRef.cardPoolId, cardRef.defineId);
                }
                return (skin?.name, image);
            }
            List<(string name, Sprite image)> list = new List<(string name, Sprite image)>();
            foreach (var cardRef in cards)
            {
                list.Add(await getCardData(cardRef));
            }
            var viewData = list.ToArray();
            _cardViewPanel.update(viewData);
            _cardPanel.SetActive(true);
        }

        #endregion
        void updateButtonStates()
        {
            onUpdateButtons?.Invoke();
        }
        void showDeckSelectionPanel(Action<int> onDeckSelect)
        {
            var decks = deckManager.GetDecks();
            var viewData = decks.Select((define =>
            {
                // 只有30张卡的卡组才能选择
                return (define.Name, cardManager.getCardSkin(define.MasterCardRef).image, (deckManager.GetDeckCards(define.ID).Count >= 30), false);
            })).ToArray();
            _deckViewPanel.update(viewData);
            _deckPanel.SetActive(true);
            _deckViewPanel.onClickDeckListItem -= onSelectCallback;
            _deckViewPanel.onClickDeckListItem += onSelectCallback;
            void onSelectCallback(int id)
            {
                _deckPanel.SetActive(false);
                onDeckSelect(id);
                _deckViewPanel.onClickDeckListItem -= onSelectCallback;
            }
        }
        #endregion
        #region 事件
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
        /// <summary>
        /// 当房主更改房间随机种子，参数为更改后的随机种子值
        /// </summary>
        public event Action<string> onRandomSeedInputFieldValueChanged;
        public event Action<bool> onShuffleToggleValueChanged;
        public event Action<string> onInitReplaceTimeInputFieldValueChanged;
        public event Action<string> onTurnTimeInputFieldValueChanged;
        public event Action<bool> onDisableRandomPlayerActOrderToggleValueChanged;
        public event Action<bool> onCustomCardsToggleValueChanged;
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

        private RoomPlayerListItem _selfPlayerItem;

        [Header("选项")]
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
        Toggle _allowCustomCardsToggle;

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
        [SerializeField]
        private GameObject _deckPanel;
        [SerializeField]
        private DeckViewPanel _deckViewPanel;
        [SerializeField]
        private ButtonBlack _closeDeckPanelButton;
        [SerializeField]
        private GameObject _cardPanel;
        [SerializeField]
        private DeckViewPanel _cardViewPanel;
        [SerializeField]
        private ButtonBlack _closeCardPanelButton;


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
        public void SetPlayerInfo(RoomPlayerData data)
        {
            playerData = data;
            NameText.text = data.name;

            DefineReference[] deck = data.getDeck()?.toDefRefArray(true);

            _uploadIconImage.gameObject.SetActive(false);
            _downloadIconImage.gameObject.SetActive(false);
            _checkIconImage.gameObject.SetActive(false);
            PlayerLoadState loadState = data.getLoadState();
            switch (loadState)
            {
                case PlayerLoadState.Upload:
                    _uploadIconImage.gameObject.SetActive(true);
                    break;
                case PlayerLoadState.Download:
                    _downloadIconImage.gameObject.SetActive(true);
                    break;
                default:
                    _checkIconImage.gameObject.SetActive(data.getReady());
                    break;
            }
            _progressBar.gameObject.SetActive(loadState != PlayerLoadState.None);

            if (deck != null)
            {
                var cardManager = this.findInstance<CardManager>();
                DefineReference masterRef = deck[0];
                var skin = cardManager.getCardSkin(masterRef);
                if (skin != null)
                {
                    CharacterText.text = skin.name;
                }
                else
                {
                    CharacterText.text = ROOM_UNKNOWN_CHARACTER_NAME;
                }
            }
            else
            {
                CharacterText.text = string.Empty;
            }
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
        [TranslateMsg("房间显示之未知角色")]
        private const string ROOM_UNKNOWN_CHARACTER_NAME = "未知角色";
        [SerializeField]
        private Image _checkIconImage;
        [SerializeField]
        private Image _uploadIconImage;
        [SerializeField]
        private Image _downloadIconImage;
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
