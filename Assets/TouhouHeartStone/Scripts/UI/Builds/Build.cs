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
using System.IO.Compression;
using System.IO;
using MukioI18n;
using NitoriNetwork.Common;
using System.Threading.Tasks;

namespace UI
{
    partial class Build
    {
        CardManager cm => ui.getManager<CardManager>();
        DeckManager dm => ui.getManager<DeckManager>();

        LangManager lang => ui.getManager<LangManager>();

        const string exportStringHeader = "# 醉梦传说卡组";

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

        [TranslateMsg]
        const string PILE_NOT_SAVE = "卡组尚未保存，退出后现有变更将丢失。\n是否退出？";

        void onBackBtnClick()
        {
            Main main = GetComponentInParent<Main>();

            if (deckChanged)
            {
                UIDialog.ShowQuestion(lang._(StringTable.HINT_TITLE), lang._(PILE_NOT_SAVE), () =>
                {
                    main.display(main.MainMenu);
                });
            }
            else
            {
                main.display(main.MainMenu);
            }
        }

        [TranslateMsg]
        const string PILE_NOT_COMPLETE = "当前卡组不完整，无法在游戏中使用。\n是否要保存？";

        /// <summary>
        /// 保存卡组按下
        /// </summary>
        void onSaveDeckClick()
        {
            // 卡组保存
            if (BuildDeckList.Select(i => i.count).Sum() < 30)
            {
                UIDialog.ShowQuestion(lang._(StringTable.HINT_TITLE), lang._(PILE_NOT_COMPLETE), () =>
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

        [TranslateMsg]
        const string PILE_EXPORTED = "卡组已导出到剪贴板";

        /// <summary>
        /// 导出卡组
        /// </summary>
        void exportDeck()
        {
            var deck = dm.GetDeck(currentDeckID);
            string outputString = exportStringHeader + " " + deck.Name + "\n";

            List<int> cards = new List<int>(1 + currentDeckCards.Count);
            cards.Add(deck.MasterID);
            cards.AddRange(currentDeckCards);

            outputString += dm.DeckToString(cards.ToArray());
            GUIUtility.systemCopyBuffer = outputString;

            UIDialog.ShowConfirm(lang._(StringTable.HINT_TITLE), lang._(PILE_EXPORTED), null);
        }

        [TranslateMsg]
        const string NO_PILE_AVALIABLE = "剪贴板中没有可导入的卡组";

        [TranslateMsg]
        const string ERROR_TITLE = "错误";

        [TranslateMsg]
        const string CARD_NOT_EXISTS = "ID为{0}的卡不存在";

        [TranslateMsg]
        const string PILE_CODE_INCOMPLETE = "卡组代码不完整";

        [TranslateMsg]
        const string PILE_CODE_CORRUPTED = "卡组数据已损坏";

        [TranslateMsg]
        const string PILE_IMPORT_ERROR = "导入错误\n{0}";

        void tryImportDeck()
        {
            // 解析数据
            string clipboardString = GUIUtility.systemCopyBuffer.Trim();
            if (!clipboardString.StartsWith(exportStringHeader))
            {
                UIDialog.ShowConfirm(lang._(ERROR_TITLE), lang._(NO_PILE_AVALIABLE), null);
                return;
            }

            var lbIndex = clipboardString.IndexOf("\n");
            var deckName = clipboardString.Substring(exportStringHeader.Length + 1, lbIndex - exportStringHeader.Length - 1).Trim();
            var deckContent = clipboardString.Substring(lbIndex + 1).Trim();

            try
            {
                var cards = dm.StringToDeck(deckContent);
                int master = cards[0];

                // 检查卡片是否存在
                for (int i = 0; i < cards.Length; i++)
                {
                    var card = cards[i];
                    if (cm.GetCardDefine(card) == null)
                    {
                        UIDialog.ShowConfirm(lang._(ERROR_TITLE), lang._(CARD_NOT_EXISTS, card), null);
                        return;
                    }
                }

                // 创建卡组
                long deckID = dm.CreateDeck(deckName, master);
                for (int i = 1; i < cards.Length; i++)
                {
                    dm.AddCard(deckID, cards[i]);
                }
                
                // 进入编辑模式
                reloadDeckList();
                deckEnterEditMode((int)deckID);
            }
            catch (FormatException)
            {
                UIDialog.ShowConfirm(lang._(ERROR_TITLE), lang._(PILE_CODE_INCOMPLETE), null);
            }
            catch (IOException)
            {
                UIDialog.ShowConfirm(lang._(ERROR_TITLE), lang._(PILE_CODE_CORRUPTED), null);
            }
            catch (Exception e)
            {
                UIDialog.ShowConfirm(lang._(ERROR_TITLE), lang._(PILE_IMPORT_ERROR, e.Message), null);
                Debug.LogWarning(e);
            }
        }

        [TranslateMsg]
        const string PILE_NOT_COMPLETE_EXPORT = "当前卡组不完整，无法在游戏中使用。\n是否要导出？";

        /// <summary>
        /// 导出当前卡组
        /// </summary>
        void onExportDeckClick()
        {
            // 卡组保存
            if (currentDeckCards.Count < 30)
            {
                UIDialog.ShowQuestion(lang._(StringTable.HINT_TITLE), lang._(PILE_NOT_COMPLETE_EXPORT), () =>
                {
                    exportDeck();
                });
            }
            else
            {
                exportDeck();
            }
        }

        partial void onAwake()
        {
            BackButtonBlack.asButton.onClick.AddListener(onBackBtnClick);

            FilterPanel.onFilterConditionChange += loadCardList;
            FilterPanel.onSortChange += onSortChange;

            EditingDeckDeckListItem.onDeckRename += onDeckRename;
            EditingDeckDeckListItem.SetMode(DeckListItem.Mode.DeckTitle);

            SaveButtonBlackIcon.asButton.onClick.AddListener(onSaveDeckClick);
            ExportButtonBlackIcon.asButton.onClick.AddListener(onExportDeckClick);
            ImportButtonBlackIcon.asButton.onClick.AddListener(tryImportDeck);
            BackButtonBlackIcon.asButton.onClick.AddListener(() =>
            {
                uiModeSwitch(true);
            });

            UploadBtn.onClick.AddListener(onUploadDeckClick);
            DownloadBtn.onClick.AddListener(onDownloadDeckClick);

            MasterCardSelection.OnSelect += onMasterSelect;

            HintMessageText.gameObject.SetActive(false);
            LoadingImg.SetActive(false);
            // UIDialog.gameObject.SetActive(false);
        }

        [TranslateMsg]
        const string PILE_UPLOAD_WARN_MSG = "卡组数据上传功能将会覆盖您在云端的数据，是否上传？";

        [TranslateMsg]
        const string PILE_UPLOAD_NO_LOGIN = "游客无法使用此功能。";

        [TranslateMsg]
        const string PILE_DOWNLOAD_WARN_MSG = "卡组数据下载功能将会从云端同步你的卡组数据并与本地数据合并，可能会覆盖本地的某些卡组，是否下载？";

        [TranslateMsg]
        const string PILE_UPLOAD_SUCCES = "卡组上传完毕";

        [TranslateMsg]
        const string PILE_DOWNLOAD_SUCCES = "卡组下载完毕";

        /// <summary>
        /// 上传卡组
        /// </summary>
        private void onDownloadDeckClick()
        {
            if (!client.AccountIsValid)
            {
                UIDialog.ShowConfirm(lang._(StringTable.HINT_TITLE), lang._(PILE_UPLOAD_NO_LOGIN), null);
                return;
            }
            UIDialog.ShowQuestion(lang._(StringTable.HINT_TITLE), lang._(PILE_DOWNLOAD_WARN_MSG), async () =>
            {
                LoadingImg.SetActive(true);
                try
                {
                    await DownloadDeck();
                    UIDialog.ShowConfirm(lang._(StringTable.HINT_TITLE), lang._(PILE_DOWNLOAD_SUCCES), null);
                    reloadDeckList();
                }
                catch (Exception e)
                {
                    UIDialog.ShowConfirm(lang._(ERROR_TITLE), e.Message, null);
                    Debug.LogError(e);
                }
                finally
                {
                    LoadingImg.SetActive(false);
                }
            });
        }

        /// <summary>
        /// 下载卡组
        /// </summary>
        private void onUploadDeckClick()
        {
            if (!client.AccountIsValid)
            {
                UIDialog.ShowConfirm(lang._(StringTable.HINT_TITLE), lang._(PILE_UPLOAD_NO_LOGIN), null);
                return;
            }
            UIDialog.ShowQuestion(lang._(StringTable.HINT_TITLE), lang._(PILE_UPLOAD_WARN_MSG), async () =>
            {
                LoadingImg.SetActive(true);
                try
                {
                    await UploadDeck();
                    UIDialog.ShowConfirm(lang._(StringTable.HINT_TITLE), lang._(PILE_UPLOAD_SUCCES), null);
                }
                catch (Exception e)
                {
                    UIDialog.ShowConfirm(lang._(ERROR_TITLE), e.Message, null);
                    Debug.LogError(e);
                }
                finally
                {
                    LoadingImg.SetActive(false);
                }
            });
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
        int sortCompareMethod(BuildCardViewData a, BuildCardViewData b)
        {
            return FilterPanel.sortCompareMethod(a.define, b.define);
        }

        /// <summary>
        /// 修改了排序
        /// </summary>
        private void onSortChange()
        {
            Array.Sort(displayedCardList, sortCompareMethod);
            BuildCardList.SetList(displayedCardList.ToArray());
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

        BuildCardViewData[] displayedCardList = new BuildCardViewData[0];
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
        /// 更新当前正在编辑的卡组的卡片列表
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
        /// 加载所有卡，并应用过滤器和排序
        /// </summary>
        void loadCardList()
        {
            var list = new List<BuildCardViewData>();

            foreach (CardDefine card in
                avaliableCards.Where(
                    c => ui.getManager<CardManager>().isStandardCard(c)
                    )
                )
            {
                if (ui.getManager<CardManager>().tryGetSkin(card.id, out var skin))
                {
                    if (FilterPanel.cardFilter(card, skin))
                    {
                        list.Add(new BuildCardViewData()
                        {
                            define = card,
                            skin = skin,
                            useCount = 0,
                            maxCount = card.hasKeyword(KEYWORD.UNIQUE) ? 1 : 2
                        });
                    }
                }
                else
                    UberDebug.LogErrorChannel("UI", "无法找到" + card + "的皮肤");
            }

            displayedCardList = list.ToArray();

            onSortChange();
            updateCardRemainText();
        }

        /// <summary>
        /// 更新卡片剩余数量文本
        /// </summary>
        void updateCardRemainText()
        {
            for (int i = 0; i < displayedCardList.Length; i++)
            {
                var card = displayedCardList[i];
                displayedCardList[i].useCount = currentDeckCards.Where(id => id == card.define.id).Count();
                BuildCardList.SetContent(i, displayedCardList[i]);
            }
        }

        /// <summary>
        /// 更新卡片剩余数量文本
        /// </summary>
        private void updateCardRemainTextAt(int cardID, int count)
        {
            for (int i = 0; i < displayedCardList.Length; i++)
            {
                var card = displayedCardList[i];
                if (card.define.id == cardID)
                {
                    displayedCardList[i].useCount = count;
                    BuildCardList.SetContent(i, displayedCardList[i]);

                }
            }
        }

        void onDeckCardChange()
        {
            DeckCountText.text = currentDeckCards.Count + "/30";
            deckChanged = true;
        }

        public void onDragCardItem(BuildCardListItem item, PointerEventData pointer)
        {
            var cd = avaliableCards.Where(i => i.id == item.CardID).FirstOrDefault();
            onDragItem(cd, pointer.position);
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
                updateCardRemainTextAt(item.card.id, cardCount);

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
            HintMessageText.gameObject.GetComponent<SEPlayer>()?.Play();
        }

        [TranslateMsg]
        const string PILE_FULL = "牌组已满";

        [TranslateMsg]
        const string PILE_MAX_CARD = "最多携带两张相同的卡片";

        [TranslateMsg]
        const string PILE_UNIQUE_CARD = "唯一的卡片只能携带一张";

        [TranslateMsg]
        const string PILE_WRONG_GROUP = "只能选择角色对应的卡或中立卡";

        [TranslateMsg]
        const string DRAG_INSTEAD_CLICK = "请将卡片拖拽到卡组列表中组卡";

        public void onClickHandCard()
        {
            if (isInDeckEditMode)
                showMessage(lang._(DRAG_INSTEAD_CLICK));
        }
        private void onReleaseItem()
        {
            var card = DragCard.BuildDeckListItem.card;
            var cardCount = currentDeckCards.Where(i => i == card.id).Count();

            // 最多30张牌
            if (currentDeckCards.Count >= 30)
            {
                showMessage(lang._(PILE_FULL));
                return;
            }
            // 每类牌最多两张
            if (cardCount >= 2)
            {
                showMessage(lang._(PILE_MAX_CARD));
                return;
            }
            // 唯一
            if (card.hasKeyword(KEYWORD.UNIQUE) && cardCount >= 1)
            {
                showMessage(lang._(PILE_UNIQUE_CARD));
                return;
            }
            // 中立卡或自己的卡
            if (card.getCharacterID() != CATEGORY.CHARACTER_NEUTRAL
                && card.getCharacterID() != currentEditingDeckMasterID)
            {
                showMessage(lang._(PILE_WRONG_GROUP));
                return;
            }

            // 添加卡
            currentDeckCards.Add(card.id);
            cardCount = currentDeckCards.Where(i => i == card.id).Count();

            // 更新可用信息
            updateCardRemainTextAt(card.id, cardCount);

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

        NetworkingManager nwManager => ui.getManager<NetworkingManager>();
        ClientManager client => nwManager.client;

        async Task UploadDeck()
        {
            if (!client.AccountIsValid) 
                return;

            List<DeckDataItem> deckToSend = new List<DeckDataItem>();
            var decks = dm.GetDecks();
            foreach (var item in decks)
            {
                List<int> cards = new List<int>();
                cards.Add(item.MasterID);

                var cs = dm.GetDeckCards(item.ID);
                foreach (var c in cs)
                    cards.Add((int)c.Value);
                
                deckToSend.Add(new DeckDataItem() {
                    ID = (int)item.ID,
                    Name = item.Name,
                    Content = dm.DeckToString(cards.ToArray())
                });
            }

            await client.ServerClient.SetUserDecksAsync(deckToSend.ToArray());
        }

        async Task DownloadDeck()
        {
            if (!client.AccountIsValid)
                return;

            var decks = await client.ServerClient.GetUserDecksAsync();
            foreach (var deck in decks)
            {
                try
                {
                    int id = deck.ID;
                    int[] cards = dm.StringToDeck(deck.Content);

                    if (dm.HasDeck(id))
                    {
                        dm.RenameDeck(id, deck.Name);
                        dm.SetDeckMasterID(id, cards[0]);
                    }
                    else
                    {
                        dm.CreateDeck(id, deck.Name, cards[0]);
                    }

                    dm.ClearDeck(id);
                    for (int i = 1; i < cards.Length; i++)
                    {
                        dm.AddCard(id, cards[i]);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }
    }
}