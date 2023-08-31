
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class VisualEffectEditorDialog : MonoBehaviour
    {
        #region 公有方法
        public void setEffects(string[] effectNames)
        {
            _effectDropdown.ClearOptions();
            _effectDropdown.AddOptions(new List<string>(effectNames));
        }
        public void setSourcePositionOptions(string[] options)
        {
            _sourcePositionDropdown.ClearOptions();
            _sourcePositionDropdown.AddOptions(new List<string>(options));
        }
        public void setTargetPositionOptions(string[] options)
        {
            _targetPositionDropdown.ClearOptions();
            _targetPositionDropdown.AddOptions(new List<string>(options));
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _effectDropdown.onValueChanged.AddListener(OnEffectDropdownValueChangedCallback);
            _sourcePositionDropdown.onValueChanged.AddListener(OnSourcePositionDropdownValueChangedCallback);
            _targetPositionDropdown.onValueChanged.AddListener(OnTargetPositionDropdownValueChangedCallback);
        }
        private void OnEffectDropdownValueChangedCallback(int value)
        {
            onEffectDropdownValueChanged?.Invoke(value);
        }
        private void OnSourcePositionDropdownValueChangedCallback(int value)
        {
            onSourcePositionDropdownValueChanged?.Invoke(value);
        }
        private void OnTargetPositionDropdownValueChangedCallback(int value)
        {
            onTargetPositionDropdownValueChanged?.Invoke(value);
        }
        #endregion
        #region 事件
        public event Action<int> onEffectDropdownValueChanged;
        public event Action<int> onSourcePositionDropdownValueChanged;
        public event Action<int> onTargetPositionDropdownValueChanged;
        #endregion
        #region 属性字段
        [SerializeField]
        private Dropdown _effectDropdown;
        [SerializeField]
        private Dropdown _sourcePositionDropdown;
        [SerializeField]
        private Dropdown _targetPositionDropdown;
        #endregion
    }
}