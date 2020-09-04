using UnityEngine;
using TouhouCardEngine;
using BJSYGameCore;
using UI;
using System.Threading.Tasks;
using TouhouHeartstone;
using System.Net.Sockets;
using System.Net;
using System;
using System.Linq;
using System.Collections.Generic;
using BJSYGameCore.UI;
using System.Text.RegularExpressions;
using NitoriNetwork.Common;

namespace Game
{
    public class NetworkingManager : Manager
    {
        [SerializeField]
        private HostManager _host;
        public HostManager host
        {
            get
            {
                if (_host == null)
                    _host = this.findInstance<HostManager>();
                return _host;
            }
        }
        public string address
        {
            get { return host.address; }
        }
        public string[] addresses => host.addresses;

        [SerializeField]
        private ClientManager _client;
        public ClientManager client
        {
            get
            {
                if (_client == null)
                    _client = this.findInstance<ClientManager>();
                return _client;
            }
        }
        private GameManager _gameManager;
        public GameManager gameManager
        {
            get
            {
                if (_gameManager == null) _gameManager = getManager<GameManager>();
                return _gameManager;
            }
        }
        public NetworkingPage ui
        {
            get { return getManager<UIManager>().getObject<NetworkingPage>(); }
        }

        RoomPanel roomPanel => ui.parent.RoomPanel;

        [SerializeField]
        List<RoomInfo> _LANRoomList;
        [SerializeField]
        BJSYGameCore.Timer _LANUpdateTimer = new BJSYGameCore.Timer() { duration = 1 };
        protected override void onAwake()
        {
            base.onAwake();
            host.logger = new ULogger("Host");
            client.logger = new ULogger("Client");

            client.addInvokeTarget(this);
            client.onRoomFound += Client_onRoomFound;
            client.onJoinRoom += Client_onJoinRoom;
            client.onRoomInfoUpdate += Client_onRoomInfoUpdate;
            _LANUpdateTimer.start();
            // todo: Client断开后退出房间
            client.onQuitRoom += Client_onQuitRoom;
            gameManager.onGameEnd += onGameEnd;

            ui.LANButton.onClick.set(() =>
            {
                displayLANPanel();
            });

            ui.RoomButton.interactable = false;
            ui.RoomButton.image.color = Color.gray;
            ui.RoomButton.onClick.set(() =>
            {
                displayRoomPanel();
            });
            ui.ReturnButton.onClick.AddListener(() =>
            {
                ui.parent.display(ui.parent.MainMenu);
            });
        }
        protected void Start()
        {
            client.start();
        }
        protected void Update()
        {
            if (_LANUpdateTimer.isExpired())
            {
                updateAllRooms();
                _LANUpdateTimer.start();
            }
        }
        private void Client_onRoomFound(RoomInfo obj)
        {
            if (obj is RoomInfo tHHRoom)
            {
                int index = _LANRoomList.FindIndex(r => r.id == tHHRoom.id);

                if (index < 0)
                {
                    if (!obj.isSameRoom(host.room))
                    {
                        _LANRoomList.Add(tHHRoom);
                        RoomListItem item = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.addItem();
                        refreshRoomListItem(item, tHHRoom);
                    }
                }
                else
                {
                    if (!obj.isSameRoom(host.room))
                    {
                        _LANRoomList[index] = tHHRoom;
                        RoomListItem item = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.getItems()[index];
                        refreshRoomListItem(item, tHHRoom);
                    }
                    else
                    {
                        _LANRoomList.RemoveAt(index);
                        RoomList list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
                        list.removeItem(list.getItems()[index]);
                    }
                }
            }
        }
        async Task updateRoomInfo(RoomInfo roomInfo)
        {
            RoomInfo newRoomInfo = await client.checkRoomInfo(roomInfo);
            if (newRoomInfo != null)
            {
                int index = _LANRoomList.FindIndex(r => r.isSameRoom(roomInfo));
                if (index >= -1)
                {
                    _LANRoomList[index] = newRoomInfo;
                    RoomList list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
                    RoomListItem item = list.getItems()[index];
                    refreshRoomListItem(item, newRoomInfo);
                }
            }
            else
            {
                int index = _LANRoomList.FindIndex(r => r.isSameRoom(roomInfo));
                if (index > -1)
                {
                    _LANRoomList.RemoveAt(index);
                    RoomList list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
                    list.removeItem(list.getItems()[index]);
                }
            }
        }
        private void Client_onJoinRoom(RoomInfo obj)
        {
            displayRoomPanel();
            setRoomPanel(obj);
        }
        private void Client_onRoomInfoUpdate(RoomInfo now, RoomInfo updated)
        {
            if (updated.playerList.Count < now.playerList.Count)
            {
                //有玩家离开了
                var roomPlayer = now.playerList.First(p => !updated.playerList.Exists(p2 => p2.RoomID == p.RoomID));
                if (getManager<GameManager>().game is THHGame game)
                {
                    game.getPlayer(roomPlayer.RoomID).cmdSurrender(game);
                }
            }
            setRoomPanel(updated);
        }
        private void Client_onQuitRoom()
        {
            displayLANPanel();
        }
        private void onGameEnd()
        {
            client.quitRoom();
            host.closeRoom();
            host.stop();
        }
        #region Logic
        /// <summary>
        /// 创建一个房间
        /// </summary>
        /// <returns></returns>
        public void createRoom()
        {
            host.start(_port);
            currentPlayer = new RoomPlayerInfo();
            currentPlayer.name = "房主"; // todo: 替换为真实的玩家名称
            currentPlayer.setProp(RoomPlayerInfoName.DECK_INTARRAY, getManager<GameManager>().deck);

            RoomInfo room = host.openRoom(new RoomInfo(currentPlayer.PlayerID));
            room.setOption(new GameOption()
            {
                randomSeed = (int)DateTime.Now.ToBinary()
            });
            displayLoadingPanel();

            Task.Run(async () => { await client.joinRoom(room, currentPlayer); });

            roomPanel.SetSelfID(currentPlayer.PlayerID);

            ui.RoomButton.interactable = true;
            ui.RoomButton.image.color = Color.white;
        }
        public void findLANRooms()
        {
            client.findRoom(_port);
        }
        public void flushLANRooms()
        {
            _LANRoomList.Clear();
            ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.clearItems();
            findLANRooms();
        }
        public void updateAllRooms()
        {
            foreach (var room in _LANRoomList)
            {
                _ = updateRoomInfo(room);
            }
        }

