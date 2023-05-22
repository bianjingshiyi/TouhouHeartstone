using System;
using System.Collections.Generic;
using System.Linq;
using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    public class MultiInputDropdown : MonoBehaviour
    {
        public void setValuesWithoutNotify(string[] values)
        {
            Set(values, false);
        }
        public void setDropdownValues(string[] names, string[] values)
        {
            _dropdownNames = names;
            _dropdownValues = values;
            updateDropdownOptions();
        }
        public void refresh()
        {
            if (!_showingDropdownList)
                return;
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            float originSpeed = _dropdown.alphaFadeSpeed;
            _dropdown.alphaFadeSpeed = 0;
            _dropdown.Show(true);
            _dropdown.alphaFadeSpeed = originSpeed;
        }
        public void onShowDropdownList()
        {
            _showingDropdownList = true;
            _tempValues.RemoveAll(v => !_values.Contains(v));
            updateDropdownOptions();
        }
        public void onHideDropdownList()
        {
            _showingDropdownList = false;
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _dropdown.onValueChanged.AddListener(onDropdownValueChangedCallback);
        }
        private void Update()
        {
            if (!_dropdown.IsActive())
            {
                updateDropdownOptions();
            }
        }
        #endregion
        #region 事件回调
        private void onDropdownValueChangedCallback(bool[] values)
        {
            List<string> strValues = new List<string>();
            for (int i = 0; i < values.Length; i++)
            {
                if (!values[i])
                    continue;
                var str = i < _tempValues.Count ? _tempValues[i] : _dropdownValues[i - _tempValues.Count];
                strValues.Add(str);
            }
            Set(strValues.ToArray(), true);
        }
        #endregion
        private void Set(string[] values, bool sendCallback = true)
        {
            _values.Clear();
            _values.AddRange(values.Where(v => !string.IsNullOrEmpty(v)));
            updateDropdownOptions();
            if (sendCallback)
                onValueChanged?.Invoke(values);
        }
        private void updateDropdownOptions()
        {
            _dropdown.ClearOptions();
            var options = new List<string>();

            bool optionsModified = false;
            foreach (var value in _values)
            {
                if (!_dropdownValues.Contains(value) && !_tempValues.Contains(value))
                {
                    _tempValues.Add(value);
                    optionsModified = true;
                }
            }

            bool[] newValues = new bool[_tempValues.Count + _dropdownNames.Length];
            for (int i = 0; i < newValues.Length; i++)
            {
                var value = i < _tempValues.Count ? _tempValues[i] : _dropdownValues[i - _tempValues.Count];
                newValues[i] = _values.Contains(value);
            }


            options.AddRange(_tempValues);
            options.AddRange(_dropdownNames);
            _dropdown.AddOptions(options);

            _dropdown.SetValueWithoutNotify(newValues);
            if (optionsModified)
                refresh();
        }
        #endregion
        public event Action<string[]> onValueChanged;
        public string[] values
        {
            get => _values.ToArray();
            set
            {
                Set(value, true);
            }
        }
        [SerializeField]
        private bool _showingDropdownList;
        [SerializeField]
        private List<string> _values;
        [SerializeField]
        private List<string> _tempValues;
        [SerializeField]
        MultiSelectDropdown _dropdown;
        [SerializeField]
        string[] _dropdownNames;
        [SerializeField]
        string[] _dropdownValues;
    }
}