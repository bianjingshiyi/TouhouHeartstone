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
namespace UI
{
    public partial class NetworkingPage : UIObject
    {
        public NetworkingManager Networking { get; set; }
        partial void onAwake()
        {
            DirectLinkButton.onClick.AddListener(onDirectLinkBtnClick);
            LANButton.onClick.AddListener(createLocalRoom);
            WANButton.onClick.AddListener(createRemoteRoom);

            LocalRoomScrollView.RoomList.networking = this;
            RemoteRoomScrollView.RoomList.networking = this;
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
            Networking.CreateRoom();
            string address = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
            StatusText.text = "本地局域网上创建游戏房间\n" + address + ":" + Networking.host.port;

            LocalRoomScrollView.RoomList.AddItem("测试本机", address + ":" + Networking.host.port);
        }

        void onDirectLinkBtnClick()
        {
            Networking.Connect(IPFieldInput.text);
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
        partial void onAwake()
        {
            Button.onClick.AddListener(onJoinClick);
        }

        void onJoinClick()
        {
            parent.JoinRoom(RoomDescText.text);
        }

        public void setContext(string title, string addr)
        {
            RoomDescText.text = addr;
            RoomNameText.text = title;
        }
    }
}