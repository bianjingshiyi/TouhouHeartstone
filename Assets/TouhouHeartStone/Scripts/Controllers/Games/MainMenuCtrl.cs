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
            _ui.setRegisterPanelVisible(false);
            _ui.setLoginPanelVisible(false);
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

            _ui.onLoginRegisterClick += onLoginPanelRegisterClickedCallback;
            _ui.onLoginCaptchaClick += onLoginPanelCaptchaClickedCallback;
            _ui.onLoginClick += onLoginPanelLoginClickedCallback;
            _ui.onLoginGuestLoginClick += onLoginPanelGuestLoginClickedCallback;
            _ui.onLoginRecoverClick += onLoginPanelRecoverClickedCallback;

            _ui.onPlayerInfoLoginClick += onPlayerInfoLoginClickedCallback;
            _ui.onPlayerInfoLogoutClick += onPlayerInfoLogoutClickedCallback;
            _ui.onPlayerInfoEditClick += onPlayerInfoEditClickedCallback;

            _ui.onRecoverCommitClick += onRecoverPanelCommitClickedCallback;
            _ui.onRecoverCloseClick += onRecoverPanelCloseClickedCallback;
            _ui.onRecoverBackClick += onRecoverPanelBackClickedCallback;
            _ui.onRecoverCaptchaClick += onRecoverPanelCaptchaClickedCallback;

            _ui.onPlayerInfoEditConfirmClick += onPlayerInfoEditConfirmClickedCallback;
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
            await scene.game.network.logic.CreateLocalRoom();
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
        private void onServerButtonClickedCallback()
        {
            if (game.network.logic.Logined)
            {
                main.display(main.NetworkingPage);
                scene.lan.display(true);
            }
            else
            {
                _ui.setRegisterPanelVisible(false);
                _ui.setLoginPanelVisible(true);
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
            var userName = info.username;
            var password = info.password;
            var email = info.email;
            var repeatPassword = info.repeatPassword;
            var invite = info.invite;
            var nickName = info.nickName;
            var captcha = info.captcha;
            if (string.IsNullOrEmpty(userName))
            {
                _ui.showRegisterMessage(lang._(NETMANAGER_USERNAME_NULL));
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                _ui.showRegisterMessage(lang._(NETMANAGER_EMAIL_NULL));
                return;
            }
            if (string.IsNullOrEmpty(captcha))
            {
                _ui.showRegisterMessage(lang._(NETMANAGER_CHECK_CODE_NULL));
                return;
            }

            if (string.IsNullOrEmpty(nickName))
            {
                _ui.showRegisterMessage(lang._(NETMANAGER_NICKNAME_NULL));
                return;
            }

            if (password != repeatPassword)
            {
                _ui.showRegisterMessage(lang._(NETMANAGER_PASSWORD_ERROR));
                return;
            }

            try
            {
                // 注册完毕，切换到登录界面
                await game.network.logic.register(new AccountInfo(userName, password, email, nickName, invite), captcha);
                game.network.loadRememberAccount(out var rememberUserName, out userName, out var rememberPassword, out password);
                _ui.setLoginContent(rememberUserName, userName, rememberPassword, password);
                await showPanel(Panel.Login);
            }
            catch (NetClientException e)
            {
                _ui.showRegisterMessage(e.Message);
                _ui.setRegisterCaptcha(await game.network.getCaptchaImage());
            }
        }
        private async void onRegisterPanelBackClickedCallback()
        {
            await showPanel(Panel.Login);
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
            await showPanel(Panel.Register);
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
            var captcha = info.captcha;
            var rememberUsername = info.rememberUsername;
            var rememberPassword = info.rememberPassword;
            if (string.IsNullOrEmpty(username))
            {
                _ui.showLoginMessage(lang._(NETMANAGER_USERNAME_NULL));
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                _ui.showLoginMessage(lang._(NETMANAGER_CHECK_CODE_NULL));
                return;
            }
            try
            {
                bool success = await game.network.logic.login(username, password, captcha);
                if (success)
                {
                    game.network.saveRemeberAccount(rememberUsername, username, rememberPassword, password);
                    _ui.setLoginPanelVisible(false);
                    updateAccountPanel();
                }
                else
                {
                    _ui.showLoginMessage(lang._(INVALID_USERNAME_OR_PASSWORD));
                    _ui.setLoginCaptcha(await game.network.getCaptchaImage());
                }
            }
            catch (NetClientException e)
            {
                Debug.Log(e);
                _ui.showLoginMessage(e.Message);
                _ui.setLoginCaptcha(await game.network.getCaptchaImage());
            }
        }
        private async void onLoginPanelGuestLoginClickedCallback()
        {
            _ui.hideLoginMessage();
            try
            {
                bool success = await game.network.logic.GuestLogin();
                if (success)
                {
                    _ui.setLoginPanelVisible(false);
                    updateAccountPanel();
                }
                else
                {
                    _ui.showLoginMessage(lang._(GUEST_LOGIN_FAILED));
                }
            }
            catch (NetClientException e)
            {
                Debug.LogError(e);
                _ui.showLoginMessage(e.Message);
            }
        }
        private async void onLoginPanelRecoverClickedCallback()
        {
            await showPanel(Panel.Recover);
        }
        #endregion

        #region 玩家信息
        private async void onPlayerInfoLoginClickedCallback()
        {
            game.network.loadRememberAccount(out var rememberUserName, out var userName, out var rememberPassword, out var password);
            _ui.setLoginContent(rememberUserName, userName, rememberPassword, password);
            await showPanel(Panel.Login);
        }
        private void onPlayerInfoLogoutClickedCallback()
        {
            _ui.setPlayerInfo(null);
            _ = game.network.logic.logout();
        }
        private void onPlayerInfoEditClickedCallback()
        {
            _ui.setPlayerEditPanelVisible(true);
        }
        #endregion

        #region 找回密码
        private async void onRecoverPanelCommitClickedCallback(RecoverInfo info)
        {
            _ui.hideRecoverMessage();
            if (!_recoverEmailSent)
            {
                var mail = info.email;
                var captcha = info.captcha1;

                if (string.IsNullOrEmpty(mail))
                {
                    _ui.showRecoverMessage(lang._(NETMANAGER_EMAIL_NULL));
                    return;
                }
                if (string.IsNullOrEmpty(captcha))
                {
                    _ui.showRecoverMessage(lang._(NETMANAGER_CHECK_CODE_NULL));
                    return;
                }
                try
                {
                    await game.network.logic.ServerClient.RecoverRequestAsync(mail, captcha);
                    _ui.switchRecoverStage(true);
                    _recoverEmailSent = true;
                }
                catch (NetClientException e)
                {
                    Debug.Log(e);
                    _ui.showRecoverMessage(e.Message);
                }
                _ui.setRecoverCaptcha(await game.network.getCaptchaImage());
            }
            else
            {
                var mail = info.email;
                var captcha = info.captcha2;
                var password = info.password;
                var repeatPassword = info.repeatPassword;
                var code = info.code.Trim();

                if (string.IsNullOrEmpty(mail))
                {
                    _ui.showRecoverMessage(lang._(NETMANAGER_EMAIL_NULL));
                    return;
                }
                if (string.IsNullOrEmpty(code))
                {
                    _ui.showRecoverMessage(lang._(NETMANAGER_RECOVER_CODE_NULL));
                    return;
                }
                if (string.IsNullOrEmpty(captcha))
                {
                    _ui.showRecoverMessage(lang._(NETMANAGER_CHECK_CODE_NULL));
                    return;
                }
                if (password != repeatPassword)
                {
                    _ui.showRecoverMessage(lang._(NETMANAGER_PASSWORD_ERROR));
                    return;
                }
                try
                {
                    await game.network.logic.ServerClient.RecoverPasswordAsync(mail, code, password, captcha);
                    _ui.setRecoverPanelVisible(false);
                    _ui.setLoginPanelVisible(true);
                    _recoverEmailSent = false;
                }
                catch (NetClientException e)
                {
                    Debug.Log(e);
                    _ui.showRecoverMessage(e.Message);
                    _ui.setRecoverCaptcha(await game.network.getCaptchaImage());
                }
            }
        }
        private void onRecoverPanelCloseClickedCallback()
        {
            _ui.setRecoverPanelVisible(false);
            _recoverEmailSent = false;
        }
        private void onRecoverPanelBackClickedCallback()
        {
            if (_recoverEmailSent)
                _ui.switchRecoverStage(false);
            else
            {
                _ui.setRecoverPanelVisible(false);
                _ui.setLoginPanelVisible(true);
            }
            _recoverEmailSent = false;
        }
        private async void onRecoverPanelCaptchaClickedCallback()
        {
            _ui.setRecoverCaptcha(await game.network.getCaptchaImage());
        }

        #endregion
        private async void onPlayerInfoEditConfirmClickedCallback(PlayerInfoEditInfo info)
        {
            var nickname = info.nickname;
            var oldPassword = info.oldPassword;
            var newPassword = info.newPassword;
            var repeatNewPassword = info.repeatPassword;
            _ui.hidePlayerEditMessage();
            if (newPassword != repeatNewPassword)
            {
                _ui.showPlayerEditMessage(lang._(NETMANAGER_PASSWORD_ERROR));
                return;
            }

            try
            {
                await game.network.logic.ChangeUserInfo(nickname, oldPassword, newPassword);
                updateAccountPanel();
                _ui.setPlayerEditPanelVisible(false);
            }
            catch (NetClientException e)
            {
                Debug.Log(e);
                _ui.showPlayerEditMessage(e.Message);
            }
        }
        private void onAutoLoginCallback()
        {
            _ui.enterAutoLoginState();
        }
        private void onAutoLoginFinishedCallback()
        {
            updateAccountPanel();
        }
        #endregion
        private async Task showPanel(Panel panel)
        {
            _ui.setLoginPanelVisible(panel == Panel.Login);
            _ui.setRegisterPanelVisible(panel == Panel.Register);
            _ui.setRecoverPanelVisible(panel == Panel.Recover);

            switch (panel)
            {
                case Panel.Login:
                    _ui.setLoginCaptcha(await game.network.getCaptchaImage());
                    break;

                case Panel.Register:
                    _ui.setRegisterCaptcha(await game.network.getCaptchaImage());
                    break;

                case Panel.Recover:
                    _ui.setRecoverCaptcha(await game.network.getCaptchaImage());
                    break;

                default:
                    break;
            }
        }
        private void updateAccountPanel()
        {
            try
            {
                _ui.setPlayerInfo(game.network.logic.UserInfo);
            }
            catch (NetClientException)
            {
                _ui.setPlayerInfo(null);
            }
        }
        #endregion
        #region 属性字段
        [TranslateMsg]
        private const string INVALID_USERNAME_OR_PASSWORD = "用户名或密码不正确";
        [TranslateMsg]
        private const string GUEST_LOGIN_FAILED = "游客登录失败，服务器压力过大";
        [TranslateMsg]
        private const string NETMANAGER_CHECK_CODE_NULL = "校验码不能为空";
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
        private LangManager lang => game.lang;
        private Main main => game.ui.getObject<Main>();
        private bool _recoverEmailSent;
        [SerializeField]
        private DebugLabelEntrance debugEntrance;
        [SerializeField]
        MainMenu _ui;
        #endregion
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
