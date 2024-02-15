using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UnityEngine;

namespace Game
{
    public class NetworkingLogic : IClient
    {
        #region 公有方法
        #region 构造器
        public NetworkingLogic(string serverUri, string version, IResourceProvider resProvider, int[] ports = null)
        {
            Debug.Log("Init server uri: " + serverUri);

            ServerClient = new ServerClient(serverUri, version, System.IO.Path.Combine(Application.persistentDataPath, "token"), new ULogger("ServerAPI"));
            clientLogic = new ClientLogic("Player.", ports, ServerClient, new ULogger("LAN"), resProvider);

            clientLogic.onRoomListChange += LocalLanLogic_onRoomListChange;
            clientLogic.onGameStart += onGameStartEvtHandler;
            clientLogic.OnRoomPlayerDataListChanged += roomPlayerDataChangeEvtHandler;
            clientLogic.OnRoomDataChange += roomDataChangeEvtHandler;
            clientLogic.PostRoomPropChange += roomPropChangeEvtHandler;
            clientLogic.OnReceiveData += roomReceiveEvtHandler;
            clientLogic.OnRecvChat += roomReceiveChatEvtHandler;
            clientLogic.OnSuggestCardPools += roomReceiveCardPoolSuggestionHandler;
            clientLogic.OnCardPoolsSuggestionAnwsered += roomReceiveCardPoolSuggestionAnwserHandler;
        }
        #endregion

        #region 用户登录
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
                Debug.Log($"自动登录失败: {e1}");
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
            await ServerClient.LogoutKratos();
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
        public async Task<bool> login(string email, string password)
        {
            string sessionToken = await ServerClient.LoginViaPassword(email, password);
            if (!string.IsNullOrEmpty(sessionToken))
            {
                if (await ServerClient.LoginByKratosAsync(sessionToken))
                {
                    userInfo = await ServerClient.GetUserInfoAsync();
                    isGuest = false;
                    return true;
                }
            }
            userInfo = null;
            return false;
        }

