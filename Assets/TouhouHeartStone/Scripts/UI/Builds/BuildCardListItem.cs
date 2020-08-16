using UnityEngine;
using System;
using UnityEngine.EventSystems;
using TouhouCardEngine;
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
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (Vector2.Distance(_dragStartPosition, eventData.position) > _dragThreshold)
            {
                _isDragging = true;
            }
            if (_isDragging)
            {
                Build build = GetComponentInParent<Build>();
                build.onDragCardItem(this, eventData);
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