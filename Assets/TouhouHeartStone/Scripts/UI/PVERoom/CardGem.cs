using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CardGem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有方法
        public void update(CardGemViewData viewData)
        {
            if (viewData == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            //_image.sprite = viewData.sprite;
        }
        public void setVisible(bool value)
        {
            _image.enabled = value;
        }
        #endregion

        #region 私有方法
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) => onBeginDrag?.Invoke(this, eventData);
        void IDragHandler.OnDrag(PointerEventData eventData) => onDrag?.Invoke(this, eventData);
        void IEndDragHandler.OnEndDrag(PointerEventData eventData) => onEndDrag?.Invoke(this, eventData);

        #endregion

        public event Action<CardGem, PointerEventData> onBeginDrag;
        public event Action<CardGem, PointerEventData> onDrag;
        public event Action<CardGem, PointerEventData> onEndDrag;

        [SerializeField]
        private Image _image;
    }
    public class CardGemViewData
    {
        public Sprite sprite;
    }
}
