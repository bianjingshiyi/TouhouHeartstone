using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using TouhouHeartstone;
using System.Reflection;
using Game;
using TouhouHeartstone.Builtin;

namespace UI
{
    partial class Build
    {
        CardManager cm => ui.getManager<CardManager>();
        DeckManager dm => ui.getManager<DeckManager>();

        partial void onAwake()
        {
            BackButtonBlack.asButton.onClick.AddListener(() =>
            {
                /**
                PlayerPrefs.SetInt("DeckCount", BuildDeckList.Select(i => i.count).Sum() + 1);
                int index = 1;
                foreach (var item in BuildDeckList.getItems())
                {
                    for (int i = 0; i < item.count; i++)
                    {
                        PlayerPrefs.SetInt("Deck" + index, item.card.id);
                        parent.game.deck[index] = item.card.id;
                        index++;
                    }
                }
                PlayerPrefs.Save();*/
                Main main = GetComponentInParent<Main>();
                main.display(main.MainMenu);
            });

            FilterPanel.onFilterConditionChange += reloadCardList;
            FilterPanel.onSortChange += onSortChange;

            EditingDeckDeckListItem.onDeckRename += onDeckRename;
            EditingDeckDeckListItem.SetMode(DeckListItem.Mode.DeckTitle);
            EditingDeckDeckListItem.onDeckSelect += (id) =>
            {
                dm.ClearDeck(id);
                foreach (var item in BuildDeckList.getItems())
                {
                    for (int i = 0; i < item.count; i++)
                    {
                        dm.AddCard(id, item.card.id);
                    }
                }
                uiModeSwitch(true);

            }; // 卡组保存

            uiModeSwitch(true);
        }

        private void onSortChange()
        {
            BuildCardList.sortItems(FilterPanel.sortCompareMethod);
        }

        void reloadDeckList()
        {
            var decks = dm.GetDecks();

            DeckList.clearItems();

            foreach (var deck in decks)
            {
                Sprite spr = cm.getSkin(deck.MasterID)?.image;

                var item = DeckList.addItem();
                item.SetContent((int)deck.ID, deck.Name, spr);
                item.SetMode(DeckListItem.Mode.DeckListItem);

                item.onDeckSelect += onDeckSelect;
                item.onDeckRemove += onDeckRemove;
            }

            var addBtn = DeckList.addItem();
            addBtn.SetMode(DeckListItem.Mode.AddDeck);
            addBtn.onDeckAdd += onDeckAdd;
        }

        void onDeckSelect(int id)
        {
            deckEnterEditMode(id);
        }

        void onDeckRemove(int id)
        {
            dm.RemoveDeck(id);
            reloadDeckList();
        }

        void onDeckAdd()
        {
            var id = dm.CreateDeck("新牌组", Patchouli.ID);
            reloadDeckList();
        }

        void onDeckRename(int id, string name)
        {
            dm.RenameDeck(id, name);
            reloadDeckList();
        }

        CardDefine[] cards { get; set; } = null;
        protected override void onDisplay()
        {
            base.onDisplay();

            cards = ui.getManager<CardManager>().GetCardDefines(d => d.GetType().Assembly == typeof(THHGame).Assembly);

            reloadDeckList();
            reloadCardList();
        }

        /// <summary>
        /// 使用卡组进入卡组编辑模式
        /// </summary>
        /// <param name="deck"></param>
        private void deckEnterEditMode(int deckID)
        {
            var deck = dm.GetDeck(deckID);
            var deckCards = dm.GetDeckCards(deckID);

            CardSkinData masterSkin = ui.getManager<CardManager>().getSkin(deck.MasterID);
            EditingDeckDeckListItem.SetContent(deckID, deck.Name, masterSkin.image);
            MasterNameText.text = masterSkin.name;

            BuildDeckList.clearItems();
            foreach (var cards in deckCards.Select(card => cards.FirstOrDefault(c => c.id == card.Value)).Where(c => c != null).GroupBy(c => c.id))
            {
                var item = BuildDeckList.addItem();
                var skin = ui.getManager<CardManager>().getSkin(cards.First().id);
                item.update(cards.First(), skin, cards.Count());
            }
            BuildDeckList.sortItems((a, b) =>
            {
                if (a.card.getCost() != b.card.getCost())
                    return a.card.getCost() - b.card.getCost();
                else
                    return a.card.id - b.card.id;
            });
            DragCard.hide();

            uiModeSwitch(false);
        }

        /// <summary>
        /// 当前是否位于卡组编辑模式
        /// </summary>
        bool isInDeckEditMode;

        /// <summary>
        /// 切换模式
        /// </summary>
        /// <param name="deckMode">卡组列表模式</param>
        void uiModeSwitch(bool deckMode)
        {
            DeckListViewScroll.gameObject.SetActive(deckMode);
            DeckCardViewImage.gameObject.SetActive(!deckMode);

            isInDeckEditMode = !deckMode;
        }

