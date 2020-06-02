using System.Collections;
using System.Collections.Generic;
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

namespace Game
{
    public class NetworkingManager : Manager
    {
        private HostManager _host;
        public HostManager host => _host;
        private ClientManager _client;
        public ClientManager client => _client;
        private GameManager _gameManager;
        public GameManager gameManager
        {
            get
            {
                if (_gameManager == null) _gameManager = getManager<GameManager>();
                return _gameManager;
            }
        }
        protected override void onAwake()
        {
            base.onAwake();
            //初始化组件
            var _nw = this.findInstance<NetworkingPage>();
            if (_nw != null)
                _nw.Networking = this;
        }

        private void initClient()
        {
            if (_client == null)
            {
                _client = this.findInstance<ClientManager>();
                if (_client == null)
                    throw new Exception("没有找到ClientManager");
                client.onConnected += Client_onConnected;
                client.onReceive += Client_onReceive;
            }
        }

        private void initHost()
        {
            if (_host == null)
            {
                _host = this.findInstance<HostManager>();
                if (_host == null)
                    throw new Exception("没有找到HostManager");
                host.onClientConnected += Host_onClientConnected;
            }
        }

        [SerializeField]
        THHRoomInfo _room = null;
        /// <summary>
        /// 创建一个房间
        /// </summary>
        /// <returns></returns>
        public Task CreateRoom()
        {
            initHost();
            initClient();
            host.logger = new UnityLogger();
            client.logger = new UnityLogger();

            host.start();
            client.start();
            return client.join("127.0.0.1", host.port);
        }
        /// <summary>
        /// 加入一个房间
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public Task Connect(string addr)
        {
            initClient();

            int port = client.port;
            string address = "";
            var uri = new Uri("http://" + addr);
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

            client.logger = new UnityLogger();
            client.start();
            Debug.Log($"Connect to {address}:{port}...");
            return client.join(address, port);
        }
        private void Host_onClientConnected(int peerID)
        {
            //连接过来了不能作数，对方的卡组，名字之类的信息都不知道
        }
        private void Client_onConnected()
        {
            //加入成功之后把自己的信息发过去
            _ = client.send(new THHRoomPlayerInfo()
            {
                id = client.id,
                name = "玩家" + client.id,
                deck = gameManager.deck
            });
        }
        private void Client_onReceive(int id, object obj)
        {
            switch (obj)
            {
                case RoomPlayerInfo playerInfo:
                    if (_room != null)
                        _room.playerList.Add(playerInfo);
                    if (host != null)
                        _ = client.send(_room);
                    break;
                case THHRoomInfo roomInfo:
                    _room = roomInfo;
                    if (_room.playerList.Count > 1)
                        gameManager.startRemoteGame(client, _room.option, _room.playerList.Cast<THHRoomPlayerInfo>().ToArray());
                    break;
            }
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