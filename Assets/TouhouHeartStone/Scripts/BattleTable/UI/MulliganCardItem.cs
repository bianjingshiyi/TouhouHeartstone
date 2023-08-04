using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class MulliganCardItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public void setMarkVisible(bool visible)
        {
            _mark.SetActive(visible);
        }
        public void destroy()
        {
            Destroy(gameObject);
        }

        #region 事件回调
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
        public event Action<MulliganCardItem, PointerEventData> onClick;
        public event Action<MulliganCardItem, PointerEventData> onPointerEnter;
        public event Action<MulliganCardItem, PointerEventData> onPointerExit;
        #endregion

        public TableCard card => _card;
        [SerializeField]
        private TableCard _card;
        [SerializeField]
        private GameObject _mark;
    }
}