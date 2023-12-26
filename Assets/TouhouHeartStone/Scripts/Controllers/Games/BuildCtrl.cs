using Game;
using MukioI18n;
using NitoriNetwork.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Deck = TouhouHeartstone.Deck;
namespace Controllers
{
    class BuildCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        /// <summary>
        /// 进入收藏模式
        /// </summary>
        public async void display()
        {
            //清理字段
            _deck = null;
            _isDeckChanged = false;
            await _cardPoolViewPanelCtrl.display(cardFilter: cardFilter);
            _selectableMasters = game.card.findCardDefines(c => {
                if (c.isMaster())
                {
                    long cardPoolId = c.cardPoolId;
                    var cardPoolRef = game.card.getLocalCardPoolReference(cardPoolId);
                    // 组卡界面能使用的英雄，必须是玩家自己有的卡池里面的
                    // 缓存卡池里面的英雄不能算进去。
                    if (cardPoolRef != null)
                    {
                        return game.card.isLocalCardPool(cardPoolRef);
                    }
                }
                return false;
            });

            //UI更新
            _ui.gameObject.SetActive(true);
            _ui.showErrorTip(false);
            _ui.setEnableDrag(false);
            _ui.setDeckListActive(true);
            _ui.setEditDeckPanelActive(false);
            await updateDeckListUI();
        }
        /// <summary>
        /// 进入构筑模式
        /// </summary>
        /// <param name="masterId"></param>
        public async void display(Deck deck)
        {
            _deck = deck;
            _isDeckChanged = false;

            //切换角色筛选
            await _cardPoolViewPanelCtrl.display(characterRefs: new DefineReference[] { deck.masterCardRef }, cardFilter: cardFilter);
            var cardPoolRef = game.card.getLocalCardPoolReference(deck.masterCardRef.cardPoolId);
            CardSkinData masterSkin = game.card.getCardSkin(cardPoolRef, deck.masterCardRef.defineId);

            // 更新UI
            _ui.gameObject.SetActive(true);
            _ui.showErrorTip(false);
            _ui.updateCurCardPoolInfo(deck.name, await loadDeckImage(deck));
            var masterName = masterSkin.name;
            if (masterName.Length > masterNameMaxLength)
                masterName = game.lang._(MASTER_NAME_OVERFLOW_TEMPLATE, masterName.Substring(0, masterNameMaxLength));
            _ui.setCharacterName(masterName);
            _ui.setDeckListActive(false);
            _ui.setEditCardListActive(true);
            _ui.setEnableDrag(true);
            _ui.hideDragCard();
            //右侧栏
            _ui.setDeckListActive(false);
            _ui.setEditDeckPanelActive(true);
            await updateDeckCardListUI();
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponent<BuildCardPanel>();
            }
            _ui.onClickReturnButton += onReturnButtonClick;
            _ui.onStartDragDeckCardListItem += onStartDragDeckCardListItem;
            _ui.onDragDeckCardListItem += onDragDeckCardListItem;
            _ui.onEndDragDeckCardListItem += onEndDragDeckCardListItem;
            _ui.onClickDeckListItem += onClickDeckListItem;
            _ui.onClickDeckListItemDeleteButton += onClickDeckListItemDeleteButton;
            _ui.onDeckListItemNameValueChanged += onDeckListItemNameValueChanged;
            _ui.onClickDeckListAddButton += onClickDeckListAddButton;
            _ui.onClickMasterSelectPanelItem += onMasterSelect;
            _ui.onClickSaveDeckButton += onSaveDeckClick;
            _ui.onClickExitEditButton += onExitEditButtonClick;
            _ui.onClickExportDeckButton += onExportDeckClick;
            _ui.onClickImportDeckButton += onClickImportButton;
            _ui.onClickUploadDeckButton += onUploadDeckClick;
            _ui.onClickDownloadDeckButton += onDownloadDeckClick;
            _ui.onDeckNameInputFieldValueChanged += onDeckNameInputFieldValueChanged;

