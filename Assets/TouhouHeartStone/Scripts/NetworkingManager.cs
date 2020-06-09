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
        [SerializeField]
        List<THHRoomInfo> _LANRoomList;
        [SerializeField]
        BJSYGameCore.Timer _LANUpdateTimer = new BJSYGameCore.Timer() { duration = 1 };
        protected override void onAwake()
        {
            base.onAwake();
            host.logger = new ULogger("Host");
            client.logger = new ULogger("Client");

            client.onRoomFound += Client_onRoomFound;
            client.onJoinRoom += Client_onJoinRoom;
            client.onRoomInfoUpdate += Client_onRoomInfoUpdate;
            _LANUpdateTimer.start();
            client.onQuitRoom += Client_onQuitRoom;
            gameManager.onGameEnd += onGameEnd;

            ui.LinkButton.onClick.set(() =>
            {
                displayIPPanel();
            });
            ui.LANButton.onClick.set(() =>
            {
                displayLANPanel();
            });
            ui.RoomButton.interactable = false;
            ui.RoomButton.image.color = Color.gray;
            ui.RoomButton.onClick.set(() =>
            {
                if (client.roomInfo is THHRoomInfo tHHRoom)
                    displayRoomPanel(tHHRoom);
            });
            ui.ReturnButton.onClick.AddListener(() =>
            {
                ui.parent.display(ui.parent.MainMenu);
            });

            ui.NetworkingPageGroup.display(ui.NetworkingPageGroup.LANPanel);
        }
        protected void Update()
        {
            if (_LANUpdateTimer.isExpired())
            {
                foreach (var room in _LANRoomList)
                {
                    _ = updateRoomInfo(room);
                }
                _LANUpdateTimer.start();
            }
        }
        private void Client_onRoomFound(RoomInfo obj)
        {
            if (obj is THHRoomInfo tHHRoom)
            {
                _LANRoomList.Add(tHHRoom);
                RoomListItem item = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.addItem();
                refreshRoomListItem(item, tHHRoom);
            }
        }
        async Task updateRoomInfo(THHRoomInfo roomInfo)
        {
            THHRoomInfo newRoomInfo = await client.checkRoomInfo(roomInfo) as THHRoomInfo;
            if (newRoomInfo != null)
            {
                int index = _LANRoomList.IndexOf(roomInfo);
                if (index > 0)
                {
                    _LANRoomList[index] = newRoomInfo;
                    RoomList list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
                    RoomListItem item = list.getItems()[index];
                    refreshRoomListItem(item, newRoomInfo);
                }
            }
            else
            {
                int index = _LANRoomList.IndexOf(roomInfo);
                if (index > 0)
                {
                    _LANRoomList.RemoveAt(index);
                    RoomList list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
                    list.removeItem(list.getItems()[index]);
                }
            }
        }
        private void Client_onJoinRoom(RoomInfo obj)
        {
            if (obj is THHRoomInfo THHRoom)
                displayRoomPanel(THHRoom);
            else
                Debug.LogError("加入的房间不是一个东方炉石房间！");
        }
        private void Client_onRoomInfoUpdate(RoomInfo obj)
        {
            if (obj is THHRoomInfo tHHRoom)
                displayRoomPanel(tHHRoom);
            else
                Debug.LogError("加入的房间不是一个东方炉石房间！");
        }
        private void Client_onQuitRoom()
        {
            displayLANPanel();
        }
        private void onGameEnd()
        {
            client.disconnect();
        }
        #region Logic
        /// <summary>
        /// 创建一个房间
        /// </summary>
        /// <returns></returns>
        public async Task createRoom()
        {
            RoomInfo room = host.openRoom(new THHRoomInfo()
            {
                port = 9050,
                option = new GameOption()
                {
                    randomSeed = (int)DateTime.Now.ToBinary()
                }
            });
            displayLoadingPanel();
            await client.joinRoom(room, new THHRoomPlayerInfo()
            {
                deck = getManager<GameManager>().deck
            });
            ui.RoomButton.interactable = true;
            ui.RoomButton.image.color = Color.white;
        }
        public void updateLANRooms()
        {
            client.findRoom();
        }
        public async Task joinRoom(RoomInfo roomInfo)
        {
            if (roomInfo == null)
                throw new ArgumentNullException(nameof(roomInfo));
            displayLoadingPanel();
            await client.joinRoom(roomInfo, new THHRoomPlayerInfo()
            {
                deck = getManager<GameManager>().deck
            });
            ui.RoomButton.interactable = true;
            ui.RoomButton.image.color = Color.white;
        }
        public void quitRoom()
        {
            if (host.roomInfo != null)
                host.closeRoom();
            if (client.roomInfo != null)
                client.quitRoom();
            ui.RoomButton.interactable = false;
            ui.RoomButton.image.color = Color.gray;
        }
        #endregion
        #region UI
        void displayLoadingPanel()
        {
            ui.NetworkingPageGroup.display(null);
        }
        void displayIPPanel()
        {
            IPPanel ipPanel = ui.NetworkingPageGroup.IPPanel;
            ui.NetworkingPageGroup.display(ipPanel);
            ipPanel.ConnectButton.onClick.set(() =>
            {
                Match m = Regex.Match(ipPanel.IPInputField.text, @"(?<ip>d+\.d+\.d+\.d+)\:(?<port>d+)");
                if (m.Success)
                {
                    _ = joinRoom(new RoomInfo()
                    {
                        ip = m.Groups["ip"].Value,
                        port = int.Parse(m.Groups["port"].Value)
                    });
                }
                else
                    getManager<UIManager>().getObject<Dialog>().display("输入的地址格式有误", null);
            });
            ipPanel.AddressText.text = "你的地址：\n" + address;
        }
        void displayLANPanel()
        {
            LANPanel lanPanel = ui.NetworkingPageGroup.LANPanel;
            ui.NetworkingPageGroup.display(lanPanel);
            lanPanel.CreateRoomButton.onClick.set(() =>
            {
                _ = createRoom();
            });
            lanPanel.NameText.text = null;
            lanPanel.IPText.text = null;
            lanPanel.PortText.text = null;
            lanPanel.DescText.text = null;
            updateLANRooms();
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
                    lanPanel.PortText.text = null;
                    lanPanel.DescText.text = null;
                }
                else
                {
                    lanPanel.NameText.text = "房间";
                    lanPanel.IPText.text = "IP:" + obj.ip;
                    lanPanel.PortText.text = "端口:" + obj.port;
                    lanPanel.DescText.text = "描述:";
                }
            });
            item.Button.onClick.set(() =>
            {
                _ = joinRoom(obj);
            });
        }
        void displayRoomPanel(THHRoomInfo room)
        {
            RoomPanel roomPanel = ui.NetworkingPageGroup.RoomPanel;
            ui.NetworkingPageGroup.display(roomPanel);
            roomPanel.RoomPlayerList.clearItems();
            foreach (var player in room.playerList.Cast<THHRoomPlayerInfo>())
            {
                var item = roomPanel.RoomPlayerList.addItem();
                item.NameText.text = player.name;
                item.CharacterText.text = getManager<CardManager>().GetCardSkin(player.deck[0]).name;
                item.Button.onClick.AddListener(() =>
                {
                    //TODO:查看卡组
                });
            }
            roomPanel.RandomSeedInputField.text = room.option.randomSeed.ToString();
            roomPanel.RandomSeedInputField.onEndEdit.RemoveAllListeners();
            roomPanel.RandomSeedInputField.onEndEdit.AddListener(value =>
            {
                if (host.roomInfo is THHRoomInfo roomInfo)
                {
                    if (int.TryParse(value, out int i))
                    {
                        roomInfo.option.randomSeed = i;
                        host.updateRoomInfo(roomInfo);
                    }
                    else
                        roomPanel.RandomSeedInputField.text = roomInfo.option.randomSeed.ToString();
                }
            });
            roomPanel.ShuffleToggle.isOn = room.option.shuffle;
            roomPanel.ShuffleToggle.onValueChanged.RemoveAllListeners();
            roomPanel.ShuffleToggle.onValueChanged.AddListener(value =>
            {
                if (host.roomInfo is THHRoomInfo roomInfo)
                {
                    roomInfo.option.shuffle = value;
                    host.updateRoomInfo(roomInfo);
                }
            });
            roomPanel.InitReplaceTimeInputField.text = room.option.timeoutForInitReplace.ToString();
            roomPanel.InitReplaceTimeInputField.onEndEdit.RemoveAllListeners();
            roomPanel.InitReplaceTimeInputField.onEndEdit.AddListener(value =>
            {
                if (host.roomInfo is THHRoomInfo roomInfo)
                {
                    if (int.TryParse(value, out int i) && 5 < i)
                    {
                        roomInfo.option.timeoutForInitReplace = i;
                        host.updateRoomInfo(roomInfo);
                    }
                    else
                    {
                        roomPanel.InitReplaceTimeInputField.text = roomInfo.option.timeoutForInitReplace.ToString();
                    }
                }
            });
            roomPanel.TurnTimeInputField.text = room.option.timeoutForTurn.ToString();
            roomPanel.TurnTimeInputField.onEndEdit.RemoveAllListeners();
            roomPanel.TurnTimeInputField.onEndEdit.AddListener(value =>
            {
                if (host.roomInfo is THHRoomInfo roomInfo)
                {
                    if (int.TryParse(value, out int i) && 5 < i)
                    {
                        roomInfo.option.timeoutForTurn = i;
                        host.updateRoomInfo(roomInfo);
                    }
                    else
                    {
                        roomPanel.TurnTimeInputField.text = roomInfo.option.timeoutForTurn.ToString();
                    }
                }
            });
            roomPanel.QuitButton.onClick.RemoveAllListeners();
            roomPanel.QuitButton.onClick.AddListener(() =>
            {
                quitRoom();
            });
        }
        #endregion
    }
    [Serializable]
    public class THHRoomInfo : RoomInfo
    {
        public GameOption option = new GameOption();
    }
    [Serializable]
    public class THHRoomPlayerInfo : RoomPlayerInfo
    {
        public int[] deck = new int[0];
    }
}