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

            RoomListParent.networking = this;
            Remote_RoomListParent.networking = this;
        }

        private void createRemoteRoom()
        {
            // todo: 向服务器发送房间信息
            StatusText.text = "正在服务器上创建游戏房间";
            Networking.CreateRoom();
        }

        private void createLocalRoom()
        {
            StatusText.text = "本地局域网上创建游戏房间";
            Networking.CreateRoom();
        }

        void onDirectLinkBtnClick()
        {
            _ = Networking.Connect(IPFieldInput.text);
        }
    }

    public partial class RoomList
    {
        public NetworkingPage networking { get; set; }

        void ReloadRoomList()
        {
            // todo: 拉取数据并更新
            var item = addItem();
            item.setContext();
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
            // todo: 加入房间
        }

        public void setContext()
        {
            // todo: 设置相关的Context
        }
    }
}