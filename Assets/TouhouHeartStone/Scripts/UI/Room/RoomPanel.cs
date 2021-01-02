
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

namespace UI
{
    partial class RoomPanel
    {
        RoomInfo roomInfo = new RoomInfo();
        DeckManager deckManager => this.findInstance<DeckManager>();
        CardManager cardManager => this.findInstance<CardManager>();

        LangManager lang => this.findInstance<LangManager>();

        partial void onAwake()
        {
            StartButtonButtonBlack.asButton.setSelectable(false);
            DeckSelectionDeckListItem.SetMode(DeckListItem.Mode.DeckSelect);
            DeckSelectionDeckListItem.onDeckSelect += (_) =>
            {
                showDeckSelectionPanel((deckID) =>
                {
                    var deck = deckManager.GetDeck(deckID);
                    var skin = cardManager.getSkin(deck.MasterID);
                    DeckSelectionDeckListItem.SetContent(deckID, deck.Name, skin.image);

                    var list = deckManager.GetDeckCards(deckID).Select(k => (int)k.Value).ToList();
                    list.Insert(0, deck.MasterID); // polyfill: 转换为传统的，第一张卡是master的格式
                    OnSetPlayerDeck?.Invoke(selfPlayerID, list.ToArray());
                });
            };

            DeckPanelMasterCardSelection.hide();
            DeckViewMasterCardSelection.hide();

            initEvents();
        }

        [TranslateMsg]
        private const string Cards_Name = "选择你的卡组";

        private void OnEnable()
        {
            DeckSelectionDeckListItem.SetContent(0, lang.i18n._(Cards_Name), null);
        }

        void initEvents()
        {
            RandomSeedContainerGameOptionItemText.InputField.onValueChanged.set(value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    roomInfo.getOption().randomSeed = 0;
                    OnRoomInfoUpdate?.Invoke(roomInfo);
                    return;
                }

                if (int.TryParse(value, out int i))
                {
                    roomInfo.getOption().randomSeed = i;
                    OnRoomInfoUpdate?.Invoke(roomInfo);
                }
                else
                {
                    RandomSeedContainerGameOptionItemText.InputField.text = roomInfo.getOption().randomSeed.ToString();
                }
            });

            ShuffleContainerGameOptionItemToggle.Toggle.onValueChanged.set(value =>
            {
                roomInfo.getOption().shuffle = value;
                OnRoomInfoUpdate?.Invoke(roomInfo);
            });

            InitReplacementContainerGameOptionItemText.InputField.onEndEdit.set(value =>
            {
                if (int.TryParse(value, out int i) && 5 < i)
                {
                    roomInfo.getOption().timeoutForInitReplace = i;
                    OnRoomInfoUpdate?.Invoke(roomInfo);
                }
                else
                {
                    InitReplacementContainerGameOptionItemText.InputField.text = roomInfo.getOption().timeoutForInitReplace.ToString();
                }
            });

            TurnTimeContainerGameOptionItemText.InputField.onEndEdit.set(value =>
            {
                if (int.TryParse(value, out int i) && 5 < i)
                {
                    roomInfo.getOption().timeoutForTurn = i;
                    OnRoomInfoUpdate?.Invoke(roomInfo);
                }
                else
                {
                    TurnTimeContainerGameOptionItemText.InputField.text = roomInfo.getOption().timeoutForTurn.ToString();
                }
            });

            IsSortedToggleGameOptionItemToggle.Toggle.onValueChanged.set((value) =>
            {
                Debug.Log("更新房间设定" + value);
                roomInfo.setOption(new GameOption(roomInfo.getOption())
                {
                    sortedPlayers = value ? roomInfo.playerList.Select(p => p.RoomID).ToArray() : null
                });
                OnRoomInfoUpdate?.Invoke(roomInfo);
            });

            StartButtonButtonBlack.asButton.onClick.set(() =>
            {
                if (isHost)
                {
                    OnStartClick?.Invoke();
                }
                else
                {
                    OnPlayerReadyChange?.Invoke(selfPlayerID);
                }
            });

