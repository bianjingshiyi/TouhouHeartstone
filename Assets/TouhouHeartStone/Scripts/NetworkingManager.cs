using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouhouCardEngine;
using BJSYGameCore;
using UI;
using System.Threading.Tasks;
using TouhouHeartstone;

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

        THHGame game => gameManager.game;

        protected override void onAwake()
        {
            base.onAwake();

            _host = GetComponent<HostManager>();
            _client = GetComponent<ClientManager>();

            if (_host == null || _client == null)
            {
                throw new Exception("没有找到HostManager或ClientManager");
            }

            host.onClientConnected += Host_onClientConnected;
            client.onConnected += Client_onConnected;

            var _nw = this.findInstance<NetworkingPage>();
            if (_nw != null)
                _nw.Networking = this;
        }

        private void Host_onClientConnected(int peerID)
        {
            // 主机等待对方加入，对方加入后自己加入
            if (peerID != client.id)
            {
                _ = client.join("127.0.0.1", host.port);
            }
        }

        private void Client_onConnected()
        {
            // 加入成功后就开启自己的游戏
            // 主机是在对方连接成功后再连接自己
            (game.answers as AnswerManager).client = client;
            gameManager.displayGameUI(game.getPlayer(1)); // todo: 替换成真实的ID
            gameManager.gameStart();
        }

        /// <summary>
        /// 创建一个房间
        /// </summary>
        /// <returns></returns>
        public void CreateRoom()
        {
            if (game == null)
                gameManager.createGame();

            host.logger = game?.logger;
            client.logger = game?.logger;

            host.start();
            client.start();
        }

        /// <summary>
        /// 加入一个房间
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public void Connect(string addr)
        {
            if (game == null)
                gameManager.createGame();

            int port = host.port;
            string address = "";
            var uri = new Uri("http://" + addr);
            if (uri.HostNameType == UriHostNameType.Dns)
            {
                address = Dns.GetHostAddresses(uri.Host).FirstOrDefault(e => e.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
            }
            else
            {
                address = uri.Host;
            }

            if (uri.Port != 80)
                port = uri.Port;

            client.logger = game?.logger;
            client.start();
            Debug.Log($"Connect to {address}:{port}...");
            _ = client.join(address, port);
        }
    }
}

