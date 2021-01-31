using UnityEngine;
using System;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using UnityEngine.UI;

namespace UI
{
    partial class BuildCardListItem : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        Vector2 _dragStartPosition;
        [SerializeField]
        float _dragThreshold = 50f;
        [SerializeField]
        bool _isDragging = false;
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _dragStartPosition = eventData.position;
            _isDragging = false;
            GetComponentInParent<ScrollRect>()?.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var delta = _dragStartPosition - eventData.position;
            if (Math.Abs(delta.x) > _dragThreshold && Math.Abs(delta.x) > Math.Abs(delta.y))
            {
                _isDragging = true;
            }
            if (_isDragging)
            {
                Build build = GetComponentInParent<Build>();
                build.onDragCardItem(this, eventData);
            } 
            else
            {
                GetComponentInParent<ScrollRect>()?.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                //结束拖拽
                Build build = GetComponentInParent<Build>();
                build.onReleaseCardItem(this, eventData);
            }
            _isDragging = false;
            GetComponentInParent<ScrollRect>()?.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// 设置剩下可用的牌的提示
        /// </summary>
        /// <param name="now"></param>
        /// <param name="count"></param>
        public void setRemainText(int now, int count)
        {
            Image.gameObject.SetActive(now != 0);
            RemainHintText.text = $"{now} / {count}";
        }
    }
}