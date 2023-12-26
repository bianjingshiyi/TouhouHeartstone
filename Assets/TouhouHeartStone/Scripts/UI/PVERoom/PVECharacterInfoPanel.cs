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
        public void updateDeck(DeckCardViewData[] viewDatas)
        {
            _deckView.update(viewDatas);
        }
        private void Awake()
        {
            if (_confirmButton)
                _confirmButton.onClick.AddListener(() => onConfirmClicked?.Invoke());
            if (_giveUpButton)
                _giveUpButton.onClick.AddListener(() => onGiveUpClicked?.Invoke());
            if (_quitButton)
                _quitButton.onClick.AddListener(() => onQuitClicked?.Invoke());
            if (_inventoryButton)
                _inventoryButton.onClick.AddListener(() => onInventoryClicked?.Invoke());
        }
        public event Action onConfirmClicked;
        public event Action onGiveUpClicked;
        public event Action onQuitClicked;
        public event Action onInventoryClicked;
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
        Button _inventoryButton;
        [SerializeField]
        DeckCardList _deckView;
    }
}