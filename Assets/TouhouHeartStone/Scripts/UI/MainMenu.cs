using System;
using BJSYGameCore.UI;
using NitoriNetwork.Common;
using UberLogger;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : UIObject
    {
        #region 公有方法
        public void setNewsVisible(bool visible)
        {
            _newsPanel.gameObject.SetActive(visible);
        }
        public void setPlayerInfo(PublicBasicUserInfo info)
        {
            _playerInfoPanel.SetPlayerInfo(info);
        }
        #region 注册面板
        public void setRegisterPanelVisible(bool visible)
        {
            if (visible)
                _registerPanel.Display();
            else
                _registerPanel.hide();
        }
        public void showRegisterMessage(string message)
        {
            _registerPanel.ShowMessage(message);
        }
        public void hideRegisterMessage()
        {
            _registerPanel.HideMessage();
        }
        public void setRegisterCaptcha(Sprite image)
        {
            _registerPanel.setCaptcha(image);
        }
        public void showEULA(string eula)
        {
            _eulaPanel.Show(eula);
        }
        #endregion
        #region 登陆面板
        public void setLoginPanelVisible(bool visible)
        {
            if (visible)
                _loginPanel.Display();
            else
                _loginPanel.hide();
        }
        public void showLoginMessage(string message)
        {
            _loginPanel.ShowMessage(message);
        }
        public void hideLoginMessage()
        {
            _loginPanel.HideMessage();
        }
        public void setLoginContent(bool rememberUsername, string username, bool rememberPassword, string password)
        {
            _loginPanel.SetContent(rememberUsername, username, rememberPassword, password);
        }
        public void setLoginCaptcha(Sprite image)
        {
            _loginPanel.setCaptcha(image);
        }
        #endregion
        #region 找回密码面板
        public void setRecoverPanelVisible(bool visible)
        {
            if (visible)
                _recoverPanel.Display();
            else
                _recoverPanel.hide();
        }
        public void showRecoverMessage(string message)
        {
            _recoverPanel.ShowMessage(message);
        }
        public void hideRecoverMessage()
        {
            _recoverPanel.HideMessage();
        }
        public void setRecoverCaptcha(Sprite image)
        {
            _recoverPanel.setCaptcha(image);
        }
        public void switchRecoverStage(bool stage)
        {
            _recoverPanel.SwitchStage(stage);
        }
        #endregion

        #region 编辑玩家信息面板
        public void setPlayerEditPanelVisible(bool visible)
        {
            if (visible)
                _playerInfoEditPanel.Show();
            else
                _playerInfoEditPanel.Hide();
        }
        public void showPlayerEditMessage(string message)
        {
            _playerInfoEditPanel.ShowMessage(message);
        }
        public void hidePlayerEditMessage()
        {
            _playerInfoEditPanel.HideMessage();
        }
        #endregion

        public void enterAutoLoginState()
        {
            _playerInfoPanel.EnterAutoLoginState();
        }

        #endregion
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
#if DEBUG || DEVELOPMENT_BUILD || UNITY_EDITOR
            _pveButton.gameObject.SetActive(true);
#else
            _pveButton.gameObject.SetActive(false);
#endif
            _vsBotButton.onClick.AddListener(onVsBotButtonClickCallback);
            _pveButton.onClick.AddListener(onPVEButtonClickCallback);
            _buildCardButton.onClick.AddListener(onBuildCardButtonClickCallback);
            _replayButton.onClick.AddListener(onReplayButtonClickCallback);
            _quitButton.onClick.AddListener(onQuitButtonClickCallback);
            _aboutButton.onClick.AddListener(onAboutButtonClickCallback);
            _settingButton.onClick.AddListener(onSettingButtonClickCallback);
            _newsButton.onClick.AddListener(onNewsButtonClickCallback);
            _lanButton.onClick.AddListener(onLanButtonClickCallback);
            _serverButton.onClick.AddListener(onServerButtonClickCallback);
            _editorButton.onClick.AddListener(onEditorButtonClickCallback);

            _registerPanel.onCaptchaClick += onRegisterCaptchaClickCallback;
            _registerPanel.onRegisterClick += onRegisterConfirmClickCallback;
            _registerPanel.onShowEULAClick += onRegisterEULAClickCallback;
            _registerPanel.onLoginClick += onRegisterBackClickCallback;

            _loginPanel.onRegisterClick += onLoginRegisterClickCallback;
            _loginPanel.onCaptchaClick += onLoginCaptchaClickCallback;
            _loginPanel.onLoginClick += onLoginClickCallback;
            _loginPanel.onGuestLoginClick += onLoginGuestLoginClickCallback;
            _loginPanel.onRecoverClick += onLoginRecoverClickCallback;

            _recoverPanel.onCommitButtonClick += onRecoverCommitClickCallback;
            _recoverPanel.onCaptchaClick += onRecoverCaptchaClickCallback;
            _recoverPanel.onCloseButtonClick += onRecoverCloseClickCallback;
            _recoverPanel.onBackButtonClick += onRecoverBackClickCallback;

            _playerInfoPanel.onLoginButtonClick += onPlayerInfoLoginClickCallback;
            _playerInfoPanel.onLogoutButtonClick += onPlayerInfoLogoutClickCallback;
            _playerInfoPanel.onEditBtnClick += onPlayerInfoEditClickCallback;
        }
        #endregion
        #region 事件回调
        private void onAboutButtonClickCallback()
        {
            onAboutButtonClick?.Invoke();
        }
        private void onSettingButtonClickCallback()
        {
            onSettingButtonClick?.Invoke();
        }
        private void onNewsButtonClickCallback()
        {
            onNewsButtonClick?.Invoke();
        }
        private void onQuitButtonClickCallback()
        {
            onQuitButtonClick?.Invoke();
        }
        private void onVsBotButtonClickCallback()
        {
            onVsBotButtonClick?.Invoke();
        }
        private void onPVEButtonClickCallback()
        {
            onPVEButtonClick?.Invoke();
        }
        private void onEditorButtonClickCallback()
        {
            onEditorButtonClick?.Invoke();
        }
        private void onBuildCardButtonClickCallback()
        {
            onBuildCardButtonClick?.Invoke();
        }
        private void onReplayButtonClickCallback()
        {
            onReplayButtonClick?.Invoke();
        }
        private void onLanButtonClickCallback()
        {
            onLanButtonClick?.Invoke();
        }
        private void onServerButtonClickCallback()
        {
            onServerButtonClick?.Invoke();
        }
        #region 注册页面
        private void onRegisterCaptchaClickCallback()
        {
            onRegisterCaptchaClick?.Invoke();
        }
        private void onRegisterConfirmClickCallback()
        {
            var info = _registerPanel.getRegisterInfo();
            onRegisterConfirmClick?.Invoke(info);
        }
        private void onRegisterEULAClickCallback()
        {
            onRegisterEULAClick?.Invoke();
        }
        private void onRegisterBackClickCallback()
        {
            onRegisterBackClick?.Invoke();
        }
        #endregion
        #region 登陆页面
        private void onLoginRegisterClickCallback()
        {
            onLoginRegisterClick?.Invoke();
        }
        private void onLoginCaptchaClickCallback()
        {
            onLoginCaptchaClick?.Invoke();
        }
        private void onLoginClickCallback()
        {
            LoginInfo info = _loginPanel.getLoginInfo();
            onLoginClick?.Invoke(info);
        }
        private void onLoginGuestLoginClickCallback()
        {
            onLoginGuestLoginClick?.Invoke();
        }
        private void onLoginRecoverClickCallback()
        {
            onLoginRecoverClick?.Invoke();
        }
        #endregion

        #region 找回密码页面
        private void onRecoverCommitClickCallback()
        {
            RecoverInfo info = _recoverPanel.getRecoverInfo();
            onRecoverCommitClick?.Invoke(info);
        }
        private void onRecoverBackClickCallback()
        {
            onRecoverBackClick?.Invoke();
        }
        private void onRecoverCloseClickCallback()
        {
            onRecoverCloseClick?.Invoke();
        }
        private void onRecoverCaptchaClickCallback()
        {
            onRecoverCaptchaClick?.Invoke();
        }
        #endregion

        #region 玩家信息图标
        private void onPlayerInfoLoginClickCallback(PlayerInfoPanel panel)
        {
            onPlayerInfoLoginClick?.Invoke();
        }
        private void onPlayerInfoLogoutClickCallback(PlayerInfoPanel panel)
        {
            onPlayerInfoLogoutClick?.Invoke();
        }
        private void onPlayerInfoEditClickCallback(PlayerInfoPanel panel)
        {
            onPlayerInfoEditClick?.Invoke();
        }
        #endregion

        private void onPlayerInfoEditConfirmClickCallback()
        {
            PlayerInfoEditInfo info = _playerInfoEditPanel.getPlayerInfoEditInfo();
            onPlayerInfoEditConfirmClick?.Invoke(info);
        }
