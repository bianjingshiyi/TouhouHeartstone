using UnityEngine;
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
        public CardDefine card { get; private set; }
        public void update(CardDefine card, CardSkinData skin)
        {
            this.card = card;

            Card.update(card, skin);
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
                }
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            ////结束拖拽
            //Build build = GetComponentInParent<Build>();
            //build.stopDrag(eventData.position);
        }
    }
}