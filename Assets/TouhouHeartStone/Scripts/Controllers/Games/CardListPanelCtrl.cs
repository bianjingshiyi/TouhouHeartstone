using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using TouhouCardEngine;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Controllers
{
    class CardListPanelCtrl : MonoBehaviour
    {
        #region 公有方法
        public void updateCards(IEnumerable<CardDefine> cardDefines)
        {
            // 筛选。
            _cardDefineList.Clear();
            _cardDefineList.AddRange(cardDefines);

            //更新UI
            _ui.updateCardListCount(_cardDefineList.Count);
        }
        public void enableCount(bool value)
        {
            _hideCount = !value;
        }
        public CardDefine getCardDefineAt(int index)
        {
            return _cardDefineList[index];
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
            game = TransformHelper.findComponentInScene<MainManager>(gameObject.scene);
            _ui.onEnableCardListItem += onEnableCardListItemCallback;
            _ui.onDisableCardListItem += onDisableCardListItemCallback;
            _ui.onUpdateCardListItem += onUpdateCardListItemCallback;
        }
        #endregion
        #region 事件回调
        private void onEnableCardListItemCallback(int index, RectTransform itemRect)
        {
            EditorCardListItem item = itemRect.GetComponent<EditorCardListItem>();
            item.onClick += onClickCardListItem;
            item.onHoverEnter += onCardListItemHoverEnterCallback;
            item.onHoverExit += onCardListItemHoverExitCallback;
            item.onBeginDrag += onStartDragCardListItemCallback;
            item.onDrag += onDragCardListItemCallback;
            item.onEndDrag += onEndDragCardListItemCallback;
        }
        private void onDisableCardListItemCallback(int index, RectTransform itemRect)
        {
            EditorCardListItem item = itemRect.GetComponent<EditorCardListItem>();
            item.onClick -= onClickCardListItem;
            item.onHoverEnter -= onCardListItemHoverEnterCallback;
            item.onHoverExit -= onCardListItemHoverExitCallback;
            item.onBeginDrag -= onStartDragCardListItemCallback;
            item.onDrag -= onDragCardListItemCallback;
            item.onEndDrag -= onEndDragCardListItemCallback;
        }
        private async void onUpdateCardListItemCallback(int index, RectTransform itemRect)
        {
            //防止索引超界
            if (index < 0 || index >= _cardDefineList.Count)
                return;
            EditorCardListItem item = itemRect.GetComponent<EditorCardListItem>();
            if (_hideCount)
            {
                item.hideCount();
            }
            CardDefine card = _cardDefineList[index];
            await item.card.setCardDefine(card, game.card);
            onUpdateCardListItem?.Invoke(index, item);
        }
        private void onClickCardListItem(EditorCardListItem item)
        {
            int index = _ui.getCardItemIndex(item.transform as RectTransform);
            CardDefine card = _cardDefineList[index];
            onClickCard?.Invoke(card);
        }
        private void onCardListItemHoverEnterCallback(EditorCardListItem item)
        {
            int index = _ui.getCardItemIndex(item.transform as RectTransform);
            CardDefine card = _cardDefineList[index];
            onCardHoverEnter?.Invoke(item, card);
        }
        private void onCardListItemHoverExitCallback(EditorCardListItem item)
        {
            int index = _ui.getCardItemIndex(item.transform as RectTransform);
            CardDefine card = _cardDefineList[index];
            onCardHoverExit?.Invoke(item, card);
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
            CardDefine card = _cardDefineList[index];
            var task = onStartDragCard?.Invoke(card, eventData);
            bool isEventUsed = task != null ? await task : false;
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
            CardDefine card = _cardDefineList[index];
            var task = onDragCard?.Invoke(card, eventData);
            bool isEventUsed = task != null ? await task : false;
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
            CardDefine card = _cardDefineList[index];
            var task = onEndDragCard?.Invoke(card, eventData);
            bool isEventUsed = task != null ? await task : false;
            if (!isEventUsed)
            {
                _ui.cardListScrollRect.OnEndDrag(eventData);
            }
        }
        #endregion
        #endregion
        #region 事件
        public event Action<CardDefine> onClickCard;
        public event Action<EditorCardListItem, CardDefine> onCardHoverEnter;
        public event Action<EditorCardListItem, CardDefine> onCardHoverExit;
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
        public ScrollRect cardListScrollRect => _ui.cardListScrollRect;
        private MainManager game;
        [SerializeField]
        private CardListPanel _ui;
        [SerializeField]
        private bool _hideCount = true;
        List<CardDefine> _cardDefineList = new List<CardDefine>();
        #endregion
    }
}
