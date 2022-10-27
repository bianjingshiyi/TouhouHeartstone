
using BJSYGameCore;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore.UI;
using NitoriNetwork.Common;
using MukioI18n;
using UnityEngine.UI;
namespace UI
{
    partial class RoomPanel
    {
        #region 公有方法
        public void SetInfo(RoomData room, bool clearAIPlayerData = false)
        {
            if (clearAIPlayerData)
            {
                aiPlayerData = null;
            }
            roomData = room;

            RoomPlayerData selfPlayer = null;

            RoomPlayerList.clearItems();
            foreach (var player in room.playerDataList)
            {
                // 顺便获取自己的引用
                if (player.id == selfPlayerID)
                    selfPlayer = player;

                var item = RoomPlayerList.addItem();
                item.SetPlayerInfo(player);
                item.OnPlayerViewDeck += viewDeck;

                // 仅当自己是主机且此玩家不是自己时，设置为可以移除
                item.SetRemoveable(isHost && selfPlayerID != player.id);
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
                    item.OnSetAIDeck += () =>
                    {
                        showDeckSelectionPanel((deckID) =>
                        {
                            var deck = deckManager.GetDeck(deckID);
                            var skin = cardManager.getCardSkin(deck.MasterID);

                            var list = deckManager.GetDeckCards(deckID).Select(k => (int)k.Value).ToList();
                            list.Insert(0, deck.MasterID); // polyfill: 转换为传统的，第一张卡是master的格式

                            onSetPlayerDeck?.Invoke(player.id, list.ToArray());
                        });
                    };
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

            if (currentMode == Mode.Local && aiPlayerData == null)
            {
                var item = RoomPlayerList.addItem();
                item.SetMode(RoomPlayerListItem.PlayerListMode.AddAI);

                item.OnAddAIPlayer += () =>
                {
                    onAIPlayerAdd?.Invoke();
                };
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
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (isHost)
            {
                // 判断是否可以开始游戏
                bool startable = room.playerDataList.Count == room.maxPlayerCount;

                foreach (var item in room.playerDataList)
                {
                    if (item.id == selfPlayerID)
                    {
                        // 主机检查卡组是否满
                        var decks = item.getDeck();
                        if (decks == null || decks.getCardCount() < 30)
                            startable = false;
                    }
                    else
                    {
                        // 其他检查是否准备好
                        startable &= item.getReady();
                    }
                }

                StartButtonButtonBlack.asButton.setSelectable(startable);
            }
            else
            {
                // 判断准备状态
                var deck = selfPlayer.getDeck();
                StartButtonButtonBlack.Text.text = selfPlayer.getReady() ? lang._(STR_REQUEST_PREPRAE) : lang._(STR_PREPARE);
                StartButtonButtonBlack.asButton.setSelectable(deck != null && deck.masterId != 0 && deck.getCardCount() == 30);
            }
        }
        #endregion
        RoomData roomData;

        DeckManager deckManager => this.findInstance<DeckManager>();
        CardManager cardManager => this.findInstance<CardManager>();

        LangManager lang => this.findInstance<LangManager>();

        [TranslateMsg]
        private const string Cards_Name = "选择你的卡组";

        private void OnEnable()
        {
            DeckSelectionDeckListItem.SetContent(0, lang._(Cards_Name), null);
        }

        void initEvents()
        {
            RandomSeedContainerGameOptionItemText.InputField.onValueChanged.set(value =>
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
                    RandomSeedContainerGameOptionItemText.InputField.text = roomData.getRandomSeed().ToString();
                }
            });

            ShuffleContainerGameOptionItemToggle.Toggle.onValueChanged.set(value =>
            {
                setNotifyRoomSetting(RoomDataPropName.Option.Shuffle, value);
            });

            InitReplacementContainerGameOptionItemText.InputField.onEndEdit.set(value =>
            {
                if (int.TryParse(value, out int i) && 5 < i)
                {
                    setNotifyRoomSetting(RoomDataPropName.Option.TimeoutForInitReplace, i);
                }
                else
                {
                    InitReplacementContainerGameOptionItemText.InputField.text = roomData.getTimeoutForInitReplace().ToString();
                }
            });

            TurnTimeContainerGameOptionItemText.InputField.onEndEdit.set(value =>
            {
                if (int.TryParse(value, out int i) && 5 < i)
                {
                    setNotifyRoomSetting(RoomDataPropName.Option.TimeoutForTurn, i);
                }
                else
                {
                    TurnTimeContainerGameOptionItemText.InputField.text = roomData.getTimeoutForTurn().ToString();
                }
            });

            IsSortedToggleGameOptionItemToggle.Toggle.onValueChanged.set((value) =>
            {
                Debug.Log("更新房间设定" + value);
                if (value)
                {
                    setNotifyRoomSetting(RoomDataPropName.Option.SortedPlayers, roomData.playerDataList.Select(p => p.id).ToArray());
                }
                else
                {
                    setNotifyRoomSetting(RoomDataPropName.Option.SortedPlayers, null);
                }
            });

            StartButtonButtonBlack.asButton.onClick.set(() =>
            {
                if (isHost)
                {
                    onStartClick?.Invoke();
                }
                else
                {
                    OnPlayerReadyChange?.Invoke(selfPlayerID);
                }
            });

            QuitButtonButtonBlack.asButton.onClick.set(() =>
            {
                onQuitClick?.Invoke();
            });
        }

