using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TouhouHeartstone;
using BJSYGameCore.UI;
using System;
namespace UI
{
    partial class HandListItem : IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        bool _isDragable = true;
        public bool isDragable
        {
            get { return _isDragable; }
            set { _isDragable = value; }
        }
        [SerializeField]
        Vector2 _startDragPosition;
        [SerializeField]
        float _dragThreshold = 50;
        [SerializeField]
        bool _isDragging = false;
        [SerializeField]
        bool _isHovered = false;
        
        public ActionEvent<HandListItem, PointerEventData> onPointerExit { get; set; } = new ActionEvent<HandListItem, PointerEventData>();
        public ActionEvent<HandListItem, Vector2> onPointerMove { get; set; } = new ActionEvent<HandListItem, Vector2>();

        private void Update()
        {
            if (_isHovered)
            {
                Vector2 position;
                position = EventSystem.current.currentInputModule.input.mousePosition;
                onPointerMove?.invoke(this, position);
                if (!rectTransform.contains(position))
                {
                    _isHovered = false;
                    onPointerExit.invoke(this, null);
                }
            }
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDragable)
                return;
            _startDragPosition = eventData.position;
            _isDragging = false;
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragable)
                return;
            if (Vector2.Distance(_startDragPosition, eventData.position) > _dragThreshold)
            {
                _isDragging = true;
            }
            if (_isDragging)
            {
                onDrag.invoke(this, eventData);
            }
        }
        public ActionEvent<HandListItem, PointerEventData> onDrag = new ActionEvent<HandListItem, PointerEventData>();
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragable)
                return;
            if (_isDragging)
            {
                onEndDrag.invoke(this, eventData);
            }
            _isDragging = false;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            onPointerEnter.invoke(this, eventData);
        }
        public ActionEvent<HandListItem, PointerEventData> onPointerEnter { get; set; } = new ActionEvent<HandListItem, PointerEventData>();

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            onPointerExit.invoke(this, eventData);
        }

        public ActionEvent<HandListItem, PointerEventData> onEndDrag = new ActionEvent<HandListItem, PointerEventData>();
        public SimpleAnim onAttackUp;
        public SimpleAnim onAttackDown;
        public SimpleAnim onLifeUp;
        public SimpleAnim onLifeDown;
        public SimpleAnim onCostUp;
        public SimpleAnim onCostResume;
        public SimpleAnim onCostDown;
        public SimpleAnim onDiscard;
        public SimpleAnim onGraveToHand;
        public SimpleAnim onCreate;
        public SimpleAnim onDraw;
        public SimpleAnim onEnemyUse;
        public SimpleAnim onSelfUse;
        public SimpleAnim onHoverCard;
        public SimpleAnim onexitHoverCard;
    }
}
