using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class DropdownListSizeController : MonoBehaviour, ILayoutSelfController
    {
        public void updateRectTransform()
        {
            m_Tracker.Clear();
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);

            var anchorMin = rectTransform.anchorMin.x;
            var anchorMax = rectTransform.anchorMax.x;
            var anchorWidth = GetParentSize().x * (anchorMax - anchorMin);

            float min = 0;
            float max = float.MaxValue;
            if (minWidth >= 0)
                min = minWidthRelative ? minWidth + anchorWidth : minWidth;
            if (maxWidth >= 0)
                max = maxWidthRelative ? maxWidth + anchorWidth : maxWidth;

            min = Mathf.Min(min, max);
            setMinWidth(min);

            var width = anchorWidth;
            if (_content)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
                width = LayoutUtility.GetPreferredWidth(_content);
            }

            width = Mathf.Clamp(width, min, max);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
        private void OnRectTransformDimensionsChange()
        {
            updateRectTransform();
        }
        private void OnTransformParentChanged()
        {
            updateRectTransform();
        }
        private void OnEnable()
        {
            updateRectTransform();
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            updateRectTransform();
        }
#endif
        private void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        private void Update()
        {
            if (_minObjElement)
            {
                _minObjElement.transform.SetAsLastSibling();
            }
        }
        private void setMinWidth(float min)
        {
            if (!_content && !_minObjElement)
                return;
            if (!_minObjElement)
            {
                var go = new GameObject("MinWidth");
                _minObjElement = go.AddComponent<LayoutElement>();
                go.transform.parent = _content;
            }
            _minObjElement.minWidth = min;
            _minObjElement.transform.SetAsLastSibling();
        }
        private Vector2 GetParentSize()
        {
            var parent = rectTransform.parent as RectTransform;
            if (!parent)
            {
                return Vector2.zero;
            }

            return parent.rect.size;
        }
        void ILayoutController.SetLayoutHorizontal()
        {
        }
        void ILayoutController.SetLayoutVertical()
        {
        }
        public RectTransform rectTransform
        {
            get
            {
                if (!_rectTrans)
                    _rectTrans = transform as RectTransform;
                return _rectTrans;
            }
        }
        public float minWidth
        {
            get { return _minWidth; }
            set
            {
                _minWidth = value;
                updateRectTransform();
            }
        }
        public bool minWidthRelative
        {
            get { return _minWidthRelative; }
            set
            {
                _minWidthRelative = value;
                updateRectTransform();
            }
        }
        public float maxWidth
        {
            get { return _maxWidth; }
            set
            {
                _maxWidth = value;
                updateRectTransform();
            }
        }
        public bool maxWidthRelative
        {
            get { return _maxWidthRelative; }
            set
            {
                _maxWidthRelative = value;
                updateRectTransform();
            }
        }
        [SerializeField]
        private float _minWidth = -1;
        [SerializeField]
        private bool _minWidthRelative = false;
        [SerializeField]
        private float _maxWidth = -1;
        [SerializeField]
        private bool _maxWidthRelative = false;
        [SerializeField]
        private RectTransform _content;
        [SerializeField]
        private LayoutElement _minObjElement;
        private DrivenRectTransformTracker m_Tracker;
        private RectTransform _rectTrans;
    }
}
