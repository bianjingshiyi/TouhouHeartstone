using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    class CardViewDeckCardListItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有方法
        public void updateView(string name, Sprite sprite)
        {
            updateView(name, sprite, 0);
        }
        public void updateView(string name, Sprite sprite, int count)
        {
            _nameText.text = name;
            _image.sprite = sprite;
            if (count > 0)
            {
                _numberText.gameObject.SetActive(true);
                _numberText.text = count.ToString();
            }
            else
            {
                _numberText.gameObject.SetActive(false);
            }
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {

        }
        #endregion
        #region 事件回调
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
        public event Action<CardViewDeckCardListItem, PointerEventData> onBeginDrag;
        public event Action<CardViewDeckCardListItem, PointerEventData> onDrag;
        public event Action<CardViewDeckCardListItem, PointerEventData> onEndDrag;
        #endregion
        [SerializeField]
        Text _nameText;
        [SerializeField]
        Image _image;
        [SerializeField]
        Text _numberText;
    }
}