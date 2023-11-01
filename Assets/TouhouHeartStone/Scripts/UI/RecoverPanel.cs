using System;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RecoverPanel : UIObject
    {
        public void Display()
        {
            SwitchStage(false);
            display();
        }

        public void SwitchStage(bool stage)
        {
            _container1RectTransform.gameObject.SetActive(!stage);
            _container2RectTransform.gameObject.SetActive(stage);
            HideMessage();
        }
        public void ShowMessage(string msg)
        {
            _hintText.text = msg;
            _hintText.transform.parent.gameObject.SetActive(true);
        }
        public void HideMessage()
        {
            _hintText.transform.parent.gameObject.SetActive(false);
        }
        public void setCaptcha(Sprite sprite)
        {
            _captchaImage1.sprite = sprite;
            _captchaImage2.sprite = sprite;
        }
        public RecoverInfo getRecoverInfo()
        {
            return new RecoverInfo()
            {
                email = _emailInput.text,
                code = _codeInput.text,
                captcha1 = _captchaInput1.text,
                password = _passwordInput.text,
                repeatPassword = _repeatPasswordInput.text,
                captcha2 = _captchaInput2.text,
            };
        }

        protected override void Awake()
        {
            base.Awake();
            _closeButton.onClick.AddListener(onCloseClickedCallback);
            _captchaButton1.onClick.AddListener(onCaptchaButtonClickCallback);
            _captchaButton2.onClick.AddListener(onCaptchaButtonClickCallback);
            _backButton.onClick.AddListener(onBackButtonClickCallback);
            _commitButton.onClick.AddListener(onCommitButtonClickCallback);
        }
        private void onCloseClickedCallback()
        {
            onCloseButtonClick?.Invoke();
        }
        private void onCaptchaButtonClickCallback()
        {
            onCaptchaClick?.Invoke();
        }
        private void onBackButtonClickCallback()
        {
            onBackButtonClick?.Invoke();
        }
        private void onCommitButtonClickCallback()
        {
            onCommitButtonClick?.Invoke();
        }
        public event Action onCloseButtonClick;
        public event Action onCaptchaClick;
        public event Action onBackButtonClick;
        public event Action onCommitButtonClick;

        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private Text _hintText;
        [SerializeField]
        private InputField _emailInput;
        [SerializeField]
        private InputField _captchaInput1;
        [SerializeField]
        private Image _captchaImage1;
        [SerializeField]
        private Button _captchaButton1;
        [SerializeField]
        private InputField _codeInput;
        [SerializeField]
        private InputField _passwordInput;
        [SerializeField]
        private InputField _repeatPasswordInput;
        [SerializeField]
        private InputField _captchaInput2;
        [SerializeField]
        private Image _captchaImage2;
        [SerializeField]
        private Button _captchaButton2;
        [SerializeField]
        private Button _backButton;
        [SerializeField]
        private Button _commitButton;
        [SerializeField]
        private RectTransform _container2RectTransform;
        [SerializeField]
        private RectTransform _container1RectTransform;
    }
    public struct RecoverInfo
    {
        public string email;
        public string code;
        public string captcha1;
        public string password;
        public string repeatPassword;
        public string captcha2;
    }
}
