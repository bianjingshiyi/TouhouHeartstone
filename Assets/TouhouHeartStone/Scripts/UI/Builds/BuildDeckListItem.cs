using UnityEngine;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using BJSYGameCore.UI;
using UnityEngine.UI;
using System;

namespace UI
{
    partial class BuildDeckListItem : IBeginDragHandler, IDragHandler, IEndDragHandler, ISelectHandler, IDeselectHandler
    {
        public void update(CardDefine card, CardSkinData skin, int count)
        {
            this.card = card;
            this.count = count;

            Image.sprite = skin.image;
            Text.text = skin.name;
        }
        [SerializeField]
        Vector2 _dragStartPosition;
        [SerializeField]
        float _dragThreshold = 50f;
        [SerializeField]
        bool _isDragging = false;

        public bool DraggingAble = true;
        public CardDefine card { get; private set; }
        [SerializeField]
        int _count;
        public int count
        {
            get { return _count; }
            set
            {
                _count = value;
                NumText.text = _count > 0 ? _count.ToString() : null;
            }
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (DraggingAble)
            {
                _dragStartPosition = eventData.position;
                _isDragging = false;
                GetComponentInParent<ScrollRect>()?.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
        }

        Vector2 savedSizeDelta = Vector2.zero;

        public void recovery()
        {
            count++;
            getChild("Root").display();

            if (savedSizeDelta != Vector2.zero)
            {
                rectTransform.sizeDelta = savedSizeDelta;
                savedSizeDelta = Vector2.zero;
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (DraggingAble)
            {
                var delta = _dragStartPosition - eventData.position;
                if (Math.Abs(delta.x) > _dragThreshold && Math.Abs(delta.x) > Math.Abs(delta.y))
                {
                    if (!_isDragging)
                    {
                        count--;
                        //从列表中隐藏
                        if (count < 1)
                        {
                            getChild("Root").hide();
                            savedSizeDelta = rectTransform.sizeDelta;
                            rectTransform.sizeDelta = Vector2.zero;
                        }
                    }
                    _isDragging = true;
                }
                if (_isDragging)
                {
                    Build build = GetComponentInParent<Build>();
                    build.onDragDeckItem(this, eventData);
                }
                else
                {
                    GetComponentInParent<ScrollRect>()?.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (DraggingAble)
            {
                if (_isDragging)
                {
                    //结束拖拽
                    Build build = GetComponentInParent<Build>();
                    build.onReleaseDeckItem(this, eventData);
                }
                //gameObject.SetActive(true);
                GetComponentInParent<ScrollRect>()?.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (DraggingAble)
            {
                Build build = GetComponentInParent<Build>();
                build.DeckItemSelect(this);
            }
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (DraggingAble)
            {
                Build build = GetComponentInParent<Build>();
                build.DeckItemDeselect(this);
            }
        }
    }
}