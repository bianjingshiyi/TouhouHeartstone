using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class TurnEndButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region 公有方法
        public void setHighlighted(bool highlighted)
        {
            _clockRenderer.setSprite(highlighted ? _highlightSprite : _normalSprite);
        }
        public void setLongHandAngle(float angle)
        {
            _longHandTrans.localEulerAngles = Vector3.down * angle;
        }
        public void setShortHandAngle(float angle)
        {
            _shortHandTrans.localEulerAngles = Vector3.down * angle;
        }
        public void setInteractable(bool interactable)
        {
            if (_isHovered && _interactable != interactable)
            {
                if (!interactable)
                {
                    onPointerExit?.Invoke(this);
                }
                else
                {
                    onPointerEnter?.Invoke(this);
                }
            }
            _interactable = interactable;
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
            _isHovered = true;
            if (!_interactable) return;
            onPointerEnter?.Invoke(this);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            if (!_interactable) return;
            onPointerExit?.Invoke(this);
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
        private Sprite _normalSprite;
        [SerializeField]
        private Sprite _highlightSprite;
        [SerializeField]
        private Transform _shortHandTrans;
        [SerializeField]
        private Transform _longHandTrans;
        #endregion
    }
}