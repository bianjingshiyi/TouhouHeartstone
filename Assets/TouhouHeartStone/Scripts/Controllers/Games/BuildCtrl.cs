using Game;
using MukioI18n;
using NitoriNetwork.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            //卡牌收藏
            _selectableMasters = game.card.getCardDefines(c => c.isMaster());
            //_sortType = 0;
            //_characterFilters = null;
            //_characterFiltersValue = null;
            //_costFiltersValue = new bool[GameUIHelper.cardPoolViewPanel_CostFilters.Length];
            //_typeFiltersValue = new bool[GameUIHelper.cardPoolViewPanel_TypeFilters.Length];

            //await loadCardListThenFilterAndSort();
            //_characterFiltersValue = new bool[_characterFilters.Length];

            _ui.gameObject.SetActive(true);
            //UI更新
            //string[] cardPoolNames = new string[_cardPoolList.Count];
            //for (int i = 0; i < cardPoolNames.Length; i++)
            //{
            //    cardPoolNames[i] = _cardPoolList[i].info.cardPoolName;
            //}
            //bool[] cardPoolValues = new bool[_cardPoolList.Count];
            //for (int i = 0; i < cardPoolValues.Length; i++)
            //{
            //    cardPoolValues[i] = _cardPoolList[i].isEnabled;
            //}
            //_ui.setCardPoolOptions(cardPoolNames);
            //_ui.setCardPoolValue(cardPoolValues);
            //_ui.setSortType(_sortType);
            //_ui.setCharacterFilters(_characterFilters.Select(id =>
            //{
            //    if (id == CATEGORY.CHARACTER_NEUTRAL)
            //        return "中立";
            //    return game.card.getSkin(id).name;
            //}).ToArray());
            //_ui.setCharacterFilter(_characterFiltersValue);
            //_ui.setCostFilter(_costFiltersValue);
            //_ui.setTypeFilter(_typeFiltersValue);
            //_ui.setCardListActive(true);
            //_ui.updateCardListCount(_cardDefineList.Count);
            _ui.setEnableDrag(false);
            //右侧栏
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
            await _cardPoolViewPanelCtrl.display(charactersId: new int[] { deck.masterId });
            //int index = Array.IndexOf(_characterFilters, deck.masterId);
            //_characterFiltersValue = new bool[_characterFilters.Length];
            //_characterFiltersValue[index] = true;
            //await loadCardListThenFilterAndSort();
            ////剔除卡组中无法识别的卡牌
            //for (int i = 0; i < deck.cardList.Count; i++)
            //{
            //    if (game.card.GetCardDefine(deck.cardList[i].id) == null)
            //    {
            //        Debug.LogWarning("卡组中存在未知卡牌" + deck.cardList[i]);
            //        deck.cardList.RemoveAt(i);
            //        i--;
            //    }
            //}

            //_ui.setCharacterFilter(_characterFiltersValue);

            // 皮肤
            CardSkinData masterSkin = game.card.getSkin(deck.masterId);
            _ui.gameObject.SetActive(true);
            _ui.updateCurCardPoolInfo(deck.name, await loadDeckImage(deck));
            _ui.setCharacterName(masterSkin.name);

            // 更新UI
            //string[] cardPoolNames = new string[_cardPoolList.Count];
            //for (int i = 0; i < cardPoolNames.Length; i++)
            //{
            //    cardPoolNames[i] = _cardPoolList[i].info.cardPoolName;
            //}
            //bool[] cardPoolValues = new bool[_cardPoolList.Count];
            //for (int i = 0; i < cardPoolValues.Length; i++)
            //{
            //    cardPoolValues[i] = _cardPoolList[i].isEnabled;
            //}
            //_ui.setCardPoolOptions(cardPoolNames);
            //_ui.setCardPoolValue(cardPoolValues);
            //_ui.setCardListActive(true);
            //_ui.updateCardListCount(_cardDefineList.Count);
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
            //_ui.onUpdateCardListItem += onUpdateCardListItem;
            //_ui.onStartDargCardListItem += onStartDragCardListItem;
            _ui.onStartDragDeckCardListItem += onStartDragDeckCardListItem;
            //_ui.onEndDragCardListItem += onEndDragCardListItem;
            _ui.onEndDragDeckCardListItem += onEndDragDeckCardListItem;
            //_ui.onCardPoolValuesChanged += onCardPoolValuesChanged;
            //_ui.onSortTypeValueChanged += onSortTypeValueChanged;
            //_ui.onCharacterFilterValueChanged += onCharacterFiltersValueChanged;
            //_ui.onCostFilterValueChanged += onCostFiltersValueChanged;
            //_ui.onTypeFilterValueChanged += onTypeFilterValueChanged;
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
        //private async void onUpdateCardListItem(int index, RectTransform itemRect)
        //{
        //    EditorCardListItem item = itemRect.GetComponentInChildren<EditorCardListItem>();
        //    if (index < _cardDefineList.Count)
        //    {
        //        //显示卡牌
        //        CardDefine cardDefine = _cardDefineList[index];
        //        //显示可用数量
        //        item.switchToCard();
        //        CardSkinData cardSkin = game.card.getDefaultSkin(cardDefine);
        //        item.card.setCard(cardDefine, cardSkin);
        //        if (cardSkin != null && cardSkin.image == null)
        //        {
        //            Sprite sprite;
        //            string path = cardSkin.imagePath;
        //            if (game.card.getCardPoolOfCard(cardDefine) is CardPoolInfo cardPool &&
        //                game.card.getCardPoolPath(cardPool) is string cardPoolPath)
        //            {
        //                if (string.IsNullOrEmpty(path))
        //                {
        //                    sprite = null;
        //                }
        //                else if (cardPoolPath.StartsWith("sa:") &&
        //                    !path.StartsWith("sa;") &&
        //                    !path.StartsWith("res:"))
        //                {
        //                    //流文件
        //                    sprite = await game.card.loadExternalImage("sa:" + Path.Combine(game.card.textureDirName, path), null);
        //                }
        //                else
        //                {
        //                    //绝对路径
        //                    sprite = await game.card.loadExternalImage(path, Path.Combine(game.card.getCardPoolPathRootDir(cardPoolPath), game.card.textureDirName));
        //                }
        //            }
        //            else
        //            {
        //                //没有卡池信息
        //                sprite = await game.card.loadExternalImage(path, game.card.textureDirName);
        //            }
        //            item.card.setImage(sprite);
        //        }
        //        if (_deck == null)
        //        {
        //            item.hideCount();
        //        }
        //        else
        //        {
        //            index = _deck.cardList.FindIndex(e => e.id == cardDefine.id);
        //            if (index >= 0)
        //            {
        //                int max = cardDefine.hasKeyword(KEYWORD.UNIQUE) ? 1 : 2;
        //                int current = _deck.cardList[index].count;
        //                item.showCount(current, max);
        //            }
        //            else
        //            {
        //                item.hideCount();
        //            }
        //        }
        //    }
        //}
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
                index = _deck.cardList.FindIndex(e => e.id == card.id);
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
        private async Task<bool> onStartDragCard(CardDefine cardDefine, PointerEventData eventData)
        {
            if (_deck == null)
            {
                //当前不在编辑卡组，禁用拖拽
                return false;
            }
            //记录拖拽信息
            _dragCard = cardDefine;
            _dragStartPosition = eventData.position;
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            return false;
        }
        private async Task<bool> onDragCard(CardDefine cardDefine, PointerEventData eventData)
        {
            if (_deck == null)
            {
                //当前不在编辑卡组，禁用拖拽
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
                            Sprite image = await game.card.loadCardImage(_dragCard.id);
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
            //重置状态字段
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            if (_ui.isPositionOverDeckCardList(eventData.position))
            {
                if (tryAddCardToDeck(_dragCard, out string errorTip))
                {
                    //更新可用信息
                    await _cardPoolViewPanelCtrl.display();
                    //更新UI
                    await updateDeckCardListUI();
                }
                else
                    _ui.showMessage(errorTip);
            }
            //隐藏拖拽卡牌
            _ui.hideDragCard();
            return true;
        }
        private async void onStartDragDeckCardListItem(int index)
        {
            //获取拖拽卡牌
            var cards = _deck.cardList.OrderBy(e => game.card.GetCardDefine(e.id).getCost()).ToArray();
            _dragCard = game.card.GetCardDefine(cards[index].id);
            index = _deck.cardList.IndexOf(cards[index]);
            //从卡组中删除一张卡牌
            _deck.cardList[index].count--;
            if (_deck.cardList[index].count <= 0)
                _deck.cardList.RemoveAt(index);
            //刷新卡池显示
            await _cardPoolViewPanelCtrl.display();
            //刷新卡组显示
            await updateDeckCardListUI();
            //刷新拖拽卡牌显示
            CardSkinData skin = game.card.getDefaultSkin(_dragCard);
            Sprite image = await game.card.loadCardImage(_dragCard.id);
            _ui.dragCard.setCard(_dragCard, skin);
            _ui.dragCard.setImage(image);
            _ui.dragDeckListItem.updateView(skin.name, image);
        }
        private async void onEndDragDeckCardListItem(bool isOverDeckCardList)
        {
            if (!isOverDeckCardList)
                return;
            if (tryAddCardToDeck(_dragCard, out string errorTip))
            {
                // 更新可用信息
                await _cardPoolViewPanelCtrl.display();
                // 更新UI
                await updateDeckCardListUI();
            }
            else
                _ui.showMessage(errorTip);
        }
        //private async void onCardPoolValuesChanged(bool[] value)
        //{
        //    //保存当前引用的卡池文件状态
        //    for (int i = 0; i < value.Length; i++)
        //    {
        //        //不能更改一定会包含的标准卡池
        //        if (game.card.isStandardCardPool(_cardPoolList[i].info))
        //        {
        //            _cardPoolList[i].isEnabled = true;
        //            continue;
        //        }
        //        _cardPoolList[i].isEnabled = value[i];
        //    }
        //    //应用筛选条件
        //    await loadCardListThenFilterAndSort();
        //    //更新卡牌列表
        //    _ui.updateCardListCount(_cardDefineList.Count);
        //}
        //private async void onSortTypeValueChanged(int index)
        //{
        //    _sortType = index;
        //    await loadCardListThenFilterAndSort();
        //    _ui.updateCardListCount(_cardDefineList.Count);
        //}
        //private async void onCharacterFiltersValueChanged(bool[] value)
        //{
        //    _characterFiltersValue = value;
        //    await loadCardListThenFilterAndSort();
        //    _ui.updateCardListCount(_cardDefineList.Count);
        //}
        //private async void onCostFiltersValueChanged(bool[] value)
        //{
        //    _costFiltersValue = value;
        //    await loadCardListThenFilterAndSort();
        //    _ui.updateCardListCount(_cardDefineList.Count);
        //}
        //private async void onTypeFilterValueChanged(bool[] value)
        //{
        //    _typeFiltersValue = value;
        //    await loadCardListThenFilterAndSort();
        //    _ui.updateCardListCount(_cardDefineList.Count);
        //}
        private void onClickDeckListItem(int index)
        {
            display(deckList[index]);
        }
        private async void onClickDeckListItemDeleteButton(int index)
        {
            game.deck.RemoveDeck(deckList[index].id);
            await updateDeckListUI();
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
                viewData[i].image = await game.card.loadCardImage(_selectableMasters[i].id);
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
            Deck newDeck = new Deck()
            {
                //id = (int)game.deck.CreateDeck(StringTable.BUILD_NEW_DECK_DEFAULT_NAME, _selectableMasters[index].id),
                name = StringTable.BUILD_NEW_DECK_DEFAULT_NAME,
                masterId = _selectableMasters[index].id
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
            if (_deck.cardList.Sum(i => i.count) < game.deck.deckMaxSize)
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
            if (!clipboardString.StartsWith(exportStringHeader))
            {
                game.ui.dialog.ShowConfirm(game.lang._(StringTable.ERROR), game.lang._(NO_PILE_AVALIABLE), null);
                return;
            }

            var lbIndex = clipboardString.IndexOf("\n");
            var deckName = clipboardString.Substring(exportStringHeader.Length + 1, lbIndex - exportStringHeader.Length - 1).Trim();
            var deckContent = clipboardString.Substring(lbIndex + 1).Trim();

            try
            {
                var cards = game.deck.StringToDeck(deckContent);
                int master = cards[0];

                // 检查卡片是否存在
                for (int i = 0; i < cards.Length; i++)
                {
                    var card = cards[i];
                    if (game.card.GetCardDefine(card) == null)
                    {
                        game.ui.dialog.ShowConfirm(game.lang._(StringTable.ERROR), game.lang._(StringTable.ERROR_CARD_WITH_ID_NOT_EXISTS, card), null);
                        return;
                    }
                }

                // 创建卡组
                Deck copiedDeck = new Deck()
                {
                    name = deckName,
                    masterId = master,
                };
                copiedDeck.addCards(cards);

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
            if (_deck.cardList.Count < game.deck.deckMaxSize)
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
        //private async Task loadCardListThenFilterAndSort()
        //{
        //    _cardDefineList.Clear();
        //    _cardSkinList.Clear();
        //    //筛选
        //    //内部卡池
        //    CardDefine[] cardDefines = game.card.GetCardDefines(d => d.GetType().Assembly == typeof(THHGame).Assembly
        //    && d.GetType().GetCustomAttribute<IgnoreCardDefineAttribute>() == null);
        //    _cardDefineList.AddRange(cardDefines);
        //    for (int i = 0; i < _cardDefineList.Count; i++)
        //    {
        //        if (game.card.tryGetSkin(_cardDefineList[i].id, out CardSkinData skin))
        //            _cardSkinList.Add(skin);
        //    }
        //    //外部卡池
        //    var cardPools = await game.card.getCardPoolInfosAsync();
        //    foreach (var pair in cardPools)
        //    {
        //        int index = _cardPoolList.FindIndex(item => item.path == pair.Key);
        //        if (index < 0)
        //        {
        //            _cardPoolList.Add(new CardPoolRuntimeData(pair.Key, pair.Value, false));
        //        }
        //        else
        //        {
        //            _cardPoolList[index] = new CardPoolRuntimeData(pair.Key, pair.Value, _cardPoolList[index].isEnabled);
        //        }
        //    }
        //    List<string> pathList = new List<string>();
        //    for (int i = 0; i < _cardPoolList.Count; i++)
        //    {
        //        //标准卡池一定包含
        //        if (game.card.isStandardCardPool(_cardPoolList[i].info))
        //        {
        //            _cardPoolList[i].isEnabled = true;
        //        }
        //        //分析一下当前卡组是否有卡牌使用到了外部卡池
        //        if (isDeckContainsExternalCard(_cardPoolList[i].info.cardPoolId))
        //        {
        //            //如果包含，那这个卡池一定被激活
        //            _cardPoolList[i].isEnabled = true;
        //        }
        //        if (_cardPoolList[i].isEnabled)
        //            pathList.Add(_cardPoolList[i].path);
        //    }
        //    var cards = await game.card.getExternalCardDefinesAsync(pathList.ToArray());
        //    _cardDefineList.AddRange(cards.defines);
        //    _cardSkinList.AddRange(cards.skins);
        //    //不显示衍生物
        //    _cardDefineList.RemoveAll(c => c.isToken());
        //    //不显示技能
        //    _cardDefineList.RemoveAll(c => c.type == DEFINE.SKILL);
        //    //重设角色筛选选项
        //    _characterFilters = new int[] { CATEGORY.CHARACTER_NEUTRAL }.Concat(_cardDefineList.Where(c => c.type == DEFINE.MASTER).Select(c => c.id)).ToArray();
        //    //重置可选英雄
        //    _selectableMasters = _cardDefineList.Where(c => isAvailableCharacter(c)).ToArray();
        //    //不显示英雄
        //    _cardDefineList.RemoveAll(c => c.type == DEFINE.MASTER);
        //    //角色
        //    if (_characterFilters != null && _characterFilters.Length > 0 &&
        //        _characterFiltersValue != null && _characterFilters.Length == _characterFiltersValue.Length && _characterFiltersValue.Any(b => b))
        //    {
        //        _cardDefineList.RemoveAll(c =>
        //        {
        //            int index = Array.IndexOf(_characterFilters, c.getCharacterID());
        //            if (index < 0)
        //                return true;
        //            else
        //                return !_characterFiltersValue[index];
        //        });
        //    }
        //    //费用
        //    if (_costFiltersValue != null && _costFiltersValue.Length == GameUIHelper.cardPoolViewPanel_CostFilters.Length && _costFiltersValue.Any(b => b))
        //    {
        //        _cardDefineList.RemoveAll(c =>
        //        {
        //            int cost = c.getCost();
        //            if (cost >= 7)
        //            {
        //                return !_costFiltersValue[7];
        //            }
        //            else if (cost <= 0)
        //            {
        //                return !_costFiltersValue[0];
        //            }
        //            else
        //            {
        //                return !_costFiltersValue[cost];
        //            }
        //        });
        //    }
        //    //类型
        //    if (_typeFiltersValue != null && _typeFiltersValue.Length == GameUIHelper.cardPoolViewPanel_TypeFilters.Length && _typeFiltersValue.Any(b => b))
        //    {
        //        _cardDefineList.RemoveAll(c =>
        //        {
        //            int index = Array.IndexOf(GameUIHelper.cardPoolViewPanel_TypeFilters, c.type);
        //            if (index < 0)
        //                return true;
        //            else
        //                return !_typeFiltersValue[index];
        //        });
        //    }

        //    //排序
        //    if (_sortType == 0)
        //    {
        //        //费用
        //        _cardDefineList = _cardDefineList.OrderBy(c => c.getCost()).ToList();
        //    }
        //    else if (_sortType == 1)
        //    {
        //        //名称
        //        _cardDefineList = _cardDefineList.OrderBy(c => game.card.getSkin(c.id).name).ToList();
        //    }
        //}
        ///// <summary>
        ///// 获取卡牌定义对应的皮肤数据
        ///// </summary>
        ///// <param name="cardDefine"></param>
        ///// <returns></returns>
        //private CardSkinData getCardSkinData(CardDefine cardDefine)
        //{
        //    var cardSkinGroup = _cardSkinList.Where(s => s.id == cardDefine.id);
        //    CardSkinData cardSkin = cardSkinGroup.FirstOrDefault(s => s.language == game.langManager.getLanguage());
        //    if (cardSkin == null)
        //        cardSkin = cardSkinGroup.FirstOrDefault();
        //    return cardSkin;
        //}
        /// <summary>
        /// 获取卡组的封面贴图
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        private Task<Sprite> loadDeckImage(Deck deck)
        {
            CardSkinData skin = game.card.getSkin(deck.masterId);
            if (skin.image != null)
                return Task.FromResult(skin.image);
            else
            {
                return game.card.loadExternalImage(skin.imagePath, game.card.textureDirName);
            }
        }
        /// <summary>
        /// 判断是不是可用的角色
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isAvailableCharacter(CardDefine c)
        {
            if (c.id == 0)
                return false;
            if (c.GetType().Assembly != typeof(THHGame).Assembly)
                return false;
            if (c is MasterCardDefine master)
                return true;
            else if (c.type == DEFINE.MASTER)
                return true;
            else
                return false;
        }
        private bool tryAddCardToDeck(CardDefine card, out string errorTip)
        {
            //卡组数量上限
            if (_deck.getCardCount() >= game.deck.deckMaxSize)
            {
                errorTip = game.lang._(StringTable.BUILD_TIP_DECK_MAX_SIZE);
                return false;
            }
            //卡牌数量上限
            int cardCount = _deck.getCardCount(card.id);
            if (cardCount >= 2 || (card.hasKeyword(KEYWORD.UNIQUE) && cardCount >= 1))
            {
                errorTip = game.lang._(StringTable.BUILD_TIP_CARD_MAX_COUNT);
                return false;
            }
            // 中立卡或自己的卡
            if (card.getCharacterID() != CATEGORY.CHARACTER_NEUTRAL
                && card.getCharacterID() != _deck.masterId)
            {
                errorTip = game.lang._(StringTable.BUILD_TIP_CARD_CARRY_LIMIT);
                return false;
            }
            // 添加卡
            _deck.addCard(card.id);
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
                _deck.id = (int)game.deck.CreateDeck(_deck.name, _deck.masterId);
                deckList.Add(_deck);
            }
            foreach (var cardId in _deck.toIdArray(includeMasterCard: false))
            {
                game.deck.AddCard(_deck.id, cardId);
            }
            _isDeckChanged = false;
        }
        /// <summary>
        /// 导出卡组
        /// </summary>
        private void exportDeck()
        {
            string outputString = exportStringHeader + " " + _deck.name + "\n";

            outputString += game.deck.DeckToString(_deck.toIdArray());
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
                    Content = game.deck.DeckToString(deck.toIdArray())
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
                    int[] cards = game.deck.StringToDeck(deck.Content);


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
        }
        private bool isDeckContainsExternalCard(int cardPoolId)
        {
            if (_deck == null)
                return false;
            for (int i = 0; i < _deck.cardList.Count; i++)
            {
                int cardId = _deck.cardList[i].id;
                if (CardDefineHelper.isExternalCard(cardId) && CardDefineHelper.getCardPoolId(cardId) == cardPoolId)
                    return true;
            }
            return false;
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
                viewData[i].errorTip = game.deck.isValidDeck(deckList[i]);
            }
            _ui.updateDeckList(viewData);
        }
        private async Task updateDeckCardListUI()
        {
            _ui.setDeckCardCount(_deck.cardList.Sum(e => e.count), game.deck.deckMaxSize);
            var cards = _deck.cardList.OrderBy(e => game.card.GetCardDefine(e.id).getCost()).ToArray();
            (string name, Sprite image, int count)[] deckCardList = new (string name, Sprite image, int count)[_deck.cardList.Count];
            for (int i = 0; i < deckCardList.Length; i++)
            {
                deckCardList[i].name = game.card.getSkin(cards[i].id).name;
                deckCardList[i].image = await game.card.loadCardImage(cards[i].id);
                deckCardList[i].count = cards[i].count;
            }
            _ui.updateDeckCardList(deckCardList);
        }
        #endregion
        #region 属性字段
        List<Deck> deckList => game.deck.getOrLoadDeckList();
        [SerializeField]
        BuildCardPanel _ui;
        [SerializeField]
        CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        //[SerializeField]
        //int _sortType;
        //[SerializeField]
        //int[] _characterFilters;
        //[SerializeField]
        //bool[] _characterFiltersValue;
        //[SerializeField]
        //bool[] _costFiltersValue;
        //[SerializeField]
        //bool[] _typeFiltersValue;
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
