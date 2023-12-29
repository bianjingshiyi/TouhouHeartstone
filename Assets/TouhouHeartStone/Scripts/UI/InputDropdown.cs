using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InputDropdown : MonoBehaviour
    {
        public void setValueWithoutNotify(string value)
        {
            _value = value;
            updateDropdownOptions();
        }
        public void setDropdownValues(string[] names, string[] values)
        {
            _dropdownNames = names;
            _dropdownValues = values;
            updateDropdownOptions();
        }
        public void hide()
        {
            _dropdown.Hide();
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _dropdown.onValueChanged.AddListener(onDropdownValueChangedCallback);
        }
        #endregion
        #region 事件回调
        private void onDropdownValueChangedCallback(int index)
        {
            if (!string.IsNullOrEmpty(_tempValue))
            {
                index--;
            }
            value = index < 0 ? _tempValue : _dropdownValues[index];
        }
        #endregion
        private void updateDropdownOptions()
        {
            _dropdown.ClearOptions();
            var options = new List<string>();
            int index = getValueDropdownIndex(value);
            if (index < 0 && !string.IsNullOrEmpty(value))
            {
                _tempValue = value;
                options.Add(_tempValue);
                index++;
            }
            else
            {
                _tempValue = null;
            }
            options.AddRange(_dropdownNames);
            _dropdown.AddOptions(options);
            _dropdown.SetValueWithoutNotify(index);
            hide();
        }
        private int getValueDropdownIndex(string value)
        {
            for (int i = 0; i < _dropdownValues.Length; i++)
            {
                if (_dropdownValues[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion
        public event Action<string> onValueChanged;
        public string value
        {
            get => _value;
            set
            {
                setValueWithoutNotify(value);
                onValueChanged?.Invoke(value);
            }
        }
        public bool interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                _dropdown.interactable = value;
            }
        }
        private bool _interactable = true;
        [SerializeField]
        private string _value;
        [SerializeField]
        private string _tempValue;
        [SerializeField]
        Dropdown _dropdown;
        [SerializeField]
        string[] _dropdownNames;
        [SerializeField]
        string[] _dropdownValues;
    }
}