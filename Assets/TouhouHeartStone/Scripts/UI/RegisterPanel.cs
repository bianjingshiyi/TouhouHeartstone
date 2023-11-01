using System;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RegisterPanel : UIObject
    {
        public void Display()
        {
            HideMessage();
            display();
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
            _captchaImage.sprite = sprite;
        }
        public RegisterInfo getRegisterInfo()
        {
            return new RegisterInfo
            {
                username = _usernameInputField.text,
                password = _passwordInputField.text,
                repeatPassword = _repeatPasswordInputField.text,
                email = _emailInputField.text,
                nickName = _nickNameInputField.text,
                invite = _inviteInputField.text,
                captcha = _captchaInputField.text,
            };
        }
        protected override void Awake()
        {
            base.Awake();
            _registerButton.onClick.AddListener(onRegisterClickedCallback);
            _closeButton.onClick.AddListener(onCloseClickedCallback);
            _loginButton.onClick.AddListener(onLoginClickedCallback);
            _eulaButton.onClick.AddListener(onShowEULAClickCallback);
            _captchaButton.onClick.AddListener(onCaptchaClickCallback);
        }
        private void onRegisterClickedCallback()
        {
            onRegisterClick?.Invoke();
        }
        private void onCloseClickedCallback()
        {
            hide();
        }
        private void onLoginClickedCallback()
        {
            onLoginClick?.Invoke();
        }
        private void onShowEULAClickCallback()
        {
            onShowEULAClick?.Invoke();
        }
        private void onCaptchaClickCallback()
        {
            onCaptchaClick?.Invoke();
        }

        public event Action onRegisterClick;
        public event Action onShowEULAClick;
        public event Action onCaptchaClick;
        public event Action onLoginClick;
        [SerializeField]
        private InputField _usernameInputField;
        [SerializeField]
        private InputField _passwordInputField;
        [SerializeField]
        private InputField _repeatPasswordInputField;
        [SerializeField]
        private InputField _emailInputField;
        [SerializeField]
        private InputField _nickNameInputField;
        [SerializeField]
        private InputField _inviteInputField;
        [SerializeField]
        private InputField _captchaInputField;
        [SerializeField]
        private Text _hintText;
        [SerializeField]
        private Image _hintImage;
        [SerializeField]
        private Button _registerButton;
        [SerializeField]
        private Button _loginButton;
        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private Button _eulaButton;
        [SerializeField]
        private Image _captchaImage;
        [SerializeField]
        private Button _captchaButton;
    }
    public struct RegisterInfo
    {
        public string username;
        public string password;
        public string repeatPassword;
        public string email;
        public string nickName;
        public string invite;
        public string captcha;
    }
}
