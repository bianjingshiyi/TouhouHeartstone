
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class VisualEffectEditorDialog : MonoBehaviour
    {
        #region 公有方法
        public void initEffects(string[] effectNames)
        {
            _effectDropdown.ClearOptions();
            _effectDropdown.AddOptions(new List<string>(effectNames));
        }
        public void initSourcePositionOptions(string[] options)
        {
            _sourcePositionDropdown.ClearOptions();
            _sourcePositionDropdown.AddOptions(new List<string>(options));
        }
        public void initTargetPositionOptions(string[] options)
        {
            _targetPositionDropdown.ClearOptions();
            _targetPositionDropdown.AddOptions(new List<string>(options));
        }
        public void setAnimation(int index)
        {
            _effectDropdown.SetValueWithoutNotify(index);
        }
        public void setSourcePosition(int index)
        {
            _sourcePositionDropdown.SetValueWithoutNotify(index);
        }
        public void setTargetPosition(int index)
        {
            _targetPositionDropdown.SetValueWithoutNotify(index);
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _effectDropdown.onValueChanged.AddListener(OnEffectDropdownValueChangedCallback);
            _sourcePositionDropdown.onValueChanged.AddListener(OnSourcePositionDropdownValueChangedCallback);
            _targetPositionDropdown.onValueChanged.AddListener(OnTargetPositionDropdownValueChangedCallback);
            _confirmButton.onClick.AddListener(OnConfirmCallback);
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
        private void OnConfirmCallback()
        {
            onConfirm?.Invoke();
        }
        #endregion
        #region 事件
        public event Action<int> onEffectDropdownValueChanged;
        public event Action<int> onSourcePositionDropdownValueChanged;
        public event Action<int> onTargetPositionDropdownValueChanged;
        public event Action onConfirm;
        #endregion
        #region 属性字段
        [SerializeField]
        private Dropdown _effectDropdown;
        [SerializeField]
        private Dropdown _sourcePositionDropdown;
        [SerializeField]
        private Dropdown _targetPositionDropdown;
        [SerializeField]
        private Button _confirmButton;
        #endregion
    }
}