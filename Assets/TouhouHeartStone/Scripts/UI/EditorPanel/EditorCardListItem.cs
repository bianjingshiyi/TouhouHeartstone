using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    public class EditorCardListItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有方法
        public void switchToCard()
        {
            _card.gameObject.SetActive(true);
            _newCardButton.gameObject.SetActive(false);
        }
        public void switchToAddButton()
        {
            _card.gameObject.SetActive(false);
            _newCardButton.gameObject.SetActive(true);
        }
        /// <summary>
        /// 设置卡牌的数量注脚，可以用于构筑界面
        /// </summary>
        /// <param name="count"></param>
        /// <param name="max"></param>
        public void setCount(int count, int max)
        {
            _countText.text = count + "/" + max;
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _button.onClick.AddListener(onClickCallback);
            _newCardButton.onClick.AddListener(onClickCallback);
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
        public event Action<EditorCardListItem, PointerEventData> onBeginDrag;
        public event Action<EditorCardListItem, PointerEventData> onDrag;
        public event Action<EditorCardListItem, PointerEventData> onEndDrag;
        #endregion
        #region 属性字段
        public Card card => _card;
        [SerializeField]
        Card _card;
        [SerializeField]
        Button _newCardButton;
        [SerializeField]
        Button _button;
        [SerializeField]
        Text _countText;
        #endregion
    }
}