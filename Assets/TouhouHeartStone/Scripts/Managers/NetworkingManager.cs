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
    public partial class NetworkingManager : Manager
    {
        #region 公有方法
        public async Task start()
        {
            await _logic.NewKratosClient();
            setLanguage(main.lang.getLanguage());
            _ = startSequence();
        }
        public bool initCookie()
        {
            try
            {
                // Unity和Mono的新版本有Bug，在主机名内包含多字节字符时CookieContainer构造方法会出错。
                _logic.initCookie();
                return true;
            }
            catch (Exception e)
            {
                var title = main.lang._(StringTable.ERROR);
                var msg = e.Message;
                main.ui.dialog.ShowConfirm(title, msg);
                Debug.LogException(e);
                return false;
            }

        }
        public void  setLanguage(string language)
        {
            logic.setLanguage(language.Replace('-', '_').ToLower());
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
        /// <summary>
        /// 开始远程游戏。
        /// </summary>
        public void StartRemoteGame()
        {
            logic.StartGame();
        }
        public bool checkExceptionUnauthorized(Exception e)
        {
            return e is NetClientException netException && netException.StatusCode == System.Net.HttpStatusCode.Unauthorized;
        }

        #region 房间相关
        /// <summary>
        /// 创建一个房间
        /// </summary>
        /// <returns></returns>
        public Task createRoom(string name = "", string password = "")
        {
            return logic.CreateRoom(name, password);
        }
        /// <summary>
        /// 创建本地房间
        /// </summary>
        /// <returns></returns>
        public Task createLocalRoom(string playerName)
        {
            return logic.CreateLocalRoom(playerName);
        }
        public Task joinRoom(string roomID, string password = "")
        {
            return logic.JoinRoom(roomID, password);
        }
        public Task joinRoom(string addr, int port, string password = "")
        {
            return logic.JoinRoom(addr, port, password);
        }
        #endregion

        #region 地址相关
        /// <summary>
        /// 设置服务器地址
        /// </summary>
        /// <param name="addr"></param>
        public async Task SetServerAddr(string addr)
        {
            logic.setServerAddress(addr);
            await start();
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
        #endregion

        #region 账号相关
        public Task updateUserInfo()
        {
            return logic.updateUserInfo();
        }
        public void clearUserInfo()
        {
            logic.clearUserInfo();
        }
        public async Task register(AccountInfo accountInfo)
        {
            await handleKratosInitException();
            await logic.register(accountInfo);
        }
        public async Task<bool> login(string email, string password)
        {
            await handleKratosInitException();
            return await logic.login(email, password);
        }
        public Task<bool> guestLogin()
        {
            return logic.GuestLogin();
        }
        public async Task logout()
        {
            await logic.logout();
        }
        public async Task<bool> recoverRequest(string mail)
        {
            await handleKratosInitException();
            return await logic.recoverRequest(mail);
        }
        public Task changeUserNickname(string nickname)
        {
            return logic.ChangeUserNickname(nickname);
        }
        public async Task changeUserPassword(string newPass)
        {
            await handleKratosInitException();
            await logic.ChangeUserPassword(newPass);
        }
        public void loadRememberAccount(out bool rememberUserName, out string userName)
        {
            rememberUserName = PlayerPrefs.GetInt("RememberAccount", 0) > 0;
            userName = rememberUserName ? PlayerPrefs.GetString("UserName", null) : null;
        }
        public void saveRememberAccount(bool rememeberUserName)
        {
            PlayerPrefs.SetInt("RememberAccount", rememeberUserName ? 1 : 0);
        }
        public void saveRememberedAccount(string userName)
        {
            PlayerPrefs.SetString("UserName", userName);
        }
        public PublicBasicUserInfo getUserInfo(int uid)
        {
            return logic.GetUserInfo(uid);
        }
        #endregion

        #region Steam帐号相关
        public async Task steamRegister(string email, string nickname, string language)
        {
            await handleKratosInitException();
            await logic.steamRegister(email, nickname, language);
        }
        public async Task<bool> steamLogin()
        {
            await handleKratosInitException();
            return await logic.steamLogin();
        }
        public async Task<bool> unlinkSteam()
        {
            await handleKratosInitException();
            return await logic.unlinkSteam();
        }
        public async Task<bool> linkSteam()
        {
            await handleKratosInitException();
            return await logic.linkSteam();
        }
        public async Task<bool> isAccountSteamLinked()
        {
            if (!await validateKratos())
                return false;
            return await logic.isAccountSteamLinked();
        }
        #endregion

        #region 创意工坊
        public Task<WorkshopCardPoolInfo[]> getWorkshopUserCardPools(int uid, int page, int limit, string sorting)
        {
            return logic.getWorkshopUserCardPool(uid, page, limit, sorting);
        }
        public Task<WorkshopCardPoolInfo[]> searchWorkshopPageCardPools(int page, int limit, string keywords, string sorting)
        {
            return logic.getWorkshopPageCardPool(page, limit, keywords, sorting);
        }
        public Task<WorkshopCardPoolInfo> getWorkshopCardPool(long id, int version)
        {
            return logic.getWorkshopCardPool(id, version);
        }
        public Task<byte[]> getWorkshopResource(string type, string id)
        {
            return logic.getWorkshopResource(type, id);
        }
        public Task<string[]> uploadWorkshopCardPool(string desc, byte[] data)
        {
            return logic.uploadWorkshopCardPool(desc, data);
        }
        public Task uploadWorkshopResource(string type, string id, byte[] data)
        {
            return logic.uploadWorkshopResource(type, id, data);
        }
        public Task<bool> deleteWorkshopCardPool(long id, int version)
        {
            return logic.deleteWorkshopCardPool(id, version);
        }
        public Task<WorkshopCardPoolReview[]> getWorkshopCardPoolReviews(long id, int version)
        {
            return logic.getWorkshopCardPoolReviews(id, version);
        }
        public Task<string> getWorkshopEULA()
        {
            return logic.getWorkshopEULA();
        }
        public Task<WorkshopCardPoolComment[]> getWorkshopCardPoolComments(long cardPoolId, int offset, int count)
        {
            return logic.getWorkshopCardPoolComments(cardPoolId, offset, count);
        }
        public Task postWorkshopCardPoolComment(long cardPoolId, string comment)
        {
            return logic.postWorkshopCardPoolComment(cardPoolId, comment);
        }
        public Task<WorkshopCardComment[]> getWorkshopCardComments(long cardPoolId, int cardId, int offset, int count)
        {
            return logic.getWorkshopCardComments(cardPoolId, cardId, offset, count);
        }
        public Task postWorkshopCardComment(long cardPoolId, int cardId, string comment)
        {
            return logic.postWorkshopCardComment(cardPoolId, cardId, comment);
        }
        #endregion

        #region 卡牌评分
        public Task<WorkshopCardPoolRating> getCardPoolRating(long id)
        {
            return logic.getCardPoolRating(id);
        }
        public Task rateCardPoolCards(long id, string parameters)
        {
            return logic.rateCardPoolCards(id, parameters);
        }
        public Task<WorkshopCardPoolRating> getCardRating(long poolId, int id)
        {
            return logic.getCardRating(poolId, id);
        }
        public Task setCardRating(long poolId, int id, bool value)
        {
            return logic.setCardRating(poolId, id, value);
        }
        #endregion

        #endregion 公有方法

        #region 私有方法

        #region 生命周期

        protected override void onAwake()
        {
            base.onAwake();
            _logic = new NetworkingLogic(serverBaseUri, version, cacheManager, _ports);

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
            bool autoLoginSuccess = false;
            if (await validateKratos())
            {
                autoLoginSuccess = await logic.AutoLogin();
            }
            onAutoLoginFinished?.Invoke(autoLoginSuccess);
        }
        #region UI


        private void setRoomData(RoomData room)
        {
            Debug.Log($"更新房间信息");
            onRoomDataChanged?.Invoke(room);
        }



        #endregion UI
        private async Task<bool> validateKratos()
        {
            if (logic.ServerClient == null)
                return false;
            if (logic.ServerClient.Kratos != null)
                return true;
            return await logic.NewKratosClient();
        }
        private async Task handleKratosInitException()
        {
            if (!await validateKratos())
            {
                var msg = main.lang._(NETWORK_KRATOS_INIT_FAILED);
                throw new NetClientException(msg);
            }
        }

        #endregion 私有方法

        #region 事件
        public event Action<RoomData> onRoomDataChanged;
        public event Action<string, object> onRoomPropChanged;
        public event Action<int, CardPoolSuggestion> onReceiveCardPoolSuggestion;
        public event Action<CardPoolSuggestion, bool> onReceiveCardPoolSuggestionAnwser;
        public event Action<LobbyRoomDataList> onRoomListChanged;
        public event Action onAutoLogin;
        public event Action<bool> onAutoLoginFinished;
        #endregion

        #region 属性字段

        public MainManager main => getManager<MainManager>();
        public CacheManager cacheManager => main.cache;

        public GameManager gameManager => main.game;

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
        [TranslateMsg("服务器初始化失败时的错误提示")]
        public const string NETWORK_KRATOS_INIT_FAILED = "网络初始化失败，请检查网络连接";

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