#endregion
#endregion
#region 事件
        public event Action onVsBotButtonClick;
        public event Action onPVEButtonClick;
        public event Action onEditorButtonClick;
        public event Action onBuildCardButtonClick;
        public event Action onReplayButtonClick;
        public event Action onLanButtonClick;
        public event Action onServerButtonClick;
        public event Action onAboutButtonClick;
        public event Action onSettingButtonClick;
        public event Action onNewsButtonClick;
        public event Action onQuitButtonClick;


        public event Action onRegisterCaptchaClick;
        public event Action<RegisterInfo> onRegisterConfirmClick;
        public event Action onRegisterEULAClick;
        public event Action onRegisterBackClick;

        public event Action onLoginRegisterClick;
        public event Action onLoginCaptchaClick;
        public event Action<LoginInfo> onLoginClick;
        public event Action onLoginGuestLoginClick;
        public event Action onLoginRecoverClick;

        public event Action onPlayerInfoLoginClick;
        public event Action onPlayerInfoLogoutClick;
        public event Action onPlayerInfoEditClick;

        public event Action<RecoverInfo> onRecoverCommitClick;
        public event Action onRecoverCloseClick;
        public event Action onRecoverBackClick;
        public event Action onRecoverCaptchaClick;

        public event Action<PlayerInfoEditInfo> onPlayerInfoEditConfirmClick;
#endregion
        [SerializeField]
        EULAPanel _eulaPanel;
        [SerializeField]
        PlayerInfoEditPanel _playerInfoEditPanel;
        [SerializeField]
        Button _vsBotButton;
        [SerializeField]
        Button _pveButton;
        [SerializeField]
        Button _buildCardButton;
        [SerializeField]
        Button _replayButton;
        [SerializeField]
        Button _lanButton;
        [SerializeField]
        Button _serverButton;
        [SerializeField]
        Button _newsButton;
        [SerializeField]
        Button _aboutButton;
        [SerializeField]
        RegisterPanel _registerPanel;
        [SerializeField]
        LoginPanel _loginPanel;
        [SerializeField]
        public NewsPanel _newsPanel;
        [SerializeField]
        public PlayerInfoPanel _playerInfoPanel;
        [SerializeField]
        Button _settingButton;
        [SerializeField]
        Button _quitButton;
        [SerializeField]
        Button _editorButton;
        public RecoverPanel _recoverPanel;
    }

}
