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

            _selectableMasters = game.card.findCardDefines(c => {
                if (c.isMaster())
                {
                    long cardPoolId = c.cardPoolId;
                    // 组卡界面能使用的英雄，必须是玩家自己有的卡池里面的
                    // 缓存卡池里面的英雄不能算进去。
                    return game.card.isUsableCardPool(cardPoolId);
                }
                return false;
            }) ;

            //UI更新
            _ui.gameObject.SetActive(true);
            _ui.showErrorTip(false);
            _ui.setEnableDrag(false);
            //右侧栏
            _ui.setDeckListActive(true);
            _ui.setEditDeckPanelActive(false);
            await _cardPoolViewPanelCtrl.display(hideBuiltinCards: false, cardFilter: cardFilter);
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
            await _cardPoolViewPanelCtrl.display(hideBuiltinCards: false, characterRefs: new DefineReference[] { deck.masterCardRef }, cardFilter: cardFilter);
            CardSkinData masterSkin = game.card.getCardSkin(deck.masterCardRef.cardPoolId, deck.masterCardRef.defineId);

            // 更新UI
            _ui.gameObject.SetActive(true);
            _ui.showErrorTip(false);
            _ui.updateCurCardPoolInfo(deck.name, await loadDeckImage(deck));
            _ui.setCharacterName(masterSkin.name);
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
            if (_deck == null)
            {
                //当前不在编辑卡组，禁用拖拽
                return Task.FromResult(false);
            }
            //记录拖拽信息
            _dragCard = cardDefine;
            _dragStartPosition = eventData.position;
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            return Task.FromResult(false);
        }
        private async Task<bool> onDragCard(CardDefine cardDefine, PointerEventData eventData)
        {
            if (_deck == null)
            {
                //当前不在编辑卡组，禁用拖拽
                return false;
            }
            if (_dragCard == null)
            {
                //判断是否在拖拽卡牌，避免事件错误发生
                return false;
            }
            if (!_isDraggingHorizontal && !_isDraggingVertical)
            {
                //还没有进入拖拽状态
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
                            _ui.showDragCard(_ui.isPositionOverDeckCardList(eventData.position));
                            //onStartDargCardListItem?.Invoke(getCardItemIndex(item.transform as RectTransform));
                            //更新UI
                            CardSkinData skin = game.card.getDefaultSkin(_dragCard);
                            _ui.dragCard.setCard(_dragCard, skin);
                            Sprite image = await game.card.loadCardImage(_dragCard.cardPoolId, _dragCard.id);
                            _ui.dragCard.setImage(image);
                            _ui.dragDeckListItem.updateView(skin.name, image);
                        }
                    }
                    else
                    {
                        //纵向拖拽
                        _isDraggingVertical = true;
                        return false;
                    }
                }
                return true;
            }
            else if (_isDraggingHorizontal)
            {
                _ui.showDragCard(_ui.isPositionOverDeckCardList(eventData.position));
                _ui.moveDragCard(eventData.position);
                return true;
            }
            else if (_isDraggingVertical)
            {
                return false;
            }
            return false;
        }
        private async Task<bool> onEndDragCard(CardDefine cardDefine, PointerEventData eventData)
        {
            if (_deck == null)
            {
                //当前不在编辑卡组，禁用拖拽
                return false;
            }
            if (_isDraggingVertical)
            {
                //横向拖拽
                //重置状态字段
                _dragCard = null;
                _dragStartPosition = Vector2.zero;
                _isDraggingHorizontal = false;
                _isDraggingVertical = false;
                return false;
            }
            if (_dragCard == null)
            {
                //判断是否在拖拽卡牌，避免事件错误发生
                return false;
            }
            bool needUpdateUI = false;
            //执行操作
            if (_ui.isPositionOverDeckCardList(eventData.position))
            {
                if (tryAddCardToDeck(_dragCard, out string errorTip))
                {
                    needUpdateUI = true;
                }
                else
                    _ui.showErrorTip(errorTip);
            }
            //重置状态字段
            _dragCard = null;
            _dragStartPosition = Vector2.zero;
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            //隐藏拖拽卡牌
            _ui.hideDragCard();
            if (needUpdateUI)
            {
                //更新可用信息
                await _cardPoolViewPanelCtrl.display();
                //更新UI
                await updateDeckCardListUI();
            }
            //不管做什么都必须return false以保证ScrollView的释放事件被触发
            //否则会导致ScrollView不能正确回弹
            return false;
        }
        private async void onStartDragDeckCardListItem(int index)
        {
            var deckList = _deck.cardList;
            //获取拖拽卡牌
            var cards = deckList.OrderBy(e =>
            {
                var define = game.card.getCardDefine(e.cardRef);
                if (define != null)
                {
                    return define.getCost();
                }
                return 0;
            }).ToArray();
            _dragCard = game.card.getCardDefine(cards[index].cardRef);
            index = deckList.IndexOf(cards[index]);
            //从卡组中删除一张卡牌
            var deckItem = deckList[index];
            deckItem.count--;
            if (deckItem.count <= 0)
                deckList.RemoveAt(index);
            //刷新卡池显示
            await _cardPoolViewPanelCtrl.display();
            //这里本来应该刷新卡组显示，但是因为列表物体销毁会导致拖拽事件不再触发，所以移动到停止拖拽
            //await updateDeckCardListUI();
            //刷新拖拽卡牌显示
            if (_dragCard != null)
            {
                CardSkinData skin = game.card.getDefaultSkin(_dragCard);
                Sprite image = await game.card.loadCardImage(_dragCard.cardPoolId, _dragCard.id);
                _ui.dragCard.setCard(_dragCard, skin);
                _ui.dragCard.setImage(image);
                _ui.dragDeckListItem.updateView(skin.name, image);
            }
            else
            {
                _ui.dragCard.setUnknownCard();
                _ui.dragDeckListItem.updateView(StringTable.BUILD_UNKNOW_CARD_NAME, null);
            }
        }
        private async void onEndDragDeckCardListItem(bool isOverDeckCardList)
        {
            if (isOverDeckCardList)
            {
                if (tryAddCardToDeck(_dragCard, out string errorTip))
                {
                    // 更新可用信息
                    await _cardPoolViewPanelCtrl.display();
                }
                else
                    _ui.showErrorTip(errorTip);
            }
            // 更新UI
            await updateDeckCardListUI();
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
                CardSkinData skin = game.card.getDefaultSkin(_selectableMasters[i]);
                viewData[i].name = skin.name;
                viewData[i].image = await game.card.loadCardImage(
                    _selectableMasters[i].cardPoolId,
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

            var lines = clipboardString.Split(new string[] {"\r", "\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries);

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
                    if (game.card.getCardDefine(card) == null)
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
            if (_disableStandardCardPool &&
                game.card.getCardPoolOfCard(c) is CardPoolInfo cardPoolInfo &&
                game.card.isStandardCardPool(cardPoolInfo))
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
        private Task<Sprite> loadDeckImage(Deck deck)
        {
            if (!game.card.tryGetCardSkin(deck.masterCardRef.cardPoolId, deck.masterCardRef.defineId, out CardSkinData skin))
            {
                Debug.LogError("不存在卡组角色" + deck.masterCardRef + "的对应皮肤");
                return Task.FromResult<Sprite>(null);
            }
            if (skin.image != null)
                return Task.FromResult(skin.image);
            else
            {
                return game.card.loadImage(skin.imagePath,
                    game.card.getCardDefine(deck.masterCardRef) is CardDefine cardDefine &&
                    game.card.getCardPoolOfCard(cardDefine) is CardPoolInfo cardPoolInfo ?
                    cardPoolInfo.cardPoolId : 0);
            }
        }
        private bool tryAddCardToDeck(CardDefine card, out string errorTip)
        {
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
                var defineA = game.card.getCardDefine(a.cardRef);
                var defineB = game.card.getCardDefine(b.cardRef);
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
            string outputString = exportStringHeader + " " + _deck.name + "\n";

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
        //private bool isDeckContainsExternalCard(int cardPoolId)
        //{
        //    if (_deck == null)
        //        return false;
        //    for (int i = 0; i < _deck.cardList.Count; i++)
        //    {
        //        int cardId = _deck.cardList[i].id;
        //        if (CardDefineHelper.isExternalCard(cardId) && CardDefineHelper.getCardPoolId(cardId) == cardPoolId)
        //            return true;
        //    }
        //    return false;
        //}
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
            var cards = _deck.cardList.OrderBy(e => game.card.getCardDefine(e.cardRef) is CardDefine cardDefine ? cardDefine.getCost() : 0).ToArray();
            (string name, Sprite image, int count)[] deckCardsViewData = new (string name, Sprite image, int count)[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                CardDefine cardDefine = game.card.getCardDefine(cards[i].cardRef);
                CardSkinData skin = cardDefine != null ? game.card.getDefaultSkin(cardDefine) : null;
                deckCardsViewData[i].name = skin != null ? skin.name : game.langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME);
                deckCardsViewData[i].image = skin != null ? skin.image != null ?
                    skin.image :
                    await game.card.loadImage(skin.imagePath, cardDefine != null && game.card.getCardPoolOfCard(cardDefine) is CardPoolInfo cardPoolInfo ? cardPoolInfo.cardPoolId : 0) : null;
                deckCardsViewData[i].count = cards[i].count;
            }
            _ui.updateDeckCardList(deckCardsViewData);
        }
        #endregion
        #region 属性字段
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
