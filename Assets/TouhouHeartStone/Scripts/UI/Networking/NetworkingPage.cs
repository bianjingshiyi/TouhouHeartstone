using System;
using UnityEngine;
using UnityEngine.UI;
using BJSYGameCore.UI;
using Game;
using TouhouHeartstone;
using TouhouCardEngine;
using System.Threading.Tasks;
using System.Net;

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
            StatusText.text = "服务器上创建游戏房间\n127.0.0.1:" + Networking.host.port;
            Networking.CreateRoom();

            RemoteRoomScrollView.RoomList.AddItem("测试本机", "127.0.0.1");
        }

        private void createLocalRoom()
        {
            Networking.CreateRoom();
            StatusText.text = "本地局域网上创建游戏房间\n127.0.0.1:" + Networking.host.port;

            LocalRoomScrollView.RoomList.AddItem("测试本机", "127.0.0.1");
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