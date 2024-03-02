using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Controllers
{
    class CardPoolViewPanelCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 公有方法
        /// <summary>
        /// 显示，并加载卡池信息。
        /// </summary>
        /// <param name="displayInfo"></param>
        /// <returns></returns>
        public async Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _displayInfo = info;
                // 自定义筛选器。
                if (info.cardFilter != null)
                    _cardFilter = info.cardFilter;
                // 卡池筛选器
                initCardPoolFilters(info);
                // 加载卡牌定义。
                await loadCardDefines();
                // 更新过滤器。
                initFilters(info);
                // 更新显示内容。
                updateContents();
            }
        }
        /// <summary>
        /// 显示并设置选项
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public Task display(long[] cardPoolsRange = null, long[] cardPoolFiltersId = null, DefineReference[] characterRefs = null, Predicate<CardDefine> cardFilter = null)
        {
            return display(new DisplayInfo()
            {
                cardPoolRangeId = cardPoolsRange,
                cardPoolFiltersId = cardPoolFiltersId,
                characterFilterRefs = characterRefs,
                cardFilter = cardFilter
            });
        }

        public void updateContents()
        {
            // 筛选。
            var cardDefines = new List<CardDefine>(_cardDefineList);
            filterCards(cardDefines);

            // 排序
            sortCards(cardDefines);

            _filteredCardDefineList = cardDefines;

            //更新UI
            updateCardPoolFilterNames();
            _ui.setCardPoolValues(_cardPoolFilterValue);
            _ui.setCharacterFilters(getCharacterNames());
            _ui.setCharacterFilter(_characterFiltersValue);
            _ui.setSortTypes(getSortTypeNames());
            _ui.setSortType(_sortType);
            _ui.setCostFilters(_costFilters);
            _ui.setCostFilter(_costFiltersValue);
            _ui.setTypeFilters(getCardTypeNames());
            _ui.setTypeFilter(_typeFiltersValue);
            _ui.setSearchInputField(_searchFilterString);
            _ui.updateCardListCount(_filteredCardDefineList.Count);
            _ui.gameObject.SetActive(true);
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            return _displayInfo;
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
        public void updateCardPoolFilterNames()
        {
            _ui.setCardPoolOptions(getCardPoolNames());
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
            _ui.onSearchInputFieldValueChanged += onSearchFilterValueChanged;
        }
        #endregion
        #region 事件回调
        private void onCardPoolValuesChanged(bool[] value)
        {
            _cardPoolFilterValue = value;
            updateContents();
        }
        private void onSortTypeValueChanged(int value)
        {
            _sortType = value;
            updateContents();
        }
        private void onCharacterFiltersValueChanged(bool[] value)
        {
            _characterFiltersValue = value;
            updateContents();
        }
        private void onCostFiltersValueChanged(bool[] value)
        {
            _costFiltersValue = value;
            updateContents();
        }
        private void onTypeFilterValueChanged(bool[] value)
        {
            _typeFiltersValue = value;
            updateContents();
        }
        private void onSearchFilterValueChanged(string value)
        {
            _searchFilterString = value;
            updateContents();
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
            await item.card.setCardDefine(card, game.card);
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
        #region 初始化
        /// <summary>
        /// 读取所有卡牌定义。
        /// </summary>
        /// <returns></returns>
        private async Task loadCardDefines()
        {
            var defines = new List<CardDefine>();
            var cardPools = _cardPoolIdList;
            foreach (var cardPoolId in cardPools)
            {
                //卡池卡牌
                var localRef = getCardPoolReference(cardPoolId);
                var (_, cardPool) = await game.card.getOrLoadCardPool(localRef);
                if (cardPool == null)
                    continue;
                if (_cardFilter == null)
                    defines.AddRange(cardPool.cardDefineList);
                else
                    defines.AddRange(cardPool.cardDefineList.Where(c => _cardFilter(c)));
            }
            _cardDefineList = defines;
        }
        /// <summary>
        /// 初始化所有卡牌筛选器。（不包括卡池筛选器）
        /// </summary>
        private void initFilters(DisplayInfo info)
        {
            // 卡牌类型筛选
            updateCardTypeFilters();
            // 人物筛选
            updateCharacterFilters(info.characterFilterRefs);
            // 验证筛选合法性。
            validateFilterValues();
        }
        /// <summary>
        /// 根据显示信息，更新卡池过滤器。
        /// </summary>
        private void initCardPoolFilters(DisplayInfo info)
        {
            List<long> cardPoolIdList = new List<long>();
            // 选项
            if (info.cardPoolRangeId != null)
            {
                cardPoolIdList.AddRange(info.cardPoolRangeId);
            }
            else
            {
                var result = game.card.getLocalCardPoolInfos();
                cardPoolIdList.AddRange(result.Select(a => a.cardPoolId));
            }
            cardPoolIdList.Sort();
            _cardPoolIdList = cardPoolIdList.ToArray();

            // 值列表
            if (info.cardPoolFiltersId != null)
            {
                var poolCount = _cardPoolIdList.Length;
                bool[] values = new bool[poolCount];
                //更改卡池过滤器的值
                for (int i = 0; i < poolCount; i++)
                {
                    values[i] = Array.Exists(info.cardPoolFiltersId, c => c == _cardPoolIdList[i]);
                }
                _cardPoolFilterValue = values;
            }
        }
        /// <summary>
        /// 更新卡牌类型过滤器。
        /// </summary>
        private void updateCardTypeFilters(string[] values = null)
        {
            // 没有指定要选中的类型，保留之前选择的卡牌类型。
            if (values == null)
            {
                values = _typeFilters.Where((c, i) => {
                    if (_typeFiltersValue == null)
                        return false;
                    if (i < 0 || i >= _typeFiltersValue.Length)
                        return false;
                    return _typeFiltersValue[i];
                }).ToArray();
            }
            // 更新选项。
            HashSet<string> cardTypeList = new HashSet<string>();
            foreach (var define in _cardDefineList)
            {
                cardTypeList.Add(define.type);
            }
            // 修改选择的类型值数组，保证同步。
            _typeFilters = cardTypeList.ToArray();
            // 根据人物引用，设置人物过滤器的值。
            if (values != null)
            {
                _typeFiltersValue = new bool[_typeFilters.Length];
                for (int i = 0; i < _typeFiltersValue.Length; i++)
                {
                    _typeFiltersValue[i] = Array.Exists(values, c => c == _typeFilters[i]);
                }
            }
        }
        /// <summary>
        /// 更新角色过滤器。
        /// </summary>
        /// <param name="values">要选中的所有角色引用。如果为null，则保留之前的选择。</param>
        private void updateCharacterFilters(DefineReference[] values = null)
        {
            // 没有指定要选中的角色引用，保留之前选择的角色。
            if (values == null)
            {
                values = _characterFilters.Where((c, i) => {
                    if (_characterFiltersValue == null)
                        return false;
                    if (i < 0 || i >= _characterFiltersValue.Length)
                        return false;
                    return _characterFiltersValue[i];
                }).ToArray();
            }
            // 更新选项。
            HashSet<DefineReference> characterRefList = new HashSet<DefineReference>()
            {
                new DefineReference(0, CATEGORY.CHARACTER_NEUTRAL)
            };
            foreach (var define in _cardDefineList)
            {
                characterRefList.Add(define.getCharacterRef());
            }
            // 修改选择的角色值数组，保证同步。
            _characterFilters = characterRefList.ToArray();
            // 根据人物引用，设置人物过滤器的值。
            if (values != null)
            {
                _characterFiltersValue = new bool[_characterFilters.Length];
                for (int i = 0; i < _characterFiltersValue.Length; i++)
                {
                    _characterFiltersValue[i] = Array.Exists(values, c => c == _characterFilters[i]);
                }
            }
        }
        /// <summary>
        /// 将过滤器的值变为合法的值。
        /// </summary>
        private void validateFilterValues()
        {
            if (_cardPoolFilterValue == null || _cardPoolFilterValue.Length != _cardPoolIdList.Length)
            {
                _cardPoolFilterValue = new bool[_cardPoolIdList.Length];
            }
            if (_characterFiltersValue == null || _characterFiltersValue.Length != _characterFilters.Length)
            {
                _characterFiltersValue = new bool[_characterFilters.Length];
            }
            if (_costFiltersValue == null || _costFiltersValue.Length != _costFilters.Length)
            {
                _costFiltersValue = new bool[_costFilters.Length];
            }
            if (_typeFiltersValue == null || _typeFiltersValue.Length != _typeFilters.Length)
            {
                _typeFiltersValue = new bool[_typeFilters.Length];
            }
        }
        #endregion
        /// <summary>
        /// 筛选一张卡牌定义。
        /// </summary>
        /// <param name="define">卡牌定义。</param>
        /// <returns>如果一张卡牌可以显示，则返回true，否则返回false。</returns>
        private bool filter(CardDefine define)
        {
            if (_cardPoolFilterValue.Contains(true))
            {
                int index = Array.IndexOf(_cardPoolIdList, define.cardPoolId);
                if (index < 0 || !_cardPoolFilterValue[index])
                    return false;
            }

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

            // 搜索筛选
            if (!string.IsNullOrEmpty(_searchFilterString))
            {
                var poolRef = getCardPoolReference(define.cardPoolId);
                if (game.card.tryGetCardSkin(poolRef, define.id, out var skin))
                {
                    if (!isCardFitsSearchString(define, skin, _searchFilterString))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool isCardFitsSearchString(CardDefine define, CardSkinData skin, string str)
        {
            var upperStr = str.ToUpper();
            // 名称
            if (!string.IsNullOrEmpty(skin.name) && skin.name.ToUpper().Contains(upperStr))
                return true;
            // 描述
            if (!string.IsNullOrEmpty(skin.desc) && skin.desc.ToUpper().Contains(upperStr))
                return true;

            // 种族
            var tags = define.getTags();
            var tagTexts = tags
                .Where(t => !string.IsNullOrEmpty(t))
                .Select(t => (game.card.tryGetTranslatedTag(t, out string text) ? text : t).ToUpper());
            if (tagTexts.Contains(upperStr))
                return true;

            // 搜索关键词
            foreach (var keyword in _searchKeywords)
            {
                var keywordText = game.lang._(keyword).ToUpper();
                var keywordPattern = $"({keywordText})[:：]([^ ]+)";
                var keywordMatches = Regex.Matches(upperStr, keywordPattern);
                for (int i = 0; i < keywordMatches.Count; i++)
                {
                    var match = keywordMatches[i];
                    if (match.Groups.Count >= 3)
                    {
                        var value = match.Groups[2].Value;
                        if (isCardFitsKeyword(define, keyword, value))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private bool isCardFitsKeyword(CardDefine define, string keyword, string value)
        {
            switch (keyword)
            {
                case SEARCH_KEYWORD_ATTACK:
                    if (!define.isServant() && !define.isItem() && !define.isMaster())
                        return false;
                    var cardAttack = define.getAttack();
                    if (fitsInt(value, cardAttack))
                        return true;
                    break;
                case SEARCH_KEYWORD_HEALTH:
                    if (!define.isServant() && !define.isItem() && !define.isMaster())
                        return false;
                    var cardLife = define.getLife();
                    if (fitsInt(value, cardLife))
                        return true;
                    break;
                case SEARCH_KEYWORD_COST:
                    var cardCost = define.getCost();
                    if (fitsInt(value, cardCost))
                        return true;
                    break;
            }
            return false;
        }
        private bool fitsInt(string str, int value)
        {
            if (int.TryParse(str, out var v))
            {
                if (value == v)
                    return true;
            }
            var rangeMatch = Regex.Match(str, rangePattern);
            if (rangeMatch.Success && rangeMatch.Groups.Count >= 3)
            {
                var minStr = rangeMatch.Groups[1].Value;
                var maxStr = rangeMatch.Groups[2].Value;
                if (int.TryParse(minStr, out var min) && int.TryParse(maxStr, out var max))
                {
                    return value >= min && value <= max;
                }
            }
            var lesserMatch = Regex.Match(str, lesserPattern);
            if (lesserMatch.Success && lesserMatch.Groups.Count >= 2)
            {
                var maxStr = lesserMatch.Groups[1].Value;
                if (int.TryParse(maxStr, out var max))
                {
                    return value <= max;
                }
            }
            var greaterMatch = Regex.Match(str, greaterPattern);
            if (greaterMatch.Success && greaterMatch.Groups.Count >= 2)
            {
                var minStr = greaterMatch.Groups[1].Value;
                if (int.TryParse(minStr, out var min))
                {
                    return value >= min;
                }
            }
            return false;
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
                    string nameA = game.card.getCardSkin(a).name;
                    string nameB = game.card.getCardSkin(b).name;
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
        /// 获取所有卡池的名称。
        /// </summary>
        /// <returns>卡池名称数组。</returns>
        private string[] getCardPoolNames()
        {
            var cardPools = _cardPoolIdList;
            string[] cardPoolNames = new string[cardPools.Length];
            for (int i = 0; i < cardPools.Length; i++)
            {
                var cardPoolId = cardPools[i];
                var localPoolRef = getCardPoolReference(cardPoolId);
                var poolInfo = game.card.getCardPoolInfo(localPoolRef);
                var cardPoolName = game.card.getCardPoolName(poolInfo);
                cardPoolNames[i] = cardPoolName;
            }
            return cardPoolNames;
        }
        /// <summary>
        /// 获取所有卡牌类型的名称。
        /// </summary>
        /// <returns>卡牌类型名称数组。</returns>
        private string[] getCardTypeNames()
        {
            return _typeFilters.Select(t => game.editor.getCardTypeName(t)).ToArray();
        }
        /// <summary>
        /// 获取排序类型的名称。
        /// </summary>
        /// <returns>排序类型名称数组。</returns>
        private string[] getSortTypeNames()
        {
            return _sortTypes.Select(t => game.lang._(t)).ToArray();
        } 
        /// <summary>
        /// 获取所有人物的名称。
        /// </summary>
        /// <returns>人物名称数组。</returns>
        /// <returns>人物名称数组。</returns>
        private string[] getCharacterNames()
        {
            List<string> characterNameList = new List<string>
            {
                game.lang._(StringTable.CARDVIEW_CHARACTER_NEUTRAL)
            };
            for (int i = 1; i < _characterFilters.Length; i++)
            {
                var characterRef = _characterFilters[i];
                var localPoolRef = getCardPoolReference(characterRef.cardPoolId);
                var character = game.card.getCardDefine(localPoolRef, characterRef.defineId);
                if (character == null)
                    continue;
                characterNameList.Add(game.card.getCardSkin(character).name);
            }
            return characterNameList.ToArray();
        }
        private ICardPoolReference getCardPoolReference(long cardPoolId)
        {
            return game.card.getLocalCardPoolReference(cardPoolId);
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
        public CardPoolViewPanel ui => _ui;

        [TranslateMsg("卡牌搜索栏所使用的关键词")]
        public const string SEARCH_KEYWORD_ATTACK = "攻击力";
        [TranslateMsg("卡牌搜索栏所使用的关键词")]
        public const string SEARCH_KEYWORD_HEALTH = "生命值";
        [TranslateMsg("卡牌搜索栏所使用的关键词")]
        public const string SEARCH_KEYWORD_COST = "灵力值";

        public const string rangePattern = "([0-9]+)-([0-9]+)";
        public const string lesserPattern = "([0-9]+)-";
        public const string greaterPattern = "([0-9]+)\\+";

        [SerializeField]
        CardPoolViewPanel _ui;
        List<CardDefine> _cardDefineList = new List<CardDefine>();
        List<CardDefine> _filteredCardDefineList = new List<CardDefine>();
        [SerializeField]
        DisplayInfo _displayInfo;
        [SerializeField]
        long[] _cardPoolIdList;
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
        private string[] _typeFilters;
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
        readonly static string[] _searchKeywords =
        {
            SEARCH_KEYWORD_ATTACK,
            SEARCH_KEYWORD_HEALTH,
            SEARCH_KEYWORD_COST,
        };
        #endregion
        #region 内部类
        class DisplayInfo : UIDisplayInfo
        {
            public long[] cardPoolRangeId = null;
            public long[] cardPoolFiltersId = null;
            public DefineReference[] characterFilterRefs = null;
            public bool[] costFiltersValue = null;
            public bool[] typeFiltersValue = null;
            public string searchFilterValue = null;
            public Predicate<CardDefine> cardFilter = null;
        }
        #endregion
    }
}
