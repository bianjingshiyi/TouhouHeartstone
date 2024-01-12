using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ValueInputFieldListItem : MonoBehaviour
    {
        private void Awake()
        {
            _removeButton.onClick.AddListener(() => onClickRemoveButton?.Invoke(this));
            _valueField.onValueChanged += (item, value) => onValueChanged?.Invoke(this, value);
            _valueField.onClickButton += (item, itemIndexes, button) => onClickButton?.Invoke(this, itemIndexes, button);
        }
        public void setReadonly(bool isReadonly)
        {
            bool canEdit = !isReadonly;
            _removeButton.interactable = canEdit;
            _valueField.setReadonly(isReadonly);
        }
        public event Action<ValueInputFieldListItem, object> onValueChanged;
        public event Action<ValueInputFieldListItem, int[], ValueInputFieldButton> onClickButton;
        public event Action<ValueInputFieldListItem> onClickRemoveButton;
        public ValueInputField valueField => _valueField;
        [SerializeField]
        private ValueInputField _valueField;
        [SerializeField]
        private Button _removeButton;
    }
}
