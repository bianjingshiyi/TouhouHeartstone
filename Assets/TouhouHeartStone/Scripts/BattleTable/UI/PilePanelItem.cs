using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class PilePanelItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void destroy()
        {
            Destroy(gameObject);
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this, eventData);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(this, eventData);
        }
        public event Action<PilePanelItem, PointerEventData> onPointerEnter;
        public event Action<PilePanelItem, PointerEventData> onPointerExit;
        public TableCard card => _card;
        [SerializeField]
        private TableCard _card;
    }
}