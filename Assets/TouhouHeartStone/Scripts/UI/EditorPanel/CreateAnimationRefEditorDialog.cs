
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreateAnimationRefEditorDialog : MonoBehaviour
    {
        #region 公有方法
        public void setKey(string key)
        {
            _nameInputField.SetTextWithoutNotify(key);
        }
        public void setErrorTip(string key)
        {
            _errorTipText.text = key;
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _confirmButton.onClick.AddListener(onConfirmButtonClickedCallback);
            _cancelButton.onClick.AddListener(onCancelButtonClickedCallback);
        }
        private void onConfirmButtonClickedCallback()
        {
            onConfirm?.Invoke(_nameInputField.text);
        }
        private void onCancelButtonClickedCallback()
        {
            onCancel?.Invoke();
        }
        #endregion
        #region 
        public event Action<string> onConfirm;
        public event Action onCancel;
        #endregion
        #region 属性字段
        [SerializeField]
        private InputField _nameInputField;
        [SerializeField]
        private Text _errorTipText;
        [SerializeField]
        private Button _confirmButton;
        [SerializeField]
        private Button _cancelButton;
        #endregion
    }
}