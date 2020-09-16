using UnityEngine;
using BJSYGameCore;
using UI;
using NitoriNetwork.Common;

namespace Game
{
    public class AccountManager : Manager
    {
        [SerializeField]
        RegisterPanel regPanel;

        [SerializeField]
        LoginPanel loginPanel;

        [SerializeField]
        PlayerInfoPanel playerInfoPanel;

        public ServerClient server { get; internal set; }

        [SerializeField]
        string serverUrl = "http://localhost:50112";

        protected override void onAwake()
        {
            base.onAwake();

            server = new ServerClient(serverUrl);

            loginPanel.account = this;
            regPanel.account = this;

            loginPanel.gameObject.SetActive(false);
            regPanel.gameObject.SetActive(false);

            regPanel.OnLoginClick += () =>
            {
                regPanel.gameObject.SetActive(false);
                loginPanel.gameObject.SetActive(true);
            };

            loginPanel.OnRegisterClick += () =>
            {
                regPanel.gameObject.SetActive(true);
                loginPanel.gameObject.SetActive(false);
            };

            playerInfoPanel.OnLoginClick += () =>
            {
                loginPanel.gameObject.SetActive(true);
            };

            playerInfoPanel.OnLogoutClick += () =>
            {
                playerInfoPanel.SetPlayerInfo(null);
            };
        }

        private void Start()
        {
            UpdateUserInfoPanel();
        }

        public void UpdateUserInfoPanel()
        {
            try
            {
                var info = server.GetUserInfo();
                playerInfoPanel.SetPlayerInfo(info);
            }
            catch (NetClientException)
            {
                playerInfoPanel.SetPlayerInfo(null);
            }
        }

        public Sprite GetCaptchaImage()
        {
            Texture2D texture = new Texture2D(512, 512);

            try
            {
                texture.LoadImage(server.GetCaptchaImage());
            }
            catch (NetClientException e)
            {
                Debug.LogWarning(e.Message);
                return null;
            }
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
    }
}