            QuitButtonButtonBlack.asButton.onClick.set(() =>
            {
                OnQuitClick?.Invoke();
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
            StartButtonButtonBlack.Text.text = isHost ? lang.i18n._(STR_START_GAME) : lang.i18n._(STR_PREPARE);
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
            DeckViewMasterCardSelection.SetItems(cards);
            DeckViewMasterCardSelection.display();
        }

        void showDeckSelectionPanel(Action<int> onDeckSelect)
        {
            var decks = deckManager.GetDecks();
            DeckPanelMasterCardSelection.SetItems(decks, (def, item) =>
            {
                // 只有30张卡的卡组才能选择
                bool isFinish = deckManager.GetDeckCards(def.ID).Count >= 30;
                item.SetSelectable(isFinish);
                item.SetWarning(!isFinish);
            });
            DeckPanelMasterCardSelection.display();
            DeckPanelMasterCardSelection.ClearEvents();

            DeckPanelMasterCardSelection.OnSelect += (id) =>
            {
                DeckPanelMasterCardSelection.hide();
                onDeckSelect(id);
            };
        }

        RoomPlayerInfo aiPlayer = null;

        /// <summary>
        /// 设置房间信息
        /// </summary>
        /// <param name="room"></param>
        public void SetInfo(RoomInfo room)
        {
            roomInfo = room;

            RoomPlayerInfo selfPlayer = null;

            RoomPlayerList.clearItems();
            foreach (var player in room.playerList)
            {
                // 顺便获取自己的引用
                if (player.PlayerID == selfPlayerID)
                    selfPlayer = player;

                var item = RoomPlayerList.addItem();
                item.SetPlayerInfo(player);
                item.OnPlayerViewDeck += viewDeck;

                // 仅当自己是主机且此玩家不是自己时，设置为可以移除
                item.SetRemoveable(isHost && selfPlayerID != player.PlayerID);
                item.OnPlayerRemove += (id) =>
                {
                    if (id == aiPlayer?.PlayerID)
                        aiPlayer = null;
                    OnPlayerRemove?.Invoke(id);
                };

                if (aiPlayer != null && player.PlayerID == aiPlayer.PlayerID)
                {
                    item.SetMode(RoomPlayerListItem.PlayerListMode.AIPlayer);
                    item.OnSetAIDeck += () =>
                    {
                        showDeckSelectionPanel((deckID) =>
                        {
                            // AI 玩家直接在这里设置为准备好
                            // 底下的 OnSetPlayerDeck 会触发变更事件的。
                            aiPlayer.ready = true;

                            var deck = deckManager.GetDeck(deckID);
                            var skin = cardManager.getSkin(deck.MasterID);

                            var list = deckManager.GetDeckCards(deckID).Select(k => (int)k.Value).ToList();
                            list.Insert(0, deck.MasterID); // polyfill: 转换为传统的，第一张卡是master的格式

                            OnSetPlayerDeck?.Invoke(aiPlayer.PlayerID, list.ToArray());
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
                Debug.LogWarning($"SelfPlayer是空的。PlayerID：{selfPlayerID}, players: {room.playerList.Select(e => e.PlayerID.ToString())}");
            }

            if (currentMode == Mode.Local && aiPlayer == null)
            {
                var item = RoomPlayerList.addItem();
                item.SetMode(RoomPlayerListItem.PlayerListMode.AddAI);

                item.OnAddAIPlayer += () =>
                {
                    aiPlayer = new RoomPlayerInfo() { RoomID = 1, name = "AI" };
                    OnAIPlayerAdd?.Invoke(aiPlayer);
                };
            }

            // 设置房间选项
            try
            {
                GameOption option = room.getOption();
                var seed = option.randomSeed;
                RandomSeedContainerGameOptionItemText.InputField.text = seed == 0 ? "" : seed.ToString();
                ShuffleContainerGameOptionItemToggle.Toggle.isOn = option.shuffle;
                InitReplacementContainerGameOptionItemText.InputField.text = option.timeoutForInitReplace.ToString();
                TurnTimeContainerGameOptionItemText.InputField.text = option.timeoutForTurn.ToString();
                IsSortedToggleGameOptionItemToggle.Toggle.isOn = option.sortedPlayers != null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (isHost)
            {
                // 判断是否可以开始游戏
                bool startable = room.playerList.Count > 1;

                foreach (var item in room.playerList)
                {
                    if (item.PlayerID == selfPlayerID)
                    {
                        // 主机检查卡组是否满
                        if (!item.tryGetDeck(out int[] deck) || deck.Length < 30)
                            startable = false;
                    }
                    else
                    {
                        // 其他检查是否准备好
                        startable &= item.ready;
                    }
                }

                StartButtonButtonBlack.asButton.setSelectable(startable);
            }
            else
            {
                // 判断准备状态
                StartButtonButtonBlack.Text.text = selfPlayer.ready ? lang.i18n._(STR_REQUEST_PREPRAE) : lang.i18n._(STR_REQUEST_PREPRAE);
                StartButtonButtonBlack.asButton.setSelectable(selfPlayer.tryGetDeck(out var deck) && deck != null && deck.Length == 31);
            }
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
        public event Action OnStartClick;
        /// <summary>
        /// 点击退出房间后触发
        /// </summary>
        public event Action OnQuitClick;
        /// <summary>
        /// 房间信息更新（主要是设置）更新后触发
        /// </summary>
        public event Action<RoomInfo> OnRoomInfoUpdate;
        /// <summary>
        /// 点击删除玩家按钮后触发
        /// </summary>
        public event Action<int> OnPlayerRemove;
        /// <summary>
        /// 添加了AI玩家
        /// </summary>
        public event Action<RoomPlayerInfo> OnAIPlayerAdd;
        /// <summary>
        /// 玩家牌组改变
        /// </summary>
        public event Action<int, int[]> OnSetPlayerDeck;
        /// <summary>
        /// 玩家准备或取消准备
        /// </summary>
        public event Action<int> OnPlayerReadyChange;

        public void ResetEvents()
        {
            aiPlayer = null;

            OnStartClick = null;
            OnQuitClick = null;
            OnRoomInfoUpdate = null;
            OnPlayerRemove = null;
            OnAIPlayerAdd = null;
            OnSetPlayerDeck = null;
            OnPlayerReadyChange = null;
        }
    }



    partial class RoomPlayerListItem
    {
        partial void onAwake()
        {
            RemoveButtonBlack.asButton.onClick.AddListener(() => OnPlayerRemove?.Invoke(playerInfo.PlayerID));
            ViewButtonBlack.asButton.onClick.AddListener(() =>
            {
                int[] deck;
                bool exists = playerInfo.tryGetProp<int[]>(RoomPlayerInfoName.DECK_INTARRAY, out deck);
                if (exists)
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
        }

        RoomPlayerInfo playerInfo;
        /// <summary>
        /// 设置当前的玩家信息
        /// </summary>
        /// <param name="info"></param>
        public void SetPlayerInfo(RoomPlayerInfo info)
        {
            playerInfo = info;

            NameText.text = info.name;
            int[] deck;
            bool exists = info.tryGetProp<int[]>(RoomPlayerInfoName.DECK_INTARRAY, out deck);

            CheckIconImage.gameObject.SetActive(info.ready);
            CharacterText.text = exists ? this.findInstance<CardManager>().getSkin(deck[0]).name : "";
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
    }
}
