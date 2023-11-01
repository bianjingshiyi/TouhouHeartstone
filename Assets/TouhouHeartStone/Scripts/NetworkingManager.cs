using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BJSYGameCore;
using BJSYGameCore.UI;
using Controllers;
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
        public async Task createRoom(string name = "", string password = "")
        {
            hideAllOtherPage();
            toggleLoadingPanel(true);

            try
            {
                await logic.CreateRoom(name, password);

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

        public void flushRooms()
        {
            var list = lanPanel.RoomScrollView.RoomList;
            list.clearItems();

            logic.ReloadRoomList();
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

        public async Task joinRoom(string roomID, string password = "")
        {
            hideAllOtherPage();
            toggleLoadingPanel(true);
            try
            {
                await logic.JoinRoom(roomID, password);
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

        public async Task joinRoom(string addr, int port, string password = "")
        {
            hideAllOtherPage();
            toggleLoadingPanel(true);

            try
            {
                await logic.JoinRoom(addr, port, password);
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

        public void quitRoom()
        {
            logic.QuitRoom();
            displayPanel();
        }

        /// <summary>
        /// 设置服务器地址
        /// </summary>
        /// <param name="addr"></param>
        public void SetServerAddr(string addr)
        {
            logic.ServerClient.SetClientBaseUri(addr);
            _ = startSequence();
        }

        /// <summary>
        /// 开始远程游戏。
        /// </summary>
        public void StartRemoteGame()
        {
            logic.StartGame();
        }

        #endregion 公有方法

        #region 私有方法

        #region 生命周期

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
            mainUI.loginPanel.GuestLoginBtn.onClick.set(onGuestLoginClick);
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
            mainUI.playerInfoPanel.onEditBtnClick += (p) =>
            {
                mainUI.playerInfoEditPanel.Show();
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
            mainUI.playerInfoEditPanel.onClickConfirmBtn += async () =>
            {
                await onChangeUserInfo(mainUI.playerInfoEditPanel.Nickname, mainUI.playerInfoEditPanel.OldPass, mainUI.playerInfoEditPanel.NewPass, mainUI.playerInfoEditPanel.RepeatPass);
            };

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

            lanPanel.onConfirmPassword += onConfirmRoomPassword;
            lanPanel.onCancelPassword += onCancelRoomPassword;
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
            roomCtrl.ReceiveCardPoolsSuggestionAnwsered(suggestion, agree);
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
            var list = lanPanel.RoomScrollView.RoomList;
            list.clearItems();

            foreach (var roomKV in obj)
            {
                var dispItem = list.addItem();
                var room = roomKV.Value;
                refreshRoomListItem(dispItem, room);
            }
        }

        private void NetworkingLogic_OnSuggestCardPools(int playerId, CardPoolSuggestion suggestion)
        {
            roomCtrl.ReceiveCardPoolsSuggested(playerId, suggestion);
        }

        private void NetworkingLogic_PostRoomPropChange(string key, object value)
        {
            roomCtrl.ReceiveRoomPropChanged(key, value);
        }

        #region UI事件处理

        private void onCancelRoomPassword()
        {
            lanPanel.HidePasswordDialog();
            _enteringRoomAddress = null;
            _enteringRoomPort = 0;
            _enteringRoomID = null;
        }

        private async Task onChangeUserInfo(string nickname, string oldPass, string newPass, string repeatNewPass)
        {
            var panel = getManager<UIManager>().getObject<MainMenu>().playerInfoEditPanel;
            panel.HideMessage();

            if (newPass != repeatNewPass)
            {
                panel.ShowMessage(lang._(NETMANAGER_PASSWORD_ERROR));
                return;
            }

            try
            {
                await logic.ChangeUserInfo(nickname, oldPass, newPass);
                updateAccountPanel();
                panel.Hide();
            }
            catch (NetClientException e)
            {
                Debug.Log(e);
                panel.ShowMessage(e.Message);
            }
        }

        private async void onConfirmRoomPassword(string password)
        {
            lanPanel.HidePasswordDialog();
            if (_enteringRoomID != null)
            {
                await joinRoom(_enteringRoomID, password);
            }
            else
            {
                await joinRoom(_enteringRoomAddress, _enteringRoomPort, password);
            }
            _enteringRoomAddress = null;
            _enteringRoomPort = 0;
            _enteringRoomID = null;
        }

        private async void onGuestLoginClick()
        {
            var panel = getManager<UIManager>().getObject<LoginPanel>();
            panel.HideMessage();
            try
            {
                bool success = await logic.GuestLogin();
                if (success)
                {
                    panel.hide();
                    updateAccountPanel();
                }
                else
                {
                    panel.ShowMessage(lang._(GUEST_LOGIN_FAILED));
                }
            }
            catch (NetClientException e)
            {
                Debug.Log(e);
                panel.ShowMessage(e.Message);
            }
        }

        private async void onLoginClick()
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
                Debug.Log(e);
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
                    Debug.Log(e);
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
                    Debug.Log(e);
                    panel.ShowMessage(e.Message);
                    panel.setCaptcha(await getCaptchaImage());
                }
            }
        }

        private async void onRegisterClick()
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

        #endregion UI事件处理

        #endregion 事件回调

        private async Task autoLogin()
        {
            // 更新Panel状态，进入加载模式
            var playerInfoPanel = getManager<UIManager>().findInstance<PlayerInfoPanel>();
            playerInfoPanel?.EnterAutoLoginState();

            await logic.AutoLogin();
        }

        private async Task ShowPanel(MainMenu mainUI, Panel panel)
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

        private async Task startSequence()
        {
            await autoLogin();
            updateAccountPanel();
        }

        #region Logic

        private void loadRememberAccount(out bool rememberUserName, out string userName, out bool rememberPassword, out string password)
        {
            rememberUserName = PlayerPrefs.GetInt("RememberAccount", 0) > 0;
            rememberPassword = PlayerPrefs.GetInt("RememberPassword", 0) > 0;
            userName = PlayerPrefs.GetString("UserName", null);
            password = PlayerPrefs.GetString("Password", null);
        }

        private void saveRemeberAccount(bool rememeberUserName, string userName, bool rememberPassword, string password)
        {
            PlayerPrefs.SetInt("RememberAccount", rememeberUserName ? 1 : 0);
            PlayerPrefs.SetInt("RememberPassword", rememberPassword ? 1 : 0);
            PlayerPrefs.SetString("UserName", rememeberUserName ? userName : null);
            PlayerPrefs.SetString("Password", rememberPassword ? password : null);
            PlayerPrefs.Save();
            // todo: 使用一种更安全的方式存储密码
        }

        private void showCreateRoomException(Exception e)
        {
            dialog.ShowConfirm(lang._(CANNOT_CREATE_ROOM), e.Message, null);
        }

        private void showJoinRoomException(Exception e)
        {
            dialog.ShowConfirm(lang._(CANNOT_JOIN_ROOM), e.Message, null);
        }

        #endregion Logic

        #region UI

        /// <summary>
        /// 地址解析
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="defaultPort"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        private void addrParser(string addr, int defaultPort, out string ip, out int port)
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

        /// <summary>
        /// 根据状态，显示对应的页面
        /// </summary>
        /// <returns></returns>
        private void displayPanel()
        {
            ui.NetworkingPageGroup.display(lanPanel);

            lanPanel.LoadingPanelImage.gameObject.SetActive(false);
            lanPanel.FlushRoomButton.onClick.set(flushRooms);
            lanPanel.CreateRoomButtonButtonBlack.asButton.interactable = !logic.RoomIsValid;
            lanPanel.CreateRoomButtonButtonBlack.asButton.onClick.set(async () =>
            {
                string name = lang._(NETMANAGER_HOME, logic.GetSelfInfo().name);
                await createRoom(name);
            });
            lanPanel.ReturnBtnButtonBlack.asButton.onClick.AddListener(() =>
            {
                ui.parent.display(ui.parent.MainMenu);
            });

            lanPanel.IP_FieldImage.gameObject.SetActive(!isServerMode);
            lanPanel.ConnectButtonButtonBlack.asButton.onClick.set(() =>
            {
                try
                {
                    addrParser(lanPanel.IPInputField.text, _ports[0], out string address, out int port);
                    //if (RoomNeedsPassword(address, port))
                    //{
                    //}
                    //else
                    //{
                    //await joinRoom(address, port);
                    //}
                    lanPanel.ShowPasswordDialog();
                    _enteringRoomAddress = address;
                    _enteringRoomPort = port;
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
        /// 显示房间页面
        /// </summary>
        private void displayRoomPanel()
        {
            var roomCtrl = _sceneCtrl.room;
            roomCtrl.ResetEvents();

            roomCtrl.onQuitClick += () =>
            {
                // 不要再quitRoom，onQuitClick本身就会调用quitRoom().
                //quitRoom();
                ui.parent.display(ui);
            };
            roomCtrl.onRoomPropChanged += (key, val) =>
            {
                roomClient.SetRoomProp(key, val);
            };
            roomCtrl.onPlayerRemove += (id) =>
            {
                if (isRoomOwner)
                {
                    logic.RemovePlayer(id);
                }
            };
            roomCtrl.onLocalPlayerPropChanged += (name, cards) =>
            {
                roomClient.SetPlayerProp(name, cards);
            };

            ui.hide();
            RoomData roomData = logic.CurrentRoom;
            roomCtrl.display(roomData, false);
            roomCtrl.SetMode(isServerMode ? RoomMode.NetworkingClient : RoomMode.NetworkingHost);
            roomCtrl.SetIPText("IP：" + string.Join(", ", addresses));
            roomCtrl.UpdateRoomData(roomData);
            //  加载聊天框
            _sceneCtrl.chatCtrl.display(roomData);
        }

        private void hideAllOtherPage()
        {
            ui.NetworkingPageGroup.display(null);
        }

        private void refreshRoomListItem(RoomListItem item, LobbyRoomData obj)
        {
            string username = "";
            if (isServerMode)
                username = logic.GetUserName(obj.OwnerId);
            else
                username = lang._(LOCAL_PLAYER_NAME, obj.OwnerId);

            item.RoomNameText.text = obj.RoomName;
            item.asButton.onClick.set(() =>
            {
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
                if (obj.IsLocked)
                {
                    lanPanel.ShowPasswordDialog();
                    _enteringRoomID = obj.RoomID;
                }
                else
                {
                    await joinRoom(obj.RoomID);
                };
            });
            item.ButtonButtonBlack.asButton.interactable = obj.PlayerCount < obj.MaxPlayerCount;

            item.ShowLockedIcon(obj.IsLocked);
        }

        private void setRoomData(RoomData room)
        {
            Debug.Log($"更新房间信息");
            _sceneCtrl.room.SetRoomData(room);
            _sceneCtrl.room.UpdateRoomData(room);
        }

        private void toggleLoadingPanel(bool on)
        {
            lanPanel.LoadingPanelImage.gameObject.SetActive(on);
        }

        private void updateAccountPanel()
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

        #endregion UI

        #endregion 私有方法

        #region 属性字段

        public string[] addresses
        {
            get
            {
                var hostName = Dns.GetHostName();
                var hostEntry = Dns.GetHostEntry(hostName);
                var interNetworks = hostEntry.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                var addresses = interNetworks.Select(i => i.ToString() + ":" + LANPort);
                return addresses.ToArray();
            }
        }

        public CacheManager cacheManager => getManager<CacheManager>();

        public GameManager gameManager
        {
            get
            {
                if (_gameManager == null)
                    _gameManager = getManager<GameManager>();
                return _gameManager;
            }
        }

        public NetworkingLogic logic
        {
            get
            {
                if (_logic == null)
                    _logic = new NetworkingLogic(serverBaseUri, version, cacheManager, _ports);
                return _logic;
            }
        }

        public IRoomClient roomClient => logic.roomClient;

        public NetworkingPage ui => getManager<UIManager>().getObject<NetworkingPage>();

        private UIDialog dialog => getManager<UIManager>().getObject<UIDialog>();

        private bool isRoomOwner => logic.isRoomOwner;

        private LangManager lang => getManager<LangManager>();

        private LANPanel lanPanel => ui.NetworkingPageGroup.LANPanel;

        private int LANPort => logic.Port;

        private RoomCtrl roomCtrl => _sceneCtrl.room;

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

        [TranslateMsg]
        private const string CANNOT_CREATE_ROOM = "无法创建房间";

        [TranslateMsg]
        private const string CANNOT_JOIN_ROOM = "无法加入房间";

        [TranslateMsg]
        private const string GUEST_LOGIN_FAILED = "游客登录失败，服务器压力过大";

        [TranslateMsg]
        private const string INVALID_ADDRESS = "输入的地址格式有误";

        [TranslateMsg]
        private const string INVALID_USERNAME_OR_PASSWORD = "用户名或密码不正确";

        [TranslateMsg("参数为玩家ID")]
        private const string LOCAL_PLAYER_NAME = "玩家{0}";

        [TranslateMsg]
        private const string NETMANAGER_CHECK_CODE_NULL = "校验码不能为空";

        [TranslateMsg]
        private const string NETMANAGER_DESCRIBE = "描述";

        [TranslateMsg]
        private const string NETMANAGER_EMAIL_NULL = "邮箱不能为空";

        [TranslateMsg("参数为玩家名称")]
        private const string NETMANAGER_HOME = "{0}的房间";

        [TranslateMsg]
        private const string NETMANAGER_NICKNAME_NULL = "昵称不能为空";

        [TranslateMsg]
        private const string NETMANAGER_PASSWORD_ERROR = "两次输入的密码不匹配";

        [TranslateMsg]
        private const string NETMANAGER_RECOVER_CODE_NULL = "密码找回代码不能为空";

        [TranslateMsg]
        private const string NETMANAGER_USERNAME_NULL = "用户名不能为空";

        [Tooltip("正式服务器地址")]
        [SerializeField]
        private string _accountServerBaseUri = "https://serv.igsk.fun";

        private GameManager _gameManager;
        private NetworkingLogic _logic;

        [Header("配置")]
        /// <summary>
        /// 端口
        /// </summary>
        [SerializeField]
        private int[] _ports = { 32900, 32901, 32902 };

        // 对MainSceneCtrl的引用，用于调用RoomCtrl的display。
        // 也要调用ChatPanel的display
        private MainSceneCtrl _sceneCtrl;

        [Tooltip("快照服务器地址")]
        [SerializeField]
        private string _snapshotAccountServerBaseUri = "https://snap.serv.igsk.fun";

        [Tooltip("测试服务器地址")]
        [SerializeField]
        private string _testAccountServerBaseUri = "https://test.serv.igsk.fun";

        /// <summary>
        /// 当前是服务器的Lobby，还是局域网模式
        /// </summary>
        private bool isServerMode = false;

        #region 密码相关

        private string _enteringRoomAddress;
        private string _enteringRoomID;
        private int _enteringRoomPort;

        #endregion 密码相关

        #endregion 属性字段

        #region 内嵌类

        private enum Panel
        {
            Login,
            Register,
            Recover
        }

        #endregion 内嵌类
    }
}