        /// <summary>
        /// 注册账号，在服务器回应之后返回。
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task register(AccountInfo account)
        {
            return ServerClient.RegisterViaPassword(
                account.mail,
                account.password,
                account.nickName
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

        /// <summary>
        /// 以游客身份登录
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GuestLogin()
        {
            bool success = await ServerClient.GuestLoginAsync();
            if (success)
            {
                userInfo = await ServerClient.GetUserInfoAsync();
                // 游客不需要手动更新Session，因为在登录的时候默认换取的是Session
                isGuest = true;
            }
            else
            {
                userInfo = null;
            }

            return success;
        }

        /// <summary>
        /// 修改玩家信息
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="oldPass">旧密码</param>
        /// <param name="newPass">新密码</param>
        /// <returns></returns>
        public async Task ChangeUserInfo(string nick, string newPass)
        {
            await ServerClient.ChangeUserInfoAsync(nick);
            await ServerClient.ChangePassword(newPass);
            userInfo = await ServerClient.GetUserInfoAsync();
        }


        public RoomPlayerData GetSelfInfo()
        {
            return clientLogic.getLocalPlayerData();
        }
        #endregion 用户登录

        #region 房间相关
        public void SwitchMode(bool isLAN)
        {
            clientLogic.SwitchMode(isLAN);
        }

        public Task AddAIPlayer()
        {
            return clientLogic.addAIPlayer();
        }

        public void RemovePlayer(int uid)
        {
            // todo: 移除一个玩家
        }

        /// <summary>
        /// 加入一个房间
        /// </summary>
        /// <param name="roomID"></param>
        /// <returns></returns>
        public async Task JoinRoom(string roomID, string password = "")
        {
            await clientLogic.joinRoom(roomID, password);
            isInitialRoomOwner = false;
        }

        /// <summary>
        /// 加入一个房间
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task JoinRoom(string addr, int port, string password = "")
        {
            await clientLogic.joinRoom(addr, port, password);
            isInitialRoomOwner = false;
        }

        /// <summary>
        /// 创建一个远程房间
        /// </summary>
        /// <returns></returns>
        public async Task CreateRoom(string name = "", string password = "")
        {
            await clientLogic.createOnlineRoom(name, password);

            // 初始化房间设置
            var option = new HearthstoneOption();
            await roomClient.SetRoomPropBatch(new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>(nameof(RoomDataPropName.Option.RandomSeed), option.randomSeed),
                new KeyValuePair<string, object>(nameof(RoomDataPropName.Option.Shuffle), option.shuffle),
                new KeyValuePair<string, object>(nameof(RoomDataPropName.Option.StartGem), option.startGem),
                new KeyValuePair<string, object>(nameof(RoomDataPropName.Option.TimeoutForInitReplace), (int)option.timeoutForInitReplace),
                new KeyValuePair<string, object>(nameof(RoomDataPropName.Option.TimeoutForTurn), (int)option.timeoutForTurn),
                new KeyValuePair<string, object>(nameof(RoomDataPropName.Option.CardPools), option.cardPools),
            });
            isInitialRoomOwner = true;
        }

        /// <summary>
        /// 创建一个本地房间
        /// </summary>
        /// <returns></returns>
        public async Task CreateLocalRoom(string playerName)
        {
            await clientLogic.createLocalRoom(playerName);
        }

        /// <summary>
        /// 游戏结束，退出并清理房间
        /// </summary>
        public void ClearRoom()
        {
            QuitRoom();

            // 局域网的主机要销毁（关闭）对应的房间
            if (clientLogic.curNetwork is LANNetworking lan && isInitialRoomOwner)
            {
                lan.DestroyRoom();
            }
        }

        public void QuitRoom()
        {
            localPlayer.propDict.Clear();
            clientLogic.quitRoom();
        }
        /// <summary>
        /// 开始游戏
        /// </summary>
        /// <returns></returns>
        public Task StartGame()
        {
            return clientLogic.curNetwork.GameStart();
        }

        /// <summary>
        /// 请求重载房间列表
        /// </summary>
        public void ReloadRoomList()
        {
            clientLogic.refreshRoomList();
        }
        #endregion
        /// <summary>
        /// 周期事件
        /// </summary>
        public void update()
        {
            clientLogic.update();
        }
        public async Task<bool> NewKratosClient()
        {
            return await ServerClient.NewKratosClient();
        }
        #endregion
        #region 私有方法
        #region 事件回调
        private void LocalLanLogic_onRoomListChange(LobbyRoomDataList obj)
        {
            onRoomListChange?.Invoke(obj);
        }

        private void roomPlayerDataChangeEvtHandler(RoomPlayerData[] obj)
        {
            OnPlayerDataChange?.Invoke(obj);
        }

        private void roomDataChangeEvtHandler(RoomData obj)
        {
            OnRoomDataChange?.Invoke(obj);
        }

        private void roomPropChangeEvtHandler(string key, object value)
        {
            PostRoomPropChange?.Invoke(key, value);
        }

        private Task roomReceiveEvtHandler(int clientID, byte[] data)
        {
            var obj = MessagePack.MessagePackSerializer.Deserialize<INetworkData>(data);
            return onReceive?.Invoke(clientID, obj);
        }

        private void onGameStartEvtHandler()
        {
            onGameStart?.Invoke();
        }

        private void roomReceiveChatEvtHandler(ChatMsg obj)
        {
            OnRecvChat?.Invoke(obj);
        }

        private void roomReceiveCardPoolSuggestionHandler(int playerId, CardPoolSuggestion suggestion)
        {
            OnSuggestCardPools?.Invoke(playerId, suggestion);
        }

        private void roomReceiveCardPoolSuggestionAnwserHandler(CardPoolSuggestion suggestion, bool agree)
        {
            OnCardPoolsSuggestionAnwsered?.Invoke(suggestion, agree);
        }
        #endregion
        #region 接口实现
        async Task<T> IClient.send<T>(T obj)
        {
            if (obj is INetworkData data)
            {
                var bytes = MessagePack.MessagePackSerializer.Serialize(data);
                var recv = await clientLogic.curNetwork.Send(bytes);

                return (T)MessagePack.MessagePackSerializer.Deserialize<INetworkData>(recv);
            }

            throw new ArgumentException("要发送接收的数据必须是INetworkData");
        }
        #endregion
        #endregion
        #region 事件
        public event Action<RoomPlayerData[]> OnPlayerDataChange;
        public event Action<RoomData> OnRoomDataChange;
        public event Action<string, object> PostRoomPropChange;
        public event Action onGameStart;
        public event Action<ChatMsg> OnRecvChat;
        public event Action<int, CardPoolSuggestion> OnSuggestCardPools;
        public event Action<CardPoolSuggestion, bool> OnCardPoolsSuggestionAnwsered;
        public event Func<int, object, Task> onReceive;
        public event Action<LobbyRoomDataList> onRoomListChange;
        #endregion
        #region 属性字段
        /// <summary>
        /// 当前房间
        /// </summary>
        public RoomData CurrentRoom => clientLogic.room;
        /// <summary>
        /// 是局域网的房主吗
        /// </summary>
        public bool isLanHost => clientLogic.curNetwork is LANNetworking && isRoomOwner;
        /// <summary>
        /// 是否为初始的房主
        /// </summary>
        public bool isInitialRoomOwner { get; private set; }
        /// <summary>
        /// 是否为房主
        /// </summary>
        public bool isRoomOwner => clientLogic.isRoomOwner;
        /// <summary>
        /// 当前是否在一个房间里面
        /// </summary>
        public bool RoomIsValid => CurrentRoom != null;
        /// <summary>
        /// 网络端口
        /// </summary>
        public int Port => clientLogic.port;
        public RoomPlayerData localPlayer => clientLogic.localPlayer;
        /// <summary>
        /// 当前登录的用户信息
        /// </summary>
        public PublicBasicUserInfo UserInfo => userInfo;
        /// <summary>
        /// 判断是否登录（非游客）
        /// </summary>
        public bool AccountIsValid => userInfo != null && !isGuest;
        public bool Logined => userInfo != null;
        /// <summary>
        /// 服务器的客户端
        /// </summary>
        public ServerClient ServerClient { get; private set; }
        public IRoomClient roomClient => clientLogic;
        PublicBasicUserInfo userInfo;
        /// <summary>
        /// 是否为游客状态
        /// </summary>
        bool isGuest { get; set; }
        int IClient.id => localPlayer.id;
        /// <summary>
        /// 本地联网游戏逻辑
        /// </summary>
        ClientLogic clientLogic;
        #endregion

    }

