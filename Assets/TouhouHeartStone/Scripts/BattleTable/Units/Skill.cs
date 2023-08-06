using System;
using System.Security.Policy;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Skill : TableUnit, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public void setHighlight(bool highlight)
        {
            _highlightObj.SetActive(highlight);
        }
        public void setCost(int value)
        {
            value = Mathf.Max(value, 0);
            _costText.text = value.ToString();
        }
        public void setOpen(bool open)
        {
            _openRoot.SetActive(open);
            _closedRoot.SetActive(!open);
        }

        public void setImageSprite(Sprite sprite)
        {
            _imageRenderer.setTableImageSprite(sprite, propertyBlock);
        }

        #region 接口实现
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(this, eventData);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(this, eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(this, eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(this, eventData);
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
        public event Action<Skill, PointerEventData> onClick;
        public event Action<Skill, PointerEventData> onBeginDrag;
        public event Action<Skill, PointerEventData> onDrag;
        public event Action<Skill, PointerEventData> onEndDrag;
        public event Action<Skill, PointerEventData> onPointerEnter;
        public event Action<Skill, PointerEventData> onPointerExit;
        public event Action<Skill, PointerEventData> onPointerMove;
        #endregion

        public PropertyNumber costNumber => _costText;
        public SimpleAnim onSetDefine;
        [SerializeField]
        private GameObject _highlightObj;
        [SerializeField]
        private GameObject _openRoot;
        [SerializeField]
        private GameObject _closedRoot;
        [SerializeField]
        private PropertyNumber _costText;
        [SerializeField]
        private MeshRenderer _imageRenderer;
        private MaterialPropertyBlock propertyBlock
        {
            get
            {
                if (_propertyBlock == null)
                {
                    _propertyBlock = new MaterialPropertyBlock();
                }
                return _propertyBlock;
            }
        }
        private MaterialPropertyBlock _propertyBlock;
    }
}