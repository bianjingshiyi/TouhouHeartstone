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
            if (Vector2.Distance(_startDragPosition, eventData.position) > _dragThreshold)
            {
                Card.rectTransform.position = eventData.position;
                HandList list = GetComponentInParent<HandList>();
                if (list.rectTransform.rect.Contains(list.rectTransform.InverseTransformPoint(eventData.position)))
                {
                    Card.rectTransform.localScale = Vector3.one;
                    removePlaceHolder();
                }
                else
                {
                    Table table = GetComponentInParent<Table>();
                    if (Card.card.isUsable(table.game, table.player, out string info))
                    {
                        Card.rectTransform.localScale = Vector3.one * .4f / list.rectTransform.localScale.x;
                        //放置随从
                        if (Card.card.define is ServantCardDefine)
                        {
                            table.SelfFieldList.addChild(table.ServantPlaceHolder.rectTransform);
                            RectTransform[] children = table.SelfFieldList.getChildren();
                            int index = 0;
                            for (int i = 0; i < children.Length; i++)
                            {
                                if (children[i] == table.ServantPlaceHolder)
                                    continue;
                                Debug.Log(children[i].gameObject.name + ":" + children[i].position.x + ":" + eventData.position.x);
                                if (children[i].position.x < eventData.position.x)
                                {
                                    index = i + 1;
                                }
                            }
                            //Debug.Log("设置Index为" + index);
                            table.ServantPlaceHolder.rectTransform.SetSiblingIndex(index);
                            table.ServantPlaceHolder.display();
                        }
                        else
                        {
                            removePlaceHolder();
                        }
                    }
                    else
                    {
                        Card.rectTransform.localScale = Vector3.one;
                        Card.rectTransform.localPosition = Vector2.zero;
                        table.showTip(info);
                        removePlaceHolder();
                    }
                }
            }
            else
            {
                Card.rectTransform.localPosition = Vector2.zero;
                removePlaceHolder();
            }
        }

        private void removePlaceHolder()
        {
            Table table = GetComponentInParent<Table>();
            table.addChild(table.ServantPlaceHolder.rectTransform);
            table.ServantPlaceHolder.hide();
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
            removePlaceHolder();
        }
    }
}
