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
using System;
using BJSYGameCore.UI;
namespace UI
{
    partial class Build
    {
        CardManager cm => ui.getManager<CardManager>();
        DeckManager dm => ui.getManager<DeckManager>();

        /// <summary>
        /// 当前正在编辑的卡组ID
        /// </summary>
        int currentDeckID = 0;

        /// <summary>
        /// 当前正在编辑的卡组的Master卡的ID
        /// </summary>
        int currentEditingDeckMasterID = 0;

        /// <summary>
        /// 当前正在编辑的卡组的卡片
        /// </summary>
        List<int> currentDeckCards { get; set; } = new List<int>();

        /// <summary>
        /// 当前卡组是否修改
        /// </summary>
        bool deckChanged { get; set; } = false;

        void onBackBtnClick()
        {
            Main main = GetComponentInParent<Main>();

            if (deckChanged)
            {
                UIDialog.ShowQuestion("卡组尚未保存，退出后现有变更将丢失。\n是否退出？", () =>
                {
                    main.display(main.MainMenu);
                });
            }
            else
            {
                main.display(main.MainMenu);
            }
        }

        /// <summary>
        /// 保存卡组按下
        /// </summary>
        void onSaveDeckClick()
        {
            // 卡组保存
            if (BuildDeckList.Select(i => i.count).Sum() < 30)
            {
                UIDialog.ShowQuestion("当前卡组不完整，无法在游戏中使用。\n是否要保存？", () =>
                {
                    saveToDeck(currentDeckID);
                    uiModeSwitch(true);
                });
            }
            else
            {
                saveToDeck(currentDeckID);
                uiModeSwitch(true);
            }
        }

        partial void onAwake()
        {
            BackButtonBlack.asButton.onClick.AddListener(onBackBtnClick);

            FilterPanel.onFilterConditionChange += reloadCardList;
            FilterPanel.onSortChange += onSortChange;

            EditingDeckDeckListItem.onDeckRename += onDeckRename;
            EditingDeckDeckListItem.SetMode(DeckListItem.Mode.DeckTitle);

            SaveButtonBlackIcon.asButton.onClick.AddListener(onSaveDeckClick);
            BackButtonBlackIcon.asButton.onClick.AddListener(() =>
            {
                uiModeSwitch(true);
            });

            MasterCardSelection.OnSelect += onMasterSelect;

            HintMessageText.gameObject.SetActive(false);
            UIDialog.gameObject.SetActive(false);
        }

        /// <summary>
        /// 保存到卡组中
        /// </summary>
        /// <param name="id"></param>
        private void saveToDeck(int id)
        {
            dm.ClearDeck(id);
            foreach (var card in currentDeckCards)
            {
                dm.AddCard(id, card);
            }

            // 更新Mark
            reloadDeckList();
        }