        RoomPlayerInfo currentPlayer;

        /// <summary>
        /// 客户端加入房间
        /// </summary>
        /// <param name="roomInfo"></param>
        /// <returns></returns>
        public async Task joinRoom(RoomInfo roomInfo)
        {
            if (roomInfo == null)
                throw new ArgumentNullException(nameof(roomInfo));
            displayLoadingPanel();
            currentPlayer = new RoomPlayerInfo();
            currentPlayer.name = "玩家2"; // todo: 替换为真实的玩家名称
            currentPlayer.setProp(RoomPlayerInfoName.DECK_INTARRAY, getManager<GameManager>().deck);
            await client.joinRoom(roomInfo, currentPlayer);

            roomPanel.SetSelfID(currentPlayer.PlayerID);

            ui.RoomButton.interactable = true;
            ui.RoomButton.image.color = Color.white;
        }
        /// <summary>
        /// [RPC] 开始游戏
        /// </summary>
        public void start()
        {
            getManager<GameManager>().startRemoteGame(client, client.roomInfo.getOption(), client.roomInfo.playerList.ToArray());
        }
        public void quitRoom()
        {
            if (host.RoomIsValid)
                host.closeRoom();
            host.stop();
            if (client.roomInfo != null)
                client.quitRoom();
            currentPlayer = null;

            ui.RoomButton.interactable = false;
            ui.RoomButton.image.color = Color.gray;
            updateAllRooms();
        }
        #endregion
        #region UI
        void displayLoadingPanel()
        {
            ui.NetworkingPageGroup.display(null);
        }
        [SerializeField]
        int _port;
        public void displayLANPanel()
        {
            LANPanel lanPanel = ui.NetworkingPageGroup.LANPanel;
            ui.NetworkingPageGroup.display(lanPanel);
            lanPanel.LoadingPanelImage.gameObject.SetActive(false);
            lanPanel.FlushRoomButton.onClick.set(() =>
            {
                flushLANRooms();
            });
            lanPanel.CreateRoomButtonButtonBlack.asButton.interactable = !host.RoomIsValid;
            lanPanel.CreateRoomButtonButtonBlack.asButton.onClick.set(() =>
            {
                lanPanel.LoadingPanelImage.gameObject.SetActive(true);
                createRoom();
            });
            lanPanel.ReturnBtnButtonBlack.asButton.onClick.AddListener(() =>
            {
                ui.parent.display(ui.parent.MainMenu);
            });
            lanPanel.ConnectButtonButtonBlack.asButton.onClick.set(() =>
            {
                int port = _port;
                string address = "";
                var uri = new Uri("http://" + lanPanel.IPInputField.text);

                if (uri.HostNameType == UriHostNameType.Dns)
                {
                    address = Dns.GetHostAddresses(uri.Host).FirstOrDefault(e => e.AddressFamily == AddressFamily.InterNetwork)?.ToString();
                }
                else
                {
                    address = uri.Host;
                }

                if (uri.Port != 80)
                    port = uri.Port;

                _ = joinRoom(new RoomInfo()
                {
                    ip = address,
                    port = port
                });

                // getManager<UIManager>().getObject<Dialog>().display("输入的地址格式有误", null);
            });
            lanPanel.NameText.text = null;
            lanPanel.IPText.text = null;
            lanPanel.DescText.text = null;
            if (host.RoomIsValid)
            {
                displayRoomPanel();
            }
            else
            {
                findLANRooms();
            }
        }
        private void refreshRoomListItem(RoomListItem item, RoomInfo obj)
        {
            item.RoomNameText.text = obj.ip + ":" + obj.port;
            item.asButton.onClick.set(() =>
            {
                LANPanel lanPanel = ui.NetworkingPageGroup.LANPanel;
                if (obj == null)
                {
                    lanPanel.NameText.text = null;
                    lanPanel.IPText.text = null;
                    lanPanel.DescText.text = null;
                }
                else
                {
                    lanPanel.NameText.text = obj.id.ToString();
                    lanPanel.IPText.text = obj.ip;
                    lanPanel.DescText.text = "描述";
                }
            });
            item.ButtonButtonBlack.asButton.onClick.set(() =>
            {
                _ = joinRoom(obj);
            });
        }
        private void setRoomPanel(RoomInfo room)
        {
            roomPanel.SetInfo(room);
        }

