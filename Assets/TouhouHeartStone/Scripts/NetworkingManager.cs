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

namespace Game
{
    public class NetworkingManager : Manager
    {
        #region 公共成员
        public Task<ClientUpdateInfo> getLatestVersion()
        {
            return client.getLatestVersion();
        }
        public Task<string> getNewVersionUrl(string currentVersion)
        {
            return client.getNewVersionUrl(currentVersion);
        }
        #endregion
        [Header("配置")]
        [Tooltip("正式服务器地址")]
        [SerializeField]
        string _accountServerBaseUri = "https://serv.igsk.fun";
        [Tooltip("测试服务器地址")]
        [SerializeField]
        string _testAccountServerBaseUri = "https://test.serv.igsk.fun";

        [SerializeField]
        private HostManager _host;
        public HostManager host
        {
            get
            {
                if (_host == null)
                    _host = this.findInstance<HostManager>();
                return _host;
            }
        }
        public string address
        {
            get { return host.address; }
        }
        public string[] addresses => host.addresses;

        [SerializeField]
        private ClientManager _client;
        public ClientManager client
        {
            get
            {
                if (_client == null)
                    _client = this.findInstance<ClientManager>();
                return _client;
            }
        }
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

        [SerializeField]
        List<RoomInfo> _LANRoomList;
        [SerializeField]
        BJSYGameCore.Timer _LANUpdateTimer = new BJSYGameCore.Timer() { duration = 1 };
        protected override void onAwake()
        {
            base.onAwake();
            var mainUI = getManager<UIManager>().getObject<MainMenu>();
            //登录与注册界面初始化
            mainUI.RegisterPanel.onRegisterClick += async () =>
            {
                mainUI.RegisterPanel.hide();
                loadRememberAccount(out var rememberUserName, out var userName, out var rememberPassword, out var password);
                mainUI.LoginPanel.display(rememberUserName, userName, rememberPassword, password, await getCaptchaImage());
            };
            mainUI.RegisterPanel.CaptchaImageButton.onClick.set(async () =>
            {
                mainUI.RegisterPanel.setCaptach(await getCaptchaImage());
            });
            mainUI.RegisterPanel.RegisterButtonBlack.asButton.onClick.AddListener(onRegisterClick);
            mainUI.RegisterPanel.hide();
            mainUI.LoginPanel.OnRegisterClick += async () =>
            {
                mainUI.RegisterPanel.display(await getCaptchaImage());
                mainUI.LoginPanel.hide();
            };
            mainUI.LoginPanel.CaptchaImgButton.onClick.set(async () =>
            {
                mainUI.LoginPanel.setCaptcha(await getCaptchaImage());
            });
            mainUI.LoginPanel.LoginButtonBlack.asButton.onClick.set(onLoginClick);
            mainUI.LoginPanel.TextButton.onClick.AddListener(async () =>
            {
                mainUI.LoginPanel.hide();
                mainUI.RecoverPanel.Show();
                mainUI.RecoverPanel.captchaImage1.sprite = await getCaptchaImage();
            });
            mainUI.LoginPanel.hide();
            mainUI.PlayerInfoPanel.OnLoginClick += async () =>
            {
                mainUI.RegisterPanel.hide();
                loadRememberAccount(out var rememberUserName, out var userName, out var rememberPassword, out var password);
                mainUI.LoginPanel.display(rememberUserName, userName, rememberPassword, password, await getCaptchaImage());
            };
            mainUI.PlayerInfoPanel.OnLogoutClick += () =>
            {
                mainUI.PlayerInfoPanel.SetPlayerInfo(null);
                _ = client.logout();
            };
            mainUI.PlayerInfoPanel.SetPlayerInfo(null);
            mainUI.RecoverPanel.onCommitButtonClick += onRecoverCommit;
            mainUI.RecoverPanel.onCloseButtonClick += () =>
            {
                mainUI.RecoverPanel.gameObject.SetActive(false);
            };
            mainUI.RecoverPanel.onBackButtonClick += () =>
            {
                if (mainUI.RecoverPanel.CurrentStage)
                    mainUI.RecoverPanel.SwitchStage(false);
                else
                    mainUI.RecoverPanel.gameObject.SetActive(false);
            };
            mainUI.RecoverPanel.onCaptchaButton1Click += async () =>
            {
                mainUI.RecoverPanel.captchaImage1.sprite = await getCaptchaImage();
            };
            mainUI.RecoverPanel.onCaptchaButton2Click += async () =>
            {
                mainUI.RecoverPanel.captchaImage2.sprite = await getCaptchaImage();
            };

            host.logger = new ULogger("Host");
            client.logger = new ULogger("Client");

            client.addInvokeTarget(gameManager);
            gameManager.client = client;
            client.onRoomFound += Client_onRoomFound;
            client.onJoinRoom += Client_onJoinRoom;
            client.onRoomInfoUpdate += Client_onRoomInfoUpdate;

            var gameVersion = CustomBuilder.BuildInformation.Instance.Version;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // 测试服务器
            client.InitServerClient(_testAccountServerBaseUri, gameVersion);
#else
            client.InitServerClient(_accountServerBaseUri, gameVersion);
#endif

            _LANUpdateTimer.start();
            // todo: Client断开后退出房间
            client.onQuitRoom += Client_onQuitRoom;
            gameManager.onGameEnd += onGameEnd;
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
            client.start();
            _ = updateAccountPanel();
        }
        protected void Update()
        {
            if (_LANUpdateTimer.isExpired())
            {
                updateAllRooms();
                _LANUpdateTimer.start();
            }
        }
        private void onGameEnd()
        {
            client.quitRoom();
            if (host != null)
            {
                host.closeRoom();
                host.stop();
            }
        }
        #region Logic
        public async Task<Sprite> getCaptchaImage()
        {
            Texture2D texture = new Texture2D(512, 512);
            try
            {
                texture.LoadImage(await client.getCaptchaImage());
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

        bool isHost = true;

        /// <summary>
        /// 创建一个房间
        /// </summary>
        /// <returns></returns>
        public async Task createRoom()
        {
            isHost = true;
            if (client.account != null)
            {
                //服务器
                currentPlayer = new RoomPlayerInfo()
                {
                    name = client.account.nickName,
                    PlayerID = client.account.uid
                };
                roomPanel.SetSelfID(currentPlayer.PlayerID);

                RoomInfo room = new RoomInfo(currentPlayer.PlayerID);
                room.setOption(new GameOption()
                {
                    randomSeed = (int)DateTime.Now.ToBinary()
                });
                displayLoadingPanel();
                await client.openRoom(room, currentPlayer);
            }
            else
            {
                //局域网
                host.start(_port);
                currentPlayer = new RoomPlayerInfo
                {
                    name = lang._(NETMANAGER_HOMEMASTER) // todo: 替换为真实的玩家名称
                };
                roomPanel.SetSelfID(currentPlayer.PlayerID);

                RoomInfo room = new RoomInfo(currentPlayer.PlayerID);
                room.setOption(new GameOption()
                {
                    randomSeed = (int)DateTime.Now.ToBinary()
                });
                await host.openRoom(room);
                displayLoadingPanel();
                await client.joinRoom(room, currentPlayer);
            }

            // ui.RoomButton.interactable = true;
            // ui.RoomButton.image.color = Color.white;
        }

        [TranslateMsg]
        private const String NETMANAGER_HOMEMASTER = "房主";

        public async Task findRooms()
        {
            if (client.account != null)
            {
                //服务器获取房间列表
                ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.clearItems();
                var rooms = await client.getRooms();
                foreach (var room in rooms)
                {
                    RoomListItem item = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.addItem();
                    refreshRoomListItem(item, room);
                }
            }
            else
                client.findRoom(_port);
        }
        public async Task flushRooms()
        {
            if (client.account != null)
            {
                //服务器获取房间列表
                ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.clearItems();
                var rooms = await client.getRooms();
                foreach (var room in rooms)
                {
                    RoomListItem item = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.addItem();
                    refreshRoomListItem(item, room);
                }
            }
            else
            {
                //局域网寻找房间
                _LANRoomList.Clear();
                ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.clearItems();
                await findRooms();
            }
        }
        public void updateAllRooms()
        {
            foreach (var room in _LANRoomList)
            {
                _ = updateRoomInfo(room);
            }
        }

        RoomPlayerInfo currentPlayer;

        /// <summary>
        /// 客户端加入房间
        /// </summary>
        /// <param name="roomInfo"></param>
        /// <returns></returns>
        public async Task joinRoom(RoomInfo roomInfo)
        {
            if (roomInfo == null)
                throw new ArgumentNullException(nameof(roomInfo));
            isHost = false;

            displayLoadingPanel();
            if (client.account != null)
            {
                //服务器
                currentPlayer = new RoomPlayerInfo
                {
                    name = client.account.nickName,
                    PlayerID = client.account.uid
                };
                roomPanel.SetSelfID(currentPlayer.PlayerID);

                await client.joinRoom(roomInfo, currentPlayer);
            }
            else
            {
                //局域网
                currentPlayer = new RoomPlayerInfo();
                currentPlayer.name = lang._(NETMANAGER_PLAYER2); // todo: 替换为真实的玩家名称
                roomPanel.SetSelfID(currentPlayer.PlayerID);

                await client.joinRoom(roomInfo, currentPlayer);
            }

            // ui.RoomButton.interactable = true;
            // ui.RoomButton.image.color = Color.white;
        }

        [TranslateMsg]
        private const String NETMANAGER_PLAYER2 = "玩家2";
        /// <summary>
        /// [RPC] 开始游戏
        /// </summary>
        public void start()
        {
            getManager<GameManager>().startRemoteGame(client, client.roomInfo);
        }
        public void quitRoom()
        {
            if (host != null)
            {
                if (host.RoomIsValid)
                    host.closeRoom();
                host.stop();
            }
            if (client.roomInfo != null)
                client.quitRoom();
            currentPlayer = null;

            // ui.RoomButton.interactable = false;
            // ui.RoomButton.image.color = Color.gray;
            updateAllRooms();
        }
        #endregion
        #region UI
        async Task updateAccountPanel()
        {
            var playerInfoPanel = getManager<UIManager>().getObject<PlayerInfoPanel>();
            try
            {
                var info = await client.getUserInfo();
                playerInfoPanel.SetPlayerInfo(info);
            }
            catch (NetClientException)
            {
                playerInfoPanel.SetPlayerInfo(null);
            }
        }
        void displayLoadingPanel()
        {
            ui.NetworkingPageGroup.display(null);
        }
        [SerializeField]
        int _port;
        public async Task displayPanelForLAN()
        {
            LANPanel lanPanel = ui.NetworkingPageGroup.LANPanel;
            ui.NetworkingPageGroup.display(lanPanel);
            lanPanel.LoadingPanelImage.gameObject.SetActive(false);
            lanPanel.FlushRoomButton.onClick.set(async () =>
            {
                await flushRooms();
            });
            lanPanel.CreateRoomButtonButtonBlack.asButton.interactable = !host.RoomIsValid;
            lanPanel.CreateRoomButtonButtonBlack.asButton.onClick.set(async () =>
            {
                lanPanel.LoadingPanelImage.gameObject.SetActive(true);
                await createRoom();
            });
            lanPanel.ReturnBtnButtonBlack.asButton.onClick.AddListener(() =>
            {
                ui.parent.display(ui.parent.MainMenu);
            });
            lanPanel.ConnectButtonButtonBlack.asButton.onClick.set(() =>
            {
                int port = _port;
                string address = "";
                var uri = new Uri("http://" + lanPanel.IPInputField.text);

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

                _ = joinRoom(new RoomInfo()
                {
                    ip = address,
                    port = port
                });

                // getManager<UIManager>().getObject<Dialog>().display("输入的地址格式有误", null);
            });
            lanPanel.NameText.text = null;
            lanPanel.IPText.text = null;
            lanPanel.DescText.text = null;
            if (host.RoomIsValid)
            {
                displayRoomPanel();
            }
            else
            {
                await findRooms();
            }
        }
        public async Task displayPanelForLobby()
        {
            LANPanel panel = ui.NetworkingPageGroup.LANPanel;
            ui.NetworkingPageGroup.display(panel);
            panel.LoadingPanelImage.gameObject.SetActive(false);
            panel.FlushRoomButton.onClick.set(async () =>
            {
                await flushRooms();
            });
            panel.CreateRoomButtonButtonBlack.asButton.interactable = !host.RoomIsValid;
            panel.CreateRoomButtonButtonBlack.asButton.onClick.set(async () =>
            {
                panel.LoadingPanelImage.gameObject.SetActive(true);
                await createRoom();
            });
            panel.ReturnBtnButtonBlack.asButton.onClick.AddListener(() =>
            {
                ui.parent.display(ui.parent.MainMenu);
            });
            panel.ConnectButtonButtonBlack.asButton.onClick.set(() =>
            {
                int port = _port;
                string address = "";
                var uri = new Uri("http://" + panel.IPInputField.text);
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
                _ = joinRoom(new RoomInfo()
                {
                    ip = address,
                    port = port
                });
                // getManager<UIManager>().getObject<Dialog>().display("输入的地址格式有误", null);
            });
            //panel.IPInputField.hide();
            //panel.ConnectButtonButtonBlack.hide();
            panel.NameText.text = null;
            panel.IPText.text = null;
            panel.DescText.text = null;
            if (host.RoomIsValid)
            {
                displayRoomPanel();
            }
            else
            {
                await findRooms();
            }
        }
        [TranslateMsg]
        private const String NETMANAGER_DESCRIBE = "描述";
        [TranslateMsg("参数的玩家名称")]
        private const String NETMANAGER_HOME = "{0}的房间";
        private void refreshRoomListItem(RoomListItem item, RoomInfo obj)
        {
            item.RoomNameText.text = lang._(NETMANAGER_HOME, obj.playerList.Find(p => p.PlayerID == obj.OwnerID)?.name);
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
                    lanPanel.NameText.text = obj.id.ToString();
                    lanPanel.IPText.text = obj.ip;
                    lanPanel.DescText.text = lang._(NETMANAGER_DESCRIBE);
                }
            });
            item.ButtonButtonBlack.asButton.onClick.set(() =>
            {
                _ = joinRoom(obj);
            });
            item.ButtonButtonBlack.asButton.interactable = obj.playerList.Count < 2;
        }
        async Task updateRoomInfo(RoomInfo roomInfo)
        {
            RoomInfo newRoomInfo = await client.checkRoomInfo(roomInfo);
            if (newRoomInfo != null)
            {
                int index = _LANRoomList.FindIndex(r => r.isSameRoom(roomInfo));
                if (index >= -1)
                {
                    _LANRoomList[index] = newRoomInfo;
                    RoomList list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
                    RoomListItem item = list.getItems()[index];
                    refreshRoomListItem(item, newRoomInfo);
                }
            }
            else
            {
                int index = _LANRoomList.FindIndex(r => r.isSameRoom(roomInfo));
                if (index > -1)
                {
                    _LANRoomList.RemoveAt(index);
                    RoomList list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
                    list.removeItem(list.getItems()[index]);
                }
            }
        }
        private void setRoomPanel(RoomInfo room)
        {
            roomPanel.SetInfo(room);
        }
        private void displayRoomPanel()
        {
            roomPanel.ResetEvents();

            roomPanel.OnQuitClick += () =>
            {
                quitRoom();
                ui.parent.display(ui);
            };
            roomPanel.OnStartClick += () =>
            {
                client.invokeHost(RPCHelper.GameStart());
            };
            roomPanel.OnRoomInfoUpdate += (info) =>
            {
                if (isHost)
                {
                    client.invokeHost(RPCHelper.RoomPropSet(RoomInfoName.OPTION_GAMEOPTION, info.getOption()));
                }
            };
            roomPanel.OnPlayerRemove += (id) =>
            {
                if (isHost)
                {
                    client.invokeHost(RPCHelper.RemovePlayer(id));
                }
            };
            roomPanel.OnSetPlayerDeck += (id, cards) =>
            {
                currentPlayer.setDeck(cards);
                _ = client.updatePlayerInfo(currentPlayer);
            };
            roomPanel.OnPlayerReadyChange += (id) =>
            {
                currentPlayer.ready = !currentPlayer.ready;
                _ = client.updatePlayerInfo(currentPlayer);
            };

            ui.parent.display(roomPanel);

            roomPanel.SetRightText("IP：" + string.Join(", ", addresses));
            roomPanel.SetMode(isHost ? RoomPanel.Mode.NetworkingHost : RoomPanel.Mode.NetworkingClient);
        }
        #endregion
        #region 网络事件处理
        private void Client_onRoomFound(RoomInfo obj)
        {
            if (obj is RoomInfo tHHRoom)
            {
                int index = _LANRoomList.FindIndex(r => r.id == tHHRoom.id);

                if (index < 0)
                {
                    if (!obj.isSameRoom(host.room))
                    {
                        _LANRoomList.Add(tHHRoom);
                        RoomListItem item = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.addItem();
                        refreshRoomListItem(item, tHHRoom);
                    }
                }
                else
                {
                    if (!obj.isSameRoom(host.room))
                    {
                        _LANRoomList[index] = tHHRoom;
                        RoomListItem item = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList.getItems()[index];
                        refreshRoomListItem(item, tHHRoom);
                    }
                    else
                    {
                        _LANRoomList.RemoveAt(index);
                        RoomList list = ui.NetworkingPageGroup.LANPanel.RoomScrollView.RoomList;
                        list.removeItem(list.getItems()[index]);
                    }
                }
            }
        }
        private void Client_onJoinRoom(RoomInfo obj)
        {
            obj.deserialize();
            displayRoomPanel();
            setRoomPanel(obj);
        }
        private void Client_onRoomInfoUpdate(RoomInfo now, RoomInfo updated)
        {
            if (updated.playerList.Count < now.playerList.Count)
            {
                //有玩家离开了
                var roomPlayer = now.playerList.First(p => !updated.playerList.Exists(p2 => p2.RoomID == p.RoomID));
                if (getManager<GameManager>().game is THHGame game)
                {
                    game.getPlayer(roomPlayer.RoomID).cmdSurrender(game);
                }
            }
            setRoomPanel(updated);
        }
        private void Client_onQuitRoom()
        {
            _ = displayPanelForLAN();
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
                await client.register(new AccountInfo(userName, password, email, nickName, invite), captcha);
                ui.hide();
                loadRememberAccount(out var rememberUserName, out userName, out var rememberPassword, out password);
                getManager<UIManager>().getObject<LoginPanel>().display(rememberUserName, userName, rememberPassword, password, await getCaptchaImage());
            }
            catch (NetClientException e)
            {
                ui.ShowMessage(e.Message);
                ui.setCaptach(await getCaptchaImage());
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
                bool success = await client.login(panel.UserName, panel.Password, panel.Captcha);
                if (success)
                {
                    saveRemeberAccount(
                        panel.RememberAccountUIToggle.asToggle.isOn, panel.UserName,
                        panel.RememberPasswordUIToggle.asToggle.isOn, panel.Password);
                    panel.hide();
                    await updateAccountPanel();
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
            var panel = getManager<UIManager>().getObject<MainMenu>().RecoverPanel;
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
                    await client.ServerClient.RecoverRequestAsync(mail, captcha);
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
                    await client.ServerClient.RecoverPasswordAsync(mail, code, password, captcha);
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
    public static class RoomInfoName
    {
        public const string OPTION_GAMEOPTION = "option";
        public static GameOption getOption(this RoomInfo roomInfo)
        {
            return roomInfo.getProp<GameOption>(OPTION_GAMEOPTION);
        }
        public static void setOption(this RoomInfo roomInfo, GameOption value)
        {
            roomInfo.setProp(OPTION_GAMEOPTION, value);
        }
    }
    public static class RoomPlayerInfoName
    {
        public const string DECK_INTARRAY = "deck";
        public static int[] getDeck(this RoomPlayerInfo playerInfo)
        {
            return playerInfo.getProp<int[]>(DECK_INTARRAY);
        }
        public static bool tryGetDeck(this RoomPlayerInfo playerInfo, out int[] deck)
        {
            return playerInfo.tryGetProp(DECK_INTARRAY, out deck);
        }
        public static void setDeck(this RoomPlayerInfo playerInfo, int[] value)
        {
            playerInfo.setProp(DECK_INTARRAY, value);
        }
    }
}