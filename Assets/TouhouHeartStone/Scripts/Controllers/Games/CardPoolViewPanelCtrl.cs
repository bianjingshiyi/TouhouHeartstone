using System;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using System.Reflection;
using Game;
using UnityEngine;
using BJSYGameCore.UI;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.EventSystems;
using TouhouCardEngine.Shared;

namespace Controllers
{
    class CardPoolViewPanelCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 公有方法
        public async Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                //加载卡池
                await loadCardPoolInfos(info);

                // 更新卡池过滤器。
                updateCardPoolFilters(info);

                // 加载卡牌定义。
                await loadCardDefines();

                // 更新筛选器。
                updateFilters(info);

                // 筛选。
                var cardDefines = new List<CardDefine>(_cardDefineList);
                filterCards(cardDefines);

                // 排序
                sortCards(cardDefines);

                _filteredCardDefineList = cardDefines;

                //更新UI
                _ui.gameObject.SetActive(true);
                _ui.setCardPoolOptions(getCardPoolNames());
                _ui.setCardPoolValues(_cardPoolFilterValue);
                _ui.setCharacterFilters(getCharacterNames());
                _ui.setCharacterFilter(_characterFiltersValue);
                _ui.setSortTypes(_sortTypes);
                _ui.setSortType(_sortType);
                _ui.setCostFilters(_costFilters);
                _ui.setCostFilter(_costFiltersValue);
                string[] typesName = new string[]
                {
                    game.langManager.getString(StringTable.GAME_CARD_TYPE_SERVANT),
                    game.langManager.getString(StringTable.GAME_CARD_TYPE_SPELL),
                    game.langManager.getString(StringTable.GAME_CARD_TYPE_ITEM)
                };
                _ui.setTypeFilters(typesName);
                _ui.setTypeFilter(_typeFiltersValue);
                _ui.setSearchInputField(_searchFilterString);
                _ui.updateCardListCount(_filteredCardDefineList.Count);
            }
        }
        public Task display(int cardId, long[] cardPoolsId = null, DefineReference[] characterRefs = null, Predicate<CardDefine> cardFilter = null)
        {
            return display(new DisplayInfo()
            {
                cardPoolFiltersId = cardPoolsId,
                characterFilterRefs = characterRefs,
                cardFilter = cardFilter,
                cardId = cardId
            });
        }
        /// <summary>
        /// 显示并设置选项
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public Task display(long[] cardPoolsId = null, bool? hideBuiltinCards = null, DefineReference[] characterRefs = null, Predicate<CardDefine> cardFilter = null)
        {
            return display(new DisplayInfo()
            {
                cardPoolFiltersId = cardPoolsId,
                hideBuiltinCards = hideBuiltinCards,
                characterFilterRefs = characterRefs,
                cardFilter = cardFilter
            });
        }
        /// <summary>
        /// 显示指定卡池中的卡牌
        /// </summary>
        /// <param name="cardPoolInfo"></param>
        /// <returns></returns>
        public Task display(CardPoolInfo cardPoolInfo, Predicate<CardDefine> cardFilter = null)
        {
            return display(new DisplayInfo()
            {
                cardPoolInfos = new CardPoolInfo[] { cardPoolInfo },
                cardPoolFiltersId = new long[] { cardPoolInfo.cardPoolId },
                cardFilter = cardFilter
            });
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            return new DisplayInfo();
        }
        public Task hide()
        {
            _ui.gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        public void enableCount(bool value)
        {
            _hideCount = !value;
        }
        public void enableCardPoolFilter(bool value)
        {
            _ui.enableCardPoolFilter(value);
        }
        public CardDefine getCardDefineAt(int index)
        {
            return _filteredCardDefineList[index];
        }
        public int getCardListItemIndex(EditorCardListItem item)
        {
            return _ui.getCardItemIndex(item.transform as RectTransform);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _ui.onEnableCardListItem += onEnableCardListItemCallback;
            _ui.onDisableCardListItem += onDisableCardListItemCallback;
            _ui.onUpdateCardListItem += onUpdateCardListItemCallback;
            _ui.onCardPoolsValueChanged += onCardPoolValuesChanged;
            _ui.onSortTypeValueChanged += onSortTypeValueChanged;
            _ui.onCharacterFilterValueChanged += onCharacterFiltersValueChanged;
            _ui.onCostFilterValueChanged += onCostFiltersValueChanged;
            _ui.onTypeFilterValueChanged += onTypeFilterValueChanged;
        }
        #endregion
        #region 事件回调
        private async void onCardPoolValuesChanged(bool[] value)
        {
            _cardPoolFilterValue = value;
            await displayForApplyFilters();
        }
        private async void onSortTypeValueChanged(int value)
        {
            _sortType = value;
            await displayForApplyFilters();
        }
        private async void onCharacterFiltersValueChanged(bool[] value)
        {
            _characterFiltersValue = value;
            await displayForApplyFilters();
        }
        private async void onCostFiltersValueChanged(bool[] value)
        {
            _costFiltersValue = value;
            await displayForApplyFilters();
        }
        private async void onTypeFilterValueChanged(bool[] value)
        {
            _typeFiltersValue = value;
            await displayForApplyFilters();
        }
        private void onEnableCardListItemCallback(int index, RectTransform itemRect)
        {
            EditorCardListItem item = itemRect.GetComponent<EditorCardListItem>();
            item.onClick += onClickCardListItem;
            item.onBeginDrag += onStartDragCardListItemCallback;
            item.onDrag += onDragCardListItemCallback;
            item.onEndDrag += onEndDragCardListItemCallback;
        }
        private void onDisableCardListItemCallback(int index, RectTransform itemRect)
        {
            EditorCardListItem item = itemRect.GetComponent<EditorCardListItem>();
            item.onClick -= onClickCardListItem;
            item.onBeginDrag -= onStartDragCardListItemCallback;
            item.onDrag -= onDragCardListItemCallback;
            item.onEndDrag -= onEndDragCardListItemCallback;
        }
        private async void onUpdateCardListItemCallback(int index, RectTransform itemRect)
        {
            //防止索引超界
            if (index < 0 || index >= _filteredCardDefineList.Count)
                return;
            EditorCardListItem item = itemRect.GetComponent<EditorCardListItem>();
            if (_hideCount)
            {
                item.hideCount();
            }
            CardDefine card = _filteredCardDefineList[index];
            CardSkinData skin = game.card.getDefaultSkin(card);
            item.card.setCard(card, skin);
            Sprite sprite = skin.image != null ?
                skin.image :
                await game.card.loadImage(skin.imagePath, game.card.getCardPoolInfoOfCard(card) is CardPoolInfo cardPoolInfo ? cardPoolInfo.cardPoolId : 0);
            item.card.setImage(sprite);
            onUpdateCardListItem?.Invoke(index, item);
        }
        private void onClickCardListItem(EditorCardListItem item)
        {
            int index = _ui.getCardItemIndex(item.transform as RectTransform);
            CardDefine card = _filteredCardDefineList[index];
            onClickCard?.Invoke(card);
        }
        private async void onStartDragCardListItemCallback(EditorCardListItem item, PointerEventData eventData)
        {
            int index = _ui.getCardItemIndex(item.transform as RectTransform);
            if (index < 0)
            {
                //物体已经被销毁
                _ui.cardListScrollRect.OnBeginDrag(eventData);
                return;
            }
            CardDefine card = _filteredCardDefineList[index];
            bool isEventUsed = onStartDragCard != null ? await onStartDragCard.Invoke(card, eventData) : false;
            if (!isEventUsed)
            {
                _ui.cardListScrollRect.OnBeginDrag(eventData);
            }
        }
        private async void onDragCardListItemCallback(EditorCardListItem item, PointerEventData eventData)
        {
            int index = _ui.getCardItemIndex(item.transform as RectTransform);
            if (index < 0)
            {
                //物体已经被销毁
                _ui.cardListScrollRect.OnDrag(eventData);
                return;
            }
            CardDefine card = _filteredCardDefineList[index];
            bool isEventUsed = onDragCard != null ? await onDragCard.Invoke(card, eventData) : false;
            if (!isEventUsed)
            {
                _ui.cardListScrollRect.OnDrag(eventData);
            }
        }
        private async void onEndDragCardListItemCallback(EditorCardListItem item, PointerEventData eventData)
        {
            int index = _ui.getCardItemIndex(item.transform as RectTransform);
            if (index < 0)
            {
                //物体已经被销毁
                _ui.cardListScrollRect.OnEndDrag(eventData);
                return;
            }
            CardDefine card = _filteredCardDefineList[index];
            bool isEventUsed = onEndDragCard != null ? await onEndDragCard.Invoke(card, eventData) : false;
            if (!isEventUsed)
            {
                _ui.cardListScrollRect.OnEndDrag(eventData);
            }
        }
        #endregion
        private async Task loadCardPoolInfos(DisplayInfo info)
        {
            if (info.cardPoolInfos != null)
            {
                _cardPoolInfoList.Clear();
                _cardPoolInfoList.AddRange(info.cardPoolInfos);
            }
            else
            {
                var result = await game.card.loadCardPoolInfosAsync();
                _cardPoolInfoList = new List<CardPoolInfo>(result.Values);
            }
            _cardPoolInfoList.Sort((a, b) => (int)(a.cardPoolId - b.cardPoolId));
        }
        /// <summary>
        /// 获取所有卡池的名称。
        /// </summary>
        /// <returns>卡池名称数组。</returns>
        private string[] getCardPoolNames()
        {
            string[] cardPoolNames = new string[_cardPoolInfoList.Count + 1];
            for (int i = 0; i < _cardPoolInfoList.Count; i++)
            {
                cardPoolNames[i] = _cardPoolInfoList[i].cardPoolName;
            }
            cardPoolNames[cardPoolNames.Length - 1] = game.langManager.getString(StringTable.CARDPOOL_NAME_BUILTIN);
            return cardPoolNames;
        }
        /// <summary>
        /// 获取所有人物的名称。
        /// </summary>
        /// <returns>人物名称数组。</returns>
        private string[] getCharacterNames()
        {
            List<string> characterNameList = new List<string>
            {
                StringTable.CARDVIEW_CHARACTER_NEUTRAL
            };
            for (int i = 0; i < _cardDefineList.Count; i++)
            {
                if (_cardDefineList[i].isMaster())
                {
                    characterNameList.Add(game.card.getDefaultSkin(_cardDefineList[i]).name);
                }
            }
            return characterNameList.ToArray();
        }
        /// <summary>
        /// 根据卡池的过滤器，读取指定卡牌内的所有卡牌定义。
        /// </summary>
        /// <returns></returns>
        private async Task loadCardDefines()
        {
            // 加载卡片
            var defines = new List<CardDefine>();
            // 内置卡池。
            if (_cardPoolFilterValue[_cardPoolFilterValue.Length - 1])
            {
                //反射硬编码卡牌
                defines.AddRange(game.card.GetCardDefines(d =>
                    d.GetType().Assembly == typeof(THHGame).Assembly &&
                    d.GetType().GetCustomAttribute<IgnoreCardDefineAttribute>() == null, includeExternalCard: false));
            }
            // 外部卡池。
            for (int i = 0; i < _cardPoolInfoList.Count; i++)
            {
                //卡池卡牌
                CardDefine[] cardDefines = await game.card.getOrLoadCardDefines(_cardPoolInfoList[i].ToReference());
                if (!_cardPoolFilterValue[i])
                    continue;
                defines.AddRange(cardDefines);
            }
            _cardDefineList = defines;
        }
        /// <summary>
        /// 筛选一张卡牌定义。
        /// </summary>
        /// <param name="define">卡牌定义。</param>
        /// <returns>如果一张卡牌可以显示，则返回true，否则返回false。</returns>
        private bool filter(CardDefine define)
        {
            // 自定义筛选器。
            if (_cardFilter != null)
            {
                if (!_cardFilter(define))
                    return false;
            }
            else
            {
                // 不可收藏。
                if (define.isToken() || define.isMaster() || define.isSkill())
                    return false;
            }

            // 筛选角色。
            if (_characterFiltersValue.Contains(true))
            {
                int index = Array.IndexOf(_characterFilters, define.getCharacterRef());
                if (index < 0 || !_characterFiltersValue[index])
                    return false;
            }

            // 筛选费用。
            if (_costFiltersValue.Contains(true))
            {
                int index = Mathf.Clamp(define.getCost(), 0, _costFiltersValue.Length - 1);
                if (index < 0 || !_costFiltersValue[index])
                    return false;
            }

            // 筛选类型。
            if (_typeFiltersValue.Contains(true))
            {
                int index = Array.IndexOf(_typeFilters, define.type);
                if (index < 0 || !_typeFiltersValue[index])
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 筛选所有卡牌。
        /// </summary>
        /// <param name="cardDefines">要筛选的卡牌。</param>
        private void filterCards(List<CardDefine> cardDefines)
        {
            cardDefines.RemoveAll(c => !filter(c));
        }
        /// <summary>
        /// 排序所有卡牌。
        /// </summary>
        /// <param name="cardDefines">要排序的卡牌。</param>
        private void sortCards(List<CardDefine> cardDefines)
        {
            //排序
            if (_sortType == 0)
            {
                //费用排序
                cardDefines.Sort((a, b) =>
                {
                    var compared = a.getCost() - b.getCost();
                    if (compared == 0)
                    {
                        return a.id.CompareTo(b.id);
                    }
                    return compared;
                });
            }
            else if (_sortType == 1)
            {
                //名称排序
                cardDefines.Sort((a, b) =>
                {
                    string nameA = game.card.getDefaultSkin(a).name;
                    string nameB = game.card.getDefaultSkin(b).name;
                    var compared = nameA.CompareTo(nameB);
                    if (compared == 0)
                    {
                        return a.id.CompareTo(b.id);
                    }
                    return compared;
                });
            }
        }
        /// <summary>
        /// 更新所有卡牌筛选器。（卡池筛选器除外）
        /// </summary>
        private void updateFilters(DisplayInfo info)
        {
            // 自定义筛选器。
            if (info.cardFilter != null)
            {
                _cardFilter = info.cardFilter;
            }
            // 人物筛选
            _characterFilters = getCardDefineCharacters(_cardDefineList);
            updateCharacterFilterValues(info);
            updateCostFilters();
            updateTypeFilters();
        }
        private void updateCardPoolFilters(DisplayInfo info)
        {
            var poolInfos = _cardPoolInfoList;
            bool[] values = _cardPoolFilterValue;
            if (info.cardPoolFiltersId != null)
            {
                //更改卡池过滤器的值
                values = new bool[poolInfos.Count + 1];
                for (int i = 0; i < poolInfos.Count; i++)
                {
                    values[i] = Array.Exists(info.cardPoolFiltersId, c => c == poolInfos[i].cardPoolId);
                }
            }
            else if (values == null || values.Length != poolInfos.Count + 1)
            {
                values = new bool[poolInfos.Count + 1];
            }
            // 显示内置卡池。
            if (info.hideBuiltinCards != null)
            {
                values[values.Length - 1] = !info.hideBuiltinCards.Value;
            }
            _cardPoolFilterValue = values;
        }
        /// <summary>
        /// 根据所有卡牌定义，获取所有可用的人物。
        /// </summary>
        /// <param name="cardDefines">卡牌定义列表。</param>
        /// <returns></returns>
        private DefineReference[] getCardDefineCharacters(IEnumerable<CardDefine> cardDefines)
        {
            HashSet<DefineReference> characterRefList = new HashSet<DefineReference>()
            {
                new DefineReference(0, CATEGORY.CHARACTER_NEUTRAL)
            };
            foreach (var define in cardDefines)
            {
                if (define.isMaster())
                {
                    characterRefList.Add(define.getDefineRef());
                }
            }
            return characterRefList.ToArray();
        }
        /// <summary>
        /// 更新人物筛选器的已选人物。
        /// </summary>
        /// <param name="info">显示信息。</param>
        private void updateCharacterFilterValues(DisplayInfo info)
        {
            if (info.characterFilterRefs != null)
            {
                _characterFiltersValue = new bool[_characterFilters.Length];
                for (int i = 0; i < _characterFiltersValue.Length; i++)
                {
                    _characterFiltersValue[i] = Array.Exists(info.characterFilterRefs, c => c == _characterFilters[i]);
                }
            }
            else if (_cardPoolFilterValue == null || _cardPoolFilterValue.Length != _cardPoolInfoList.Count + 1)
            {
                _cardPoolFilterValue = new bool[_cardPoolInfoList.Count + 1];
            }
        }

        private void updateCostFilters()
        {
            if (_costFiltersValue == null || _costFiltersValue.Length != _costFilters.Length)
            {
                _costFiltersValue = new bool[_costFilters.Length];
            }
        }
        private void updateTypeFilters()
        {
            if (_typeFiltersValue == null || _typeFiltersValue.Length != _typeFilters.Length)
            {
                _typeFiltersValue = new bool[_typeFilters.Length];
            }
        }


        /// <summary>
        /// 为了引用过滤而重新显示界面
        /// </summary>
        /// <returns></returns>
        private async Task displayForApplyFilters()
        {
            List<long> cardPoolsIdList = new List<long>();
            for (int i = 0; i < _cardPoolInfoList.Count; i++)
            {
                if (_cardPoolFilterValue[i])
                {
                    cardPoolsIdList.Add(_cardPoolInfoList[i].cardPoolId);
                }
            }
            List<DefineReference> charactersIdList = new List<DefineReference>();
            for (int i = 0; i < _characterFiltersValue.Length; i++)
            {
                if (_characterFiltersValue[i])
                {
                    charactersIdList.Add(_characterFilters[i]);
                }
            }
            await display(new DisplayInfo()
            {
                cardPoolInfos = _cardPoolInfoList.ToArray(),
                hideBuiltinCards = !_cardPoolFilterValue[_cardPoolFilterValue.Length - 1],
                cardPoolFiltersId = cardPoolsIdList.ToArray(),
                sortType = _sortType,
                characterFilterRefs = charactersIdList.ToArray(),
                costFiltersValue = _costFiltersValue,
                typeFiltersValue = _typeFiltersValue,
                searchFilterValue = _searchFilterString,
                cardFilter = _cardFilter
            });
        }
        #endregion
        #region 事件
        public event Action<CardDefine> onClickCard;
        public event Action<int, EditorCardListItem> onUpdateCardListItem;
        /// <summary>
        /// 当列表中的卡牌开始被拖拽，参数为被拖拽的卡牌和拖拽指针信息，返回值代表拖拽事件是否被使用，如果拖拽事件没有被使用那么会触发卡牌列表的滚动。
        /// </summary>
        public event Func<CardDefine, PointerEventData, Task<bool>> onStartDragCard;
        /// <summary>
        /// 当列表中的卡牌被拖拽，参数为被拖拽的卡牌和拖拽指针信息，返回值代表拖拽事件是否被使用，如果拖拽事件没有被使用那么会触发卡牌列表的滚动。
        /// </summary>
        public event Func<CardDefine, PointerEventData, Task<bool>> onDragCard;
        /// <summary>
        /// 当列表中的卡牌结束被拖拽，参数为被拖拽的卡牌和拖拽指针信息，返回值代表拖拽事件是否被使用，如果拖拽事件没有被使用那么会触发卡牌列表的滚动。
        /// </summary>
        public event Func<CardDefine, PointerEventData, Task<bool>> onEndDragCard;
        #endregion
        #region 属性字段
        [SerializeField]
        CardPoolViewPanel _ui;
        List<CardPoolInfo> _cardPoolInfoList = new List<CardPoolInfo>();
        List<CardDefine> _cardDefineList = new List<CardDefine>();
        List<CardDefine> _filteredCardDefineList = new List<CardDefine>();
        [SerializeField]
        bool _hideCount = false;
        [SerializeField]
        bool[] _cardPoolFilterValue;
        [SerializeField]
        int _sortType;
        [SerializeField]
        DefineReference[] _characterFilters;
        [SerializeField]
        bool[] _characterFiltersValue;
        [SerializeField]
        bool[] _costFiltersValue;
        [SerializeField]
        bool[] _typeFiltersValue;
        [SerializeField]
        string _searchFilterString;
        Predicate<CardDefine> _cardFilter;
        readonly string[] _sortTypes = new string[]
        {
            StringTable.CARDVIEW_SORTTYPE_COST,
            StringTable.CARDVIEW_SORTTYPE_NAME
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
        #endregion
        #region 内部类
        class DisplayInfo : UIDisplayInfo
        {
            public CardPoolInfo[] cardPoolInfos = null;
            public bool? hideBuiltinCards = null;
            public long[] cardPoolFiltersId = null;
            public int sortType = -1;
            public DefineReference[] characterFilterRefs = null;
            public bool[] costFiltersValue = null;
            public bool[] typeFiltersValue = null;
            public string searchFilterValue = null;
            public Predicate<CardDefine> cardFilter = null;
            public int cardId;
        }
        #endregion
    }
}
