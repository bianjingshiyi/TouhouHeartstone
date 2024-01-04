using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CardViewDeckCardListItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
    {
        #region 公有方法
        public void updateView(string name, Sprite sprite)
        {
            updateView(name, sprite, 0);
        }
        public void updateView(string name, Sprite sprite, int count)
        {
            _nameText.text = name;
            _image.sprite = sprite;
            if (_numberText != null)
            {
                if (count > 0)
                {
                    _numberText.gameObject.SetActive(true);
                    _numberText.text = count.ToString();
                }
                else
                {
                    _numberText.gameObject.SetActive(false);
                }
            }
        }
        #endregion
        #region 私有方法
        #region 事件回调
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (interactable)
                onBeginDrag?.Invoke(this, eventData);
            if (!eventData.used)
            {
                GetComponentInParent<ScrollRect>()?.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (interactable)
                onDrag?.Invoke(this, eventData);
            if (!eventData.used)
            {
                GetComponentInParent<ScrollRect>()?.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (interactable)
                onEndDrag?.Invoke(this, eventData);
            if (!eventData.used)
            {
                GetComponentInParent<ScrollRect>()?.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
        }
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (interactable)
                onClick?.Invoke(this, eventData);
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this, eventData);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(this, eventData);
        }
        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            onDeselect?.Invoke(this);
        }
        
        #endregion
        #endregion
        #region 事件
        public event Action<CardViewDeckCardListItem, PointerEventData> onBeginDrag;
        public event Action<CardViewDeckCardListItem, PointerEventData> onDrag;
        public event Action<CardViewDeckCardListItem, PointerEventData> onEndDrag;
        public event Action<CardViewDeckCardListItem, PointerEventData> onClick;
        public event Action<CardViewDeckCardListItem, PointerEventData> onPointerEnter;
        public event Action<CardViewDeckCardListItem, PointerEventData> onPointerExit;
        public event Action<CardViewDeckCardListItem> onDeselect;
        #endregion
        public bool interactable { get; set; } = true;
        [SerializeField]
        Text _nameText;
        [SerializeField]
        Image _image;
        [SerializeField]
        Text _numberText;
    }
}