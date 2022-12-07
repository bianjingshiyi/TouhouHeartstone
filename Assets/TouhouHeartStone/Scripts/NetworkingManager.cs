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
using System.Collections.Generic;
using BJSYGameCore.UI;
using System.Text.RegularExpressions;
using NitoriNetwork.Common;
using MukioI18n;
using Controllers;

namespace Game
{
    public class NetworkingManager : Manager
    {
        #region 公共成员
        public Task<ClientUpdateInfo> getLatestVersion()
        {
            return logic.ServerClient.GetLatestUpdateAsync();
        }
        /// <summary>
        /// 向房主提议增加卡池。
        /// </summary>
        /// <param name="cardPools">卡池信息列表。</param>
        public void SuggestCardPools(CardPoolSuggestion suggestion)
        {
            logic.SuggestCardPools(suggestion);
        }
        /// <summary>
        /// 房主同意增加卡池的提议。
        /// </summary>
        /// <param name="playerId">同意的玩家ID。</param>
        public void AnwserCardPoolsSuggestion(int playerId, CardPoolSuggestion suggestion, bool agree)
        {
            logic.AnwserCardPoolsSuggestion(playerId, suggestion, agree);
        }
        public void NotifyRoomPropChanged(string name, object val)
        {
            if (isRoomOwner)
            {
                logic.SetRoomProp(name, val);
            }
        }
        public Task<byte[]> GetResourceAsync(TouhouCardEngine.ResourceType type, string id)
        {
            return logic.GetResourceAsync(type, id);
        }
        public Task UploadResourceAsync(TouhouCardEngine.ResourceType type, string id, byte[] bytes)
        {
            return logic.UploadResourceAsync(type, id, bytes);
        }
        public Task<bool> ResourceExistsAsync(TouhouCardEngine.ResourceType type, string id)
        {
            return logic.ResourceExistsAsync(type, id);
        }
        /// <summary>
        /// 向每个玩家分发卡池。
        /// </summary>
        /// <param name="cardPools">卡池信息列表。</param>
        public void DistributeCardPools(CardPoolInfoPack[] cardPools)
        {
            //TODO
        }
        #endregion
        [Header("配置")]
        [Tooltip("正式服务器地址")]
        [SerializeField]
        string _accountServerBaseUri = "https://serv.igsk.fun";
        [Tooltip("测试服务器地址")]
        [SerializeField]
        string _testAccountServerBaseUri = "https://test.serv.igsk.fun";

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 测试服务器
        string serverBaseUri => _testAccountServerBaseUri;
#else
        string serverBaseUri => _accountServerBaseUri;
#endif

        int LANPort => logic.Port;

        public string[] addresses => Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Select(i => i.ToString() + ":" + LANPort).ToArray();

        private GameManager _gameManager;
        public GameManager gameManager
        {
            get
            {
                if (_gameManager == null) _gameManager = getManager<GameManager>();
                return _gameManager;
            }
        }
        public NetworkingPage ui
        {
            get { return getManager<UIManager>().getObject<NetworkingPage>(); }
        }

        LangManager lang => getManager<LangManager>();

        RoomPanel roomPanel => ui.parent.RoomPanel;