        bool isHost => currentMode == Mode.Local || currentMode == Mode.NetworkingHost;

        public enum Mode
        {
            Local,
            NetworkingHost,
            NetworkingClient
        }

        Mode currentMode;

        /// <summary>
        /// 设置房间模式，是本地、客户端还是远程
        /// </summary>
        /// <param name="mode"></param>
        public void SetMode(Mode mode)
        {
            currentMode = mode;

            RandomSeedContainerGameOptionItemText.InputField.setSelectable(isHost);
            ShuffleContainerGameOptionItemToggle.Toggle.setSelectable(isHost);
            InitReplacementContainerGameOptionItemText.InputField.setSelectable(isHost);
            TurnTimeContainerGameOptionItemText.InputField.setSelectable(isHost);
            IsSortedToggleGameOptionItemToggle.Toggle.setSelectable(isHost);

            // 准备按钮
            StartButtonButtonBlack.Text.text = isHost ? lang._(STR_START_GAME) : lang._(STR_PREPARE);
            IPText.gameObject.SetActive(currentMode == Mode.NetworkingHost);
        }
        [TranslateMsg]
        const string STR_START_GAME = "开始游戏";
        [TranslateMsg]
        const string STR_PREPARE = "准备";

        int selfPlayerID = -1;

        /// <summary>
        /// 设置自己的玩家ID
        /// </summary>
        /// <param name="id"></param>
        public void SetSelfID(int id)
        {
            selfPlayerID = id;
        }

