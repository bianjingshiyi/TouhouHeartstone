using TouhouCardEngine;
using NitoriNetwork.Common;
using System.Threading.Tasks;
using TouhouHeartstone;
using System;
using TouhouCardEngine.Interfaces;
using UnityEngine;

namespace Game
{
    public class NetworkingLogic: IClient
    {
        /// <summary>
        /// 服务器的客户端
        /// </summary>
        public ServerClient ServerClient { get; private set; }
        /// <summary>
        /// 本地联网游戏逻辑
        /// </summary>
        ClientLogic localLanLogic;

        public NetworkingLogic(string serverUri, string version, int[] ports = null)
        {
            Debug.Log("Init server uri: " + serverUri);

            ServerClient = new ServerClient(serverUri, version, System.IO.Path.Combine(Application.persistentDataPath, "token"));
            localLanLogic = new ClientLogic("Player.", ports, ServerClient, new ULogger("LAN"));

            localLanLogic.onRoomListChange += LocalLanLogic_onRoomListChange;
            localLanLogic.onGameStart += onGameStartEvtHandler;
            localLanLogic.OnPlayerDataChange += roomPlayerDataChangeEvtHandler;
            localLanLogic.OnRoomDataChange += roomDataChangeEvtHandler;
            localLanLogic.OnReceiveData += roomReceiveEvtHandler;
        }

        public event Func<int, object, Task> onReceive;

        int IClient.id => localLanLogic.localPlayer.id;

        Task<T> IClient.send<T>(T obj)
        {
            return localLanLogic.curNetwork.Send<T>(obj);
        }

        #region 事件代理
        public event Action<LobbyRoomDataList> onRoomListChange;
        private void LocalLanLogic_onRoomListChange(LobbyRoomDataList obj)
        {
            onRoomListChange?.Invoke(obj);
        }

        public event Action<RoomPlayerData[]> OnPlayerDataChange;
        private void roomPlayerDataChangeEvtHandler(RoomPlayerData[] obj)
        {
            OnPlayerDataChange?.Invoke(obj);
        }

        public event Action<RoomData> OnRoomDataChange;
        private void roomDataChangeEvtHandler(RoomData obj)
        {
            OnRoomDataChange?.Invoke(obj);
        }

        private Task roomReceiveEvtHandler(int clientID, object obj)
        {
            return onReceive?.Invoke(clientID, obj);
        }

        public event Action onGameStart;
        private void onGameStartEvtHandler()
        {
            onGameStart?.Invoke();
        }
        #endregion

        /// <summary>
        /// 加入一个房间
        /// </summary>
        /// <param name="roomID"></param>
        /// <returns></returns>
        public async Task JoinRoom(string roomID)
        {
            await localLanLogic.joinRoom(roomID);
            isClient = true;
        }

        /// <summary>
        /// 加入一个房间
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task JoinRoom(string addr, int port)
        {
            await localLanLogic.joinRoom(addr, port);
            isClient = true;
        }

        /// <summary>
        /// 当前房间
        /// </summary>
        public RoomData CurrentRoom => localLanLogic.room;
        /// <summary>
        /// 是客户端吗？
        /// </summary>
        public bool isClient { get; private set; }
        /// <summary>
        /// 当前是否在一个房间里面
        /// </summary>
        public bool RoomIsValid => CurrentRoom != null;
        /// <summary>
        /// 网络端口
        /// </summary>
        public int Port => localLanLogic.port;

        /// <summary>
        /// 创建一个远程房间
        /// </summary>
        /// <returns></returns>
        public async Task CreateRoom()
        {
            await localLanLogic.createOnlineRoom();

            // 初始化房间设置
            var option = new HearthstoneOption();
            await localLanLogic.setRoomProp(nameof(RoomDataPropName.Option.RandomSeed), option.randomSeed);
            await localLanLogic.setRoomProp(nameof(RoomDataPropName.Option.Shuffle), option.shuffle);
            await localLanLogic.setRoomProp(nameof(RoomDataPropName.Option.StartGem), option.startGem);
            await localLanLogic.setRoomProp(nameof(RoomDataPropName.Option.TimeoutForInitReplace), (int)option.timeoutForInitReplace);
            await localLanLogic.setRoomProp(nameof(RoomDataPropName.Option.TimeoutForTurn), (int)option.timeoutForTurn);

            isClient = false;
        }

