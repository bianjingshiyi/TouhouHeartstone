using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class CardPoolDependencyItem : MonoBehaviour
    {
        public void update(string name)
        {
            _nameText.text = name;
        }
        protected void Awake()
        {
            _removeButton.onClick.AddListener(onRemoveButtonClickCallback);
        }
        private void onRemoveButtonClickCallback()
        {
            onRemoveButtonClick?.Invoke(this);
        }
        public event Action<CardPoolDependencyItem> onRemoveButtonClick;
        [SerializeField]
        Button _removeButton;
        [SerializeField]
        TextMeshProUGUI _nameText;
    }
}