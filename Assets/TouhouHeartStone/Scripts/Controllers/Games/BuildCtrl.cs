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
            //卡牌收藏
            _sortType = 0;
            _characterFilters = null;
            _characterFiltersValue = null;
            _costFiltersValue = new bool[_costFilters.Length];
            _typeFiltersValue = new bool[_typeFilters.Length];

            loadCardListThenFilterAndSort();
            _characterFilters = new int[] { CATEGORY.CHARACTER_NEUTRAL }.Concat(_cardDefineList.Where(c => c.type == DEFINE.MASTER).Select(c => c.id)).ToArray();
            _characterFiltersValue = new bool[_characterFilters.Length];

            //构筑卡组
            loadDeckList();

            //UI更新
            _ui.setSortType(_sortType);
            _ui.setCharacterFilters(_characterFilters.Select(id =>
            {
                if (id == CATEGORY.CHARACTER_NEUTRAL)
                    return "中立";
                return game.card.getSkin(id).name;
            }).ToArray());
            _ui.setCharacterFilter(_characterFiltersValue);
            _ui.setCostFilter(_costFiltersValue);
            _ui.setTypeFilter(_typeFiltersValue);

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

            // 皮肤
            CardSkinData masterSkin = game.card.getSkin(deck.masterId);
            _ui.updateCurCardPoolInfo(deck.name, await loadDeckImage(deck));
            _ui.setCharacterName(masterSkin.name);

            // 更新UI
            _ui.setDeckListActive(false);
            _ui.setEditCardListActive(true);
            _ui.setDeckCardCount(_deck.cardList.Count, _deckMaxSize);
            _ui.hideDragCard();
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
            _ui.onUpdateCardListItem += onUpdateCardListItem;
            _ui.onStartDargCardListItem += onStartDragCardListItem;
            _ui.onStartDragDeckCardListItem += onStartDragDeckCardListItem;
            _ui.onEndDragCardListItem += onEndDragCardListItem;
            _ui.onEndDragDeckCardListItem += onEndDragDeckCardListItem;
            _ui.setSortTypes(_sortTypes);
            _ui.onSortTypeValueChanged += onSortTypeValueChanged;
            _ui.onCharacterFilterValueChanged += onCharacterFiltersValueChanged;
            _ui.setCostFilters(_costFilters);
            _ui.onCostFilterValueChanged += onCostFiltersValueChanged;
            _ui.setTypeFilters(_typeFilters);
            _ui.onTypeFilterValueChanged += onTypeFilterValueChanged;
            _ui.onSwitchRightToFilterDetailButtonClick += onSwitchRightToFilterDetailButtonClick;
            _ui.onFilterDetailCardPoolItemToggleValueChanged += onFilterDetailCardPoolItemToggleValueChanged;
            _ui.onClickDeckListItem += onClickDeckListItem;
            _ui.onClickDeckListItemDeleteButton += onClickDeckListItemDeleteButton;
            _ui.onDeckListItemNameValueChanged += onDeckListItemNameValueChanged;
            _ui.onClickDeckListAddButton += onClickDeckListAddButton;
            _ui.onClickMasterSelectPanelItem += onMasterSelect;
            _ui.onClickSaveDeckButton += onSaveDeckClick;
            _ui.onClickExportDeckButton += onExportDeckClick;
            _ui.onClickImportDeckButton += onClickImportButton;
            _ui.onClickUploadDeckButton += onUploadDeckClick;
            _ui.onClickDownloadDeckButton += onDownloadDeckClick;
        }
        #endregion
        #region 事件回调
        void onReturnButtonClick()
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
        private async void onUpdateCardListItem(int index, RectTransform itemRect)
        {
            EditorCardListItem item = itemRect.GetComponentInChildren<EditorCardListItem>();
            if (index < _cardDefineList.Count)
            {
                //显示卡牌
                CardDefine cardDefine = _cardDefineList[index];
                CardSkinData cardSkin = getCardSkinData(cardDefine);
                item.card.setCard(cardDefine, cardSkin);
                item.switchToCard();
                if (cardSkin.image == null)
                {
                    item.card.setImage(await game.card.loadExternalImage(cardSkin.imagePath));
                }
                //显示可用数量
                int max = cardDefine.hasKeyword(KEYWORD.UNIQUE) ? 1 : 2;
                int current = _deck.cardList.Where(id => id == cardDefine.id).Count();
                //TODO:更新UI
                item.setCount(current, max);
            }
        }
        private async void onStartDragCardListItem(int index)
        {
            _dragCard = _cardDefineList[index];
            CardSkinData skin = getCardSkinData(_dragCard);
            Sprite image = await game.card.loadCardImage(_dragCard.id);
            _ui.dragCard.setCard(_dragCard, skin);
            _ui.dragCard.setImage(image);
            _ui.dragDeckListItem.updateView(skin.name, image);
        }
        private async void onEndDragCardListItem(bool isOverDeckCardList)
        {
            if (!isOverDeckCardList)
                return;
            if (tryAddCardToDeck(_dragCard, out string errorTip))
            {
                // 更新可用信息
                _ui.updateCardListCount(_cardDefineList.Count);
                // 更新UI
                await updateDeckCardListUI();
            }
            else
                _ui.showMessage(errorTip);
        }
        private async void onStartDragDeckCardListItem(int index)
        {
            var cards = _deck.cardList.OrderBy(id => game.card.GetCardDefine(id).getCost()).GroupBy(id => id).ToArray();
            _dragCard = game.card.GetCardDefine(cards[index].Key);
            _deck.cardList.Remove(cards[index].Key);
            _ui.updateCardListCount(_cardDefineList.Count);
            await updateDeckCardListUI();
            CardSkinData skin = getCardSkinData(_dragCard);
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
                _ui.updateCardListCount(_cardDefineList.Count);
                // 更新UI
                await updateDeckCardListUI();
            }
            else
                _ui.showMessage(errorTip);
        }
        private void onSortTypeValueChanged(int index)
        {
            _sortType = index;
            loadCardListThenFilterAndSort();
            _ui.updateCardListCount(_cardDefineList.Count);
        }
        private void onCharacterFiltersValueChanged(bool[] value)
        {
            _characterFiltersValue = value;
            loadCardListThenFilterAndSort();
            _ui.updateCardListCount(_cardDefineList.Count);
        }
        private void onCostFiltersValueChanged(bool[] value)
        {
            _costFiltersValue = value;
            loadCardListThenFilterAndSort();
            _ui.updateCardListCount(_cardDefineList.Count);
        }
        private void onTypeFilterValueChanged(bool[] value)
        {
            _typeFiltersValue = value;
            loadCardListThenFilterAndSort();
            _ui.updateCardListCount(_cardDefineList.Count);
        }
        private async void onSwitchRightToFilterDetailButtonClick()
        {
            _ui.switchRightPanelToFilterDetail();
            _ui.clearFilterDetailCardPoolItems();
            await game.card.getCardPoolInfosAsync((path, info) =>
            {
                _ui.addFilterDetailCardPoolItem(info.cardPoolName);
                _cardPoolList.Add((path, info, false));
            });
        }
        private void onFilterDetailCardPoolItemToggleValueChanged(int index, bool value)
        {
            //保存当前引用的卡池文件状态
            _cardPoolList[index] = (_cardPoolList[index].path, _cardPoolList[index].info, value);
            //应用筛选条件
            loadCardListThenFilterAndSort();
            //更新卡牌列表
            _ui.updateCardListCount(_cardDefineList.Count);
        }
        private void onClickDeckListItem(int index)
        {
            display(_deckList[index]);
        }
        private async void onClickDeckListItemDeleteButton(int index)
        {
            game.deck.RemoveDeck(_deckList[index].id);
            _deckList.RemoveAt(index);
            await updateDeckListUI();
        }
        /// <summary>
        /// 点击新建卡组按钮
        /// </summary>
        private async void onClickDeckListAddButton()
        {
            _selectableMasters = _cardDefineList.Where(c => isAvailableCharacter(c)).ToArray();
            (string name, Sprite image)[] viewData = new (string name, Sprite image)[_selectableMasters.Length];
            for (int i = 0; i < viewData.Length; i++)
            {
                CardSkinData skin = getCardSkinData(_selectableMasters[i]);
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
                id = (int)game.deck.CreateDeck(StringTable.BUILD_NEW_DECK_DEFAULT_NAME, _selectableMasters[index].id),
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
        async void onDeckListItemNameValueChanged(int index, string name)
        {
            _deckList[index].name = name;
            game.deck.RenameDeck(_deckList[index].id, name);
            await updateDeckListUI();
        }
        /// <summary>
        /// 点击保存卡组按钮
        /// </summary>
        void onSaveDeckClick()
        {
            // 卡组保存
            if (_deck.cardList.Count < _deckMaxSize)
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
                copiedDeck.cardList.AddRange(cards);

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
            if (_deck.cardList.Count < _deckMaxSize)
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
        #endregion
        private async void loadCardListThenFilterAndSort()
        {
            _cardDefineList.Clear();
            _cardSkinList.Clear();
            //筛选
            //内部卡池
            CardDefine[] cardDefines = game.card.GetCardDefines(d => d.GetType().Assembly == typeof(THHGame).Assembly
            && d.GetType().GetCustomAttribute<IgnoreCardDefineAttribute>() == null);
            _cardDefineList.AddRange(cardDefines);
            //外部卡池
            List<string> pathList = new List<string>();
            for (int i = 0; i < _cardPoolList.Count; i++)
            {
                if (_cardPoolList[i].isEnabled)
                    pathList.Add(_cardPoolList[i].path);
            }
            var cards = await game.card.getExternalCardDefinesAsync(pathList.ToArray());
            _cardDefineList.AddRange(cards.defines);
            _cardSkinList.AddRange(cards.skins);
            //角色
            if (_characterFilters != null && _characterFilters.Length > 0 &&
                _characterFiltersValue != null && _characterFilters.Length == _characterFiltersValue.Length && _characterFiltersValue.Any(b => b))
            {
                _cardDefineList.RemoveAll(c =>
                {
                    int index = Array.IndexOf(_characterFilters, c.getCharacterID());
                    return !_characterFiltersValue[index];
                });
            }
            //费用
            if (_costFiltersValue != null && _costFiltersValue.Length == _costFilters.Length && _costFiltersValue.Any(b => b))
            {
                _cardDefineList.RemoveAll(c =>
                {
                    int cost = c.getCost();
                    if (cost >= 7 && !_costFiltersValue[7])
                    {
                        return false;
                    }
                    else if (cost <= 0 && !_costFiltersValue[0])
                    {
                        return false;
                    }
                    else
                    {
                        return !_costFiltersValue[cost];
                    }
                });
            }
            //类型
            //不显示英雄
            _cardDefineList.RemoveAll(c => c.type == DEFINE.MASTER);
            if (_typeFiltersValue != null && _typeFiltersValue.Length == _typeFilters.Length && _typeFiltersValue.Any(b => b))
            {
                _cardDefineList.RemoveAll(c =>
                {
                    int index = Array.IndexOf(_typeFilters, c.type);
                    return _typeFiltersValue[index];
                });
            }

            //排序
            if (_sortType == 0)
            {
                //费用
                _cardDefineList = _cardDefineList.OrderBy(c => c.getCost()).ToList();
            }
            else if (_sortType == 1)
            {
                //名称
                _cardDefineList = _cardDefineList.OrderBy(c => game.card.getSkin(c.id).name).ToList();
            }
        }
        /// <summary>
        /// 获取卡牌定义对应的皮肤数据
        /// </summary>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        private CardSkinData getCardSkinData(CardDefine cardDefine)
        {
            var cardSkinGroup = _cardSkinList.GroupBy(s => s.id).First(g => g.Key == cardDefine.id);
            CardSkinData cardSkin = cardSkinGroup.FirstOrDefault(s => s.language == game.langManager.getLanguage());
            if (cardSkin == null)
                cardSkin = cardSkinGroup.FirstOrDefault();
            return cardSkin;
        }
        /// <summary>
        /// 加载构筑卡组列表
        /// </summary>
        private void loadDeckList()
        {
            _deckList.Clear();
            foreach (var deckData in game.deck.GetDecks())
            {
                _deckList.Add(new Deck()
                {
                    id = (int)deckData.ID,
                    name = deckData.Name,
                    masterId = deckData.MasterID,
                    cardList = game.deck.GetDeckCards(deckData.ID).Values.Cast<int>().ToList()
                });
            }
        }
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
                return game.card.loadExternalImage(skin.imagePath);
            }
        }
        /// <summary>
        /// 判断卡组中是否存在需要提示的错误
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        private bool isDeckHasErrorTip(Deck deck)
        {
            return deck.cardList.Count < _deckMaxSize;
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
            if (_deck.cardList.Count >= _deckMaxSize)
            {
                errorTip = game.lang._(StringTable.BUILD_TIP_DECK_MAX_SIZE);
                return false;
            }
            //卡牌数量上限
            int cardCount = _deck.cardList.Where(i => i == card.id).Count();
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
            _deck.cardList.Add(card.id);
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
            game.deck.ClearDeck(_deck.id);
            foreach (var card in _deck.cardList)
            {
                game.deck.AddCard(_deck.id, card);
            }
            _isDeckChanged = false;
        }
        /// <summary>
        /// 导出卡组
        /// </summary>
        private void exportDeck()
        {
            string outputString = exportStringHeader + " " + _deck.name + "\n";

            List<int> cards = new List<int>(1 + _deck.cardList.Count);
            cards.Add(_deck.masterId);
            cards.AddRange(_deck.cardList);

            outputString += game.deck.DeckToString(cards.ToArray());
            GUIUtility.systemCopyBuffer = outputString;

            game.ui.dialog.ShowConfirm(game.lang._(StringTable.HINT_TITLE), game.lang._(PILE_EXPORTED), null);
        }
        private async Task uploadDeck()
        {
            if (!game.network.logic.AccountIsValid)
                return;

            List<DeckDataItem> deckToSend = new List<DeckDataItem>();
            foreach (var deck in _deckList)
            {
                List<int> cards = new List<int>();
                cards.Add(deck.masterId);
                cards.AddRange(deck.cardList);

                deckToSend.Add(new DeckDataItem()
                {
                    ID = deck.id,
                    Name = deck.name,
                    Content = game.deck.DeckToString(cards.ToArray())
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
        /// <summary>
        /// 更新卡组列表UI
        /// </summary>
        /// <returns></returns>
        private async Task updateDeckListUI()
        {
            (string name, Sprite image, bool errorTip)[] viewData = new (string name, Sprite image, bool errorTip)[_deckList.Count];
            for (int i = 0; i < _deckList.Count; i++)
            {
                viewData[i].name = _deckList[i].name;
                viewData[i].image = await loadDeckImage(_deckList[i]);
                viewData[i].errorTip = isDeckHasErrorTip(_deckList[i]);
            }
            _ui.updateDeckList(viewData);
        }
        private async Task updateDeckCardListUI()
        {
            var cards = _deck.cardList.OrderBy(id => game.card.GetCardDefine(id).getCost()).GroupBy(id => id).ToArray();
            (string name, Sprite image, int count)[] deckCardList = new (string name, Sprite image, int count)[cards.Length];
            for (int i = 0; i < deckCardList.Length; i++)
            {
                deckCardList[i].name = game.card.getSkin(cards[i].Key).name;
                deckCardList[i].image = await game.card.loadCardImage(cards[i].Key);
                deckCardList[i].count = cards[i].Count();
            }
            _ui.updateDeckCardList(deckCardList);
        }
        #endregion
        [SerializeField]
        BuildCardPanel _ui;
        [SerializeField]
        int _sortType;
        [SerializeField]
        int[] _characterFilters;
        [SerializeField]
        bool[] _characterFiltersValue;
        [SerializeField]
        bool[] _costFiltersValue;
        [SerializeField]
        bool[] _typeFiltersValue;
        [SerializeField]
        int _deckMaxSize = 30;
        [SerializeField]
        List<Deck> _deckList = new List<Deck>();
        [SerializeField]
        Deck _deck;
        [SerializeField]
        bool _isDeckChanged = false;
        [SerializeField]
        CardDefine[] _selectableMasters;
        [SerializeField]
        CardDefine _dragCard;
        List<(string path, CardPoolInfo info, bool isEnabled)> _cardPoolList = new List<(string path, CardPoolInfo info, bool isEnabled)>();
        List<CardDefine> _cardDefineList = new List<CardDefine>();
        List<CardSkinData> _cardSkinList = new List<CardSkinData>();
        readonly string[] _sortTypes = new string[]
        {
            "费用",
            "名称"
        };
        readonly string[] _costFilters = new string[]
        {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7+"
        };
        readonly string[] _typeFilters = new string[]
        {
            DEFINE.SERVANT,
            DEFINE.SPELL,
            DEFINE.ITEM
        };
        const string exportStringHeader = "# 醉梦传说卡组";
        [TranslateMsg]
        const string PILE_EXPORTED = "卡组已导出到剪贴板";
        [TranslateMsg]
        const string PILE_NOT_COMPLETE = "当前卡组不完整，无法在游戏中使用。\n是否要保存？";
        [TranslateMsg]
        const string NO_PILE_AVALIABLE = "剪贴板中没有可导入的卡组";
        [TranslateMsg]
        const string PILE_NOT_COMPLETE_EXPORT = "当前卡组不完整，无法在游戏中使用。\n是否要导出？";
    }
    public struct CardDefineRef
    {
        public int poolId;
        public int cardId;
    }
    [Serializable]
    public class Deck
    {
        public int id;
        public string name;
        public int masterId;
        public List<int> cardList = new List<int>();
    }
}
