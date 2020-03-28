using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TouhouHeartstone;
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
                HandList list = GetComponentInParent<HandList>();
                if (list.rectTransform.rect.Contains(list.rectTransform.InverseTransformPoint(eventData.position)))
                    Card.rectTransform.localScale = Vector3.one;
                else
                {
                    if (Card.card.isUsable(parent.game, parent.player, out string info))
                        Card.rectTransform.localScale = Vector3.one * .4f / list.rectTransform.localScale.x;
                    else
                    {
                        Card.rectTransform.localScale = Vector3.one;
                        Card.rectTransform.localPosition = Vector2.zero;
                        parent.showTip(info);
                    }
                }
            }
            else
                Card.rectTransform.localPosition = Vector2.zero;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            HandList list = GetComponentInParent<HandList>();
            if (list.rectTransform.rect.Contains(list.rectTransform.InverseTransformPoint(eventData.position)))
            {
                Card.rectTransform.localScale = Vector3.one;
                Card.rectTransform.localPosition = Vector2.zero;
            }
            else
            {
                Card.rectTransform.localScale = Vector3.one;
                Card.rectTransform.localPosition = Vector2.zero;
                Table table = GetComponentInParent<Table>();
                if (Card.card.isUsable(table.game, table.player, out string info))
                    table.player.cmdUse(table.game, Card.card, 0);
                else
                {
                    table.showTip(info);
                }
            }
        }
    }
}