        /// <summary>
        /// 创建一个本地房间
        /// </summary>
        /// <returns></returns>
        public async Task CreateLocalRoom()
        {
            await localLanLogic.createLocalRoom();
        }

        /// <summary>
        /// 游戏结束，退出并清理房间
        /// </summary>
        public void ClearRoom()
        {
            localLanLogic.quitRoom();
            if (!isClient)
            {
                if (localLanLogic.curNetwork is LANNetworking lan)
                {
                    // 如果是房主，要销毁（关闭）对应的房间
                    lan.DestroyRoom();
                }
            }
        }

        public void QuitRoom()
        {
            localLanLogic.quitRoom();
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        /// <returns></returns>
        public Task StartGame()
        {
            return localLanLogic.curNetwork.GameStart();
        }

        /// <summary>
        /// 请求重载房间列表
        /// </summary>
        public void ReloadRoomList()
        {
            localLanLogic.refreshRoomList();
        }

        public RoomPlayerData GetSelfInfo()
        {
            return localLanLogic.getLocalPlayerData();
        }

        public RoomPlayerData localPlayer => localLanLogic.localPlayer;

        /// <summary>
        /// 周期事件
        /// </summary>
        public void update()
        {
            localLanLogic.update();
        }

        public void SwitchMode(bool isLAN)
        {
            localLanLogic.SwitchMode(isLAN);
        }

        #region RoomMethod

        public Task AddAIPlayer()
        {
            return localLanLogic.addAIPlayer();
        }

        public Task SetRoomProp(string name, object val)
        {
            return localLanLogic.curNetwork.SetRoomProp(name, val);
        }

        public void RemovePlayer(int uid)
        {
            // todo: 移除一个玩家
        }

        public Task SetPlayerProp(string name, object val)
        {
            return localLanLogic.curNetwork.SetPlayerProp(name, val);
        }

        #endregion

        #region ServerClient
        /// <summary>
        /// 当前登录的用户信息
        /// </summary>
        public PublicBasicUserInfo UserInfo => userInfo;
        PublicBasicUserInfo userInfo;
        bool isGuest = false;

        /// <summary>
        /// 判断是否登录（非游客）
        /// </summary>
        public bool AccountIsValid => userInfo != null && !isGuest;

        public bool Logined => userInfo != null;

        /// <summary>
        /// 尝试使用之前登录的用户信息自动登录，如果不成功则尝试使用游客身份登录。
        /// </summary>
        public async Task AutoLogin()
        {
            try
            {
                userInfo = await ServerClient.GetUserInfoAsync();

                // 更新Session，防止Session太旧没更新导致无法连接服务器
                await ServerClient.GetSessionAsync();
                isGuest = false;
            }
            catch (NetClientException e1)
            {
                Debug.Log($"自动登录失败，{ e1 }，尝试使用游客身份登录.");

                try
                {
                    await ServerClient.GuestLoginAsync();
                    userInfo = await ServerClient.GetUserInfoAsync();
                    // 游客不需要手动更新Session，因为在登录的时候默认换取的是Session
                    isGuest = true;
                }
                catch (NetClientException e2)
                {
                    Debug.Log($"游客身份登录失败，{ e2 }.");
                    userInfo = null;
                }
            }
        }

        /// <summary>
        /// 获取许可协议等内容
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetEULAAndPrivacyPolicy()
        {
            var eula = await ServerClient.GetEULAAsync();
            var pp = await ServerClient.GetPrivacyPolicyAsync();
            return eula + "\n" + pp;
        }
        /// <summary>
        /// 从服务器登出，在服务器返回消息之后返回。
        /// </summary>
        /// <returns></returns>
        public async Task logout()
        {
            if (ServerClient == null)
                return;
            await ServerClient.LogoutAsync();
            userInfo = null;
        }

        /// <summary>
        /// 登录指定服务器，在收到服务器的回应之后返回。
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> login(string account, string password, string captcha)
        {
            bool success = await ServerClient.LoginAsync(account, password, captcha);
            if (success)
            {
                userInfo = await ServerClient.GetUserInfoAsync();
            }
            else
            {
                userInfo = null;
            }
            return success;
        }

        /// <summary>
        /// 注册账号，在服务器回应之后返回。
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task register(AccountInfo account, string captcha)
        {
            return ServerClient.RegisterAsync(
                account.userName,
                account.mail,
                account.password,
                account.nickName,
                account.invite,
                captcha
            );
        }
        public Task<byte[]> getCaptchaImage()
        {
            return ServerClient.GetCaptchaImageAsync();
        }
        public string GetUserName(int uid)
        {
            return ServerClient.GetUserInfo(uid).Name;
        }
        #endregion

    }

