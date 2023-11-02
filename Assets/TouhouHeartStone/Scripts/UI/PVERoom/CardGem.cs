using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CardGem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有方法
        public void update(CardGemViewData viewData)
        {
            if (viewData == null)
            {
                _image.gameObject.SetActive(false);
                return;
            }
            _image.gameObject.SetActive(true);
            //_image.sprite = viewData.sprite;
        }
        public void startDrag()
        {
            _lockPosition = false;
            _image.maskable = false;
        }
        public void releaseDrag()
        {
            _lockPosition = true;
        }
        public void move(Vector2 position)
        {
            var pos = _rootTrans.position;
            pos.x = position.x;
            pos.y = position.y;
            _rootTrans.position = pos;
        }
        #endregion

        #region 私有方法

        private void OnEnable()
        {
            _lockPosition = true;
            _image.maskable = true;
            _rootTrans.localPosition = Vector3.zero;
        }
        private void Update()
        {
            if (_lockPosition)
            {
                _rootTrans.localPosition = _rootTrans.localPosition * 0.5f;
                if (_rootTrans.localPosition.sqrMagnitude < 2000)
                {
                    _image.maskable = true;
                }
            }
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) => onBeginDrag?.Invoke(this, eventData);
        void IDragHandler.OnDrag(PointerEventData eventData) => onDrag?.Invoke(this, eventData);
        void IEndDragHandler.OnEndDrag(PointerEventData eventData) => onEndDrag?.Invoke(this, eventData);

        #endregion

        public event Action<CardGem, PointerEventData> onBeginDrag;
        public event Action<CardGem, PointerEventData> onDrag;
        public event Action<CardGem, PointerEventData> onEndDrag;

        [SerializeField]
        private bool _lockPosition = true;
        [SerializeField]
        private RectTransform _rootTrans;
        [SerializeField]
        private Image _image;
    }
    public class CardGemViewData
    {
        public Sprite sprite;
    }
}
