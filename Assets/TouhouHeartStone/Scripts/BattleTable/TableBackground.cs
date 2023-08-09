using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class TableBackground : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke(eventData);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke(eventData);
        }
        public event Action<PointerEventData> onPointerDown;
        public event Action<PointerEventData> onPointerUp;
    }
}