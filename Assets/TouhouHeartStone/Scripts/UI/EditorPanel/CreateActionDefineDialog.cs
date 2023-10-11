
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreateActionDefineDialog : MonoBehaviour
    {
        public void display()
        {
            gameObject.SetActive(true);
            _nameInputField.text = string.Empty;
            _categoryInputField.text = string.Empty;
            _errorTipText.text = string.Empty;
            _defineTypeDropdown.value = 0;
        }
        public void hide()
        {
            gameObject.SetActive(false);
        }
        public void showErrorTip(string errorTip)
        {
            _errorTipText.text = errorTip;
        }
        public void setDefineTypeOptions(string[] typeOptions)
        {
            _defineTypeDropdown.ClearOptions();
            _defineTypeDropdown.AddOptions(new List<string>(typeOptions));
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _confirmButton.onClick.AddListener(onConfirmClicked);
            _cancelButton.onClick.AddListener(onCancelClicked);
        }
        #endregion
        #region 事件回调
        private void onConfirmClicked()
        {
            onConfirm?.Invoke(_nameInputField.text, _categoryInputField.text, _defineTypeDropdown.value);
        }
        private void onCancelClicked()
        {
            hide();
        }
        #endregion

        #endregion

        #region 事件
        public event Action<string, string, int> onConfirm;
        #endregion
        [SerializeField]
        private InputField _nameInputField;
        [SerializeField]
        private InputField _categoryInputField;
        [SerializeField]
        private Dropdown _defineTypeDropdown;
        [SerializeField]
        private Text _errorTipText;
        [SerializeField]
        private Button _confirmButton;
        [SerializeField]
        private Button _cancelButton;
    }
}