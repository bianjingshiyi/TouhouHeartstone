using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using TouhouHeartstone;
using System.Reflection;
using Game;
namespace UI
{
    partial class Build
    {
        partial void onAwake()
        {
            ReturnButton.onClick.AddListener(() =>
            {
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
                PlayerPrefs.Save();
                parent.display(parent.MainMenu);
            });

            FilterPanel.onFilterConditionChange += reloadCardList;
            FilterPanel.onSortChange += onSortChange;
        }

        private void onSortChange()
        {
            BuildCardList.sortItems(FilterPanel.sortCompareMethod);
        }

        CardDefine[] cards { get; set; } = null;
        protected override void onDisplay()
        {
            base.onDisplay();
            //目前只有一个卡组
            cards = parent.game.getManager<CardManager>().GetCardDefines(d => d.GetType().Assembly == typeof(THHGame).Assembly);
            int[] deck = parent.game.deck;
            CardSkinData masterSkin = parent.game.cards.getSkin(deck[0]);
            MasterButton.setSprite(masterSkin.image);
            MasterNameText.text = masterSkin.name;
            BuildDeckList.clearItems();
            foreach (var cards in deck.Skip(1).Select(id => cards.FirstOrDefault(c => c.id == id)).Where(c => c != null).GroupBy(c => c.id))
            {
                var item = BuildDeckList.addItem();
                var skin = parent.game.cards.getSkin(cards.First().id);
                item.update(cards.First(), skin, cards.Count());
            }
            reloadCardList();
            BuildDeckList.sortItems((a, b) =>
            {
                if (a.card.getCost() != b.card.getCost())
                    return a.card.getCost() - b.card.getCost();
                else
                    return a.card.id - b.card.id;
            });
            DragCard.hide();
        }

        private void reloadCardList()
        {
            BuildCardList.clearItems();
            foreach (CardDefine card in
                cards.Where(
                    c => isStandardCard(c)
                    )
                )
            {
                if (parent.game.cards.tryGetSkin(card.id, out var skin))
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

        private bool isStandardCard(CardDefine c)
        {
            if (c.id == 0)
                return false;
            //if (!(c is SpellCardDefine))
            //{
            //    if (!(c is ServantCardDefine) && !(c is GeneratedCardDefine))
            //        return false;
            //}
            if (!(c is ServantCardDefine) && !(c is GeneratedCardDefine))
                return false;
            if (c is ServantCardDefine servant)
            {
                if (servant.isToken)
                    return false;
            }
            else if (c is GeneratedCardDefine generated)
            {
                if (generated.type != CardDefineType.SERVANT)
                    return false;
                if (generated.getProp<bool>(nameof(ServantCardDefine.isToken)))
                    return false;
            }
            return true;
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
            DragCard.BuildDeckListItem.update(card, parent.game.cards.getSkin(card.id), 1);
            DragCard.Card.update(card, parent.game.cards.getSkin(card.id));
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
                newItem.update(DragCard.BuildDeckListItem.card, parent.game.cards.getSkin(DragCard.BuildDeckListItem.card.id), 1);
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