    [Serializable]
    public class AccountInfo
    {
        public string password;
        public string mail;
        public string nickName;
        public int uid;
        public bool guest = false;

        /// <summary>
        /// 登录账号构造器
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="nickName"></param>
        /// <param name="uid"></param>
        /// <param name="guest"></param>
        public AccountInfo(string password, string nickName, int uid, bool guest = false) : this(password, null, nickName, uid)
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
        public AccountInfo(string password, string mail, string nickName) : this(password, mail, nickName, 0)
        {
        }
        public AccountInfo(string password, string mail, string nickName, int uid)
        {
            this.password = password;
            this.mail = mail;
            this.nickName = nickName;
            this.uid = uid;
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
            AllowCustomCards,
            CardPools
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
                cardPools = room.getCardPools()
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
            room.setOption(Option.TimeoutForInitReplace, (int)value.timeoutForInitReplace);
            room.setOption(Option.TimeoutForTurn, (int)value.timeoutForTurn);
            room.setOption(Option.StartGem, value.startGem);
            room.setOption(Option.CardPools, value.cardPools);
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
        public static CardPoolInfoPack[] getCardPools(this RoomData roomData)
        {
            if (roomData.getOption<CardPoolInfoPack[]>(Option.CardPools) is CardPoolInfoPack[] pools)
            {
                return pools;
            }
            return new CardPoolInfoPack[0];
        }
        public static void setRoomName(this RoomData room, string name)
        {
            room.setProp(RoomData.PROP_ROOM_NAME, name);
        }
        public static string getRoomName(this RoomData room)
        {
            if (room.tryGetProp(RoomData.PROP_ROOM_NAME, out string value))
            {
                return value;
            }
            return string.Empty;
        }
        public static void setRoomPassword(this RoomData roomData, string password)
        {
            roomData.setProp(RoomData.PROP_ROOM_PASSWORD, password);
        }
        public static string getRoomPassword(this RoomData roomData)
        {
            if (roomData.tryGetProp(RoomData.PROP_ROOM_PASSWORD, out string value))
            {
                return value;
            }
            return string.Empty;
        }
    }
}