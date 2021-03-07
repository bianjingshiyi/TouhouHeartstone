using System;
using Game;
using NitoriNetwork.Common;
using UnityEngine;

namespace UI
{
    partial class PlayerInfoPanel
    {
        public event Action OnLoginClick;
        public event Action OnLogoutClick;
        partial void onAwake()
        {
            LoginButtonBlack.asButton.onClick.AddListener(() =>
            {
                OnLoginClick?.Invoke();
            });

            LogoutButtonBlack.asButton.onClick.AddListener(() =>
            {
                OnLogoutClick?.Invoke();
            });
        }
        public void SetPlayerInfo(PublicBasicUserInfo info)
        {
            if (info == null || info.UID >= 1000000000)
            {
                // 未登录或是游客状态
                toggleLoginStatus(false);
            }
            else
            {
                NickNameText.text = info.Name;
                EmailText.text = "UID: " + info.UID;
                // todo: 加载头像
                toggleLoginStatus(true);
            }
        }
        private void toggleLoginStatus(bool logined)
        {
            LoginButtonBlack.gameObject.SetActive(!logined);
            HeadImage.gameObject.SetActive(logined);
            InfoContainerImage.gameObject.SetActive(logined);
        }
    }

    partial class LoginPanel
    {
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
