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
            _dropdownNames.Clear();
            _dropdownNames.AddRange(names);
            _dropdownValues.Clear();
            _dropdownValues.AddRange(values);
            refreshOptions();
        }
        public void updateOptionToggles()
        {
            if (!_showingDropdownList || !_dropdownList)
                return;
            var options = _tempValues.Concat(_dropdownValues).ToArray();
            for (int i = 0; i < options.Length; i++)
            {
                _dropdownList.setOptionSelected(i, _values.Contains(options[i]));
            }
        }
        public void refreshShowingDropdown()
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
        public void onShowDropdownList(MultiDropdownList dropdownList)
        {
            _dropdownList = dropdownList;
            _showingDropdownList = true;
            _tempValues.RemoveAll(v => !_values.Contains(v));
            refreshOptions();
        }
        public void onHideDropdownList(MultiDropdownList dropdownList)
        {
            _dropdownList = null;
            _showingDropdownList = false;
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _dropdown.template.GetComponent<MultiDropdownList>().isTemplate = true;
            _dropdown.onValueChanged.AddListener(onDropdownValueChangedCallback);
        }
        private void Update()
        {
            if (!_dropdown.IsActive())
            {
                refreshOptions();
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
            refreshOptions();
            if (sendCallback)
                onValueChanged?.Invoke(values);
        }
        private bool refreshOptions()
        {
            bool optionsModified = false;
            foreach (var value in _values)
            {
                if (!_dropdownValues.Contains(value) && !_tempValues.Contains(value))
                {
                    _tempValues.Add(value);
                    optionsModified = true;
                }
            }
            _dropdown.ClearOptions();
            _dropdown.AddOptions(_tempValues.Concat(_dropdownNames).ToList());
            updateDropdownValues();
            if (optionsModified)
                refreshShowingDropdown();

            return optionsModified;
        }
        private void updateDropdownValues()
        {
            var newValues = _tempValues.Concat(_dropdownValues).Select(v => _values.Contains(v)).ToArray();
            _dropdown.SetValueWithoutNotify(newValues);
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
        private MultiDropdownList _dropdownList;
        [SerializeField]
        private bool _showingDropdownList;
        [SerializeField]
        private List<string> _values;
        [SerializeField]
        private List<string> _tempValues;
        [SerializeField]
        MultiSelectDropdown _dropdown;
        [SerializeField]
        List<string> _dropdownNames;
        [SerializeField]
        List<string> _dropdownValues;
    }
}