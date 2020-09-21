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
        public AccountManager account { get; set; }


        private void OnEnable()
        {
            HideMessage();
            reloadCaptcha();
            loadRemember();
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
                gameObject.SetActive(false);
            });

            CaptchaImgButton.onClick.AddListener(reloadCaptcha);

            LoginButtonBlack.asButton.onClick.AddListener(login);
        }

        void reloadCaptcha()
        {
            CaptchaImgButton.image.sprite = account.GetCaptchaImage();
        }

        string UserName { get => InputFieldDark.asInputField.text; set => InputFieldDark.asInputField.text = value; }
        string Password { get => Password_InputFieldDark.asInputField.text; set => Password_InputFieldDark.asInputField.text = value; }
        string Captcha => Captcha_InputFieldDark.asInputField.text;

        void login()
        {
            HideMessage();

            if (string.IsNullOrEmpty(UserName))
            {
                ShowMessage("用户名不能为空");
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                ShowMessage("验证码不能为空");
                return;
            }

            try
            {
                bool success = account.server.Login(UserName, Password, Captcha);
                if (success)
                {
                    saveRemeber();
                    account.UpdateUserInfoPanel();
                    hide();
                }
                else
                {
                    ShowMessage("用户名或密码不正确");
                    reloadCaptcha();
                }
            }
            catch (NetClientException e)
            {
                ShowMessage(e.Message);
                reloadCaptcha();
            }
        }

        void saveRemeber()
        {
            PlayerPrefs.SetInt("RememberAccount", RememberAccountUIToggle.asToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt("RememberPassword", RememberPasswordUIToggle.asToggle.isOn ? 1 : 0);
            PlayerPrefs.SetString("UserName", RememberAccountUIToggle.enabled ? UserName : "");
            PlayerPrefs.SetString("Password", RememberPasswordUIToggle.enabled ? Password : "");
            PlayerPrefs.Save();
            // todo: 使用一种更安全的方式存储密码
        }

        void loadRemember()
        {
            RememberAccountUIToggle.asToggle.isOn = PlayerPrefs.GetInt("RememberAccount", 0) > 0;
            RememberPasswordUIToggle.asToggle.isOn = PlayerPrefs.GetInt("RememberPassword", 0) > 0;
            UserName = PlayerPrefs.GetString("UserName", "");
            Password = PlayerPrefs.GetString("Password", "");
        }
    }

    partial class RegisterPanel
    {
        public event Action OnLoginClick;
        public AccountManager account { get; set; }

        private void OnEnable()
        {
            HideMessage();
            reloadCaptcha();
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
                OnLoginClick?.Invoke();
            });

            RegisterButtonBlack.asButton.onClick.AddListener(register);

            CloseButtonBlackIcon.asButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });

            CaptchaImageButton.onClick.AddListener(reloadCaptcha);
        }

        void reloadCaptcha()
        {
            CaptchaImageButton.image.sprite = account.GetCaptchaImage();
        }

        void register()
        {
            HideMessage();

            var user = InputFieldDark.asInputField.text;
            var password = Password_InputFieldDark.asInputField.text;
            var repeatPassword = RepeatPassword_InputFieldDark.asInputField.text;
            var email = Mail_InputFieldDark.asInputField.text;
            var captcha = Captcha_InputFieldDark.asInputField.text;
            var nick = NickName_InputFieldDark.asInputField.text;

            if (string.IsNullOrEmpty(user))
            {
                ShowMessage("用户名不能为空");
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                ShowMessage("邮箱不能为空");
                return;
            }

            if (string.IsNullOrEmpty(captcha))
            {
                ShowMessage("验证码不能为空");
                return;
            }

            if (string.IsNullOrEmpty(nick))
            {
                ShowMessage("昵称不能为空");
                return;
            }

            if (password != repeatPassword)
            {
                ShowMessage("两次输入的密码不匹配");
                return;
            }

            try
            {
                account.server.Register(user, email, password, nick, captcha);
                OnLoginClick?.Invoke();
            }
            catch (NetClientException e)
            {
                ShowMessage(e.Message);
                reloadCaptcha();
            }
        }
    }
}