            _cardPoolViewPanelCtrl.onUpdateCardListItem += onUpdateCardListItem;
            _cardPoolViewPanelCtrl.onStartDragCard += onStartDragCard;
            _cardPoolViewPanelCtrl.onDragCard += onDragCard;
            _cardPoolViewPanelCtrl.onEndDragCard += onEndDragCard;
        }
        #endregion
        #region 事件回调
        private void onReturnButtonClick()
        {
            if (_isDeckChanged)
            {
                game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), game.lang._(StringTable.BUILD_CONFIRM_EXIT_WITHOUT_SAVE), () =>
                {
                    _ui.gameObject.SetActive(false);
                    scene.menu.display();
                });
            }
            else
            {
                _ui.gameObject.SetActive(false);
                scene.menu.display();
            }
        }
        /// <summary>
        /// 当卡牌列表需要更新显示，更新对应物体的显示
        /// </summary>
        /// <param name="index"></param>
        /// <param name="itemRect"></param>
        private void onUpdateCardListItem(int index, EditorCardListItem item)
        {
            if (_deck == null)
            {
                item.hideCount();
            }
            else
            {
                CardDefine card = _cardPoolViewPanelCtrl.getCardDefineAt(index);
                index = _deck.cardList.FindIndex(e => e.cardRef.cardPoolId == card.cardPoolId && e.cardRef.defineId == card.id);
                if (index >= 0)
                {
                    int max = card.hasKeyword(KEYWORD.UNIQUE) ? 1 : 2;
                    int current = _deck.cardList[index].count;
                    item.showCount(current, max);
                }
                else
                {
                    item.hideCount();
                }
            }
        }
        private Task<bool> onStartDragCard(CardDefine cardDefine, PointerEventData eventData)
        {
            if (cardDefine == null)
                return Task.FromResult(false);
            _dragCard = cardDefine;
            return Task.FromResult(startDragDeckItem(eventData));
        }
        private Task<bool> onDragCard(CardDefine cardDefine, PointerEventData eventData)
        {
            if (cardDefine == null)
                return Task.FromResult(false);
            return dragDeckItem(eventData);
        }
        private async Task<bool> onEndDragCard(CardDefine cardDefine, PointerEventData eventData)
        {
            if (cardDefine == null)
                return false;
            await endDragDeckItem(eventData);
            //不管做什么都必须return false以保证ScrollView的释放事件被触发
            //否则会导致ScrollView不能正确回弹
            return false;
        }
        private void onStartDragDeckCardListItem(int index, PointerEventData eventData)
        {
            startDragDeckItem(eventData);
        }
        private async void onDragDeckCardListItem(int index, PointerEventData eventData)
        {
            await dragDeckItem(eventData, index);
        }
        private async void onEndDragDeckCardListItem(int index, PointerEventData eventData)
        {
            await endDragDeckItem(eventData);
        }
        private void onClickDeckListItem(int index)
        {
            display(deckList[index]);
        }

        [TranslateMsg("{0} 为卡组名称")]
        const string DECK_DELETE_CONFIRM_CONTENT = "你是否要删除卡组 {0}? 删除后将无法恢复。";

        private void onClickDeckListItemDeleteButton(int index)
        {
            game.ui.dialog.ShowQuestion(game.i18n._(StringTable.WARNING), game.i18n._(DECK_DELETE_CONFIRM_CONTENT, deckList[index].name), async () =>
            {
                game.deck.RemoveDeck(deckList[index].id);
                await updateDeckListUI();
            });
        }
        /// <summary>
        /// 点击新建卡组按钮
        /// </summary>
        private async void onClickDeckListAddButton()
        {
            (string name, Sprite image)[] viewData = new (string name, Sprite image)[_selectableMasters.Length];
            for (int i = 0; i < viewData.Length; i++)
            {
                var cardDefine = _selectableMasters[i];
                var cardPoolRef = game.card.getCardPoolReferenceOfCard(cardDefine);
                CardSkinData skin = game.card.getCardSkin(cardDefine);
                viewData[i].name = skin.name;
                viewData[i].image = await game.card.loadCardImage(
                    cardPoolRef,
                    _selectableMasters[i].id);
            }
            _ui.showMasterSelectPanel(viewData);
        }
        /// <summary>
        /// 点击新建卡组按钮选择角色之后
        /// </summary>
        /// <param name="index">角色索引</param>
        private void onMasterSelect(int index)
        {
            // 然后在窗口选择之后再正式新建卡组
            _ui.hideMasterSelectPanel();
            Deck newDeck = new Deck(new DefineReference(_selectableMasters[index].cardPoolId, _selectableMasters[index].id), Enumerable.Empty<DefineReference>())
            {
                //id = (int)game.deck.CreateDeck(StringTable.BUILD_NEW_DECK_DEFAULT_NAME, _selectableMasters[index].id),
                name = StringTable.BUILD_NEW_DECK_DEFAULT_NAME,
            };
            display(newDeck);
        }
        /// <summary>
        /// 卡组重命名后
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        private async void onDeckListItemNameValueChanged(int index, string name)
        {
            deckList[index].name = name;
            //game.deck.RenameDeck(deckList[index].id, name);
            await updateDeckListUI();
        }
        /// <summary>
        /// 点击保存卡组按钮
        /// </summary>
        private void onSaveDeckClick()
        {
            // 卡组保存
            if (_deck.cardList.Sum(i => i.count) < game.deck.deckValidSize)
            {
                game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), game.lang._(PILE_NOT_COMPLETE), () =>
                {
                    saveDeck();
                });
            }
            else
            {
                saveDeck();
            }
        }
        /// <summary>
        /// 当玩家点击退出编辑模式按钮
        /// </summary>
        private void onExitEditButtonClick()
        {
            if (_isDeckChanged)
            {
                game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), game.lang._(StringTable.BUILD_CONFIRM_EXIT_WITHOUT_SAVE), () =>
                {
                    display();
                });
            }
            else
            {
                display();
            }
        }
        private void onClickImportButton()
        {
            // 解析数据
            string clipboardString = GUIUtility.systemCopyBuffer.Trim();

            var lines = clipboardString.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            string deckName = "", deckContent = "";
            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    if (line.StartsWith(exportStringHeader))
                        deckName = line.Substring(exportStringHeader.Length + 1, line.Length - exportStringHeader.Length - 1).Trim();
                }
                else
                {
                    deckContent = line.Trim();
                }
            }

            if (string.IsNullOrEmpty(deckContent))
            {
                game.ui.dialog.ShowConfirm(game.lang._(StringTable.ERROR), game.lang._(NO_PILE_AVALIABLE), null);
                return;
            }

            try
            {
                DefineReference[] cards = game.deck.StringToDeckWithCardPoolID(deckContent);
                DefineReference master = cards[0];

                // 检查卡片是否存在
                for (int i = 0; i < cards.Length; i++)
                {
                    DefineReference card = cards[i];
                    if (game.card.getLocalCardDefine(card) == null)
                    {
                        game.ui.dialog.ShowConfirm(game.lang._(StringTable.ERROR), game.lang._(StringTable.ERROR_CARD_WITH_ID_NOT_EXISTS, card), null);
                        return;
                    }
                }

                // 创建卡组
                Deck copiedDeck = new Deck()
                {
                    name = deckName,
                    masterCardRef = master,
                };
                //添加卡牌，跳过第一个角色卡
                copiedDeck.addCards(cards.Skip(1));

                // 进入编辑模式
                display(copiedDeck);
            }
            catch (FormatException)
            {
                game.ui.dialog.ShowConfirm(game.lang._(StringTable.ERROR), game.lang._(StringTable.BUILD_ERROR_DECKCODE_FORMATE_ERROR), null);
            }
            catch (Exception e)
            {
                game.ui.dialog.ShowConfirm(game.lang._(StringTable.ERROR), game.lang._(StringTable.BUILD_ERROR_IMPORT_FAILED, e.Message), null);
                Debug.LogWarning(e);
            }
        }
        /// <summary>
        /// 点击导出卡组按钮
        /// </summary>
        private void onExportDeckClick()
        {
            // 卡组保存
            if (_deck.getCardCount() < game.deck.deckValidSize)
            {
                game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), game.lang._(PILE_NOT_COMPLETE_EXPORT), () =>
                {
                    exportDeck();
                });
            }
            else
            {
                exportDeck();
            }
        }
        /// <summary>
        /// 点击上传卡组到服务器按钮
        /// </summary>
        private void onUploadDeckClick()
        {
            if (!game.network.logic.AccountIsValid)
            {
                game.ui.dialog.ShowConfirm(game.lang._(StringTable.HINT_TITLE), game.lang._(StringTable.ERROR_INVALID_IN_VISITOR_MODE), null);
                return;
            }
            game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), game.lang._(StringTable.BUILD_CONFIRM_UPLOAD), async () =>
            {
                _ui.setLoadingUIActive(true);
                try
                {
                    await uploadDeck();
                    game.ui.dialog.ShowConfirm(game.lang._(StringTable.HINT_TITLE), game.lang._(StringTable.BUILD_DECK_UPLOAD_COMPLETE), null);
                }
                catch (Exception e)
                {
                    game.ui.dialog.ShowConfirm(game.lang._(StringTable.ERROR), e.Message, null);
                    Debug.LogError(e);
                }
                finally
                {
                    _ui.setLoadingUIActive(false);
                }
            });
        }
        /// <summary>
        /// 点击从服务器下载同步卡组按钮
        /// </summary>
        private void onDownloadDeckClick()
        {
            if (!game.network.logic.AccountIsValid)
            {
                game.ui.dialog.ShowConfirm(game.lang._(StringTable.HINT_TITLE), game.lang._(StringTable.ERROR_INVALID_IN_VISITOR_MODE), null);
                return;
            }
            game.ui.dialog.ShowQuestion(game.lang._(StringTable.HINT_TITLE), game.lang._(StringTable.BUILD_CONFIRM_DOWNLOAD), async () =>
            {
                _ui.setLoadingUIActive(true);
                try
                {
                    await downloadDeck();
                    game.ui.dialog.ShowConfirm(game.lang._(StringTable.HINT_TITLE), game.lang._(StringTable.BUILD_DECK_DOWNLOAD_COMPLETE), null);
                    await updateDeckListUI();
                }
                catch (Exception e)
                {
                    game.ui.dialog.ShowConfirm(game.lang._(StringTable.ERROR), e.Message, null);
                    Debug.LogError(e);
                }
                finally
                {
                    _ui.setLoadingUIActive(false);
                }
            });
        }
        private void onDeckNameInputFieldValueChanged(string value)
        {
            _deck.name = value;
        }
        #endregion
        private bool cardFilter(CardDefine c)
        {
            if (_disableStandardCardPool && game.card.isInnateCardPool(c.cardPoolId))
            {
                return false;
            }
            if (c.isToken())
            {
                //衍生物
                return false;
            }
            if (c.isMaster())
            {
                //角色卡
                return false;
            }
            if (c.isSkill())
            {
                //技能卡
                return false;
            }
            return true;
        }
        /// <summary>
        /// 获取卡组的封面贴图
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        private async Task<Sprite> loadDeckImage(Deck deck)
        {
            var cardRef = deck.masterCardRef;
            var cardPoolRef = game.card.getLocalCardPoolReference(cardRef.cardPoolId);
            await game.card.getOrLoadCardPool(cardPoolRef, true);
            return await game.card.loadCardImage(cardPoolRef, cardRef.defineId);
        }
        private bool tryAddCardToDeck(CardDefine card, out string errorTip)
        {
            if (card == null)
            {
                errorTip = string.Empty;
                return false;
            }
            //卡组数量上限
            if (_deck.getCardCount() >= game.deck.deckValidSize)
            {
                errorTip = game.lang._(StringTable.BUILD_TIP_DECK_MAX_SIZE);
                return false;
            }
            //卡牌数量上限
            int cardCount = _deck.getCardCount(card.cardPoolId, card.id);
            if (cardCount >= 2 || (card.hasKeyword(KEYWORD.UNIQUE) && cardCount >= 1))
            {
                errorTip = game.lang._(StringTable.BUILD_TIP_CARD_MAX_COUNT);
                return false;
            }
            // 中立卡或自己的卡
            if ((card.getCharacterRef().cardPoolId != 0 ||
                card.getCharacterRef().defineId != CATEGORY.CHARACTER_NEUTRAL) &&
                card.getCharacterRef() != _deck.masterCardRef)
            {
                errorTip = game.lang._(StringTable.BUILD_TIP_CARD_CARRY_LIMIT);
                return false;
            }
            // 添加卡
            _deck.addCard(card.getDefineRef());
            // 标记为已经修改
            _isDeckChanged = true;
            errorTip = string.Empty;
            return true;
        }
        /// <summary>
        /// 保存卡组
        /// </summary>
        private void saveDeck()
        {
            if (game.deck.HasDeck(_deck.id))
            {
                game.deck.RenameDeck(_deck.id, _deck.name);
                game.deck.ClearDeck(_deck.id);
            }
            else
            {
                _deck.id = (int)game.deck.CreateDeck(_deck.name, _deck.masterCardRef);
                deckList.Add(_deck);
            }
            // 排序卡牌。
            _deck.cardList.Sort((a, b) =>
            {
                var defineA = game.card.getLocalCardDefine(a.cardRef);
                var defineB = game.card.getLocalCardDefine(b.cardRef);
                if (defineA != null && defineB != null)
                {
                    return defineA.getCost().CompareTo(defineB.getCost());
                }
                return 0;
            });
            foreach (var cardRef in _deck.toDefRefArray(includeMasterCard: false))
            {
                game.deck.AddCard(_deck.id, cardRef);
            }
            _isDeckChanged = false;
        }
        /// <summary>
        /// 导出卡组
        /// </summary>
        private void exportDeck()
        {
            string outputString = $"{exportStringHeader} {_deck.name}\n";

            outputString += game.deck.DeckToString(_deck.toDefRefArray());
            GUIUtility.systemCopyBuffer = outputString;

            game.ui.dialog.ShowConfirm(game.lang._(StringTable.HINT_TITLE), game.lang._(PILE_EXPORTED), null);
        }
        private async Task uploadDeck()
        {
            if (!game.network.logic.AccountIsValid)
                return;

            List<DeckDataItem> deckToSend = new List<DeckDataItem>();
            foreach (var deck in deckList)
            {
                deckToSend.Add(new DeckDataItem()
                {
                    ID = deck.id,
                    Name = deck.name,
                    Content = game.deck.DeckToString(deck.toDefRefArray())
                });
            }

            await game.network.logic.ServerClient.SetUserDecksAsync(deckToSend.ToArray());
        }
        private async Task downloadDeck()
        {
            if (!game.network.logic.AccountIsValid)
                return;

            var decks = await game.network.logic.ServerClient.GetUserDecksAsync();
            foreach (var deck in decks)
            {
                try
                {
                    int id = deck.ID;
                    DefineReference[] cards = game.deck.StringToDeckWithCardPoolID(deck.Content);


                    if (game.deck.HasDeck(id))
                    {
                        game.deck.RenameDeck(id, deck.Name);
                        game.deck.SetDeckMasterID(id, cards[0]);
                    }
                    else
                    {
                        game.deck.CreateDeck(id, deck.Name, cards[0]);
                    }

                    game.deck.ClearDeck(id);
                    for (int i = 1; i < cards.Length; i++)
                    {
                        game.deck.AddCard(id, cards[i]);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
            game.deck.reloadDeckList();
        }
        /// <summary>
        /// 更新卡组列表UI
        /// </summary>
        /// <returns></returns>
        private async Task updateDeckListUI()
        {
            (string name, Sprite image, bool errorTip)[] viewData = new (string name, Sprite image, bool errorTip)[deckList.Count];
            for (int i = 0; i < deckList.Count; i++)
            {
                viewData[i].name = deckList[i].name;
                viewData[i].image = await loadDeckImage(deckList[i]);
                viewData[i].errorTip = !game.deck.isValidDeck(deckList[i]);
            }
            _ui.updateDeckList(viewData);
        }
        private async Task updateDeckCardListUI()
        {
            _ui.setDeckCardCount(_deck.cardList.Sum(e => e.count), game.deck.deckValidSize);
            var cards = _deck.cardList.OrderBy(e => game.card.getLocalCardDefine(e.cardRef) is CardDefine cardDefine ? cardDefine.getCost() : 0).ToArray();
            (string name, Sprite image, int count)[] deckCardsViewData = new (string name, Sprite image, int count)[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                CardDefine cardDefine = game.card.getLocalCardDefine(cards[i].cardRef);
                CardSkinData skin = cardDefine != null ? game.card.getCardSkin(cardDefine) : null;

                string name;
                Sprite image;
                if (skin != null)
                {
                    name = skin.name;
                    image = await game.card.loadCardImage(cardDefine);
                }
                else
                {
                    name = game.langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME);
                    image = game.card.getDefaultSprite();
                }
                deckCardsViewData[i].name = name;
                deckCardsViewData[i].image = image;
                deckCardsViewData[i].count = cards[i].count;
            }
            _ui.updateDeckCardList(deckCardsViewData);
        }
        private async Task showDragCard(bool isDeckListItem, bool force = false)
        {
            _ui.showDragCard(isDeckListItem);
            if (_isDraggingDeckListItem != isDeckListItem || force)
            {
                _isDraggingDeckListItem = isDeckListItem;
                //更新UI
                if (_dragCard != null)
                {
                    CardSkinData skin = game.card.getCardSkin(_dragCard);
                    var cardPoolRef = game.card.getLocalCardPoolReference(_dragCard.cardPoolId);
                    Sprite image = await game.card.loadCardImage(cardPoolRef, _dragCard.id);
                    if (isDeckListItem)
                        _ui.dragDeckListItem.updateView(skin.name, image);
                    else
                        await _ui.dragCard.setCardDefine(_dragCard, game.card, true);
                }
                else
                {
                    var image = game.card.getDefaultSprite();
                    if (isDeckListItem)
                        _ui.dragDeckListItem.updateView(game.langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME), image);
                    else
                    {
                        _ui.dragCard.setUnknownCard(true);
                        _ui.dragCard.setImage(image);
                    }
                }
            }
        }
        private bool startDragDeckItem(PointerEventData eventData)
        {
            if (_deck == null)
                //当前不在编辑卡组，禁用拖拽
                return false;
            //记录拖拽信息
            _dragStartPosition = eventData.position;
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            _isDraggingDeckListItem = false;
            return false;
        }
        private async Task initDeckItemDrag(PointerEventData eventData, int deckListItemIndex)
        {
            var delta = _dragStartPosition - eventData.position;
            if (delta.magnitude > 0)
            {
                if (Math.Abs(delta.x) > Math.Abs(delta.y))
                {
                    //横向拖拽
                    if (Math.Abs(delta.x) > _dragThreshold)
                    {
                        //横向拖拽并且大于阈值，设置卡牌显示
                        _isDraggingHorizontal = true;

                        if (deckListItemIndex >= 0)
                        {
                            var deckItem = await removeDeckListItem(deckListItemIndex);
                            var cardRef = deckItem?.cardRef;
                            if (cardRef != null)
                                _dragCard = game.card.getLocalCardDefine(cardRef);
                        }
                        await showDragCard(_ui.isPositionOverDeckCardList(eventData.position), true);
                    }
                }
                else
                {
                    //纵向拖拽
                    _isDraggingVertical = true;
                }
            }
        }
        private async Task<DeckItem> removeDeckListItem(int index)
        {
            var deckList = _deck.cardList;

            // 超出卡组上下限，刷新卡牌列表。
            if (index < 0 || index >= deckList.Count)
            {
                await updateDeckCardListUI();
                return null;
            }

            //获取拖拽卡牌
            var orderedCards = deckList.OrderBy(e => game.card.getLocalCardDefine(e.cardRef)?.getCost() ?? 0).ToArray();
            var deckItem = orderedCards[index];
            _deck.removeCard(deckItem);
            //刷新卡池显示
            _cardPoolViewPanelCtrl.updateContents();
            return deckItem;
        }
        private async Task<bool> dragDeckItem(PointerEventData eventData, int index = -1)
        {
            if (_deck == null)
                //当前不在编辑卡组，禁用拖拽
                return false;
            if (!_isDraggingHorizontal && !_isDraggingVertical)
                //还没有进入拖拽状态
                await initDeckItemDrag(eventData, index);
            if (_isDraggingHorizontal)
            {
                await showDragCard(_ui.isPositionOverDeckCardList(eventData.position));
                _ui.moveDragCard(eventData.position);
                eventData.Use();
                return true;
            }
            return false;
        }
        private async Task endDragDeckItem(PointerEventData eventData)
        {
            if (_deck == null)
                //当前不在编辑卡组，禁用拖拽
                return;
            if (_isDraggingHorizontal)
            {
                //横向拖拽
                //执行操作
                if (_dragCard != null && _ui.isPositionOverDeckCardList(eventData.position))
                {
                    if (tryAddCardToDeck(_dragCard, out string errorTip))
                    {
                        //更新可用信息
                        _cardPoolViewPanelCtrl.updateContents();
                    }
                    else
                        _ui.showErrorTip(errorTip);
                }
                //隐藏拖拽卡牌
                _ui.hideDragCard();
                //更新UI
                await updateDeckCardListUI();
            }
            //重置状态字段
            _dragCard = null;
            _dragStartPosition = Vector2.zero;
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            _isDraggingDeckListItem = false;
        }
        #endregion
        #region 属性字段
        public const int masterNameMaxLength = 8;
        List<Deck> deckList => game.deck.getOrLoadDeckList();
        [SerializeField]
        BuildCardPanel _ui;
        [SerializeField]
        CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        [SerializeField]
        bool _disableStandardCardPool;
        [SerializeField]
        float _dragThreshold = 50f;
        [SerializeField]
        Deck _deck;
        [SerializeField]
        bool _isDeckChanged = false;
        [SerializeField]
        Vector2 _dragStartPosition;
        [SerializeField]
        bool _isDraggingHorizontal = false;
        [SerializeField]
        bool _isDraggingVertical = false;
        bool _isDraggingDeckListItem = false;
        CardDefine[] _selectableMasters;
        CardDefine _dragCard;
        //List<CardPoolRuntimeData> _cardPoolList = new List<CardPoolRuntimeData>();
        //List<CardDefine> _cardDefineList = new List<CardDefine>();
        //List<CardSkinData> _cardSkinList = new List<CardSkinData>();
        const string exportStringHeader = "# 醉梦传说卡组";
        [TranslateMsg]
        const string PILE_EXPORTED = "卡组已导出到剪贴板";
        [TranslateMsg]
        const string PILE_NOT_COMPLETE = "当前卡组不完整，无法在游戏中使用。\n是否要保存？";
        [TranslateMsg]
        const string NO_PILE_AVALIABLE = "剪贴板中没有可导入的卡组";
        [TranslateMsg]
        const string PILE_NOT_COMPLETE_EXPORT = "当前卡组不完整，无法在游戏中使用。\n是否要导出？";
        [TranslateMsg("卡组中英雄名称超过上限之后，省略后续内容的文字模板，参数0为裁切的文本")]
        const string MASTER_NAME_OVERFLOW_TEMPLATE = "{0}..."; 
        #endregion
        #region 内部类
        class CardPoolRuntimeData
        {
            public CardPoolRuntimeData(string path, CardPoolInfo info, bool isEnabled)
            {
                this.path = path;
                this.info = info;
                this.isEnabled = isEnabled;
            }
            public string path;
            public CardPoolInfo info;
            public bool isEnabled;
        }
        #endregion
    }
}
