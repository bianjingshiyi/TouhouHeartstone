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
            _isArrayToggle.SetIsOnWithoutNotify(value);
        }
        public void setIsParams(bool value)
        {
            _isParamsToggle.SetIsOnWithoutNotify(value);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _typeDropdown.onValueChanged.AddListener(onTypeDropdownValueChangedCallback);
            _nameInputField.onValueChanged.AddListener(onNameInputFieldValueChangedCallback);
            _isArrayToggle.onValueChanged.AddListener(onIsArrayToggleValueChangedCallback);
            _isParamsToggle.onValueChanged.AddListener(onIsParamsToggleValueChangedCallback);
            _deleteButton.onClick.AddListener(onDeleteButtonClickedCallback);
        }
        #endregion
        #region 事件回调
        private void onTypeDropdownValueChangedCallback(int index)
        {
            onTypeDropdownValueChanged?.Invoke(this, index);
        }
        private void onNameInputFieldValueChangedCallback(string value)
        {
            onNameInputFieldValueChanged?.Invoke(this, value);
        }
        private void onIsArrayToggleValueChangedCallback(bool value)
        {
            onIsArrayToggleValueChanged?.Invoke(this, value);
        }
        private void onIsParamsToggleValueChangedCallback(bool value)
        {
            onIsParamsToggleValueChanged?.Invoke(this, value);
        }
        private void onDeleteButtonClickedCallback()
        {
            onDeleteButtonClicked?.Invoke(this);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<EffectEditorValueDefineItem, int> onTypeDropdownValueChanged;
        public event Action<EffectEditorValueDefineItem, string> onNameInputFieldValueChanged;
        public event Action<EffectEditorValueDefineItem, bool> onIsArrayToggleValueChanged;
        public event Action<EffectEditorValueDefineItem, bool> onIsParamsToggleValueChanged;
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
        Toggle _isParamsToggle;
        [SerializeField]
        Button _deleteButton;
        #endregion
    }
}