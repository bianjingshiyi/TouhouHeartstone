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
        /// <summary>
        /// 设置皮肤语言索引
        /// </summary>
        /// <param name="languageIndex"></param>
        public void setBuffLanguage(int languageIndex)
        {
            _buffLanguageDropdown.SetValueWithoutNotify(languageIndex);
        }
        /// <summary>
        /// 设置皮肤有哪些语言可选
        /// </summary>
        /// <param name="languages"></param>
        public void initBuffLanguages(string[] languages, string[] keywordNames, string[] keywordValues)
        {
            _buffLanguageDropdown.ClearOptions();
            _buffLanguageDropdown.AddOptions(new List<string>(languages));
            _propModifierTemplate.GetComponent<PropertyModifierListItem>().init(keywordNames, keywordValues);
        }
        /// <summary>
        /// 设置增益皮肤
        /// </summary>
        /// <param name="buffName"></param>
        /// <param name="buffDesc"></param>
        /// <param name="buffImage"></param>
        public void setBuffSkin(string buffName, string buffDesc, Sprite buffImage)
        {
            _buffNameInputField.SetTextWithoutNotify(buffName);
            _buffDescInputField.SetTextWithoutNotify(buffDesc);
            _buffImage.sprite = buffImage;
        }
        public void setupPropModifierTemplate(string[] propTypes)
        {
            PropertyModifierListItem item = _propModifierTemplate.GetComponent<PropertyModifierListItem>();
            item.updateSelectableValues(propTypes);
        }
        public void updatePropModifierList(ActionNodePropertyModifierViewData[] viewData)
        {
            UIHelper.updateList(_propModifierRoot, _propModifierTemplate, _propModifierList, viewData.Length, (i, itemRect) =>
            {
                PropertyModifierListItem item = itemRect.GetComponent<PropertyModifierListItem>();
                item.updateValues(viewData[i].propertyTypeIndex, viewData[i].modifyTypes, viewData[i].modifyTypeIndex, new ActionNodeValueInfo()
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
        private void onAwake_Buff()
        {
            _buffDropdown.onValueChanged.AddListener(onBuffDropdownValueChanged);
            _newBuffButton.onClick.AddListener(onClickNewBuffButtonCallback);
            _deleteBuffButton.onClick.AddListener(onClickDeleteBuffButtonCallback);
            _buffLanguageDropdown.onValueChanged.AddListener(onBuffLanguageDropdownValueChanged);
            _buffNameInputField.onEndEdit.AddListener(onBuffNameInputFieldEndEdit);
            _buffDescInputField.onEndEdit.AddListener(onBuffDescInputFieldEndEdit);
            _buffImageButton.onClick.AddListener(onClickBuffImage);
        }
        #region 事件回调
        private void onBuffDropdownValueChanged(int value)
        {
            onBuffChanged?.Invoke(this, value);
        }
        private void onClickNewBuffButtonCallback()
        {
            onClickNewBuffButton?.Invoke(this);
        }
        private void onClickDeleteBuffButtonCallback()
        {
            onClickDeleteBuffButton?.Invoke(this);
        }
        private void onBuffLanguageDropdownValueChanged(int value)
        {
            onBuffLanguageChanged?.Invoke(this, value);
        }
        private void onBuffNameInputFieldEndEdit(string value)
        {
            onBuffNameChanged?.Invoke(this, value);
        }
        private void onBuffDescInputFieldEndEdit(string value)
        {
            onBuffDescChanged?.Invoke(this, value);
        }
        private void onClickBuffImage()
        {
            onNeedSelectBuffImage?.Invoke(this);
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
        #endregion
        #region 事件
        public event Action<ActionNodeItem, int> onBuffChanged;
        public event Action<ActionNodeItem> onClickNewBuffButton;
        public event Action<ActionNodeItem> onClickDeleteBuffButton;
        public event Action<ActionNodeItem, int> onBuffLanguageChanged;
        public event Action<ActionNodeItem, string> onBuffNameChanged;
        public event Action<ActionNodeItem, string> onBuffDescChanged;
        public event Action<ActionNodeItem> onNeedSelectBuffImage;
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
        Dropdown _buffLanguageDropdown;
        [SerializeField]
        Image _buffImage;
        [SerializeField]
        Button _buffImageButton;
        [SerializeField]
        InputField _buffNameInputField;
        [SerializeField]
        InputField _buffDescInputField;
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