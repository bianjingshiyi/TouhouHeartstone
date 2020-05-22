using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TouhouHeartstone;
using BJSYGameCore.UI;
namespace UI
{
    partial class HandListItem : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        //[SerializeField]
        //bool _isDragging = false;
        [SerializeField]
        Vector2 _startDragPosition;
        public void OnBeginDrag(PointerEventData eventData)
        {
            _startDragPosition = eventData.position;
            //_isDragging = true;
        }
        [SerializeField]
        float _dragThreshold;
        public void OnDrag(PointerEventData eventData)
        {
            //if (!_isDragging)
            //    return;
            Table table = GetComponentInParent<Table>();
            //HandList handlist = GetComponentInParent<HandList>();
            //if (handlist == table.EnemyHandList)//你不能用对方的手牌
            //    return;
            //if (!table.canControl)//不是你的回合
            //    return;
            if (Vector2.Distance(_startDragPosition, eventData.position) > _dragThreshold)
                table.onDragHand(this, eventData);
            //{
            //    if (handlist.placingCard == null)
            //    {
            //        handlist.startPlacing(_startDragPosition, Card);
            //        _isDragging = false;
            //    }
            //}
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            //_isDragging = false;
        }
    }
}