        private void displayRoomPanel()
        {
            roomPanel.ResetEvents();

            roomPanel.OnQuitClick += () =>
            {
                quitRoom();
                ui.parent.display(ui);
            };
            roomPanel.OnStartClick += () =>
            {
                client.invokeHost(RPCHelper.GameStart());
            };
            roomPanel.OnRoomInfoUpdate += (info) =>
            {
                if (host.RoomIsValid)
                {
                    client.invokeHost(RPCHelper.RoomPropSet(RoomInfoName.OPTION_GAMEOPTION, info.getOption()));
                }
            };
            roomPanel.OnPlayerRemove += (id) =>
            {
                if (host.RoomIsValid)
                {
                    client.invokeHost(RPCHelper.RemovePlayer(id));
                }
            };
            roomPanel.OnSetPlayerDeck += (id, cards) =>
            {
                currentPlayer.setDeck(cards);
                _ = client.updatePlayerInfo(currentPlayer);
            };
            roomPanel.OnPlayerReadyChange += (id) =>
            {
                currentPlayer.ready = !currentPlayer.ready;
                _ = client.updatePlayerInfo(currentPlayer);
            };

            ui.parent.display(roomPanel);

            roomPanel.SetRightText("IP：" + string.Join(", ", addresses));
            roomPanel.SetMode(host.RoomIsValid ? RoomPanel.Mode.NetworkingHost : RoomPanel.Mode.NetworkingClient);
        }
        #endregion
    }
    public static class RoomInfoName
    {
        public const string OPTION_GAMEOPTION = "option";
        public static GameOption getOption(this RoomInfo roomInfo)
        {
            return roomInfo.getProp<GameOption>(OPTION_GAMEOPTION);
        }
        public static void setOption(this RoomInfo roomInfo, GameOption value)
        {
            roomInfo.setProp(OPTION_GAMEOPTION, value);
        }
    }
    public static class RoomPlayerInfoName
    {
        public const string DECK_INTARRAY = "deck";
        public static int[] getDeck(this RoomPlayerInfo playerInfo)
        {
            return playerInfo.getProp<int[]>(DECK_INTARRAY);
        }
        public static bool tryGetDeck(this RoomPlayerInfo playerInfo, out int[] deck)
        {
            return playerInfo.tryGetProp(DECK_INTARRAY, out deck);
        }
        public static void setDeck(this RoomPlayerInfo playerInfo, int[] value)
        {
            playerInfo.setProp(DECK_INTARRAY, value);
        }
    }
}