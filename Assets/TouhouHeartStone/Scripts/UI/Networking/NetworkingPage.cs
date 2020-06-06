using System;
using UnityEngine;
using UnityEngine.UI;
using BJSYGameCore.UI;
using Game;
using TouhouHeartstone;
using TouhouCardEngine;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using BJSYGameCore;
namespace UI
{
    partial class RoomPanel
    {
        protected override void onDisplay()
        {
            base.onDisplay();

        }
    }
    public partial class NetworkingPage : UIObject
    {
        public NetworkingManager Networking { get; set; }
        [SerializeField]
        HostManager _host;
        public HostManager host
        {
            get
            {
                if (_host == null)
                    _host = this.findInstance<HostManager>();
                return _host;
            }
        }
        [SerializeField]
        ClientManager _client;
        public ClientManager client
        {
            get
            {
                if (_client == null)
                    _client = this.findInstance<ClientManager>();
                return _client;
            }
        }
        partial void onAwake()
        {
            ReturnButton.onClick.AddListener(() =>
            {
                parent.display(parent.MainMenu);
            });
            NetworkingPageGroup.display(NetworkingPageGroup.LANPanel);

            NetworkingPageGroup.IPPanel.ConnectButton.onClick.AddListener(onDirectLinkBtnClick);
            LANButton.onClick.AddListener(createLocalRoom);
            WANButton.onClick.AddListener(createRemoteRoom);

            //LocalRoomScrollView.RoomList.networking = this;
            RemoteRoomScrollView.RoomList.networking = this;
        }
        protected void Start()
        {
            host.start();
            client.start();
        }
        protected void Update()
        {
            NetworkingPageGroup.IPPanel.AddressText.text = host.address;
        }
        private void createRemoteRoom()
        {
            // todo: 向服务器发送房间信息
            //Networking.CreateRoom();
            //StatusText.text = "服务器上创建游戏房间\n" + address + ":" + Networking.host.port;

            //RemoteRoomScrollView.RoomList.AddItem("测试本机", address + ":" + Networking.host.port);
        }

        private void createLocalRoom()
        {
            Networking.createRoom();
            string address = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
            NetworkingPageGroup.IPPanel.AddressText.text = "本地局域网上创建游戏房间\n" + address + ":" + Networking.host.port;

            //LocalRoomScrollView.RoomList.AddItem("测试本机", address + ":" + Networking.host.port);
        }

        async void onDirectLinkBtnClick()
        {
            Uri uri = new Uri(NetworkingPageGroup.IPPanel.IPInputField.text);
            try
            {
                await client.joinRoom(new RoomInfo()
                {
                    ip = uri.Host,
                    port = uri.Port
                }, new THHRoomPlayerInfo()
                {
                    deck = ui.getManager<GameManager>().deck
                });
            }
            catch (Exception e)
            {
                Debug.LogError("加入" + uri + "的游戏失败：" + e);
            }
            //Networking.Connect(IPInputField.text);
        }

        public void JoinRoom(string addr)
        {
            Networking.Connect(addr);
        }
    }

    public partial class RoomList
    {
        public NetworkingPage networking { get; set; }

        void ReloadRoomList()
        {
            // todo: 拉取数据并更新
        }

        public void AddItem(string name, string addr)
        {
            var item = addItem();
            item.setContext(name, addr);
        }

        public void JoinRoom(string addr)
        {
            networking.JoinRoom(addr);
        }
    }

    public partial class RoomListItem
    {
        [SerializeField]
        RoomInfo _room;
        public RoomInfo room
        {
            get { return _room; }
        }
        partial void onAwake()
        {
            asButton.onClick.AddListener(() =>
            {
                ui.getObject<LANPanel>().refreshRoomInfo(_room);
            });
            Button.onClick.AddListener(onJoinClick);
        }
        void onJoinClick()
        {
            _ = ui.getManager<NetworkingManager>().joinRoom(_room);
        }
        public void refresh(RoomInfo roomInfo)
        {
            _room = roomInfo;
            RoomNameText.text = roomInfo.ip + ":" + roomInfo.port;
        }
        public void setContext(string title, string addr)
        {
            //RoomDescText.text = addr;
            RoomNameText.text = title;
        }
    }
}