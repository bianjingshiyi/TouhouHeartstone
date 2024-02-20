using System;
using BJSYGameCore.UI;
using UberLogger;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LoginPanel : UIObject
    {
        public void Display()
        {
            HideMessage();
            display();
        }
        public void SetContent(bool rememberUserName, string userName, Sprite captcha = null)
        {
            if (captcha != null)
                _captchaImage.sprite = captcha;

            _rememberAccountToggle.isOn = rememberUserName;
            _usernameInputField.text = userName;
        }
        public void setSteamButtonsValid(bool valid)
        {
            _steamButtonsObj.SetActive(valid);
        }
        public void ShowMessage(string msg)
        {
            _hintText.text = msg;
            _hintImage.gameObject.SetActive(true);
        }
        public void HideMessage()
        {
            _hintImage.gameObject.SetActive(false);
        }
        public void setCaptcha(Sprite sprite)
        {
            _captchaButton.image.sprite = sprite;
        }
        public LoginInfo getLoginInfo()
        {
            return new LoginInfo()
            {
                username = _usernameInputField.text,
                password = _passwordInputField.text,
                captcha = _captchaInputField.text,
                rememberUsername = _rememberAccountToggle.isOn,
            };
        }
        public void setInteractable(bool interactable)
        {
            _usernameInputField.interactable = interactable;
            _passwordInputField.interactable = interactable;
            _captchaInputField.interactable = interactable;
            _captchaButton.interactable = interactable;
            _rememberAccountToggle.interactable = interactable;
            _loginButton.interactable = interactable;
            _guestLoginBtn.interactable = interactable;
            _registerButton.interactable = interactable;
            _recoverButton.interactable = interactable;
            _closeButton.interactable = interactable;
            _steamLoginButton.interactable = interactable;
        }
        protected override void Awake()
        {
            base.Awake();
            _registerButton.onClick.AddListener(() => onRegisterClick?.Invoke());
            _guestLoginBtn.onClick.AddListener(() => onGuestLoginClick?.Invoke());
            _loginButton.onClick.AddListener(() => onLoginClick?.Invoke());
            _recoverButton.onClick.AddListener(() => onRecoverClick?.Invoke());
            _captchaButton.onClick.AddListener(() => onCaptchaClick?.Invoke());
            _closeButton.onClick.AddListener(() => onCloseClick?.Invoke());
            _steamLoginButton.onClick.AddListener(() => onSteamLoginClick?.Invoke());
        }

        public event Action onRegisterClick;
        public event Action onGuestLoginClick;
        public event Action onCaptchaClick;
        public event Action onLoginClick;
        public event Action onRecoverClick;
        public event Action onCloseClick;
        public event Action onSteamLoginClick;

        [SerializeField]
        private GameObject _steamButtonsObj;

        [SerializeField]
        private Button _guestLoginBtn;
        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private Image _hintImage;
        [SerializeField]
        private Text _hintText;
        [SerializeField]
        private InputField _usernameInputField;
        [SerializeField]
        private InputField _passwordInputField;
        [SerializeField]
        private Image _captchaImage;
        [SerializeField]
        private InputField _captchaInputField;
        [SerializeField]
        private Button _captchaButton;
        [SerializeField]
        private Toggle _rememberAccountToggle;
        [SerializeField]
        private Button _recoverButton;
        [SerializeField]
        private Button _registerButton;
        [SerializeField]
        private Button _loginButton;
        [SerializeField]
        private Button _steamLoginButton;
    }
    public struct LoginInfo
    {
        public string username;
        public string password;
        public string captcha;
        public bool rememberUsername;
    }
}
