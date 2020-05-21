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

        THHGame _game;
        THHGame game
        {
            get
            {
                if (_game != null) return _game;
                _game = this.getManager<GameManager>()?.game;
                return _game;
            }
        }

        protected override void onAwake()
        {
            base.onAwake();

            _host = GetComponent<HostManager>();
            _client = GetComponent<ClientManager>();

            if (_host == null || _client == null)
            {
                throw new System.Exception("没有找到HostManager或ClientManager");
            }

            host.logger = game.logger;
            client.logger = game.logger;
            client.onConnected += Host_Client_onConnected;

            var _nw = this.findInstance<NetworkingPage>();
            if (_nw != null)
                _nw.Networking = this;
        }

        private void Host_Client_onConnected()
        {
            game.run();
        }

        public async Task CreateRoom()
        {
            host.start();
            client.start();
            await client.join("127.0.0.1", host.port);

            (game.answers as AnswerManager).client = client;
        }

        public async Task Connect(string addr)
        {
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

            client.start();
            await client.join(address, port);

            (game.answers as AnswerManager).client = client;
        }
    }
}

