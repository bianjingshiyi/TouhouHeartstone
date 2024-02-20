using System;
using System.Linq;
using System.Threading.Tasks;
using Game;
using IGensoukyo.Utilities;
using MukioI18n;
using NitoriNetwork.Common;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    class MainMenuCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public void display()
        {
            gameObject.SetActive(true);
#if DEBUG || DEVELOPMENT_BUILD || UNITY_EDITOR
            _ui.pveButton.gameObject.SetActive(true);
#else
            _ui.pveButton.gameObject.SetActive(false);
#endif
            _ui.pveButton.setNewMarkVisible(!game.tutorial.isTutorialClosed(TutorialID.PVE));
            _ui.buildButton.setNewMarkVisible(!game.tutorial.isTutorialClosed(TutorialID.Build));
            _ui.setLoginSteamButtonsValid(SteamManager.isValid);
            _ui.setPlayerEditPanelSteamValid(SteamManager.isValid);
        }
        public void hide()
        {
            gameObject.SetActive(false);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            //登录与注册界面初始化
            hideAccountPanels();
            _ui.setPlayerInfo(null);

            _ui.onPVEButtonClick += onEntryPVEMode;
            _ui.onVsBotButtonClick += onClickVsBotButton;
            _ui.onBuildCardButtonClick += onBuildButtonClick;
            _ui.onEditorButtonClick += onEditorButtonClick;
            _ui.onReplayButtonClick += onReplayButtonClickedCallback;
            _ui.onLanButtonClick += onLanButtonClickedCallback;
            _ui.onServerButtonClick += onServerButtonClickedCallback;
            _ui.onQuitButtonClick += onQuitButtonClickedCallback;
            _ui.onAboutButtonClick += onAboutButtonClickedCallback;
            _ui.onSettingButtonClick += onSettingButtonClickedCallback;
            _ui.onNewsButtonClick += onNewsButtonClickedCallback;


            _ui.onRegisterCaptchaClick += onRegisterPanelCaptchaClickedCallback;
            _ui.onRegisterConfirmClick += onRegisterPanelConfirmClickedCallback;
            _ui.onRegisterEULAClick += onRegisterPanelEULAClickedCallback;
            _ui.onRegisterBackClick += onRegisterPanelBackClickedCallback;
            _ui.onRegisterViaSteamConfirmClick += onRegisterViaSteamPanelConfirmClickedCallback;

            _ui.onLoginRegisterClick += onLoginPanelRegisterClickedCallback;
            _ui.onLoginCaptchaClick += onLoginPanelCaptchaClickedCallback;
            _ui.onLoginClick += onLoginPanelLoginClickedCallback;
            _ui.onLoginGuestLoginClick += onLoginPanelGuestLoginClickedCallback;
            _ui.onLoginRecoverClick += onLoginPanelRecoverClickedCallback;
            _ui.onLoginSteamLoginClick += onLoginPanelSteamLoginClickedCallback;

            _ui.onPlayerInfoLoginClick += onPlayerInfoLoginClickedCallback;
            _ui.onPlayerInfoLogoutClick += onPlayerInfoLogoutClickedCallback;
            _ui.onPlayerInfoEditClick += onPlayerInfoEditClickedCallback;

            _ui.onRecoverCommitClick += onRecoverPanelCommitClickedCallback;
            _ui.onRecoverCloseClick += onRecoverPanelCloseClickedCallback;
            _ui.onRecoverBackClick += onRecoverPanelBackClickedCallback;
            _ui.onRecoverCaptchaClick += onRecoverPanelCaptchaClickedCallback;

            _ui.onPlayerInfoEditNicknameConfirmClick += onPlayerInfoEditNicknameConfirmClickedCallback;
            _ui.onPlayerInfoEditPasswordConfirmClick += onPlayerInfoEditPasswordConfirmClickedCallback;
            _ui.onPlayerInfoEditSteamBindClick += onPlayerInfoEditSteamBindClickedCallback;
            game.network.onAutoLogin += onAutoLoginCallback;
            game.network.onAutoLoginFinished += onAutoLoginFinishedCallback;
        }
        private void OnEnable()
        {
            debugEntrance?.setEnabled(true);
        }
        private void OnDisable()
        {
            debugEntrance?.setEnabled(false);
        }
        #endregion
        #region 事件回调
        private async void onClickVsBotButton()
        {
            //Main main = _ui.getObject<Main>();
            var playerName = game.lang._(StringTable.LOCAL_PLAYER);
            await scene.game.network.logic.CreateLocalRoom(playerName);
            RoomData currentRoomData = scene.game.network.logic.CurrentRoom;
            currentRoomData.maxPlayerCount = 2; // 炉石两个人

            HearthstoneOption option = new HearthstoneOption();
            currentRoomData.setOption(option);

            hide();
            await scene.room.display(currentRoomData, string.Empty, RoomMode.Local);
        }
        private void onBuildButtonClick()
        {
            hide();
            scene.build.display();
        }
        private async void onReplayButtonClickedCallback()
        {
            hide();
            await scene.replay.display();
        }
        private void onLanButtonClickedCallback()
        {
            main.display(main.NetworkingPage);
            scene.lan.display(false);
        }
        private async void onServerButtonClickedCallback()
        {
            if (game.network.logic.Logined)
            {
                main.display(main.NetworkingPage);
                scene.lan.display(true);
            }
            else
            {
                await showAccountPanel(AccountPanel.Login);
            }
        }
        private void onQuitButtonClickedCallback()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        private void onAboutButtonClickedCallback()
        {
            main.display(main.AboutPage);
        }
        private void onSettingButtonClickedCallback()
        {
            main.display(main.SettingPage);
        }
        private void onNewsButtonClickedCallback()
        {
            // 等之后有空再改
            _ui.setNewsVisible(true);
        }

        /// <summary>
        /// 进入PVE模式界面
        /// </summary>
        private async void onEntryPVEMode()
        {
            await scene.pveRoom.display();
            hide();
        }

        private void onEditorButtonClick()
        {
            hide();
            scene.editor.display();
        }
        #region 注册界面
        private async void onRegisterPanelConfirmClickedCallback(RegisterInfo info)
        {
            _ui.hideRegisterMessage();
            var password = info.password;
            var email = info.email;
            var repeatPassword = info.repeatPassword;
            var nickName = info.nickName;
            if (string.IsNullOrEmpty(email))
            {
                _ui.showRegisterMessage(lang._(NETMANAGER_EMAIL_NULL));
                return;
            }
            if (string.IsNullOrEmpty(nickName))
            {
                _ui.showRegisterMessage(lang._(NETMANAGER_NICKNAME_NULL));
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                _ui.showRegisterMessage(lang._(StringTable.ACCOUNT_PASSWORD_NULL));
                return;
            }
            if (password != repeatPassword)
            {
                _ui.showRegisterMessage(lang._(NETMANAGER_PASSWORD_ERROR));
                return;
            }

            var panel = AccountPanel.Register;
            try
            {
                startWaiting(panel);
                // 注册完毕，切换到登录界面
                await game.network.logic.register(new AccountInfo(password, email, nickName));
                game.network.loadRememberAccount(out var rememberUserName, out var userName);
                _ui.setLoginContent(rememberUserName, userName);
                await showAccountPanel(AccountPanel.Login);
            }
            catch (NetClientException e)
            {
                showErrorMessage(panel, e);
                _ui.setRegisterCaptcha(await game.network.getCaptchaImage());
            }
            finally
            {
                stopWaiting(panel);
            }
        }
        private async void onRegisterViaSteamPanelConfirmClickedCallback(RegisterViaSteamInfo info)
        {
            _ui.hideRegisterMessage();
            var nickName = info.nickName;
            var email = info.email;
            if (string.IsNullOrEmpty(nickName))
            {
                _ui.showRegisterViaSteamMessage(lang._(NETMANAGER_NICKNAME_NULL));
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                _ui.showRegisterViaSteamMessage(lang._(NETMANAGER_EMAIL_NULL));
                return;
            }

            var panel = AccountPanel.RegisterViaSteam;
            try
            {
                startWaiting(panel);
                var ticket = await SteamManager.getAuthTicketForWebApi();
                await game.network.logic.steamRegister(email, nickName);
                // 注册完毕，登录
                if (await game.network.logic.steamLogin())
                {
                    // 登陆完毕
                    updatePlayerInfo();
                    hideAccountPanels();
                }
            }
            catch (Exception e)
            {
                showErrorMessage(panel, e);
            }
            finally
            {
                stopWaiting(panel);
            }
        }
        private async void onRegisterPanelBackClickedCallback()
        {
            await showAccountPanel(AccountPanel.Login);
        }
        private async void onRegisterPanelEULAClickedCallback()
        {
            var content = await game.network.logic.GetEULAAndPrivacyPolicy();
            _ui.showEULA(content);
        }
        private async void onRegisterPanelCaptchaClickedCallback()
        {
            _ui.setRegisterCaptcha(await game.network.getCaptchaImage());
        }
        #endregion
        #region 登录界面
        private async void onLoginPanelRegisterClickedCallback()
        {
            await showAccountPanel(AccountPanel.Register);
        }
        private async void onLoginPanelCaptchaClickedCallback()
        {
            _ui.setLoginCaptcha(await game.network.getCaptchaImage());
        }
        private async void onLoginPanelLoginClickedCallback(LoginInfo info)
        {
            _ui.hideLoginMessage();
            var username = info.username;
            var password = info.password;
            var rememberUsername = info.rememberUsername;
            if (string.IsNullOrEmpty(username))
            {
                _ui.showLoginMessage(lang._(NETMANAGER_USERNAME_NULL));
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                _ui.showLoginMessage(lang._(StringTable.ACCOUNT_PASSWORD_NULL));
                return;
            }
            var panel = AccountPanel.Login;
            try
            {
                startWaiting(panel);
                bool success = await game.network.logic.login(username, password);
                if (success)
                {
                    game.network.saveRemeberAccount(rememberUsername, username);
                    updatePlayerInfo();
                    hideAccountPanels();
                }
                else
                {
                    _ui.showLoginMessage(lang._(StringTable.ACCOUNT_EMAIL_OR_PASSWORD_WRONG));
                    _ui.setLoginCaptcha(await game.network.getCaptchaImage());
                }
            }
            catch (NetClientException e)
            {
                showErrorMessage(panel, e, true);
                _ui.setLoginCaptcha(await game.network.getCaptchaImage());
            }
            finally
            {
                stopWaiting(panel);
            }
        }
        private async void onLoginPanelGuestLoginClickedCallback()
        {
            _ui.hideLoginMessage();

            var panel = AccountPanel.Login;
            try
            {
                startWaiting(panel);
                bool success = await game.network.logic.GuestLogin();
                if (success)
                {
                    updatePlayerInfo();
                    hideAccountPanels();
                }
                else
                {
                    _ui.showLoginMessage(lang._(GUEST_LOGIN_FAILED));
                }
            }
            catch (NetClientException e)
            {
                showErrorMessage(panel, e, true);
            }
            finally
            {
                stopWaiting(panel);
            }
        }
        private async void onLoginPanelRecoverClickedCallback()
        {
            await showAccountPanel(AccountPanel.Recover);
        }
        private async void onLoginPanelSteamLoginClickedCallback()
        {
            var panel = AccountPanel.Login;
            try
            {
                startWaiting(panel);
                if (await game.network.logic.steamLogin())
                {
                    updatePlayerInfo();
                    hideAccountPanels();
                }
            }
            catch (Exception e)
            {
                if (e is NetClientException && e.Message == "steamid is not associated")
                {
                    var title = game.lang._(StringTable.HINT_TITLE);
                    var desc = game.lang._(STEAMBINDING_NOT_REGISTERED);
                    game.ui.dialog.ShowQuestion(title, desc, async () =>
                    {
                        await showAccountPanel(AccountPanel.RegisterViaSteam);
                    });
                }
                else
                {
                    showErrorMessage(panel, e, true);
                }
            }
            finally
            {
                stopWaiting(panel);
            }
        }
        #endregion

        #region 玩家信息
        private async void onPlayerInfoLoginClickedCallback()
        {
            game.network.loadRememberAccount(out var rememberUserName, out var userName);
            _ui.setLoginContent(rememberUserName, userName);
            await showAccountPanel(AccountPanel.Login);
        }
        private async void onPlayerInfoLogoutClickedCallback()
        {
            var panel = AccountPanel.PlayerInfo;
            try
            {
                startWaiting(panel);
                await game.network.logic.logout();
                updatePlayerInfo();
            }
            catch (Exception e)
            {
                Debug.LogError($"登出失败：{e}");
            }
            finally
            {
                stopWaiting(panel);
            }
        }
        private async void onPlayerInfoEditClickedCallback()
        {
            await showAccountPanel(AccountPanel.PlayerInfoEdit);
        }
        #endregion

        #region 找回密码
        private async void onRecoverPanelCommitClickedCallback(RecoverInfo info)
        {
            _ui.hideRecoverMessage();
            var mail = info.email;

            if (string.IsNullOrEmpty(mail))
            {
                _ui.showRecoverMessage(lang._(NETMANAGER_EMAIL_NULL));
                return;
            }
            var panel = AccountPanel.Recover;
            try
            {
                startWaiting(panel);
                if (await game.network.logic.recoverRequest(mail))
                {
                    _ui.switchRecoverStage(true);
                }
            }
            catch (NetClientException e)
            {
                showErrorMessage(panel, e);
            }
            finally
            {
                stopWaiting(panel);
            }
            _ui.setRecoverCaptcha(await game.network.getCaptchaImage());
        }
        private void onRecoverPanelCloseClickedCallback()
        {
            hideAccountPanels();
        }
        private async void onRecoverPanelBackClickedCallback()
        {
            await showAccountPanel(AccountPanel.Login);
        }
        private async void onRecoverPanelCaptchaClickedCallback()
        {
            _ui.setRecoverCaptcha(await game.network.getCaptchaImage());
        }

        #endregion
        private async void onPlayerInfoEditNicknameConfirmClickedCallback(string nickname)
        {
            _ui.hidePlayerEditMessage();
            if (string.IsNullOrEmpty(nickname))
            {
                _ui.showPlayerEditMessage(lang._(NETMANAGER_NICKNAME_NULL));
                return;
            }

            var panel = AccountPanel.PlayerInfoEdit;
            try
            {
                startWaiting(panel);
                await game.network.logic.ChangeUserNickname(nickname);
                var message = game.lang._(NETMANAGER_EDIT_NICKNAME_SUCCESSFULLY);
                _ui.showPlayerEditMessage(message);
                updatePlayerInfo();
            }
            catch (NetClientException e)
            {
                showErrorMessage(panel, e);
            }
            finally
            {
                stopWaiting(panel);
            }
        }
        private async void onPlayerInfoEditPasswordConfirmClickedCallback(string newPassword, string repeatPassword)
        {
            _ui.hidePlayerEditMessage();
            if (string.IsNullOrEmpty(newPassword))
            {
                _ui.showPlayerEditMessage(lang._(StringTable.ACCOUNT_PASSWORD_NULL));
                return;
            }
            if (newPassword != repeatPassword)
            {
                _ui.showPlayerEditMessage(lang._(NETMANAGER_PASSWORD_ERROR));
                return;
            }

            var panel = AccountPanel.PlayerInfoEdit;
            try
            {
                startWaiting(panel);
                await game.network.logic.ChangeUserPassword(newPassword);
                var message = game.lang._(NETMANAGER_EDIT_PASSWORD_SUCCESSFULLY);
                _ui.showPlayerEditMessage(message);
                updatePlayerInfo();
            }
            catch (NetClientException e)
            {
                showErrorMessage(panel, e);
            }
            finally
            {
                stopWaiting(panel);
            }
        }
        private async void onPlayerInfoEditSteamBindClickedCallback(bool unbind)
        {
            var panel = AccountPanel.PlayerInfoEdit;
            try
            {
                startWaiting(panel);
                bool success;
                if (unbind)
                    success = await game.network.logic.unlinkSteam();
                else
                {
                    var ticket = await SteamManager.getAuthTicketForWebApi();
                    success = await game.network.logic.linkSteam();
                }
                if (success)
                {
                    var message = game.lang._(unbind ? STEAMBINDING_UNBOND_SUCCESSFULLY : STEAMBINDING_BOND_SUCCESSFULLY);
                    _ui.showPlayerEditMessage(message);
                    updatePlayerInfo();
                }
            }
            catch (Exception e)
            {
                showErrorMessage(panel, e);
            }
            finally
            {
                stopWaiting(panel);
            }
        }
        private void onAutoLoginCallback()
        {
            _ui.enterAutoLoginState();
        }
        private void onAutoLoginFinishedCallback(bool success)
        {
            updatePlayerInfo();
            if (!success && SteamManager.isValid && PlayerPrefs.GetInt(KEY_STEAM_ACCOUNT_NOTED, 0) <= 0)
            {
                PlayerPrefs.SetInt(KEY_STEAM_ACCOUNT_NOTED, 1);
                // 自动登录没成功，并且Steam有效
                string title = game.lang._(StringTable.HINT_TITLE);
                string desc = game.lang._(STEAMBINDING_DESC);
                string[] options = new string[]
                {
                    game.lang._(STEAMBINDING_CREATE_ACCOUNT),
                    game.lang._(StringTable.ASK_LATER)
                };
                game.ui.dialog.ShowOptions(title, desc, options, async index =>
                {
                    switch (index)
                    {
                        case 0: // 创建
                            await showAccountPanel(AccountPanel.RegisterViaSteam);
                            break;
                    }
                });
            }
        }
        #endregion

        #region 用户帐号
        private void hideAccountPanels()
        {
            _ui.setRegisterPanelVisible(false);
            _ui.setLoginPanelVisible(false);
            _ui.setRecoverPanelVisible(false);
            _ui.setRegisterViaSteamPanelVisible(false);
            _ui.setPlayerEditPanelVisible(false);
        }
        private async Task showAccountPanel(AccountPanel panel)
        {
            _ui.setLoginPanelVisible(panel == AccountPanel.Login);
            _ui.setRegisterPanelVisible(panel == AccountPanel.Register);
            _ui.setRecoverPanelVisible(panel == AccountPanel.Recover);
            _ui.setPlayerEditPanelVisible(panel == AccountPanel.PlayerInfoEdit);
            _ui.setRegisterViaSteamPanelVisible(panel == AccountPanel.RegisterViaSteam);

            switch (panel)
            {
                case AccountPanel.Login:
                    _ui.setLoginCaptcha(await game.network.getCaptchaImage());
                    break;

                case AccountPanel.Register:
                    _ui.setRegisterCaptcha(await game.network.getCaptchaImage());
                    break;

                case AccountPanel.Recover:
                    _ui.setRecoverCaptcha(await game.network.getCaptchaImage());
                    break;
            }
        }
        private async Task updatePlayerEditPanelSteamState()
        {
            if (!game.network.logic.Logined)
                return;
            var gettingText = game.lang._(STEAMBINDING_GETTING_STATE);
            _ui.setPlayerEditPanelSteamState(gettingText, false, false);

            bool bond = await game.network.logic.isAccountSteamLinked();
            var stateText = game.lang._(bond ? STEAMBINDING_BOND : STEAMBINDING_UNBOND);
            _ui.setPlayerEditPanelSteamState(stateText, bond, true);
        }
        private void updatePlayerInfo()
        {
            try
            {
                _ui.setPlayerInfo(game.network.logic.UserInfo);
                _ = updatePlayerEditPanelSteamState();
            }
            catch (NetClientException)
            {
                _ui.setPlayerInfo(null);
            }
        }
        #endregion

        private void startWaiting(AccountPanel panel)
        {
            scene.waitingBlocker.addWaitingTask(WaitingTask.UserAccount);
            setPanelInteractable(panel, false);
        }
        private void stopWaiting(AccountPanel panel)
        {
            scene.waitingBlocker.removeWaitingTask(WaitingTask.UserAccount);
            setPanelInteractable(panel, true);
        }
        private void setPanelInteractable(AccountPanel panel, bool interactable)
        {
            switch (panel)
            {
                case AccountPanel.Login:
                    _ui.setLoginPanelInteractable(interactable);
                    break;
                case AccountPanel.Register:
                    _ui.setRegisterPanelInteractable(interactable);
                    break;
                case AccountPanel.Recover:
                    _ui.setRecoverPanelInteractable(interactable);
                    break;
                case AccountPanel.RegisterViaSteam:
                    _ui.setRegisterViaSteamPanelInteractable(interactable);
                    break;
                case AccountPanel.PlayerInfo:
                    _ui.setPlayerInfoInteractable(interactable);
                    break;
                case AccountPanel.PlayerInfoEdit:
                    _ui.setPlayerEditPanelInteractable(interactable);
                    break;
            }
        }
        private void showErrorMessage(AccountPanel panel, Exception e, bool logError = false)
        {
            if (logError)
                Debug.LogError(e);
            else
                Debug.Log(e);
            string msg = game.network.translateNetErrorMessage(e);
            switch (panel)
            {
                case AccountPanel.Login:
                    _ui.showLoginMessage(msg);
                    break;
                case AccountPanel.Register:
                    _ui.showRegisterMessage(msg);
                    break;
                case AccountPanel.Recover:
                    _ui.showRecoverMessage(msg);
                    break;
                case AccountPanel.RegisterViaSteam:
                    _ui.showRegisterViaSteamMessage(msg);
                    break;
                case AccountPanel.PlayerInfoEdit:
                    _ui.showPlayerEditMessage(msg);
                    break;
            }
        }

        #endregion
        #region 属性字段
        [TranslateMsg]
        private const string GUEST_LOGIN_FAILED = "游客登录失败，服务器压力过大";
        [TranslateMsg]
        private const string NETMANAGER_EMAIL_NULL = "邮箱不能为空";
        [TranslateMsg]
        private const string NETMANAGER_NICKNAME_NULL = "昵称不能为空";
        [TranslateMsg]
        private const string NETMANAGER_PASSWORD_ERROR = "两次输入的密码不匹配";
        [TranslateMsg]
        private const string NETMANAGER_RECOVER_CODE_NULL = "密码找回代码不能为空";
        [TranslateMsg]
        private const string NETMANAGER_USERNAME_NULL = "用户名不能为空";
        [TranslateMsg]
        private const string NETMANAGER_EDIT_NICKNAME_SUCCESSFULLY = "修改昵称成功。";
        [TranslateMsg]
        private const string NETMANAGER_EDIT_PASSWORD_SUCCESSFULLY = "修改密码成功。";
        [TranslateMsg]
        private const string STEAMBINDING_DESC = "当前Steam账号并未与IGensoukyo帐号绑定，使用Steam账号信息注册一个帐号吗？（您也可以在登录IGensoukyo账号后在帐号编辑页面中绑定Steam帐号。）";
        [TranslateMsg]
        private const string STEAMBINDING_CREATE_ACCOUNT = "创建账号";
        [TranslateMsg]
        private const string STEAMBINDING_NOT_REGISTERED = "当前Steam账号并未注册IGensoukyo帐号，使用Steam账号信息注册一个帐号吗？\n";
        [TranslateMsg]
        private const string STEAMBINDING_BOND = "已绑定";
        [TranslateMsg]
        private const string STEAMBINDING_UNBOND = "未绑定";
        [TranslateMsg]
        private const string STEAMBINDING_BOND_SUCCESSFULLY = "成功绑定Steam帐号。";
        [TranslateMsg]
        private const string STEAMBINDING_UNBOND_SUCCESSFULLY = "成功解绑Steam帐号。";
        [TranslateMsg]
        private const string STEAMBINDING_GETTING_STATE = "获取中...";

        private const string KEY_STEAM_ACCOUNT_NOTED = "SteamAccountNoted";
        private LangManager lang => game.lang;
        private Main main => game.ui.getObject<Main>();
        [SerializeField]
        private DebugLabelEntrance debugEntrance;
        [SerializeField]
        MainMenu _ui;
        #endregion
        #region 内嵌类

        private enum AccountPanel
        {
            Login,
            Register,
            Recover,
            PlayerInfo,
            PlayerInfoEdit,
            RegisterViaSteam,
        }

        #endregion 内嵌类
    }
}
