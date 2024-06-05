using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ScaleScrollHandler : MonoBehaviour, IScrollHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        #region 私有方法

        #region 事件回调
        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            var delta = Input.GetAxis("Mouse ScrollWheel") * _scrollZoomSpeed;//读取滚轮的变化           
            var lastScale = _content.localScale.x;
            var desiredScale = lastScale + delta;//进行缩放
            desiredScale = Mathf.Clamp(desiredScale, _minScale, _maxScale);//对缩放目标进行一个数据的clamp以保证成倍缩放
            _content.localScale = Vector3.one * desiredScale;

            // 调整缩放中心位置
            var scaleCenter = _content.parent.InverseTransformPoint(_viewport.position);
            if (eventData is PointerEventData pointer)
            {
                if (pointer.pointerCurrentRaycast.isValid)
                {
                    scaleCenter = _content.parent.InverseTransformPoint(pointer.pointerCurrentRaycast.worldPosition);
                }
            }
            var scaledMultiplier = desiredScale / lastScale;
            var posOffset = _content.localPosition - scaleCenter;
            posOffset.x *= scaledMultiplier;
            posOffset.y *= scaledMultiplier;
            _content.localPosition = scaleCenter + posOffset;
            onScroll?.Invoke(this, eventData);
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (Input.touchCount <= 1)
                return;

            var _currentTouch1 = Input.GetTouch(0);
            var _currentTouch2 = Input.GetTouch(1);
            if (_currentTouch1.phase == TouchPhase.Began || _currentTouch2.phase == TouchPhase.Began)
            {
                _perviousTouch1 = _currentTouch1;
                _perviousTouch2 = _currentTouch2;
            }
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (Input.touchCount <= 1)
            {
                eventData.Use();
                return;
            }

            _isTouchZooming = true;
            var _currentTouch1 = Input.GetTouch(0);
            var _currentTouch2 = Input.GetTouch(1);
            var camera = eventData.pressEventCamera;
            Vector2 _perviousPosition1 = camera.ScreenToWorldPoint(_perviousTouch1.position);
            Vector2 _perviousPosition2 = camera.ScreenToWorldPoint(_perviousTouch2.position);
            Vector2 _currentPosition1 = camera.ScreenToWorldPoint(_currentTouch1.position);
            Vector2 _currentPosition2 = camera.ScreenToWorldPoint(_currentTouch2.position);

            float leng1 = Vector2.Distance(_perviousPosition1, _perviousPosition2);
            float leng2 = Vector2.Distance(_currentPosition1, _currentPosition2);
            var scaleFactor = leng2 / leng1;
            Vector3 prevCenter = (_perviousPosition1 + _perviousPosition2) / 2;
            Vector3 currCenter = (_currentPosition1 + _currentPosition2) / 2;

            Vector3 _localScale = _content.localScale;
            Vector3 desiredScale = _localScale * scaleFactor;

            if (scaleFactor != 1 && desiredScale.x < _maxScale && desiredScale.y < _maxScale && desiredScale.x > _minScale && desiredScale.y > _minScale)
            {
                var localPos = _content.position;
                var targetPos = (1 - scaleFactor) * prevCenter + scaleFactor * localPos;

                _content.localScale = desiredScale;
                _content.position = targetPos;
            }

            _content.position += currCenter - prevCenter;

            _perviousTouch1 = _currentTouch1;
            _perviousTouch2 = _currentTouch2;
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (Input.touchCount > 1)
                return;
            _isTouchZooming = false;
        }
        #endregion

        #endregion

        #region 事件
        public event Action<ScaleScrollHandler, PointerEventData> onScroll;
        #endregion

        #region 属性字段
        public bool isTouchZooming => _isTouchZooming;
        [SerializeField]
        private RectTransform _viewport;
        [SerializeField]
        private RectTransform _content;
        [SerializeField]
        private float _minScale = 1;
        [SerializeField]
        private float _maxScale = 2;
        [SerializeField]
        private float _scrollZoomSpeed = 0.1f;

        private bool _isTouchZooming;
        private Touch _perviousTouch1;
        private Touch _perviousTouch2;
        #endregion
    }
}