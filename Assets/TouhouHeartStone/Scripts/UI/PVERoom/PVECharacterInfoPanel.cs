using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PVECharacterInfoPanel : MonoBehaviour
    {
        public void update(string name, Sprite sprite, string desc)
        {
            _masterName.text = name;
            _masterImage.sprite = sprite;
            _descText.text = desc;
        }
        public void updateDeck((string name, Sprite image, int count)[] viewDatas)
        {
            _deckView.update(viewDatas);
        }
        private void Awake()
        {
            if (_confirmButton)
                _confirmButton.onClick.AddListener(onConfirmButtonClickedCallback);
            if (_giveUpButton)
                _giveUpButton.onClick.AddListener(onGiveUpButtonClickedCallback);
            if (_quitButton)
                _quitButton.onClick.AddListener(onQuitButtonClickedCallback);
        }
        private void onConfirmButtonClickedCallback()
        {
            onConfirmClicked?.Invoke();
        }
        private void onGiveUpButtonClickedCallback()
        {
            onGiveUpClicked?.Invoke();
        }
        private void onQuitButtonClickedCallback()
        {
            onQuitClicked?.Invoke();
        }
        public event Action onConfirmClicked;
        public event Action onGiveUpClicked;
        public event Action onQuitClicked;
        [SerializeField]
        TextMeshProUGUI _masterName;
        [SerializeField]
        Image _masterImage;
        [SerializeField]
        TextMeshProUGUI _descText;
        [SerializeField]
        Button _confirmButton;
        [SerializeField]
        Button _giveUpButton;
        [SerializeField]
        Button _quitButton;
        [SerializeField]
        DeckCardList _deckView;
    }
}