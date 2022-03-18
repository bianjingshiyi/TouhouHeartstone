using System;
using Game;
using BJSYGameCore;
using System.Collections.Generic;
using TouhouCardEngine;
using System.Linq;
using UnityEngine;

namespace UI
{
    partial class MasterCardSelection
    {
        CardManager cardManager => this.findInstance<CardManager>();

        partial void onAwake()
        {
            asButton.onClick.AddListener(hide);
            CloseButtonBlack.asButton.onClick.AddListener(hide);
        }

        public void SetItems(IEnumerable<DeckDefine> decks, Action<DeckDefine, DeckListItem> action = null)
        {
            MasterCardList.clearItems();
            foreach (var deck in decks)
            {
                var c = MasterCardList.addItem();
                var skin = cardManager.getSkin(deck.MasterID);
                c.SetContent((int)deck.ID, deck.Name, skin?.image);
                c.SetMode(DeckListItem.Mode.DeckSelect);
                c.onDeckSelect += (id) =>
                {
                    OnSelect?.Invoke(id);
                };
                action?.Invoke(deck, c);
            }
        }

        public void SetItems(IEnumerable<int> cards)
        {
            MasterCardList.clearItems();
            foreach (var card in cards)
            {
                var c = MasterCardList.addItem();
                var skin = cardManager.getSkin(card);
                c.SetContent(card, skin?.name, skin?.image);
                c.SetMode(DeckListItem.Mode.DeckSelect);
                c.onDeckSelect += (id) =>
                {
                    OnSelect?.Invoke(id);
                };
            }
        }
        public void updateItemList((string name, Sprite image)[] viewData)
        {
            MasterCardList.clearItems();
            _itemList.Clear();
            for (int i = 0; i < viewData.Length; i++)
            {
                var c = MasterCardList.addItem();
                _itemList.Add(c.transform as RectTransform);
                c.updateView(viewData[i].name, viewData[i].image);
                c.SetMode(DeckListItem.Mode.DeckSelect);
                c.onClick += onClickItemCallback;
            }
        }
        [Obsolete("此方法已弃用，请使用\"UI.DeckViewPanel.update()\"")]
        public void updateItemList((string name, Sprite image, bool isWarning)[] viewData)
        {
            MasterCardList.clearItems();
            _itemList.Clear();
            for (int i = 0; i < viewData.Length; i++)
            {
                var c = MasterCardList.addItem();
                _itemList.Add(c.transform as RectTransform);
                c.updateView(viewData[i].name, viewData[i].image);
                c.setWarning(viewData[i].isWarning);
                c.SetMode(DeckListItem.Mode.DeckSelect);
                c.SetSelectable(!viewData[i].isWarning);
                c.onClick += onClickItemCallback;
            }
        }
        public void SetItems(IEnumerable<CardDefine> cards)
        {
            SetItems(cards.Select(c => c.id));
        }

        public event Action<int> OnSelect;
        [Obsolete("此事件已弃用，请使用\"UI.DeckViewPanel.onClickDeckListItem\"")]
        public event Action<int> onClickItem;
        public void ClearEvents()
        {
            OnSelect = null;
        }
        private void onClickItemCallback(DeckListItem item)
        {
            onClickItem?.Invoke(_itemList.IndexOf(item.transform as RectTransform));
        }
        [SerializeField]
        List<RectTransform> _itemList = new List<RectTransform>();
    }
}