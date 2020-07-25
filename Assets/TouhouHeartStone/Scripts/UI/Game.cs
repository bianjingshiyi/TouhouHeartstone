using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    partial class Game : IPointerDownHandler
    {
        partial void onAwake()
        {
            QuitButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                parent.game.game.Dispose();
                parent.display(parent.MainMenu);
            });
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            Table.clickNoWhere(eventData);
        }

        [SerializeField]
        UnityEvent onPageOpen;

        [SerializeField]
        UnityEvent onPageClose;

        void OnEnable()
        {
            onPageOpen?.Invoke();
        }

        void OnDisable()
        {
            onPageClose?.Invoke();
        }
    }
}
