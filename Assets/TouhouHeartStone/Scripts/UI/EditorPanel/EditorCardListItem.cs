using UnityEngine;
using System;
using UnityEngine.UI;

namespace UI
{
    public class EditorCardListItem : MonoBehaviour
    {
        public void switchToCard()
        {
            _card.gameObject.SetActive(true);
            _addButtonGameObject.SetActive(false);
        }
        public void switchToAddButton()
        {
            _card.gameObject.SetActive(false);
            _addButtonGameObject.SetActive(true);
        }
        protected void Awake()
        {
            _button.onClick.AddListener(onClickCallback);
        }
        private void onClickCallback()
        {
            onClick?.Invoke(this);
        }
        public event Action<EditorCardListItem> onClick;
        public Card card => _card;
        [SerializeField]
        Card _card;
        [SerializeField]
        GameObject _addButtonGameObject;
        [SerializeField]
        Button _button;
    }
}