    [Serializable]
    public class AccountInfo
    {
        public string userName;
        public string password;
        public string mail;
        public string nickName;
        public int uid;
        public string invite;
        public bool guest = false;

        /// <summary>
        /// 登录账号构造器
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="nickName"></param>
        /// <param name="uid"></param>
        /// <param name="guest"></param>
        public AccountInfo(string userName, string password, string nickName, int uid, bool guest = false) : this(userName, password, null, nickName, uid, null)
        {
            this.guest = guest;
        }
        /// <summary>
        /// 注册账号构造器
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="mail"></param>
        /// <param name="nickName"></param>
        public AccountInfo(string userName, string password, string mail, string nickName, string key) : this(userName, password, mail, nickName, 0, key)
        {
        }
        public AccountInfo(string userName, string password, string mail, string nickName, int uid, string key)
        {
            this.userName = userName;
            this.password = password;
            this.mail = mail;
            this.nickName = nickName;
            this.uid = uid;
            this.invite = key;
        }
    }

    public static class RoomDataPropName
    {
        public enum Option
        {
            RandomSeed,
            SortedPlayers,
            Shuffle,
            TimeoutForInitReplace,
            TimeoutForTurn,
            StartGem,
            AllowCustomCards
        }

        [Obsolete]
        public const string OPTION_GAMEOPTION = "option";

        /// <summary>
        /// 兼容性接口。
        /// 
        /// 将 RoomData 转换为 HearthstoneOption
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static HearthstoneOption getOption(this RoomData room)
        {
            return new HearthstoneOption()
            {
                randomSeed = room.getRandomSeed(),
                shuffle = room.getShuffle(),
                sortedPlayers = room.getSortedPlayers(),
                startGem = room.getStartGem(),
                timeoutForInitReplace = room.getTimeoutForInitReplace(),
                timeoutForTurn = room.getTimeoutForTurn(),
                allowCustomCards = room.getAllowCustomCards()
            };
        }

        /// <summary>
        /// 兼容性接口
        /// 
        /// 使用 HearthstoneOption 设置 RoomData 的 Property
        /// </summary>
        /// <param name="room"></param>
        /// <param name="value"></param>
        public static void setOption(this RoomData room, HearthstoneOption value)
        {
            room.setOption(Option.RandomSeed, value.randomSeed);
            room.setOption(Option.SortedPlayers, value.sortedPlayers);
            room.setOption(Option.Shuffle, value.shuffle);
            room.setOption(Option.TimeoutForInitReplace, value.timeoutForInitReplace);
            room.setOption(Option.TimeoutForTurn, value.timeoutForTurn);
            room.setOption(Option.StartGem, value.startGem);
        }

        /// <summary>
        /// 设置指定选项的值
        /// </summary>
        /// <param name="room"></param>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        public static void setOption(this RoomData room, Option prop, object value)
        {
            room.setProp(prop.ToString(), value);
        }

        /// <summary>
        /// 获取指定选项的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="room"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static T getOption<T>(this RoomData room, Option prop)
        {
            if (room.tryGetProp<T>(prop.ToString(), out T data))
                return data;
            return default(T);
        }
        public static int getRandomSeed(this RoomData room)
        {
            return room.getOption<int>(Option.RandomSeed);
        }
        public static bool getShuffle(this RoomData room)
        {
            return room.getOption<bool>(Option.Shuffle);
        }
        public static int[] getSortedPlayers(this RoomData room)
        {
            return room.getOption<int[]>(Option.SortedPlayers);
        }
        public static int getTimeoutForInitReplace(this RoomData room)
        {
            return room.getOption<int>(Option.TimeoutForInitReplace);
        }
        public static int getTimeoutForTurn(this RoomData room)
        {
            return room.getOption<int>(Option.TimeoutForTurn);
        }
        public static int getStartGem(this RoomData room)
        {
            return room.getOption<int>(Option.StartGem);
        }
        public static bool getAllowCustomCards(this RoomData room)
        {
            return room.getOption<bool>(Option.AllowCustomCards);
        }
    }
}