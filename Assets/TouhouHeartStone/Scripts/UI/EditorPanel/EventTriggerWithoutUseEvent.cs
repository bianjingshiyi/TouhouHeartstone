using UnityEngine.EventSystems;
using System;
using UnityEngine;
namespace UI
{
    public class EventTriggerWithoutUseEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        #region 私有方法
        #region 事件回调
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke(this, eventData);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke(this, eventData);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<EventTriggerWithoutUseEvent, PointerEventData> onPointerDown;
        public event Action<EventTriggerWithoutUseEvent, PointerEventData> onPointerUp;
        #endregion
    }
}