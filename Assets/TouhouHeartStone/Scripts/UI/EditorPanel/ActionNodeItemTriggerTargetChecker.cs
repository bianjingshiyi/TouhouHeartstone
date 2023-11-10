using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class ActionNodeItemTriggerTargetChecker : MonoBehaviour
    {
        public void setTargetTypes(string[] targetTypes)
        {
            _targetDropdown.ClearOptions();
            _targetDropdown.AddOptions(new List<string>(targetTypes));
        }
        public void setTargetType(int value)
        {
            _targetDropdown.SetValueWithoutNotify(value);
        }
        public void setName(string name)
        {
            _nameText.text = name;
        }
        public void setErrorTip(string text)
        {
            _errorTipInputField.SetTextWithoutNotify(text);
        }
        public void setReadonly(bool isReadonly)
        {
            var canEdit = !isReadonly;
            _targetDropdown.interactable = canEdit;
            _editButton.interactable = canEdit;
            _removeButton.interactable = canEdit;
            _errorTipInputField.interactable = canEdit;
        }
        protected void Awake()
        {
            _targetDropdown.onValueChanged.AddListener(onTargetDropdownValueChangedCallback);
            _editButton.onClick.AddListener(onClickEditButtonCallback);
            _removeButton.onClick.AddListener(onClickRemoveButtonCallback);
            _errorTipInputField.onEndEdit.AddListener(onEndEditErrorTipInputField);
        }
        private void onTargetDropdownValueChangedCallback(int value)
        {
            onTargetDropdownValueChanged?.Invoke(this, value);
        }
        private void onClickEditButtonCallback()
        {
            onClickEditButton?.Invoke(this);
        }
        private void onClickRemoveButtonCallback()
        {
            onClickRemoveButton?.Invoke(this);
        }
        private void onEndEditErrorTipInputField(string text)
        {
            onErrorTipInputFieldValueChanged?.Invoke(this, text);
        }
        public event Action<ActionNodeItemTriggerTargetChecker, int> onTargetDropdownValueChanged;
        public event Action<ActionNodeItemTriggerTargetChecker> onClickEditButton;
        public event Action<ActionNodeItemTriggerTargetChecker> onClickRemoveButton;
        public event Action<ActionNodeItemTriggerTargetChecker, string> onErrorTipInputFieldValueChanged;
        [SerializeField]
        Dropdown _targetDropdown;
        [SerializeField]
        Button _editButton;
        [SerializeField]
        Button _removeButton;
        [SerializeField]
        Text _nameText;
        [SerializeField]
        InputField _errorTipInputField;
    }
}