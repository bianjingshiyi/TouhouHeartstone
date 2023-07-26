using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class TurnEndButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region 公有方法
        public void setHighlighted(bool highlighted)
        {
        }
        public void setLongHandAngle(float angle)
        {
            _longHandTrans.eulerAngles = Vector3.up * angle;
        }
        public void setShortHandAngle(float angle)
        {
            _shortHandTrans.eulerAngles = Vector3.up * angle;
        }
        #endregion

        #region 接口实现
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(this);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(this);
        }
        #endregion

        #region 事件
        public event Action<TurnEndButton> onClick;
        public event Action<TurnEndButton> onPointerEnter;
        public event Action<TurnEndButton> onPointerExit;
        #endregion

        #region 属性字段
        [SerializeField]
        private MeshRenderer _clockRenderer;
        [SerializeField]
        private Texture _highlightTexture;
        [SerializeField]
        private Transform _shortHandTrans;
        [SerializeField]
        private Transform _longHandTrans;
        #endregion
    }
}