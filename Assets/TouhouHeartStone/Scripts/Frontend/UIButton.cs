using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace TouhouHeartstone.Frontend
{
    /// <summary>
    /// 一个按钮
    /// </summary>

    public class UIButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action OnMouseClick;
        public event Action OnMouseIn;
        public event Action OnMouseOut;

        [SerializeField]
        UnityEvent onClick = new UnityEvent();

        private void OnMouseUpAsButton()
        {
            OnPointerClick(null);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnMouseClick?.Invoke();
            onClick?.Invoke();
        }

        private void OnMouseEnter()
        {
            OnPointerEnter(null);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseIn?.Invoke();
        }

        private void OnMouseExit()
        {
            OnPointerExit(null);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            OnMouseOut?.Invoke();
        }
    }
}
