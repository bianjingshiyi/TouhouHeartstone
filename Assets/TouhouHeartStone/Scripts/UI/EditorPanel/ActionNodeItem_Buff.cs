using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    partial class ActionNodeItem
    {
        #region 公有方法
        public void switchToBuffNode()
        {
            _buffFieldsRoot.gameObject.SetActive(true);
            _triggerFieldsRoot.gameObject.SetActive(false);
            _inputRoot.gameObject.SetActive(false);
            _valueRoot.gameObject.SetActive(false);
            _outputRoot.gameObject.SetActive(true);
        }
        public void setBuffValues(string[] buffNames)
        {
            _buffDropdown.ClearOptions();
            _buffDropdown.AddOptions(new List<string>(buffNames));
        }
        public void setBuffValue(int index)
        {
            _buffDropdown.SetValueWithoutNotify(index);
        }
        public void setBuffId(string text)
        {
            _buffIdText.text = text;
        }
        public void setupPropModifierTemplate(string[] propTypes, string[] modifyTypes)
        {
            PropertyModifierListItem item = _propModifierTemplate.GetComponent<PropertyModifierListItem>();
            item.updateSelectableValues(propTypes, modifyTypes);
        }
        public void updatePropModifierList(ActionNodePropertyModifierViewData[] viewData)
        {
            UIHelper.updateList(_propModifierRoot, _propModifierTemplate, _propModifierList, viewData.Length, (i, itemRect) =>
            {
                PropertyModifierListItem item = itemRect.GetComponent<PropertyModifierListItem>();
                item.updateValues(viewData[i].propertyTypeIndex, viewData[i].modifyTypeIndex, new ActionNodeValueInfo()
                {
                    type = viewData[i].valueType,
                    value = viewData[i].value
                });
            }, itemRect =>
            {
                PropertyModifierListItem item = itemRect.GetComponent<PropertyModifierListItem>();
                item.onPropertyDropdownValueChanged += onPropertyModifierPropTypeValueChangedCallback;
                item.onModifyDropdownValueChanged += onPropertyModifierModifyTypeValueChangedCallback;
                item.onValueFieldValueChanged += onPropertyModifierValueChangedCallback;
                item.onClickRemoveButton += onClickRemovePropModifierButtonCallback;
            }, itemRect =>
            {
                PropertyModifierListItem item = itemRect.GetComponent<PropertyModifierListItem>();
                item.onPropertyDropdownValueChanged -= onPropertyModifierPropTypeValueChangedCallback;
                item.onModifyDropdownValueChanged -= onPropertyModifierModifyTypeValueChangedCallback;
                item.onValueFieldValueChanged -= onPropertyModifierValueChangedCallback;
                item.onClickRemoveButton -= onClickRemovePropModifierButtonCallback;
            });
        }
        public void updateEffectList(IEnumerable<string> effectTypeNames, string[] effectNames)
        {
            _effectList.setAddEffectMenu(effectTypeNames);
            _effectList.updateList(effectNames);
        }
        #endregion
        #region 私有方法
        private void onClickNewBuffButtonCallback()
        {
            onClickNewBuffButton?.Invoke(this);
        }
        private void onClickDeleteBuffButtonCallback()
        {
            onClickDeleteBuffButton?.Invoke(this);
        }
        private void onClickAddPropModifierButtonCallback()
        {
            onClickAddPropModifierButton?.Invoke(this);
        }
        private void onClickRemovePropModifierButtonCallback(PropertyModifierListItem item)
        {
            onClickRemovePropModifierButton?.Invoke(this, _propModifierList.IndexOf(item.transform as RectTransform));
        }
        private void onPropertyModifierPropTypeValueChangedCallback(PropertyModifierListItem item, int index)
        {
            onPropModifierPropertyTypeChanged?.Invoke(this, _propModifierList.IndexOf(item.transform as RectTransform), index);
        }
        private void onPropertyModifierModifyTypeValueChangedCallback(PropertyModifierListItem item, int index)
        {
            onPropModifierModifyTypeChanged?.Invoke(this, _propModifierList.IndexOf(item.transform as RectTransform), index);
        }
        private void onPropertyModifierValueChangedCallback(PropertyModifierListItem item, object value)
        {
            onPropModifierValueChanged?.Invoke(this, _propModifierList.IndexOf(item.transform as RectTransform), value);
        }
        private void onClickPropertyModifierAddEffectMenuCallback(int index)
        {
            onPropModifierAddEffect?.Invoke(this, index);
        }
        private void onClickPropertyModifierEffectRemoveButtonCallback(int index)
        {
            onPropModifierRemoveEffect?.Invoke(this, index);
        }
        private void onClickPropertyModifierEffectEditButtonCallback(int index)
        {
            onPropModifierEditEffect?.Invoke(this, index);
        }
        #endregion
        #region 事件
        public event Action<ActionNodeItem, int> onBuffChanged;
        public event Action<ActionNodeItem> onClickNewBuffButton;
        public event Action<ActionNodeItem> onClickDeleteBuffButton;
        public event Action<ActionNodeItem> onClickAddPropModifierButton;
        public event Action<ActionNodeItem, int> onClickRemovePropModifierButton;
        public event Action<ActionNodeItem, int, int> onPropModifierPropertyTypeChanged;
        public event Action<ActionNodeItem, int, int> onPropModifierModifyTypeChanged;
        public event Action<ActionNodeItem, int, object> onPropModifierValueChanged;
        public event Action<ActionNodeItem, int> onPropModifierAddEffect;
        public event Action<ActionNodeItem, int> onPropModifierRemoveEffect;
        public event Action<ActionNodeItem, int> onPropModifierEditEffect;
        #endregion
        #region 属性字段
        [SerializeField]
        RectTransform _buffFieldsRoot;
        [SerializeField]
        Dropdown _buffDropdown;
        [SerializeField]
        Button _newBuffButton;
        [SerializeField]
        Button _deleteBuffButton;
        [SerializeField]
        Text _buffIdText;
        [SerializeField]
        RectTransform _propModifierRoot;
        [SerializeField]
        RectTransform _propModifierTemplate;
        [SerializeField]
        List<RectTransform> _propModifierList = new List<RectTransform>();
        [SerializeField]
        Button _addPropModifierButton;
        [SerializeField]
        EffectList _effectList;
        #endregion
    }
}