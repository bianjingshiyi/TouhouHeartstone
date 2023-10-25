using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PVEInfoPanel : MonoBehaviour
    {
        public void update(string name, Sprite sprite, string desc)
        {
            _masterName.text = name;
            _masterImage.sprite = sprite;
            _descText.text = desc;
        }
        public void updateDeck((string name, Sprite image, int count)[] viewDatas)
        {
            UIHelper.updateList(_deckCardListRoot, _deckCardItemTemplate, _deckCardList, viewDatas.Length,
                (i, item) =>
                {
                    var listItem = item.GetComponent<CardViewDeckCardListItem>();
                    var viewData = viewDatas[i];
                    listItem.updateView(viewData.name, viewData.image, viewData.count);
                });
        }
        private void Awake()
        {
            if (_confirmButton)
                _confirmButton.onClick.AddListener(onConfirmButtonClickedCallback);
            if (_giveUpButton)
                _giveUpButton.onClick.AddListener(onGiveUpButtonClickedCallback);
            if (_quitButton)
                _quitButton.onClick.AddListener(onQuitButtonClickedCallback);
            if (_backgroundButton)
                _backgroundButton.onClick.AddListener(onBackgroundClickedCallback);
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
        private void onBackgroundClickedCallback()
        {
            onBackgroundClicked?.Invoke();
        }
        public event Action onConfirmClicked;
        public event Action onGiveUpClicked;
        public event Action onQuitClicked;
        public event Action onBackgroundClicked;
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
        Button _backgroundButton;

        [Header("卡组显示")]
        [SerializeField]
        RectTransform _deckCardListRoot;
        [SerializeField]
        RectTransform _deckCardItemTemplate;
        [SerializeField]
        List<RectTransform> _deckCardList;
    }
}