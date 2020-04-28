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
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            Build build = GetComponentInParent<Build>();

            if (!build.DragCard.isDisplaying)
            {
                if (Vector2.Distance(_dragStartPosition, eventData.position) > _dragThreshold)
                {
                    //开始拖拽
                    build.startDrag(card, eventData.position);
                    count--;
                    //从列表中移除
                    if (count < 1)
                        GetComponentInParent<BuildDeckList>().removeItem(this);
                }
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            ////结束拖拽
            //Build build = GetComponentInParent<Build>();
            //build.stopDrag(eventData.position);
            //gameObject.SetActive(true);
        }
    }
}