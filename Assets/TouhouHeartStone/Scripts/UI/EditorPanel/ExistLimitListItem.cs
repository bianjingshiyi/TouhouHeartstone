using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ExistLimitListItem : MonoBehaviour
    {
        #region 公有方法
        public void update(string header, int eventIndex, int count)
        {
            _header.text = header;
            _eventDropdown.SetValueWithoutNotify(eventIndex);
            _countInputField.SetTextWithoutNotify(count.ToString());
        }
        public void initEvents(string[] eventNames)
        {
            _eventDropdown.ClearOptions();
            _eventDropdown.AddOptions(new List<string>(eventNames));
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _eventDropdown.onValueChanged.AddListener(onEventDropdownValueChangedCallback);
            _countInputField.onValueChanged.AddListener(onCountValueChangedCallback);
            _removeButton.onClick.AddListener(onClickRemoveButtonCallback);
        }
        private void onEventDropdownValueChangedCallback(int index)
        {
            onEventDropdownValueChanged?.Invoke(this, index);
        }
        private void onCountValueChangedCallback(string value)
        {
            if (!int.TryParse(value, out int intValue))
            {
                intValue = 0;
            }
            onCountValueChanged?.Invoke(this, intValue);
        }
        private void onClickRemoveButtonCallback()
        {
            onClickRemoveButton?.Invoke(this);
        }
        #endregion
        #region 事件
        public event Action<ExistLimitListItem, int> onEventDropdownValueChanged;
        public event Action<ExistLimitListItem, int> onCountValueChanged;
        public event Action<ExistLimitListItem> onClickRemoveButton;
        #endregion
        #region 属性字段
        [SerializeField]
        Text _header;
        [SerializeField]
        Dropdown _eventDropdown;
        [SerializeField]
        InputField _countInputField;
        [SerializeField]
        Button _removeButton;
        #endregion
    }
}