        NetworkingLogic _logic;
        public NetworkingLogic logic
        {
            get
            {
                if (_logic == null)
                {
                    var version = CustomBuilder.BuildInformation.Instance.Version;
                    _logic = new NetworkingLogic(serverBaseUri, version, _ports);
                }

                return _logic;
            }
        }
        // 对MainSceneCtrl的引用，用于调用RoomCtrl的display。
        // 也要调用ChatPanel的display
        private MainSceneCtrl _sceneCtrl;
        protected override void onAwake()
        {
            base.onAwake();

            var mainUI = getManager<UIManager>().getObject<MainMenu>();
            //登录与注册界面初始化
            mainUI.registerPanel.onRegisterClick += async () =>
            {
                // 注册完毕，切换到登录界面
                loadRememberAccount(out var rememberUserName, out var userName, out var rememberPassword, out var password);
                mainUI.loginPanel.SetContent(rememberUserName, userName, rememberPassword, password);
                await ShowPanel(mainUI, Panel.Login);
            };
            mainUI.registerPanel.CaptchaImageButton.onClick.set(async () =>
            {
                mainUI.registerPanel.setCaptcha(await getCaptchaImage());
            });
            mainUI.registerPanel.RegisterButtonBlack.asButton.onClick.AddListener(onRegisterClick);
            mainUI.registerPanel.OnShowEULAClick += async () =>
            {
                var content = await logic.GetEULAAndPrivacyPolicy();
                mainUI.EULAPanel.Show(content);
            };
            mainUI.registerPanel.hide();
            mainUI.loginPanel.OnRegisterClick += async () =>
            {
                await ShowPanel(mainUI, Panel.Register);
            };
            mainUI.loginPanel.CaptchaImgButton.onClick.set(async () =>
            {
                mainUI.loginPanel.setCaptcha(await getCaptchaImage());
            });
            mainUI.loginPanel.LoginButtonBlack.asButton.onClick.set(onLoginClick);
            mainUI.loginPanel.TextButton.onClick.AddListener(async () =>
            {
                await ShowPanel(mainUI, Panel.Recover);
            });
            mainUI.loginPanel.hide();
            mainUI.playerInfoPanel.onLoginButtonClick += async (p) =>
            {
                loadRememberAccount(out var rememberUserName, out var userName, out var rememberPassword, out var password);
                mainUI.loginPanel.SetContent(rememberUserName, userName, rememberPassword, password);
                await ShowPanel(mainUI, Panel.Login);
            };
            mainUI.playerInfoPanel.onLogoutButtonClick += (p) =>
            {
                mainUI.playerInfoPanel.SetPlayerInfo(null);
                _ = logic.logout();
            };
            mainUI.playerInfoPanel.SetPlayerInfo(null);
            mainUI.recoverPanel.onCommitButtonClick += onRecoverCommit;
            mainUI.recoverPanel.onCloseButtonClick += () =>
            {
                mainUI.recoverPanel.hide();
            };
            mainUI.recoverPanel.onBackButtonClick += () =>
            {
                if (mainUI.recoverPanel.CurrentStage)
                    mainUI.recoverPanel.SwitchStage(false);
                else
                    mainUI.recoverPanel.hide();
            };
            mainUI.recoverPanel.onCaptchaButton1Click += async () =>
            {
                mainUI.recoverPanel.captchaImage1.sprite = await getCaptchaImage();
            };
            mainUI.recoverPanel.onCaptchaButton2Click += async () =>
            {
                mainUI.recoverPanel.captchaImage2.sprite = await getCaptchaImage();
            };

            gameManager.nwLogic = logic;

            logic.onRoomListChange += NetworkingLogic_onRoomListChange;
            logic.OnRoomDataChange += NetworkingLogic_OnRoomDataChange;
            logic.OnPlayerDataChange += NetworkingLogic_OnPlayerDataChange;
            logic.onGameStart += Logic_onGameStart;
            logic.OnSuggestCardPools += NetworkingLogic_OnSuggestCardPools;
            logic.OnCardPoolsSuggestionAnwsered += NetworkingLogic_OnCardPoolsSuggestionAnwsered;

            // todo: Client断开后退出房间
            gameManager.onGameEnd += logic.ClearRoom;
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

        #region UI
        enum Panel
        {
            Login,
            Register,
            Recover
        }
        async Task ShowPanel(MainMenu mainUI, Panel panel)
        {
            if (panel != Panel.Login)
                mainUI.loginPanel.hide();
            if (panel != Panel.Register)
                mainUI.registerPanel.hide();
            if (panel != Panel.Recover)
                mainUI.recoverPanel.hide();

            switch (panel)
            {
                case Panel.Login:
                    mainUI.loginPanel.Display();
                    mainUI.loginPanel.setCaptcha(await getCaptchaImage());
                    break;
                case Panel.Register:
                    mainUI.registerPanel.Display();
                    mainUI.registerPanel.setCaptcha(await getCaptchaImage());
                    break;
                case Panel.Recover:
                    mainUI.recoverPanel.Display();
                    mainUI.recoverPanel.setCaptcha(await getCaptchaImage());
                    break;
                default:
                    break;
            }
        }
        #endregion

        protected void Start()
        {
            _ = startSequence();
            // 初始化_sceneCtrl.
            if (_sceneCtrl == null)
            {
                _sceneCtrl = TransformHelper.findComponentInAllScenes<MainSceneCtrl>();
            }
        }
        protected void Update()
        {
            logic.update();
        }

        async Task startSequence()
        {
            await logic.AutoLogin();
            updateAccountPanel();
        }

        #region Logic
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
        void saveRemeberAccount(bool rememeberUserName, string userName, bool rememberPassword, string password)
        {
            PlayerPrefs.SetInt("RememberAccount", rememeberUserName ? 1 : 0);
            PlayerPrefs.SetInt("RememberPassword", rememberPassword ? 1 : 0);
            PlayerPrefs.SetString("UserName", rememeberUserName ? userName : null);
            PlayerPrefs.SetString("Password", rememberPassword ? password : null);
            PlayerPrefs.Save();
            // todo: 使用一种更安全的方式存储密码
        }
        void loadRememberAccount(out bool rememberUserName, out string userName, out bool rememberPassword, out string password)
        {
            rememberUserName = PlayerPrefs.GetInt("RememberAccount", 0) > 0;
            rememberPassword = PlayerPrefs.GetInt("RememberPassword", 0) > 0;
            userName = PlayerPrefs.GetString("UserName", null);
            password = PlayerPrefs.GetString("Password", null);
        }

        bool isRoomOwner => logic.isRoomOwner;

        [TranslateMsg]
        const string CANNOT_CREATE_ROOM = "无法创建房间";

        [TranslateMsg]
        const string CANNOT_JOIN_ROOM = "无法加入房间";

        UIDialog dialog => getManager<UIManager>().getObject<UIDialog>();

        void showCreateRoomException(Exception e)
        {
            dialog.ShowConfirm(lang._(CANNOT_CREATE_ROOM), e.Message, null);
        }

        void showJoinRoomException(Exception e)
        {
            dialog.ShowConfirm(lang._(CANNOT_JOIN_ROOM), e.Message, null);
        }

        /// <summary>
        /// 创建一个房间
        /// </summary>
        /// <returns></returns>
        public async Task createRoom()
        {
            hideAllOtherPage();
            toggleLoadingPanel(true);

            try
            {
                await logic.CreateRoom();

                roomPanel.SetSelfID(logic.GetSelfInfo().id);
                displayRoomPanel();
            }
            catch (Exception e)
            {
                showCreateRoomException(e);
                Debug.LogException(e);
            }
            finally
            {
                toggleLoadingPanel(false);
            }
        }

        public void flushRooms()
        {
            var list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
            list.clearItems();

            logic.ReloadRoomList();
        }

        public async Task joinRoom(string roomID)
        {
            hideAllOtherPage();
            toggleLoadingPanel(true);
            try
            {
                roomPanel.SetSelfID(logic.GetSelfInfo().id);
                await logic.JoinRoom(roomID);
                displayRoomPanel();
            }
            catch (Exception e)
            {
                showJoinRoomException(e);
                Debug.LogException(e);
            }
            finally
            {
                toggleLoadingPanel(false);
            }
        }

        public async Task joinRoom(string addr, int port)
        {
            hideAllOtherPage();
            toggleLoadingPanel(true);

            try
            {
                roomPanel.SetSelfID(logic.GetSelfInfo().id);
                await logic.JoinRoom(addr, port);
                displayRoomPanel();
            }
            catch (Exception e)
            {
                showJoinRoomException(e);
                Debug.LogException(e);
            }
            finally
            {
                toggleLoadingPanel(false);
            }
        }

        [TranslateMsg]
        private const String NETMANAGER_PLAYER2 = "玩家2";

        [TranslateMsg]
        const string INVALID_ADDRESS = "输入的地址格式有误";

        public void quitRoom()
        {
            logic.QuitRoom();
            displayPanel();
        }

        private void Logic_onGameStart()
        {
            gameManager.startRemoteGame(logic.CurrentRoom);
        }

        #endregion
        #region UI
        void updateAccountPanel()
        {
            var playerInfoPanel = getManager<UIManager>().findInstance<PlayerInfoPanel>();
            try
            {
                playerInfoPanel.SetPlayerInfo(logic.UserInfo);
            }
            catch (NetClientException)
            {
                playerInfoPanel.SetPlayerInfo(null);
            }
        }
        void hideAllOtherPage()
        {
            ui.NetworkingPageGroup.display(null);
        }

        void toggleLoadingPanel(bool on)
        {
            ui.NetworkingPageGroup.LANPanel.LoadingPanelImage.gameObject.SetActive(on);
        }

        /// <summary>
        /// 端口
        /// </summary>
        [SerializeField]
        int[] _ports = { 32900, 32901, 32902 };

        /// <summary>
        /// 当前是服务器的Lobby，还是局域网模式
        /// </summary>
        bool isServerMode = false;

        /// <summary>
        /// 根据状态，显示对应的页面
        /// </summary>
        /// <returns></returns>
        void displayPanel()
        {
            LANPanel lanPanel = ui.NetworkingPageGroup.LANPanel;
            ui.NetworkingPageGroup.display(lanPanel);

            lanPanel.LoadingPanelImage.gameObject.SetActive(false);
            lanPanel.FlushRoomButton.onClick.set(flushRooms);
            lanPanel.CreateRoomButtonButtonBlack.asButton.interactable = !logic.RoomIsValid;
            lanPanel.CreateRoomButtonButtonBlack.asButton.onClick.set(async () =>
            {
                await createRoom();
            });
            lanPanel.ReturnBtnButtonBlack.asButton.onClick.AddListener(() =>
            {
                ui.parent.display(ui.parent.MainMenu);
            });

            lanPanel.IP_FieldImage.gameObject.SetActive(!isServerMode);
            lanPanel.ConnectButtonButtonBlack.asButton.onClick.set(async () =>
            {
                try
                {
                    addrParser(lanPanel.IPInputField.text, _ports[0], out string address, out int port);
                    await joinRoom(address, port);
                } 
                catch (UriFormatException)
                {
                    dialog.ShowConfirm(lang._(StringTable.ERROR), lang._(INVALID_ADDRESS), null);
                }
            });
            lanPanel.NameText.text = null;
            lanPanel.IPText.text = null;
            lanPanel.DescText.text = null;

            if (logic.RoomIsValid)
            {
                displayRoomPanel();
            }
            else
            {
                flushRooms();
            }
        }

        /// <summary>
        /// 地址解析
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="defaultPort"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void addrParser(string addr, int defaultPort, out string ip, out int port)
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

        public void displayPanelForLAN()
        {
            isServerMode = false;
            logic.SwitchMode(true);
            displayPanel();
        }
        public void displayPanelForLobby()
        {
            isServerMode = true;
            logic.SwitchMode(false);
            displayPanel();
        }

        [TranslateMsg]
        private const String NETMANAGER_DESCRIBE = "描述";
        [TranslateMsg("参数为玩家名称")]
        private const String NETMANAGER_HOME = "{0}的房间";
        [TranslateMsg("参数为玩家ID")]
        private const String LOCAL_PLAYER_NAME = "玩家{0}";
        private void refreshRoomListItem(RoomListItem item, LobbyRoomData obj)
        {
            string username = "";
            if (isServerMode)
                username = logic.GetUserName(obj.OwnerId);
            else
                username = lang._(LOCAL_PLAYER_NAME, obj.OwnerId);

            item.RoomNameText.text = lang._(NETMANAGER_HOME, username);
            item.asButton.onClick.set(() =>
            {
                LANPanel lanPanel = ui.NetworkingPageGroup.LANPanel;
                if (obj == null)
                {
                    lanPanel.NameText.text = null;
                    lanPanel.IPText.text = null;
                    lanPanel.DescText.text = null;
                }
                else
                {
                    lanPanel.NameText.text = obj.RoomID.ToString();
                    lanPanel.IPText.text = isServerMode ? "???" : obj.IP;
                    lanPanel.DescText.text = lang._(NETMANAGER_DESCRIBE);
                }
            });
            item.ButtonButtonBlack.asButton.onClick.set(async () =>
            {
                await joinRoom(obj.RoomID);
            });
            item.ButtonButtonBlack.asButton.interactable = obj.PlayerCount < obj.MaxPlayerCount;

        }

        private void setRoomPanel(RoomData room)
        {
            Debug.Log($"更新房间信息: {room.toJson()}");
            roomPanel.SetInfo(room);
        }

        /// <summary>
        /// 显示房间页面
        /// </summary>
        private void displayRoomPanel()
        {
            roomPanel.ResetEvents();

            roomPanel.onQuitClick += () =>
            {
                // 不要再quitRoom，onQuitClick本身就会调用quitRoom().
                //quitRoom();
                ui.parent.display(ui);
            };
            roomPanel.onStartClick += () =>
            {
                logic.StartGame();
            };
            roomPanel.onRoomPropUpdate += NotifyRoomPropChanged;
            roomPanel.onPlayerRemove += (id) =>
            {
                if (isRoomOwner)
                {
                    logic.RemovePlayer(id);
                }
            };
            roomPanel.onSetPlayerDeck += (id, cards) =>
            {
                _ = logic.SetPlayerProp(THHRoomPlayerDataHelper.PROP_DECK, cards);
            };
            roomPanel.OnPlayerReadyChange += (id, state) =>
            {
                _ = logic.SetPlayerProp(THHRoomPlayerDataHelper.PROP_READY, state);
            };

            ui.parent.display(roomPanel);
            _sceneCtrl.room.display(logic.CurrentRoom, false);
            //  加载聊天框
            _sceneCtrl.chatCtrl.display(logic.CurrentRoom);
            roomPanel.SetRightText("IP：" + string.Join(", ", addresses));
            roomPanel.SetMode(isServerMode ? RoomPanel.Mode.NetworkingClient : RoomPanel.Mode.NetworkingHost);

            setRoomPanel(logic.CurrentRoom);
        }

        #endregion
        #region 网络事件处理
        private void NetworkingLogic_onRoomListChange(LobbyRoomDataList obj)
        {
            var list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
            list.clearItems();

            foreach (var roomKV in obj)
            {
                var dispItem = list.addItem();
                var room = roomKV.Value;
                refreshRoomListItem(dispItem, room);
            }
        }

        private void NetworkingLogic_OnRoomDataChange(RoomData obj)
        {
            setRoomPanel(obj);
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
            setRoomPanel(logic.CurrentRoom);
        }
        private void NetworkingLogic_OnSuggestCardPools(int playerId, CardPoolSuggestion suggestion)
        {
            roomPanel.ReceiveCardPoolsSuggestion(playerId, suggestion);
        }
        private void NetworkingLogic_OnCardPoolsSuggestionAnwsered(CardPoolSuggestion suggestion, bool agree)
        {
            roomPanel.ReceiveCardPoolsSuggestionAnwser(suggestion, agree);
        }
        #endregion
        #region UI事件处理
        [TranslateMsg]
        private const String NETMANAGER_USERNAME_NULL = "用户名不能为空";
        [TranslateMsg]
        private const String NETMANAGER_EMAIL_NULL = "邮箱不能为空";
        [TranslateMsg]
        private const String NETMANAGER_CHECK_CODE_NULL = "校验码不能为空";
        [TranslateMsg]
        private const String NETMANAGER_NICKNAME_NULL = "昵称不能为空";
        [TranslateMsg]
        private const String NETMANAGER_PASSWORD_ERROR = "两次输入的密码不匹配";
        [TranslateMsg]
        const string NETMANAGER_RECOVER_CODE_NULL = "密码找回代码不能为空";
        [TranslateMsg]
        const string INVALID_USERNAME_OR_PASSWORD = "用户名或密码不正确";
        async void onRegisterClick()
        {
            var ui = getManager<UIManager>().getObject<RegisterPanel>();

            ui.HideMessage();

            var userName = ui.InputFieldDark.asInputField.text;
            var password = ui.Password_InputFieldDark.asInputField.text;
            var repeatPassword = ui.RepeatPassword_InputFieldDark.asInputField.text;
            var email = ui.Mail_InputFieldDark.asInputField.text;
            var nickName = ui.NickName_InputFieldDark.asInputField.text;
            var invite = ui.Invite_InputFieldDark.asInputField.text;
            var captcha = ui.Captcha_InputFieldDark.asInputField.text;

            if (string.IsNullOrEmpty(userName))
            {
                ui.ShowMessage(lang._(NETMANAGER_USERNAME_NULL));
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                ui.ShowMessage(lang._(NETMANAGER_EMAIL_NULL));
                return;
            }
            if (string.IsNullOrEmpty(captcha))
            {
                ui.ShowMessage(lang._(NETMANAGER_CHECK_CODE_NULL));
                return;
            }

            if (string.IsNullOrEmpty(nickName))
            {
                ui.ShowMessage(lang._(NETMANAGER_NICKNAME_NULL));
                return;
            }

            if (password != repeatPassword)
            {
                ui.ShowMessage(lang._(NETMANAGER_PASSWORD_ERROR));
                return;
            }

            try
            {
                await logic.register(new AccountInfo(userName, password, email, nickName, invite), captcha);
                var mainUI = getManager<UIManager>().getObject<MainMenu>();

                loadRememberAccount(out var rememberUserName, out userName, out var rememberPassword, out password);
                mainUI.loginPanel.SetContent(rememberUserName, userName, rememberPassword, password);

                await ShowPanel(mainUI, Panel.Login);
            }
            catch (NetClientException e)
            {
                ui.ShowMessage(e.Message);
                ui.setCaptcha(await getCaptchaImage());
            }
        }
        async void onLoginClick()
        {
            var panel = getManager<UIManager>().getObject<LoginPanel>();
            panel.HideMessage();
            if (string.IsNullOrEmpty(panel.UserName))
            {
                panel.ShowMessage(lang._(NETMANAGER_USERNAME_NULL));
                return;
            }
            if (string.IsNullOrEmpty(panel.Password))
            {
                panel.ShowMessage(lang._(NETMANAGER_CHECK_CODE_NULL));
                return;
            }
            try
            {
                bool success = await logic.login(panel.UserName, panel.Password, panel.Captcha);
                if (success)
                {
                    saveRemeberAccount(
                        panel.RememberAccountUIToggle.asToggle.isOn, panel.UserName,
                        panel.RememberPasswordUIToggle.asToggle.isOn, panel.Password);
                    panel.hide();
                    updateAccountPanel();
                }
                else
                {
                    panel.ShowMessage(lang._(INVALID_USERNAME_OR_PASSWORD));
                    panel.setCaptcha(await getCaptchaImage());
                }
            }
            catch (NetClientException e)
            {
                panel.ShowMessage(e.Message);
                panel.setCaptcha(await getCaptchaImage());
            }
        }

        private async void onRecoverCommit()
        {
            var panel = getManager<UIManager>().getObject<MainMenu>().recoverPanel;
            panel.HideMessage();
            if (!panel.CurrentStage)
            {
                var mail = panel.mailInput.text;
                var captcha = panel.captchaInput1.text;

                if (string.IsNullOrEmpty(mail))
                {
                    panel.ShowMessage(lang._(NETMANAGER_EMAIL_NULL));
                    return;
                }
                if (string.IsNullOrEmpty(captcha))
                {
                    panel.ShowMessage(lang._(NETMANAGER_CHECK_CODE_NULL));
                    return;
                }
                try
                {
                    await logic.ServerClient.RecoverRequestAsync(mail, captcha);
                    panel.SwitchStage(true);
                }
                catch (NetClientException e)
                {
                    panel.ShowMessage(e.Message);
                }
                panel.setCaptcha(await getCaptchaImage());
            }
            else
            {
                var mail = panel.mailInput.text;
                var captcha = panel.captchaInput2.text;
                var password = panel.passwordInput.text;
                var repeatPassword = panel.repeatPasswordInput.text;
                var code = panel.codeInput.text.Trim();

                if (string.IsNullOrEmpty(mail))
                {
                    panel.ShowMessage(lang._(NETMANAGER_EMAIL_NULL));
                    return;
                }
                if (string.IsNullOrEmpty(code))
                {
                    panel.ShowMessage(lang._(NETMANAGER_RECOVER_CODE_NULL));
                    return;
                }
                if (string.IsNullOrEmpty(captcha))
                {
                    panel.ShowMessage(lang._(NETMANAGER_CHECK_CODE_NULL));
                    return;
                }
                if (password != repeatPassword)
                {
                    panel.ShowMessage(lang._(NETMANAGER_PASSWORD_ERROR));
                    return;
                }
                try
                {
                    await logic.ServerClient.RecoverPasswordAsync(mail, code, password, captcha);
                    panel.gameObject.SetActive(false);
                    getManager<UIManager>().getObject<LoginPanel>().display();
                }
                catch (NetClientException e)
                {
                    panel.ShowMessage(e.Message);
                    panel.setCaptcha(await getCaptchaImage());
                }
            }
        }
        #endregion
    }
}