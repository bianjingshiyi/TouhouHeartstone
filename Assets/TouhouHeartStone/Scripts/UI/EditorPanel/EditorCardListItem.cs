using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    public class EditorCardListItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有方法
        /// <summary>
        /// 设置卡牌的数量注脚，可以用于构筑界面
        /// </summary>
        /// <param name="count"></param>
        /// <param name="max"></param>
        public void showCount(int count, int max)
        {
            if (_countGameObject == null)
                return;
            if (_countText == null)
                return;
            _countGameObject.SetActive(true);
            _countText.text = $"{count}/{max}";
        }
        public void hideCount()
        {
            if (_countGameObject == null)
                return;
            _countGameObject.SetActive(false);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _button.onClick.AddListener(onClickCallback);
            _card.onHoverEnter += card => onHoverEnter?.Invoke(this);
            _card.onHoverExit += card => onHoverExit?.Invoke(this);
        }
        #endregion
        #region 事件回调
        private void onClickCallback()
        {
            onClick?.Invoke(this);
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
        #endregion
        #endregion
        #region 事件
        public event Action<EditorCardListItem> onClick;
        public event Action<EditorCardListItem> onHoverEnter;
        public event Action<EditorCardListItem> onHoverExit;
        public event Action<EditorCardListItem, PointerEventData> onBeginDrag;
        public event Action<EditorCardListItem, PointerEventData> onDrag;
        public event Action<EditorCardListItem, PointerEventData> onEndDrag;
        #endregion
        #region 属性字段
        public Card card => _card;
        [SerializeField]
        Card _card;
        [SerializeField]
        Button _button;
        [SerializeField]
        GameObject _countGameObject;
        [SerializeField]
        Text _countText;
        #endregion
    }
}