using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace TouhouHeartstone.Frontend
{
    /// <summary>
    /// 一个按钮
    /// </summary>

    public class UIButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action OnMouseClick;

        private void OnMouseUpAsButton()
        {
            OnPointerClick(null);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnMouseClick?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }
    }
}
