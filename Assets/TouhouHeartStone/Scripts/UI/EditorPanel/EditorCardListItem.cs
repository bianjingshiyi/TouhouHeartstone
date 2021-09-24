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
            _newCardButton.gameObject.SetActive(false);
        }
        public void switchToAddButton()
        {
            _card.gameObject.SetActive(false);
            _newCardButton.gameObject.SetActive(true);
        }
        protected void Awake()
        {
            _button.onClick.AddListener(onClickCallback);
            _newCardButton.onClick.AddListener(onClickCallback);
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
        Button _newCardButton;
        [SerializeField]
        Button _button;
    }
}