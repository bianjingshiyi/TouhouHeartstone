using System;
using Game;
using BJSYGameCore;
using System.Collections.Generic;
using TouhouCardEngine;
using System.Linq;

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

        public void SetItems(IEnumerable<DeckDefine> decks)
        {
            MasterCardList.clearItems();
            foreach (var deck in decks)
            {
                var c = MasterCardList.addItem();
                var skin = cardManager.getSkin(deck.MasterID);
                c.SetContent((int)deck.ID, deck.Name, skin?.image);
                c.SetMode(DeckListItem.Mode.DeckSelect);
                c.onDeckSelect += (id) => {
                    OnSelect?.Invoke(id);
                };
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
                c.onDeckSelect += (id) => {
                    OnSelect?.Invoke(id);
                };
            }
        }

        public void SetItems(IEnumerable<CardDefine> cards)
        {
            SetItems(cards.Select(c => c.id));
        }

        public event Action<int> OnSelect;

        public void ClearEvents()
        {
            OnSelect = null;
        }
    }
}