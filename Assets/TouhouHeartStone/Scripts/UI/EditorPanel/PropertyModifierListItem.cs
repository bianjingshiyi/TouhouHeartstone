using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace UI
{
    public class PropertyModifierListItem : MonoBehaviour
    {
        #region 公有方法
        public void updateSelectableValues(string[] propertyTypes, string[] modifyTypes)
        {
            _propertyTypeDropdown.ClearOptions();
            _propertyTypeDropdown.AddOptions(new List<string>(propertyTypes));
            _modifyTypeDropdown.ClearOptions();
            _modifyTypeDropdown.AddOptions(new List<string>(modifyTypes));
        }
        public void updateValues(int propertyTypeIndex, int modifyTypeIndex, ActionNodeValueInfo valueInfo)
        {
            _propertyTypeDropdown.SetValueWithoutNotify(propertyTypeIndex);
            _modifyTypeDropdown.SetValueWithoutNotify(modifyTypeIndex);
            _valueField.setValueInfo(valueInfo);
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _propertyTypeDropdown.onValueChanged.AddListener(onPropertyDropdownValueChangedCallback);
            _modifyTypeDropdown.onValueChanged.AddListener(onModifyDropdownValueChangedCallback);
            _valueField.onValueChanged += onValueFieldValueChangedCallback;
            _removeButton.onClick.AddListener(onClickRemoveButtonCallback);
        }
        private void onPropertyDropdownValueChangedCallback(int index)
        {
            onPropertyDropdownValueChanged?.Invoke(this, index);
        }
        private void onModifyDropdownValueChangedCallback(int index)
        {
            onModifyDropdownValueChanged?.Invoke(this, index);
        }
        private void onValueFieldValueChangedCallback(ValueInputField item, object value)
        {
            onValueFieldValueChanged?.Invoke(this, value);
        }
        private void onClickRemoveButtonCallback()
        {
            onClickRemoveButton?.Invoke(this);
        }
        #endregion
        #region 事件
        public event Action<PropertyModifierListItem, int> onPropertyDropdownValueChanged;
        public event Action<PropertyModifierListItem, int> onModifyDropdownValueChanged;
        public event Action<PropertyModifierListItem, object> onValueFieldValueChanged;
        public event Action<PropertyModifierListItem> onClickRemoveButton;
        #endregion
        #region 属性字段
        [SerializeField]
        Dropdown _propertyTypeDropdown;
        [SerializeField]
        Dropdown _modifyTypeDropdown;
        [SerializeField]
        ValueInputField _valueField;
        [SerializeField]
        Button _removeButton;
        #endregion
    }
}