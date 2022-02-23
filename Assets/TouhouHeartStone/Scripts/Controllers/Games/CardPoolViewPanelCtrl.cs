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
                var result = await game.card.loadCardPoolInfosAsync();
                _cardPoolInfoList = new List<CardPoolInfo>(result.Values);
                _cardPoolInfoList.Sort((a, b) => a.cardPoolId - b.cardPoolId);
                _cardPoolFilterValue = new bool[_cardPoolInfoList.Count];
                for (int i = 0; i < _cardPoolFilterValue.Length; i++)
                {
                    _cardPoolFilterValue[i] = info.cardPoolsId == null || Array.Exists(info.cardPoolsId, cardPooId => _cardPoolInfoList[i].cardPoolId == cardPooId);
                }
                if (info.cardPoolsId != null)
                {
                    //更改卡池过滤器的值
                    _cardPoolFilterValue = new bool[_cardPoolInfoList.Count];
                    for (int i = 0; i < _cardPoolFilterValue.Length; i++)
                    {
                        _cardPoolFilterValue[i] =
                            game.card.isStandardCardPool(_cardPoolInfoList[i])//标准卡池一定包含
                            || Array.Exists(info.cardPoolsId, cardPoolId => cardPoolId == _cardPoolInfoList[i].cardPoolId);
                    }
                }
                else if (_cardPoolFilterValue == null || _cardPoolFilterValue.Length != _cardPoolInfoList.Count)
                {
                    _cardPoolFilterValue = new bool[_cardPoolInfoList.Count];
                }
                //加载卡片
                _cardDefineList = new List<CardDefine>();
                //反射硬编码卡牌
                _cardDefineList.AddRange(game.card.GetCardDefines(d =>
                    d.GetType().Assembly == typeof(THHGame).Assembly &&
                    d.GetType().GetCustomAttribute<IgnoreCardDefineAttribute>() == null, includeExternalCard: false));
                foreach (var cardPoolInfo in _cardPoolInfoList)
                {
                    //卡池卡牌
                    CardDefine[] cardDefines = await game.card.getOrLoadCardDefines(cardPoolInfo.cardPoolId);
                    _cardDefineList.AddRange(cardDefines);
                }
                //过滤
                _cardFilter = info.cardFilter;
                _filteredCardDefineList = new List<CardDefine>(_cardDefineList);
                if (_cardFilter == null)
                {
                    _filteredCardDefineList.RemoveAll(c =>
                    {
                        if (c.isToken())
                        {
                            //衍生物
                            return true;
                        }
                        if (c.isMaster())
                        {
                            //角色卡
                            return true;
                        }
                        if (c.isSkill())
                        {
                            //技能卡
                            return true;
                        }
                        return false;
                    });
                }
                else
                {
                    _filteredCardDefineList.RemoveAll(c => !_cardFilter(c));
                }
                List<int> characterIdList = new List<int>();
                characterIdList.Add(CATEGORY.CHARACTER_NEUTRAL);
                for (int i = 0; i < _cardDefineList.Count; i++)
                {
                    if (_cardPoolFilterValue.Any(v => v))
                    {
                        //有对卡池进行过滤
                        CardPoolInfo cardPoolInfo = game.card.getCardPoolOfCard(_cardDefineList[i]);
                        if (cardPoolInfo != null)
                        {
                            int cardPoolInfoIndex = _cardPoolInfoList.FindIndex(c => c.cardPoolId == cardPoolInfo.cardPoolId);
                            if (!_cardPoolFilterValue[cardPoolInfoIndex])
                            {
                                //忽略不在选中卡池中的角色
                                continue;
                            }
                        }
                    }
                    if (_cardDefineList[i].isMaster())
                    {
                        characterIdList.Add(_cardDefineList[i].id);
                    }
                }
                _characterFilters = characterIdList.ToArray();
                if (info.charactersId != null)
                {
                    _characterFiltersValue = new bool[_characterFilters.Length];
                    for (int i = 0; i < _characterFiltersValue.Length; i++)
                    {
                        _characterFiltersValue[i] = Array.Exists(info.charactersId, characterId => characterId == _characterFilters[i]);
                    }
                }
                else if (_characterFiltersValue == null || _characterFiltersValue.Length != _characterFilters.Length)
                {
                    _characterFiltersValue = new bool[_characterFilters.Length];
                }
                if (_characterFiltersValue.Any(v => v))
                {
                    //过滤角色
                    _filteredCardDefineList.RemoveAll(c =>
                    {
                        int index = Array.IndexOf(_characterFilters, c.getCharacterID());
                        if (index < 0)
                        {
                            return true;
                        }
                        else
                        {
                            return !_characterFiltersValue[index];
                        }
                    });
                }
                if (_costFiltersValue == null || _costFiltersValue.Length != _costFilters.Length)
                {
                    _costFiltersValue = new bool[_costFilters.Length];
                }
                if (_costFiltersValue.Any(v => v))
                {
                    //过滤费用
                    _filteredCardDefineList.RemoveAll(c =>
                    {
                        int cost = c.getCost();
                        if (cost >= 7)
                        {
                            return !_costFiltersValue[7];
                        }
                        else if (cost <= 0)
                        {
                            return !_costFiltersValue[0];
                        }
                        else
                        {
                            return !_costFiltersValue[cost];
                        }
                    });
                }
                if (_typeFiltersValue == null || _typeFiltersValue.Length != _typeFilters.Length)
                {
                    _typeFiltersValue = new bool[_typeFilters.Length];
                }
                if (_typeFiltersValue.Any(v => v))
                {
                    //过滤类型
                    _filteredCardDefineList.RemoveAll(c =>
                    {
                        int index = Array.IndexOf(_typeFilters, c.type);
                        if (index < 0)
                            return true;
                        else
                            return !_typeFiltersValue[index];
                    });
                }
                //排序
                if (_sortType == 0)
                {
                    //费用排序
                    _filteredCardDefineList.Sort((a, b) => a.getCost() - b.getCost());
                }
                else if (_sortType == 1)
                {
                    //名称排序
                    _filteredCardDefineList.Sort((a, b) =>
                    {
                        string nameA = game.card.getDefaultSkin(a).name;
                        string nameB = game.card.getDefaultSkin(b).name;
                        return nameA.CompareTo(nameB);
                    });
                }
                //更新UI
                string[] cardPoolNames = new string[_cardPoolInfoList.Count];
                for (int i = 0; i < cardPoolNames.Length; i++)
                {
                    cardPoolNames[i] = _cardPoolInfoList[i].cardPoolName;
                }
                _ui.setCardPoolOptions(cardPoolNames);
                _ui.setCardPoolValues(_cardPoolFilterValue);
                _ui.setSortType(_sortType);
                string[] characterNames = new string[_characterFilters.Length];
                for (int i = 0; i < characterNames.Length; i++)
                {
                    if (_characterFilters[i] == CATEGORY.CHARACTER_NEUTRAL)
                    {
                        characterNames[i] = game.langManager.getString(StringTable.CARDVIEW_CHARACTER_NEUTRAL);
                    }
                    else
                    {
                        characterNames[i] = game.card.getDefaultSkin(game.card.GetCardDefine(_characterFilters[i])).name;
                    }
                }
                List<string> characterNameList = new List<string>();
                characterNameList.Add(StringTable.CARDVIEW_CHARACTER_NEUTRAL);
                for (int i = 0; i < _cardDefineList.Count; i++)
                {
                    if (_cardDefineList[i].isMaster())
                    {
                        characterNameList.Add(game.card.getDefaultSkin(_cardDefineList[i]).name);
                    }
                }
                _ui.setCharacterFilters(characterNameList.ToArray());
                _ui.setCharacterFilter(_characterFiltersValue);
                _ui.setCostFilter(_costFiltersValue);
                _ui.setTypeFilter(_typeFiltersValue);
                _ui.setSearchInputField(_searchFilterString);
                _ui.updateCardListCount(_filteredCardDefineList.Count);
                _ui.gameObject.SetActive(true);
            }
        }
        public Task display(int cardId, int[] cardPoolsId = null, int[] charactersId = null, Predicate<CardDefine> cardFilter = null)
        {
            return display(new DisplayInfo()
            {
                cardPoolsId = cardPoolsId,
                charactersId = charactersId,
                cardFilter = cardFilter,
                cardId = cardId
            });
        }
        /// <summary>
        /// 显示并设置选项
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public Task display(int[] cardPoolsId = null, int[] charactersId = null, Predicate<CardDefine> cardFilter = null)
        {
            return display(new DisplayInfo()
            {
                cardPoolsId = cardPoolsId,
                charactersId = charactersId,
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
            _ui.setSortTypes(_sortTypes);
            _ui.setCostFilters(_costFilters);
            _ui.setTypeFilters(_typeFilters);
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
            EditorCardListItem item = itemRect.GetComponent<EditorCardListItem>();
            CardDefine card = _filteredCardDefineList[index];
            CardSkinData skin = game.card.getDefaultSkin(card);
            item.card.setCard(card, skin);
            Sprite sprite = skin.image != null ?
                skin.image :
                await game.card.loadImage(skin.imagePath, game.card.getCardPoolOfCard(card) is CardPoolInfo cardPoolInfo ? cardPoolInfo.cardPoolId : 0);
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
        /// <summary>
        /// 为了引用过滤而重新显示界面
        /// </summary>
        /// <returns></returns>
        private async Task displayForApplyFilters()
        {
            List<int> cardPoolsIdList = new List<int>();
            for (int i = 0; i < _cardPoolFilterValue.Length; i++)
            {
                if (_cardPoolFilterValue[i])
                {
                    cardPoolsIdList.Add(_cardPoolInfoList[i].cardPoolId);
                }
            }
            List<int> charactersIdList = new List<int>();
            for (int i = 0; i < _characterFiltersValue.Length; i++)
            {
                if (_characterFiltersValue[i])
                {
                    charactersIdList.Add(_characterFilters[i]);
                }
            }
            await display(new DisplayInfo()
            {
                cardPoolsId = cardPoolsIdList.ToArray(),
                sortType = _sortType,
                charactersId = charactersIdList.ToArray(),
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
        bool[] _cardPoolFilterValue;
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
            public int[] cardPoolsId = null;
            public int sortType = -1;
            public int[] charactersId = null;
            public bool[] costFiltersValue = null;
            public bool[] typeFiltersValue = null;
            public string searchFilterValue = null;
            public Predicate<CardDefine> cardFilter = null;
            public int cardId;
        }
        #endregion
    }
}
