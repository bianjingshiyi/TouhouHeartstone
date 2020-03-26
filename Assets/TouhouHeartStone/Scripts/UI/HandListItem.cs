
using UnityEngine;
using UnityEngine.EventSystems;

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
            if (Vector2.Distance(_startDragPosition, eventData.position) > _dragThreshold)
            {
                Card.rectTransform.position = eventData.position;
                if (parent.rectTransform.rect.Contains(parent.rectTransform.InverseTransformPoint(eventData.position)))
                    Card.rectTransform.localScale = Vector3.one;
                else
                    Card.rectTransform.localScale = Vector3.one * .4f / parent.rectTransform.localScale.x;
            }
            else
                Card.rectTransform.localPosition = Vector2.zero;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            Card.rectTransform.localPosition = Vector2.zero;
            Card.rectTransform.localScale = Vector3.one;
            Table table = GetComponentInParent<Table>();
            table.player.cmdUse(table.game, Card.card, 0);
        }
    }
}
