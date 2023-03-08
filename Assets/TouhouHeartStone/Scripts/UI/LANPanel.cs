using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class LANPanel
    {
        #region 公有方法
        public void ShowPasswordDialog()
        {
            _passwordDialog.SetActive(true);
        }
        public void HidePasswordDialog()
        {
            _passwordDialog.SetActive(false);
            _errorMessageObj.SetActive(false);
            _passwordInputField.text = string.Empty;
        }
        public void ShowErrorMessage(string message)
        {
            _errorMessageObj.SetActive(true);
            _errorMessageText.text = message;
        }
        #endregion
        #region 私有方法
        partial void onAwake()
        {
            _confirmPasswordButton.onClick.AddListener(onConfirmPasswordClick);
            _cancelPasswordButton.onClick.AddListener(onCancelPasswordClick);
        }
        private void onCancelPasswordClick()
        {
            onCancelPassword?.Invoke();
        }
        private void onConfirmPasswordClick()
        {
            onConfirmPassword?.Invoke(_passwordInputField.text);
        }
        #endregion
        #region 事件
        public event Action<string> onConfirmPassword;
        public event Action onCancelPassword;
        #endregion
        #region 私有字段
        [Header("密码")]
        [SerializeField]
        private GameObject _passwordDialog;
        [SerializeField]
        private InputField _passwordInputField;
        [SerializeField]
        private GameObject _errorMessageObj;
        [SerializeField]
        private Text _errorMessageText;
        [SerializeField]
        private Button _confirmPasswordButton;
        [SerializeField]
        private Button _cancelPasswordButton;
        #endregion
    }
}
