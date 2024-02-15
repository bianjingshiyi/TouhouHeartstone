using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using BJSYGameCore;
using MukioI18n;
using NitoriNetwork.Common;
using TouhouCardEngine;
using UI;
using UnityEngine;

namespace Game
{
    public class NetworkingManager : Manager
    {
        #region 公有方法
        public void start()
        {
            _ = startSequence();
        }
        /// <summary>
        /// 创建一个房间
        /// </summary>
        /// <returns></returns>
        public Task createRoom(string name = "", string password = "")
        {
            return logic.CreateRoom(name, password);
        }
        public async Task<Sprite> getCaptchaImage()
        {
            Texture2D texture = new Texture2D(512, 512);
            try
            {
                texture.LoadImage(await logic.getCaptchaImage());
            }
            catch (NetClientException e)
            {
                Debug.LogWarning(e.Message);
                return null;
            }
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        public Task<ClientUpdateInfo> getLatestVersion()
        {
            return logic.ServerClient.GetLatestUpdateAsync();
        }
        public Task joinRoom(string roomID, string password = "")
        {
            return logic.JoinRoom(roomID, password);
        }
        public Task joinRoom(string addr, int port, string password = "")
        {
            return logic.JoinRoom(addr, port, password);
        }
        /// <summary>
        /// 设置服务器地址
        /// </summary>
        /// <param name="addr"></param>
        public async Task SetServerAddr(string addr)
        {
            logic.setServerAddress(addr);
            await logic.NewKratosClient();
            _ = startSequence();
        }
        /// <summary>
        /// 开始远程游戏。
        /// </summary>
        public void StartRemoteGame()
        {
            logic.StartGame();
        }
        /// <summary>
        /// 地址解析
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="defaultPort"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void parseAddress(string addr, int defaultPort, out string ip, out int port)
        {
            var uri = new Uri("http://" + addr);
            if (uri.HostNameType == UriHostNameType.Dns)
            {
                ip = Dns.GetHostAddresses(uri.Host).FirstOrDefault(e => e.AddressFamily == AddressFamily.InterNetwork)?.ToString();
            }
            else
            {
                ip = uri.Host;
            }
            port = uri.Port != 80 ? uri.Port : defaultPort;
        }
        public string[] getAddresses()
        {
            List<string> result = new List<string>();
            foreach (var inf in NetworkInterface.GetAllNetworkInterfaces()) 
            {
                if (inf.IsReceiveOnly || 
                    inf.OperationalStatus != OperationalStatus.Up || 
                    inf.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                IPInterfaceProperties ipProps = inf.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    result.Add($"{addr.Address.ToString()}:{LANPort}");
                }
            }

            return result.ToArray();
        }
        public int[] getPorts()
        {
            return _ports;
        }
        public void loadRememberAccount(out bool rememberUserName, out string userName, out bool rememberPassword, out string password)
        {
            rememberUserName = PlayerPrefs.GetInt("RememberAccount", 0) > 0;
            rememberPassword = PlayerPrefs.GetInt("RememberPassword", 0) > 0;
            userName = PlayerPrefs.GetString("UserName", null);
            password = PlayerPrefs.GetString("Password", null);
        }
        public void saveRemeberAccount(bool rememeberUserName, string userName, bool rememberPassword, string password)
        {
            PlayerPrefs.SetInt("RememberAccount", rememeberUserName ? 1 : 0);
            PlayerPrefs.SetInt("RememberPassword", rememberPassword ? 1 : 0);
            PlayerPrefs.SetString("UserName", rememeberUserName ? userName : null);
            PlayerPrefs.SetString("Password", rememberPassword ? password : null);
            PlayerPrefs.Save();
            // todo: 使用一种更安全的方式存储密码
        }
        #endregion 公有方法

        #region 私有方法

        #region 生命周期

        protected override async void onAwake()
        {
            base.onAwake();
            _logic = new NetworkingLogic(serverBaseUri, version, cacheManager, _ports);
            await _logic.NewKratosClient();

            gameManager.nwLogic = logic;

            logic.onRoomListChange += NetworkingLogic_onRoomListChange;
            logic.OnRoomDataChange += NetworkingLogic_OnRoomDataChange;
            logic.PostRoomPropChange += NetworkingLogic_PostRoomPropChange;
            logic.OnPlayerDataChange += NetworkingLogic_OnPlayerDataChange;
            logic.onGameStart += Logic_onGameStart;
            logic.OnSuggestCardPools += NetworkingLogic_OnSuggestCardPools;
            logic.OnCardPoolsSuggestionAnwsered += NetworkingLogic_OnCardPoolsSuggestionAnwsered;

            // todo: Client断开后退出房间
            gameManager.onGameEnd += Logic_onGameEnd;

            //局域网界面回调注册
            /**
            ui.LANButton.onClick.set(() =>
            {
                _ = displayPanelForLAN();
            });
            //房间界面初始化和回调注册
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
            **/
        }

        protected void Start()
        {
        }

        protected void Update()
        {
            logic.update();
        }

        #endregion 生命周期

        #region 事件回调

        private void Logic_onGameStart()
        {
            gameManager.startRemoteGame(logic.CurrentRoom);
        }
        private void Logic_onGameEnd()
        {
            if (logic.RoomIsValid)
                logic.ClearRoom();
        }

        private void NetworkingLogic_OnCardPoolsSuggestionAnwsered(CardPoolSuggestion suggestion, bool agree)
        {
            onReceiveCardPoolSuggestionAnwser?.Invoke(suggestion, agree);
        }

        private void NetworkingLogic_OnPlayerDataChange(RoomPlayerData[] obj)
        {
            if (obj.Count() < logic.CurrentRoom.maxPlayerCount)
            {
                if (getManager<GameManager>().game is CardEngine game)
                {
                    // todo: 玩家离开，需要发送一个投降命令
                    // var roomPlayer = obj.playerDataList.First(p => !updated.playerList.Exists(p2 => p2.RoomID == p.RoomID));
                    // game.getPlayer(roomPlayer.RoomID).cmdSurrender(game);
                }
            }
            setRoomData(logic.CurrentRoom);
        }

        private void NetworkingLogic_OnRoomDataChange(RoomData obj)
        {
            setRoomData(obj);
        }

        private void NetworkingLogic_onRoomListChange(LobbyRoomDataList obj)
        {
            onRoomListChanged?.Invoke(obj);
        }

        private void NetworkingLogic_OnSuggestCardPools(int playerId, CardPoolSuggestion suggestion)
        {
            onReceiveCardPoolSuggestion?.Invoke(playerId, suggestion);
        }

        private void NetworkingLogic_PostRoomPropChange(string key, object value)
        {
            onRoomPropChanged?.Invoke(key, value);
        }

        #endregion 事件回调
        private async Task startSequence()
        {
            // 更新Panel状态，进入加载模式
            onAutoLogin?.Invoke();
            await logic.AutoLogin();
            onAutoLoginFinished?.Invoke();
        }
        #region UI


        private void setRoomData(RoomData room)
        {
            Debug.Log($"更新房间信息");
            onRoomDataChanged?.Invoke(room);
        }



        #endregion UI

        #endregion 私有方法

        #region 事件
        public event Action<RoomData> onRoomDataChanged;
        public event Action<string, object> onRoomPropChanged;
        public event Action<int, CardPoolSuggestion> onReceiveCardPoolSuggestion;
        public event Action<CardPoolSuggestion, bool> onReceiveCardPoolSuggestionAnwser;
        public event Action<LobbyRoomDataList> onRoomListChanged;
        public event Action onAutoLogin;
        public event Action onAutoLoginFinished;
        #endregion

        #region 属性字段


        public CacheManager cacheManager => getManager<CacheManager>();

        public GameManager gameManager => getManager<GameManager>();

        public NetworkingLogic logic => _logic;

        public IRoomClient roomClient => logic.roomClient;
        private int LANPort => logic.Port;

        // 测试服务器
        private string serverBaseUri
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return _testAccountServerBaseUri;
#elif SNAPSHOT_BUILD
                // 快照服务器
                return _snapshotAccountServerBaseUri;
#else
                return _accountServerBaseUri;
#endif
            }
        }

        private string version => CustomBuilder.BuildInformation.Instance.Version;

        private NetworkingLogic _logic;

        [Header("配置")]
        /// <summary>
        /// 端口
        /// </summary>
        [SerializeField]
        private int[] _ports = { 32900, 32901, 32902 };
        [Tooltip("正式服务器地址")]
        [SerializeField]
        private string _accountServerBaseUri = "https://serv.igsk.fun";
        [Tooltip("快照服务器地址")]
        [SerializeField]
        private string _snapshotAccountServerBaseUri = "https://snap.serv.igsk.fun";
        [Tooltip("测试服务器地址")]
        [SerializeField]
        private string _testAccountServerBaseUri = "https://test.serv.igsk.fun";
        #endregion 属性字段

    }
}