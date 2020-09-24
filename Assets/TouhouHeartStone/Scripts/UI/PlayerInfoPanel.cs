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
            if (info == null)
            {
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
        public void display(bool rememberUserName, string userName, bool rememberPassword, string password, Sprite captcha)
        {
            HideMessage();
            CaptchaImgButton.image.sprite = captcha;
            RememberAccountUIToggle.asToggle.isOn = rememberUserName;
            RememberPasswordUIToggle.asToggle.isOn = rememberPassword;
            UserName = userName;
            Password = password;
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
        public void display(Sprite captcha)
        {
            HideMessage();
            CaptchaImageButton.image.sprite = captcha;
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
        public void setCaptach(Sprite sprite)
        {
            CaptchaImageButton.image.sprite = sprite;
        }
    }
}
