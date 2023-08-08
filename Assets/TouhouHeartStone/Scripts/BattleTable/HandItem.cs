using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class HandItem : TableUnit
    {
        public void setHovered(bool hovered)
        {
            _animator.SetBool("Show", hovered);
        }
        public void destroy()
        {
            Destroy(gameObject);
        }

        #region 生命周期
        private void Awake()
        {
            card.onClick += OnPointerClickCallback;
            card.onBeginDrag += OnBeginDragCallback;
            card.onDrag += OnDragCallback;
            card.onEndDrag += OnEndDragCallback;
            card.onPointerEnter += OnPointerEnterCallback;
            card.onPointerExit += OnPointerExitCallback;
        }
        #endregion

        #region 事件回调
        private void OnPointerClickCallback(TableCard card, PointerEventData eventData)
        {
            onClick?.Invoke(this, eventData);
        }

        private void OnBeginDragCallback(TableCard card, PointerEventData eventData)
        {
            onBeginDrag?.Invoke(this, eventData);
        }

        private void OnDragCallback(TableCard card, PointerEventData eventData)
        {
            onDrag?.Invoke(this, eventData);
        }

        private void OnEndDragCallback(TableCard card, PointerEventData eventData)
        {
            onEndDrag?.Invoke(this, eventData);
        }
        private void OnPointerEnterCallback(TableCard card, PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this, eventData);
        }

        private void OnPointerExitCallback(TableCard card, PointerEventData eventData)
        {
            onPointerExit?.Invoke(this, eventData);
        }
        #endregion

        #region 事件
        public event Action<HandItem, PointerEventData> onClick;
        public event Action<HandItem, PointerEventData> onBeginDrag;
        public event Action<HandItem, PointerEventData> onDrag;
        public event Action<HandItem, PointerEventData> onEndDrag;
        public event Action<HandItem, PointerEventData> onPointerEnter;
        public event Action<HandItem, PointerEventData> onPointerExit;
        #endregion

        #region 属性字段
        [SerializeField]
        private bool _isDraggable = true;
        [SerializeField]
        private TableCard _card;

        public TableCard card => _card;
        public bool isDraggable
        {
            get { return _isDraggable; }
            set { _isDraggable = value; }
        }

        public SimpleAnim onSetDefine;
        public SimpleAnim onDraw;
        public SimpleAnim onEnemyUse;
        public SimpleAnim onSelfUse;
        #endregion
    }
}