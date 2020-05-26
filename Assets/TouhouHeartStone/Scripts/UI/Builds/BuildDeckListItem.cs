using UnityEngine;
using UnityEngine.EventSystems;
using TouhouCardEngine;
namespace UI
{
    partial class BuildDeckListItem : IBeginDragHandler, IDragHandler, IEndDragHandler
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
        public CardDefine card { get; private set; }
        [SerializeField]
        int _count;
        public int count
        {
            get { return _count; }
            set
            {
                _count = value;
                NumText.text = _count > 1 ? _count.ToString() : null;
            }
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _dragStartPosition = eventData.position;
            _isDragging = false;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (Vector2.Distance(_dragStartPosition, eventData.position) > _dragThreshold)
            {
                if (!_isDragging)
                {
                    count--;
                    //从列表中隐藏
                    if (count < 1)
                    {
                        getChild("Root").hide();
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
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                //结束拖拽
                Build build = GetComponentInParent<Build>();
                build.onReleaseDeckItem(this, eventData);
            }
            //gameObject.SetActive(true);
        }
    }
}