        /// <summary>
        /// 查看卡组
        /// </summary>
        /// <param name="cards"></param>
        void viewDeck(int[] cards)
        {
            //_cardViewPanel.setItems(cardManager,cards);
            var viewData = cards.Select(i =>
            {
                var skin = cardManager.getCardSkin(i);
                return (skin?.name,skin?.image);
            }).ToArray();
            _cardViewPanel.update(viewData);
            _cardPanel.SetActive(true);
        }
        #region 公有方法
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
            _initReplaceTimeInputField.SetTextWithoutNotify(value.ToString());
        }
        public void enableInitReplaceTime(bool value)
        {
            _initReplaceTimeInputField.interactable = value;
        }
        public void setTurnTime(float value)
        {
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
        public void displayDeckSelectPanel((string name, Sprite image, bool isWarning)[] viewData)
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
            DeckSelectionDeckListItem.onClick += (_) =>
            {
                if (this.findInstance<Controllers.MainSceneCtrl>().room.isDisplay)
                    return;
                showDeckSelectionPanel((deckID) =>
                {
                    var deck = deckManager.GetDeck(deckID);
                    var skin = cardManager.getCardSkin(deck.MasterID);
                    DeckSelectionDeckListItem.SetContent(deckID, deck.Name, skin.image);

                    var list = deckManager.GetDeckCards(deckID).Select(k => (int)k.Value).ToList();
                    list.Insert(0, deck.MasterID); // polyfill: 转换为传统的，第一张卡是master的格式
                    onSetPlayerDeck?.Invoke(selfPlayerID, list.ToArray());
                });
            };
            initEvents();

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
            _randomSeedInputField.onEndEdit.AddListener(onRandomSeedInputFieldValueChangedCallback);
            _shuffleToggle.onValueChanged.AddListener(onShuffleToggleValueChangedCallback);
            _initReplaceTimeInputField.onEndEdit.AddListener(onInitReplaceTimeInputFieldValueChangedCallback);
            _turnTimeInputField.onEndEdit.AddListener(onTurnTimeInputFieldValueChangedCallback);
            _disableRandomPlayerActOrderToggle.onValueChanged.AddListener(onDisableRandomPlayerActOrderValueChangedCallback);
            _startButton.onClick.AddListener(onClickStartButtonCallback);
            _readyButton.onClick.AddListener(onClickReadyButtonCallback);
            _cancleReadyButton.onClick.AddListener(onClickCancleReadyButtonCallback);
            _quitButton.onClick.AddListener(onClickQuitButtonCallback);
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
            if (int.TryParse(value, out int seed))
            {
                onRandomSeedInputFieldValueChanged?.Invoke(seed);
            }
        }
        private void onShuffleToggleValueChangedCallback(bool value)
        {
            onShuffleToggleValueChanged?.Invoke(value);
        }
        private void onInitReplaceTimeInputFieldValueChangedCallback(string text)
        {
            if (float.TryParse(text, out float value))
            {
                onInitReplaceTimeInputFieldValueChanged?.Invoke(value);
            }
        }
        private void onTurnTimeInputFieldValueChangedCallback(string text)
        {
            if (float.TryParse(text, out float value))
            {
                onTurnTimeInputFieldValueChanged?.Invoke(value);
            }
        }
        private void onDisableRandomPlayerActOrderValueChangedCallback(bool value)
        {
            onDisableRandomPlayerActOrderToggleValueChanged?.Invoke(value);
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
        #endregion
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
        public event Action<int> onRandomSeedInputFieldValueChanged;
        public event Action<bool> onShuffleToggleValueChanged;
        public event Action<float> onInitReplaceTimeInputFieldValueChanged;
        public event Action<float> onTurnTimeInputFieldValueChanged;
        public event Action<bool> onDisableRandomPlayerActOrderToggleValueChanged;
        public event Action onClickStartButton;
        public event Action onClickReadyButton;
        public event Action onClickCancleReadyButton;
        public event Action onClickQuitButton;
        #endregion
        #region 属性字段
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
        Text _localPlayerDeckNameText;
        [SerializeField]
        Image _localPlayerDeckImage;
        [SerializeField]
        Button _selectDeckButton;
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
        
        #endregion
        void showDeckSelectionPanel(Action<int> onDeckSelect)
        {
            var decks = deckManager.GetDecks();
            var viewData = decks.Select((define =>
            {
                // 只有30张卡的卡组才能选择
                return (define.Name, cardManager.getCardSkin(define.MasterID).image,(deckManager.GetDeckCards(define.ID).Count >= 30));
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

        RoomPlayerData aiPlayerData;

        [Obsolete("请使用 setNotifyRoomSetting")]
        void notifyRoomSettingChange(GameOption option)
        {
            onRoomPropUpdate?.Invoke(RoomDataPropName.OPTION_GAMEOPTION, option);
        }

        /// <summary>
        /// 设置本地的房间值，并发送房间值改变的通知
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        void setNotifyRoomSetting(RoomDataPropName.Option prop, object value)
        {
            roomData.setOption(prop, value);
            onRoomPropUpdate?.Invoke(prop.ToString(), value);
        }

        [TranslateMsg]
        private const String STR_REQUEST_PREPRAE = "取消准备";

        /// <summary>
        /// 设置右上角的（IP地址）文本
        /// </summary>
        /// <param name="text"></param>
        public void SetRightText(string text)
        {
            IPText.text = text;
        }

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
        public event Action<string, object> onRoomPropUpdate;
        /// <summary>
        /// 点击删除玩家按钮后触发
        /// </summary>
        public event Action<int> onPlayerRemove;
        /// <summary>
        /// 添加了AI玩家
        /// </summary>
        public event Action onAIPlayerAdd;
        /// <summary>
        /// 玩家牌组改变
        /// </summary>
        public event Action<int, int[]> onSetPlayerDeck;
        /// <summary>
        /// 玩家准备或取消准备
        /// </summary>
        public event Action<int> OnPlayerReadyChange;

        public void ResetEvents()
        {
            aiPlayerData = null;

            onStartClick = null;
            onQuitClick = null;
            onRoomPropUpdate = null;
            onPlayerRemove = null;
            onAIPlayerAdd = null;
            onSetPlayerDeck = null;
            OnPlayerReadyChange = null;
        }
    }

    partial class RoomPlayerListItem
    {
        partial void onAwake()
        {
            RemoveButtonBlack.asButton.onClick.AddListener(() => OnPlayerRemove?.Invoke(playerData.id));
            ViewButtonBlack.asButton.onClick.AddListener(() =>
            {
                if (playerData == null)
                    return;
                int[] deck = playerData.getDeck().toIdArray(true);
                if (deck != null)
                    OnPlayerViewDeck?.Invoke(deck);
            });
            asButton.onClick.AddListener(() =>
            {
                if (currentMode == PlayerListMode.AddAI)
                    OnAddAIPlayer?.Invoke();
            });
            SetButtonBlack.asButton.onClick.AddListener(() =>
            {
                OnSetAIDeck?.Invoke();
            });
            ViewButtonBlack.asButton.onClick.AddListener(onClickViewDeckButtonCallback);
            SetButtonBlack.asButton.onClick.AddListener(onClickSetDeckButtonCallback);
            RemoveButtonBlack.asButton.onClick.AddListener(onClickRemoveButtonCallback);
        }
        [Obsolete]
        RoomPlayerInfo playerInfo;
        /// <summary>
        /// 设置当前的玩家信息
        /// </summary>
        /// <param name="info"></param>
        [Obsolete]
        public void SetPlayerInfo(RoomPlayerInfo info)
        {
            playerInfo = info;

            NameText.text = info.name;
            int[] deck;
            bool exists = info.tryGetProp<int[]>(RoomPlayerInfoName.DECK_INTARRAY, out deck);

            CheckIconImage.gameObject.SetActive(info.ready);
            CharacterText.text = exists ? this.findInstance<CardManager>().getCardSkin(deck[0]).name : "";
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

            int[] deck = data.getDeck()?.toIdArray(true);

            CheckIconImage.gameObject.SetActive(data.getReady());
            CharacterText.text = deck != null ? this.findInstance<CardManager>().getCardSkin(deck[0]).name : "";
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
        #region 私有方法
        #region 事件回调
        private void onClickViewDeckButtonCallback()
        {
            onClickViewDeckButton?.Invoke(this);
        }
        private void onClickSetDeckButtonCallback()
        {
            onClickSetDeckButton?.Invoke(this);
        }
        private void onClickRemoveButtonCallback()
        {
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
        public event Action<int[]> OnPlayerViewDeck;
        /// <summary>
        /// 点击添加AI玩家的事件
        /// </summary>
        public event Action OnAddAIPlayer;
        public event Action<RoomPlayerListItem> onClickViewDeckButton;
        public event Action<RoomPlayerListItem> onClickSetDeckButton;
        public event Action<RoomPlayerListItem> onClickRemoveButton;
        #endregion
    }
}
