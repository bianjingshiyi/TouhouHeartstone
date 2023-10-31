using System;
using System.Collections.Generic;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;
using UIHelper = BJSYGameCore.UI.UIHelper;

namespace UI
{
    public class PVESelectCardPanel : MonoBehaviour
    {
        public void updateCards(CardDefine[] cards, CardManager cardManager)
        {
            UIHelper.updateList(_cardListRoot, _cardItemTemplate, _cardList, cards.Length,
                async (i, item) =>
                {
                    var choice = item.GetComponent<PVECardChoice>();
                    var card = cards[i];
                    await choice.update(card, cardManager);
                },
                item =>
                {
                    var choice = item.GetComponent<PVECardChoice>();
                    choice.onClicked += onChoiceClickedCallback;
                },
                item =>
                {
                    var choice = item.GetComponent<PVECardChoice>();
                    choice.onClicked -= onChoiceClickedCallback;
                });
        }
        private void Awake()
        {
            if (_confirmButton)
                _confirmButton.onClick.AddListener(onConfirmButtonClickedCallback);
            if (_skipButton)
                _skipButton.onClick.AddListener(onSkipButtonClickedCallback);
            if (_quitButton)
                _quitButton.onClick.AddListener(onQuitButtonClickedCallback);
        }
        private void onChoiceClickedCallback(PVECardChoice choice)
        {
            onChoiceClicked?.Invoke(_cardList.IndexOf(choice.transform as RectTransform));
        }
        private void onConfirmButtonClickedCallback()
        {
            onConfirmClicked?.Invoke();
        }
        private void onSkipButtonClickedCallback()
        {
            onSkipClicked?.Invoke();
        }
        private void onQuitButtonClickedCallback()
        {
            onQuitClicked?.Invoke();
        }
        public event Action<int> onChoiceClicked;
        public event Action onConfirmClicked;
        public event Action onSkipClicked;
        public event Action onQuitClicked;
        [SerializeField]
        RectTransform _cardListRoot;
        [SerializeField]
        RectTransform _cardItemTemplate;
        [SerializeField]
        List<RectTransform> _cardList = new List<RectTransform>();
        [SerializeField]
        Button _confirmButton;
        [SerializeField]
        Button _skipButton;
        [SerializeField]
        Button _quitButton;
    }
}