        /// <summary>
        /// 比较方法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        int sortCompareMethod(BuildCardListItem a, BuildCardListItem b)
        {
            var ca = getCardItem(a);
            var cb = getCardItem(b);
            return FilterPanel.sortCompareMethod(ca.define, cb.define);
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
                item.SetSelectable(true);
                item.SetWarning(dm.GetDeckCards(deck.ID).Count < 30);

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

        /// <summary>
        /// 添加卡组点击后
        /// </summary>
        void onDeckAdd()
        {
            // 点击添加后，弹出人物选择窗口
            showMasterSelection();
        }

        /// <summary>
        /// 卡组重命名后
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        void onDeckRename(int id, string name)
        {
            dm.RenameDeck(id, name);
            reloadDeckList();
        }

        /// <summary>
        /// 卡组添加完选择完人物后
        /// </summary>
        /// <param name="id"></param>
        void onMasterSelect(int id)
        {
            // 然后在窗口选择之后再正式新建卡组
            MasterCardSelection.hide();
            var skin = cm.getSkin(id);
            var deckID = dm.CreateDeck(skin?.name ?? "新牌组", id);
            reloadDeckList();
            deckEnterEditMode((int)deckID);
        }

        /// <summary>
        /// 显示任务选择对话框
        /// </summary>
        void showMasterSelection()
        {
            var masters = avaliableCards.Where(c => isMaster(c));
            foreach (var master in masters)
            {
                Debug.Log("Found " + master);
            }
            MasterCardSelection.SetItems(masters);
            MasterCardSelection.display();
        }

        /// <summary>
        /// 判断是不是Master卡
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
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

        private CardItem getCardItem(BuildCardListItem a)
        {
            return cardItemList.Where(c => c.buildItem == a).FirstOrDefault();
        }
        private CardItem getCardItem(CardDefine a)
        {
            return GetCardItem(a.id);
        }
        private CardItem GetCardItem(int cardID)
        {
            return cardItemList.Where(c => c.define.id == cardID).FirstOrDefault();
        }

        /// <summary>
        /// 所有可用的卡
        /// </summary>
        CardDefine[] avaliableCards { get; set; } = null;
        protected override void onDisplay()
        {
            base.onDisplay();

            avaliableCards = ui.getManager<CardManager>().GetCardDefines(d => d.GetType().Assembly == typeof(THHGame).Assembly
            && d.GetType().GetCustomAttribute<IgnoreCardDefineAttribute>() == null);

            reloadDeckList();
            loadCardList();

            HintMessageText.gameObject.SetActive(false);
        }

        /// <summary>
        /// 使用卡组进入卡组编辑模式
        /// </summary>
        /// <param name="deck"></param>
        private void deckEnterEditMode(int deckID)
        {
            currentDeckID = deckID;

            var deck = dm.GetDeck(deckID);

            // 皮肤
            CardSkinData masterSkin = ui.getManager<CardManager>().getSkin(deck.MasterID);
            EditingDeckDeckListItem.SetContent(deckID, deck.Name, masterSkin?.image);
            currentEditingDeckMasterID = deck.MasterID;
            MasterNameText.text = masterSkin?.name;

            // 卡片列表
            var deckCards = dm.GetDeckCards(deckID);
            currentDeckCards = deckCards.Values.Select(i => (int)i).ToList();

            // 更新UI
            onDeckCardChange();
            deckChanged = false;

            DragCard.hide();
            updateUIDeckCardsList();

            uiModeSwitch(false);
        }

        /// <summary>
        /// 更新列表
        /// </summary>
        void updateUIDeckCardsList()
        {
            BuildDeckList.clearItems();
            foreach (var cards in currentDeckCards.Select(card => avaliableCards.FirstOrDefault(c => c.id == card)).Where(c => c != null).GroupBy(c => c.id))
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
            DeckListViewImage.gameObject.SetActive(deckMode);
            DeckCardViewImage.gameObject.SetActive(!deckMode);

            isInDeckEditMode = !deckMode;

            // 退出组卡时清理
            if (deckMode)
            {
                currentDeckCards.Clear();
            }

            updateCardRemainText();
            deckChanged = false;
        }

        /// <summary>
        /// 加载所有卡
        /// </summary>
        void loadCardList()
        {
            BuildCardList.clearItems();
            cardItemList.Clear();

            foreach (CardDefine card in
                avaliableCards.Where(
                    c => ui.getManager<CardManager>().isStandardCard(c)
                    )
                )
            {
                if (ui.getManager<CardManager>().tryGetSkin(card.id, out var skin))
                {
                    var item = BuildCardList.addItem();
                    item.setRemainText(0, card.hasKeyword(KEYWORD.UNIQUE) ? 1 : 2);
                    cardItemList.Add(new CardItem() { buildItem = item, define = card, skin = skin });
                    item.Card.setCard(card, skin);
                }
                else
                    UberDebug.LogErrorChannel("UI", "无法找到" + card + "的皮肤");
            }

            BuildCardList.sortItems(sortCompareMethod);
        }

        /// <summary>
        /// 重载卡片列表
        /// </summary>
        private void reloadCardList()
        {
            foreach (var item in cardItemList)
            {
                if (item.buildItem != null)
                {
                    item.buildItem.gameObject.SetActive(FilterPanel.cardFilter(item.define, item.skin));
                    item.buildItem.Card.isFaceup = true; // Animator默认状态是false，重置之后得还原
                    item.buildItem.Card.setCard(item.define, item.skin);
                }
                else
                {
                    Debug.LogWarning($"{item}.buildItem does not exists.");
                }
            }

            onSortChange(); // 更新排序
        }

        /// <summary>
        /// 更新卡片剩余数量文本
        /// </summary>
        void updateCardRemainText()
        {
            foreach (var card in cardItemList)
            {
                var cardCount = currentDeckCards.Where(i => i == card.define.id).Count();
                card.buildItem.setRemainText(cardCount, card.define.hasKeyword(KEYWORD.UNIQUE) ? 1 : 2);
            }
        }

        void onDeckCardChange()
        {
            DeckCountText.text = currentDeckCards.Count + "/30";
            deckChanged = true;
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
                item.recovery();
            }
            else
            {
                if (currentDeckCards.Contains(item.card.id))
                    currentDeckCards.Remove(item.card.id);

                // 更新可用信息
                var cardCount = currentDeckCards.Where(i => i == item.card.id).Count();
                getCardItem(item.card).buildItem.setRemainText(cardCount, item.card.hasKeyword(KEYWORD.UNIQUE) ? 1 : 2);

                // 更新UI
                if (item.count < 1)
                    BuildDeckList.removeItem(item);

                onDeckCardChange();
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
            var cardCount = currentDeckCards.Where(i => i == card.id).Count();

            // 最多30张牌
            if (currentDeckCards.Count >= 30)
            {
                showMessage("牌组已满");
                return;
            }
            // 每类牌最多两张
            if (cardCount >= 2)
            {
                showMessage("最多携带两张相同的卡片");
                return;
            }
            // 唯一
            if (card.hasKeyword(KEYWORD.UNIQUE) && cardCount >= 1)
            {
                showMessage("唯一的卡片只能携带一张");
                return;
            }
            // 中立卡或自己的卡
            if (card.getCharacterID() != CATEGORY.CHARACTER_NEUTRAL
                && card.getCharacterID() != currentEditingDeckMasterID)
            {
                showMessage("只能选择角色对应的卡或中立卡");
                return;
            }

            // 添加卡
            currentDeckCards.Add(card.id);
            cardCount = currentDeckCards.Where(i => i == card.id).Count();

            // 更新可用信息
            getCardItem(card).buildItem.setRemainText(cardCount, card.hasKeyword(KEYWORD.UNIQUE) ? 1 : 2);

            // 标记为已经修改
            onDeckCardChange();

            // 更新UI
            var deckItem = BuildDeckList.getItems().FirstOrDefault(i => i.card == card);
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
                deckItem = newItem;
            }
            BuildDeckList.sortItems((a, b) =>
            {
                if (a.card.getCost() != b.card.getCost())
                    return a.card.getCost() - b.card.getCost();
                else
                    return a.card.id - b.card.id;
            });
        }

        public void DeckItemSelect(BuildDeckListItem item)
        {
            DragCard.display();
            DragCard.Card.display();
            DragCard.BuildDeckListItem.hide();

            DragCard.Card.setCard(item.card, ui.getManager<CardManager>().getSkin(item.card.id));

            var rt = DragCard.rectTransform;
            var rt2 = item.GetComponent<RectTransform>();

            Vector2 point = new Vector2(rt2.position.x - (rt2.rect.width + rt.rect.width) / 2, rt2.position.y);
            point.y = Math.Max(point.y, rt.rect.height / 2 + 10);

            rt.position = point;
        }

        public void DeckItemDeselect(BuildDeckListItem item)
        {
            DragCard.hide();
        }
    }
}