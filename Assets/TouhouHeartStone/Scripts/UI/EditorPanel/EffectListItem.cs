using System;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class EffectListItem : MonoBehaviour
    {
        public void update(string name)
        {
            _nameText.text = name;
        }
        public void showInputField(string value)
        {
            _nameInputFieldObj.SetActive(true);
            _nameInputField.text = value;
            _nameInputField.ActivateInputField();
            _nameText.gameObject.SetActive(false);
        }
        public void hideInputField()
        {
            _nameInputFieldObj.SetActive(false);
            _nameText.gameObject.SetActive(true);
        }
        protected void Awake()
        {
            _editButton.onClick.AddListener(onEditButtonClickCallback);
            _removeButton.onClick.AddListener(onRemoveButtonClickCallback);
            _nameInputField?.onEndEdit.AddListener(onInputFieldEndEditCallback);
        }
        private void onEditButtonClickCallback()
        {
            onEditButtonClick?.Invoke(this);
        }
        private void onRemoveButtonClickCallback()
        {
            onRemoveButtonClick?.Invoke(this);
        }
        private void onInputFieldEndEditCallback(string value)
        {
            onInputFieldEndEdit?.Invoke(this, value);
            hideInputField();
        }
        public event Action<EffectListItem> onEditButtonClick;
        public event Action<EffectListItem> onRemoveButtonClick;
        public event Action<EffectListItem, string> onInputFieldEndEdit;
        [SerializeField]
        Button _editButton;
        [SerializeField]
        Button _removeButton;
        [SerializeField]
        Text _nameText;
        [SerializeField]
        GameObject _nameInputFieldObj;
        [SerializeField]
        InputField _nameInputField;
    }
}