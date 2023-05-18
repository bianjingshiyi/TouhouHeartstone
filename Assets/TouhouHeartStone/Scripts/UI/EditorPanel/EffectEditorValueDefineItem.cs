using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class EffectEditorValueDefineItem : MonoBehaviour
    {
        #region 公有方法
        public void initTypeDropdown(string[] types)
        {
            _typeDropdown.ClearOptions();
            _typeDropdown.AddOptions(new List<string>(types));
        }
        public void setType(int typeIndex)
        {
            _typeDropdown.SetValueWithoutNotify(typeIndex);
        }
        public void setName(string name)
        {
            _nameInputField.SetTextWithoutNotify(name);
        }
        public void setIsArray(bool value)
        {
            if (_isArrayToggle != null)
            {
                _isArrayToggle.SetIsOnWithoutNotify(value);
            }
        }
        public void setShowParams(bool value)
        {
            if (_isParamsObj != null)
            {
                _isParamsObj.SetActive(value);
            }
        }
        public void setIsParams(bool value)
        {
            if (_isParamsToggle != null)
            {
                _isParamsToggle.SetIsOnWithoutNotify(value);
            }
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _deleteButton.onClick.AddListener(onDeleteButtonClickedCallback);
        }
        #endregion
        #region 事件回调
        private void onDeleteButtonClickedCallback()
        {
            onDeleteButtonClicked?.Invoke(this);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<EffectEditorValueDefineItem> onDeleteButtonClicked;
        #endregion
        #region 属性字段
        [SerializeField]
        Dropdown _typeDropdown;
        [SerializeField]
        InputField _nameInputField;
        [SerializeField]
        Toggle _isArrayToggle;
        [SerializeField]
        GameObject _isParamsObj;
        [SerializeField]
        Toggle _isParamsToggle;
        [SerializeField]
        Button _deleteButton;
        #endregion
    }
}