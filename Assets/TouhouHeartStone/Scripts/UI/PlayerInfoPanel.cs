using System;
using BJSYGameCore.UI;
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
            emailText.text = $"UID: {info.UID}";
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
}
