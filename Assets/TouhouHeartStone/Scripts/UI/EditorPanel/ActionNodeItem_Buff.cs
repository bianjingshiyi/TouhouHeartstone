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
            _effectPropertyFields.SetActive(false);
            _inputRoot.gameObject.SetActive(false);
            _outputRoot.gameObject.SetActive(true);
            SetDirty();
        }
        public void setBuffValues(string[] buffNames)
        {
            _buffDropdown.ClearOptions();
            _buffDropdown.AddOptions(new List<string>(buffNames));
            SetDirty();
        }
        public void setBuffValue(int index)
        {
            _buffDropdown.SetValueWithoutNotify(index);
            SetDirty();
        }
        #endregion
        #region 私有方法
        private void onAwake_Buff()
        {
            _buffDropdown.onValueChanged.AddListener(onBuffDropdownValueChanged);
            _editBuffButton.onClick.AddListener(onClickEditBuffButtonCallback);
        }
        #region 事件回调
        private void onBuffDropdownValueChanged(int value)
        {
            onBuffChanged?.Invoke(this, value);
        }
        private void onClickEditBuffButtonCallback()
        {
            onClickEditBuffButton?.Invoke(this);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<ActionNodeItem, int> onBuffChanged;
        public event Action<ActionNodeItem> onClickEditBuffButton;
        #endregion
        #region 属性字段

        [Header("增益")]
        [SerializeField]
        RectTransform _buffFieldsRoot;
        [SerializeField]
        Dropdown _buffDropdown;
        [SerializeField]
        Button _editBuffButton;
        #endregion
    }
}