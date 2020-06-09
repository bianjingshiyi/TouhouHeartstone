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
        THHRoomInfo _room = null;
        public THHRoomInfo room
        {
            get { return _room; }
            private set { _room = value; }
        }
        protected override void onAwake()
        {
            base.onAwake();
            host.logger = new ULogger("Host");
            client.logger = new ULogger("Client");
            client.onRoomFound += Client_onRoomFound;
            client.onJoinRoom += Client_onJoinRoom;
            gameManager.onGameEnd += onGameEnd;
        }
        [SerializeField]
        List<RoomInfo> _LANRoomList;
        public void updateLANRooms()
        {
            client.findRoom();
        }
        private void Client_onRoomFound(RoomInfo obj)
        {
            _LANRoomList.Add(obj);
            RoomListItem item = getManager<UIManager>().getObject<LANPanel>().RoomScrollView.RoomList.addItem();
            item.refresh(obj);
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
        }
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
        }
        private void Client_onJoinRoom(RoomInfo obj)
        {
            if (obj is THHRoomInfo THHRoom)
            {
                displayRoomPanel(THHRoom);
            }
            else
                Debug.LogError("加入的房间不是一个东方炉石房间！");
        }
        void displayLoadingPanel()
        {
            ui.NetworkingPageGroup.display(null);
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
                if (host.roomInfo != null)
                    host.closeRoom();
                client.quitRoom();
            });
        }
        private void onGameEnd()
        {
            client.disconnect();
        }
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