        private void reloadCardList()
        {
            BuildCardList.clearItems();
            foreach (CardDefine card in
                cards.Where(
                    c => ui.getManager<CardManager>().isStandardCard(c)
                    )
                )
            {
                if (ui.getManager<CardManager>().tryGetSkin(card.id, out var skin))
                {
                    if (FilterPanel.cardFilter(card, skin))
                    {
                        var item = BuildCardList.addItem();
                        item.update(card, skin);
                    }
                }
                else
                    UberDebug.LogErrorChannel("UI", "无法找到" + card + "的皮肤");

                BuildCardList.sortItems(FilterPanel.sortCompareMethod);
            }
        }
        private void Update()
        {
            DeckCountText.text = BuildDeckList.getItems().Select(i => i.count).Sum() + "/30";

            //if (DragCard.isDisplaying)
            //{
            //    var input = EventSystem.current.currentInputModule.input;
            //    DragCard.transform.position = input.mousePosition;
            //    Vector2 position = input.mousePosition;
            //    if (DeckViewScroll.GetComponent<RectTransform>().rect.Contains(DeckViewScroll.GetComponent<RectTransform>().InverseTransformPoint(position)))
            //    {
            //        if (!DragCard.BuildDeckListItem.isDisplaying)
            //        {
            //            DragCard.BuildDeckListItem.display();
            //            DragCard.BuildDeckListItem.update(DragCard.BuildDeckListItem.card, parent.game.cards.GetCardSkin(DragCard.BuildDeckListItem.card.id), 1);
            //        }
            //        DragCard.Card.hide();
            //    }
            //    else
            //    {
            //        if (!DragCard.Card.isDisplaying)
            //        {
            //            DragCard.Card.display();
            //            DragCard.Card.update(DragCard.BuildDeckListItem.card, parent.game.cards.GetCardSkin(DragCard.BuildDeckListItem.card.id));
            //        }
            //        DragCard.BuildDeckListItem.hide();
            //    }
            //    if (input.GetMouseButtonUp(0))
            //    {
            //        //停止拖拽
            //        stopDrag(position);
            //    }
            //}
        }
        public void onDragCardItem(BuildCardListItem item, PointerEventData pointer)
        {
            onDragItem(item.card, pointer.position);
        }
        public void onDragDeckItem(BuildDeckListItem item, PointerEventData pointer)
        {
            onDragItem(item.card, pointer.position);
        }
        public void onReleaseCardItem(BuildCardListItem item, PointerEventData pointer)
        {
            DragCard.hide();
            Vector3 localPosition = DeckViewScroll.GetComponent<RectTransform>().InverseTransformPoint(pointer.position);
            if (DeckViewScroll.GetComponent<RectTransform>().rect.Contains(localPosition))
            {
                onReleaseItem();
            }
        }
        public void onReleaseDeckItem(BuildDeckListItem item, PointerEventData pointer)
        {
            DragCard.hide();
            Vector3 localPosition = DeckViewScroll.GetComponent<RectTransform>().InverseTransformPoint(pointer.position);
            if (DeckViewScroll.GetComponent<RectTransform>().rect.Contains(localPosition))
            {
                onReleaseItem();
            }
            else
            {
                if (item.count < 1)
                    BuildDeckList.removeItem(item);
            }
        }
        void onDragItem(CardDefine card, Vector2 position)
        {
            if (!isInDeckEditMode) return;

            DragCard.display();
            DragCard.rectTransform.position = position;
            if (DeckViewScroll.GetComponent<RectTransform>().rect.Contains(DeckViewScroll.GetComponent<RectTransform>().InverseTransformPoint(position)))
            {
                DragCard.BuildDeckListItem.display();
                DragCard.Card.hide();
            }
            else
            {
                DragCard.Card.display();
                DragCard.BuildDeckListItem.hide();
            }
            DragCard.BuildDeckListItem.update(card, ui.getManager<CardManager>().getSkin(card.id), 1);
            DragCard.Card.update(card, ui.getManager<CardManager>().getSkin(card.id));
        }
        private void onReleaseItem()
        {
            if (BuildDeckList.Select(i => i.count).Sum() >= 30)
            {
                //最多30张牌
                return;
            }
            var deckItem = BuildDeckList.getItems().FirstOrDefault(i => i.card == DragCard.BuildDeckListItem.card);
            if (deckItem != null)
            {
                deckItem.count++;
                deckItem.getChild("Root").display();
                deckItem.rectTransform.sizeDelta = BuildDeckList.defaultItem.rectTransform.sizeDelta;
            }
            else
            {
                var newItem = BuildDeckList.addItem();
                newItem.update(DragCard.BuildDeckListItem.card, ui.getManager<CardManager>().getSkin(DragCard.BuildDeckListItem.card.id), 1);
            }
            BuildDeckList.sortItems((a, b) =>
            {
                if (a.card.getCost() != b.card.getCost())
                    return a.card.getCost() - b.card.getCost();
                else
                    return a.card.id - b.card.id;
            });
        }
    }
}