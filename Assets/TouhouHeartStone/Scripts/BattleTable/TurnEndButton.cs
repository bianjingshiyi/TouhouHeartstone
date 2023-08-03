using System;
using TMPro;
using UI;
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
        public void setInteractable(bool interactable)
        {
            _interactable = interactable;
            if (_isHovered && !_interactable)
            {
                onPointerExit?.Invoke(this);
            }
        }
        #endregion

        #region 接口实现
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!_interactable) return;
            onClick?.Invoke(this);
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactable) return;
            onPointerEnter?.Invoke(this);
            _isHovered = true;
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!_interactable) return;
            onPointerExit?.Invoke(this);
            _isHovered = false;
        }
        #endregion

        #region 事件
        public event Action<TurnEndButton> onClick;
        public event Action<TurnEndButton> onPointerEnter;
        public event Action<TurnEndButton> onPointerExit;
        #endregion

        #region 属性字段
        private bool _isHovered;
        private bool _interactable;
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