using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Deck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public float getCardHeight()
        {
            return _cardHeight;
        }
        #region 事件回调
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this, eventData);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(this, eventData);
        }
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(this, eventData);
        }
        #endregion

        #region 事件
        public event Action<Deck, PointerEventData> onClick;
        public event Action<Deck, PointerEventData> onPointerEnter;
        public event Action<Deck, PointerEventData> onPointerExit;
        #endregion
        /// <summary>
        /// 高于牌堆一段距离的物体变换，用作卡牌移动的目标。
        /// </summary>
        public Transform HoverTransform => _hoverTransform;
        [SerializeField]
        private Transform _hoverTransform;
        [SerializeField]
        private float _cardHeight = 3;
    }
}