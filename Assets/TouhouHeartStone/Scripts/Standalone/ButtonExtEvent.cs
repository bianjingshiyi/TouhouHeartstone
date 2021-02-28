using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace IGensoukyo.Utilities
{
    [RequireComponent(typeof(Button))]
    public class ButtonExtEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        Button btn => this.GetComponent<Button>();

        [SerializeField]
        UnityEvent onPointerDown;

        [SerializeField]
        UnityEvent onPointerUp;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (btn.isActiveAndEnabled && btn.interactable)
                onPointerDown?.Invoke();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (btn.isActiveAndEnabled && btn.interactable)
                onPointerUp?.Invoke();
        }
    }
}
