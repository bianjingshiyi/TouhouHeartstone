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
        protected void Awake()
        {
            _editButton.onClick.AddListener(onEditButtonClickCallback);
            _removeButton.onClick.AddListener(onRemoveButtonClickCallback);
        }
        private void onEditButtonClickCallback()
        {
            onEditButtonClick?.Invoke(this);
        }
        private void onRemoveButtonClickCallback()
        {
            onRemoveButtonClick?.Invoke(this);
        }
        public event Action<EffectListItem> onEditButtonClick;
        public event Action<EffectListItem> onRemoveButtonClick;
        [SerializeField]
        Button _editButton;
        [SerializeField]
        Button _removeButton;
        [SerializeField]
        Text _nameText;
    }
}