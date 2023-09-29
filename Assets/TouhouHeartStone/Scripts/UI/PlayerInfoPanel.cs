using System;
using Game;
using NitoriNetwork.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    partial class PlayerInfoPanel : MonoBehaviour
    {
        [SerializeField]
        GameObject _loadingImage;

        [SerializeField]
        Button _editBtn;

        public event Action<PlayerInfoPanel> onEditBtnClick;


        void Awake()
        {
            autoInit();
            _editBtn.onClick.AddListener(() => onEditBtnClick?.Invoke(this));
        }
        
        enum LoginState
        {
            NotLogin,
            AutoLogin,
            Logined
        }

        /// <summary>
        /// 设置当前状态为正在自动登录
        /// </summary>
        public void EnterAutoLoginState()
        {
            toggleLoginStatus(LoginState.AutoLogin);
        }

        public void SetPlayerInfo(PublicBasicUserInfo info)
        {
            // 未登录状态
            if (info == null)
            {
                toggleLoginStatus(LoginState.NotLogin);
                return;
            }
            // 游客
            bool isGuest = info.UID >= 1000000000;
            if (isGuest)
            {
               // todo: 本地化游客名称
            }
            nickNameText.text = info.Name;
            emailText.text = "UID: " + info.UID;
            // todo: 加载头像
            toggleLoginStatus(LoginState.Logined, isGuest);
        }
        private void toggleLoginStatus(LoginState state, bool isGuest = false)
        {
            loginButton.gameObject.SetActive(state == LoginState.NotLogin);
            headImage.gameObject.SetActive(state != LoginState.NotLogin);
            infoContainerImage.gameObject.SetActive(state == LoginState.Logined);
            _loadingImage.SetActive(state == LoginState.AutoLogin);
            _editBtn.gameObject.SetActive(state == LoginState.Logined && !isGuest);
        }
    }

    partial class LoginPanel
    {
        [SerializeField]
        Button _guestLoginBtn;

        public Button GuestLoginBtn => _guestLoginBtn;

        public event Action OnRegisterClick;
        [Obsolete("Use Display + SetContent instead")]
        public void display(bool rememberUserName, string userName, bool rememberPassword, string password, Sprite captcha)
        {
            HideMessage();
            SetContent(rememberUserName, userName, rememberPassword, password, captcha);
            display();
        }
        public void Display()
        {
            HideMessage();
            display();
        }
        public void SetContent(bool rememberUserName, string userName, bool rememberPassword, string password, Sprite captcha = null)
        {
            if (captcha != null)
                CaptchaImgButton.image.sprite = captcha;

            RememberAccountUIToggle.asToggle.isOn = rememberUserName;
            RememberPasswordUIToggle.asToggle.isOn = rememberPassword;
            UserName = userName;
            Password = password;
        }
        public void ShowMessage(string msg)
        {
            Hint_Text.text = msg;
            HintImage.gameObject.SetActive(true);
        }
        public void HideMessage()
        {
            HintImage.gameObject.SetActive(false);
        }
        partial void onAwake()
        {
            RegisterButtonBlack.asButton.onClick.AddListener(() =>
            {
                OnRegisterClick?.Invoke();
            });
            CloseButtonBlackIcon.asButton.onClick.AddListener(() =>
            {
                hide();
            });
        }
        public void setCaptcha(Sprite sprite)
        {
            CaptchaImgButton.image.sprite = sprite;
        }
        public string UserName { get => InputFieldDark.asInputField.text; private set => InputFieldDark.asInputField.text = value; }
        public string Password { get => Password_InputFieldDark.asInputField.text; private set => Password_InputFieldDark.asInputField.text = value; }
        public string Captcha => Captcha_InputFieldDark.asInputField.text;
    }
    partial class RegisterPanel
    {
        public event Action onRegisterClick;
        [Obsolete("Use Display instead")]
        public void display(Sprite captcha)
        {
            HideMessage();
            CaptchaImageButton.image.sprite = captcha;
            display();
        }
        public void Display()
        {
            HideMessage();
            display();
        }
        public void ShowMessage(string msg)
        {
            Hint_Text.text = msg;
            HintImage.gameObject.SetActive(true);
        }
        public void HideMessage()
        {
            HintImage.gameObject.SetActive(false);
        }
        partial void onAwake()
        {
            LoginButtonBlack.asButton.onClick.AddListener(() =>
            {
                onRegisterClick?.Invoke();
            });
            CloseButtonBlackIcon.asButton.onClick.AddListener(() =>
            {
                hide();
            });
        }
        public void setCaptcha(Sprite sprite)
        {
            CaptchaImageButton.image.sprite = sprite;
        }

        public event Action OnShowEULAClick;

        public void onShowEULAClick()
        {
            OnShowEULAClick?.Invoke();
        }
    }
    partial class RecoverPanel
    {
        string mail = "";
        public void hide()
        {
            gameObject.SetActive(false);
        }

        bool currentStage = false;

        public bool CurrentStage => currentStage;
        public void Display()
        {
            SwitchStage(false);
            gameObject.SetActive(true);
        }

        public void SwitchStage(bool stage)
        {
            currentStage = stage;
            container1RectTransform.gameObject.SetActive(!stage);
            container2RectTransform.gameObject.SetActive(stage);
            HideMessage();
        }
        
        private void Awake()
        {
            init();
            onCloseButtonClick += hide;
        }
        public void ShowMessage(string msg)
        {
            hintText.text = msg;
            hintText.transform.parent.gameObject.SetActive(true);
        }
        public void HideMessage()
        {
            hintText.transform.parent.gameObject.SetActive(false);
        }
        public void setCaptcha(Sprite sprite)
        {
            if (currentStage)
            {
                captchaImage2.sprite = sprite;
            }
            else
            {
                captchaImage1.sprite = sprite;
            }
        }
    }
}
