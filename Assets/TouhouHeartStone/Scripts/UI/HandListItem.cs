using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TouhouHeartstone;
using BJSYGameCore.UI;
namespace UI
{
    partial class HandListItem : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        Vector2 _startDragPosition;
        public void OnBeginDrag(PointerEventData eventData)
        {
            _startDragPosition = eventData.position;
        }
        [SerializeField]
        float _dragThreshold;
        public void OnDrag(PointerEventData eventData)
        {
            HandList handlist = GetComponentInParent<HandList>();
            if (Vector2.Distance(_startDragPosition, eventData.position) > _dragThreshold)
            {
                if (handlist.placingCard == null)
                    handlist.startPlacing(_startDragPosition, Card);
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
        }
    }
}
