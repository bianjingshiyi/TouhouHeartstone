using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class CardUIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public void destroy()
        {
            Destroy(gameObject);
        }
        protected virtual void Awake()
        {
            _card.onClick += onPointerClickCallback;
            _card.onPointerEnter += onPointerEnterCallback;
            _card.onPointerExit += onPointerExitCallback;
        }
        #region 事件回调
        void onPointerClickCallback(TableCard card, PointerEventData eventData)
        {
            onClick?.Invoke(this, eventData);
        }

        void onPointerEnterCallback(TableCard card, PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this, eventData);
        }

        void onPointerExitCallback(TableCard card, PointerEventData eventData)
        {
            onPointerExit?.Invoke(this, eventData);
        }
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
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
        #endregion

        #region 事件
        public event Action<CardUIItem, PointerEventData> onClick;
        public event Action<CardUIItem, PointerEventData> onPointerEnter;
        public event Action<CardUIItem, PointerEventData> onPointerExit;
        #endregion

        public TableCard card => _card;
        [SerializeField]
        private TableCard _card;
    }
}