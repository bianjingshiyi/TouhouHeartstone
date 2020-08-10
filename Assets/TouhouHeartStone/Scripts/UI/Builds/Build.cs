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
                // 返回的时候也顺便保存
                saveToDeck(EditingDeckDeckListItem.ID);

                Main main = GetComponentInParent<Main>();
                main.display(main.MainMenu);
            });

            FilterPanel.onFilterConditionChange += reloadCardList;
            FilterPanel.onSortChange += onSortChange;

            EditingDeckDeckListItem.onDeckRename += onDeckRename;
            EditingDeckDeckListItem.SetMode(DeckListItem.Mode.DeckTitle);
            EditingDeckDeckListItem.onDeckSelect += (id) =>
            {
                // 卡组保存
                saveToDeck(id);
                uiModeSwitch(true);
            };

            MasterCardSelection.OnSelect += onMasterSelect;

            HintMessageText.gameObject.SetActive(false);
        }

        private void saveToDeck(int id)
        {
            dm.ClearDeck(id);
            foreach (var item in BuildDeckList.getItems())
            {
                for (int i = 0; i < item.count; i++)
                {
                    dm.AddCard(id, item.card.id);
                }
            }
        }

        int sortCompareMethod(BuildCardListItem a, BuildCardListItem b)
        {
            var ca = getCardItem(a);
            var cb = getCardItem(b);
            return FilterPanel.sortCompareMethod(ca.define, cb.define);
        }

        private CardItem getCardItem(BuildCardListItem a)
        {
            return cardItemList.Where(c => c.buildItem == a).FirstOrDefault();
        }

        /// <summary>
        /// 修改了排序
        /// </summary>
        private void onSortChange()
        {
            BuildCardList.sortItems(sortCompareMethod);
        }

        private void OnEnable()
        {
            MasterCardSelection.hide();
            uiModeSwitch(true);
        }

        /// <summary>
        /// 重载当前卡组的列表
        /// </summary>
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

                item.onDeckSelect += deckEnterEditMode;
                item.onDeckRemove += onDeckRemove;
            }

            var addBtn = DeckList.addItem();
            addBtn.SetMode(DeckListItem.Mode.AddDeck);
            addBtn.onDeckAdd += onDeckAdd;
        }

        /// <summary>
        /// 移除卡组并重载列表
        /// </summary>
        /// <param name="id"></param>
        void onDeckRemove(int id)
        {
            dm.RemoveDeck(id);
            reloadDeckList();
        }

        void onDeckAdd()
        {
            // 点击添加后，弹出人物选择窗口
            showMasterSelection();
        }

        void onDeckRename(int id, string name)
        {
            dm.RenameDeck(id, name);
            reloadDeckList();
        }

        void onMasterSelect(int id)
        {
            // 然后在窗口选择之后再正式新建卡组
            MasterCardSelection.hide();
            var skin = cm.getSkin(id);
            var deckID = dm.CreateDeck(skin?.name ?? "新牌组", id);
            reloadDeckList();
            deckEnterEditMode((int)deckID);
        }

        void showMasterSelection()
        {
            MasterCardSelection.SetItems(cards.Where(c => isMaster(c)));
            MasterCardSelection.display();
        }

        private static bool isMaster(CardDefine c)
        {
            if (c.id == 0)
                return false;
            if (c.GetType().Assembly != typeof(THHGame).Assembly)
                return false;
            if (c is MasterCardDefine master)
            {
                return true;
            }
            else
                return false;
        }

        struct CardItem
        {
            public CardDefine define;
            public CardSkinData skin;
            public BuildCardListItem buildItem;
        }

        List<CardItem> cardItemList = new List<CardItem>();

        CardDefine[] cards { get; set; } = null;
        protected override void onDisplay()
        {
            base.onDisplay();

            cards = ui.getManager<CardManager>().GetCardDefines(d => d.GetType().Assembly == typeof(THHGame).Assembly);

            reloadDeckList();
            loadCardList();
        }

        int currentEditingDeckMasterID = 0;

        /// <summary>
        /// 使用卡组进入卡组编辑模式
        /// </summary>
        /// <param name="deck"></param>
        private void deckEnterEditMode(int deckID)
        {
            var deck = dm.GetDeck(deckID);
            var deckCards = dm.GetDeckCards(deckID);

            CardSkinData masterSkin = ui.getManager<CardManager>().getSkin(deck.MasterID);
            EditingDeckDeckListItem.SetContent(deckID, deck.Name, masterSkin?.image);
            currentEditingDeckMasterID = deck.MasterID;

            MasterNameText.text = masterSkin?.name;

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

        void loadCardList()
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
                    var item = BuildCardList.addItem();
                    cardItemList.Add(new CardItem() { buildItem = item, define = card, skin = skin });
                    item.Card.setCard(card, skin);
                    UberDebug.LogChannel("UI", "添加卡" + card + "列表项目");
                }
                else
                    UberDebug.LogErrorChannel("UI", "无法找到" + card + "的皮肤");

                BuildCardList.sortItems(sortCompareMethod);
            }
        }

        private void reloadCardList()
        {
            foreach (var item in cardItemList)
            {
                item.buildItem.gameObject.SetActive(FilterPanel.cardFilter(item.define, item.skin));
                item.buildItem.Card.isFaceup = true; // Animator默认状态是false，重置之后得还原
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
            onDragItem(getCardItem(item).define, pointer.position);
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
            DragCard.Card.setCard(card, ui.getManager<CardManager>().getSkin(card.id));
        }

        void showMessage(string msg)
        {
            HintMessageText.text = msg;
            HintMessageText.gameObject.SetActive(false);
            HintMessageText.gameObject.SetActive(true);
        }

        private void onReleaseItem()
        {
            var card = DragCard.BuildDeckListItem.card;
            var deckItem = BuildDeckList.getItems().FirstOrDefault(i => i.card == card);

            // 最多30张牌
            if (BuildDeckList.Select(i => i.count).Sum() >= 30)
            {
                showMessage("牌组已满");
                return;
            }
            // 每类牌最多两张
            if (deckItem?.count >= 2)
            {
                showMessage("最多携带两张相同的卡片");
                return;
            }
            // 唯一
            if (card.hasKeyword(Keyword.UNIQUE) && deckItem?.count >= 1)
            {
                showMessage("唯一的卡片只能携带一张");
                return;
            }
            // 中立卡或自己的卡
            if (card.getCharacterID() != CardCategory.CHARACTER_NEUTRAL
                && card.getCharacterID() != currentEditingDeckMasterID)
            {
                showMessage("只能选择角色对应的卡或中立卡");
                return;
            }

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