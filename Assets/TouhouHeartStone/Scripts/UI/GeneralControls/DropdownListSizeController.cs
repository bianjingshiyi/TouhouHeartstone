using UnityEngine;

namespace UI
{
    [ExecuteAlways]
    public class DropdownListSizeController : MonoBehaviour
    {
        private void LateUpdate()
        {
            updateRectTransform();
        }
        private void updateRectTransform()
        {
            var parent = rectTransform.parent as RectTransform;

            var anchorMin = rectTransform.anchorMin;
            var anchorMax = rectTransform.anchorMax;

            var anchorSize = parent.rect.size * (anchorMax - anchorMin);

            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = 0;
            if (minWidth > 0)
                sizeDelta.x = Mathf.Max(sizeDelta.x, minWidth - anchorSize.x);
            if (flexible && _content)
            {
                sizeDelta.x = Mathf.Max(sizeDelta.x, _content.rect.width - anchorSize.x + extraFlexibleWidth);
                if (maxWidth > 0)
                    sizeDelta.x = Mathf.Min(sizeDelta.x, maxWidth - anchorSize.x);
            }
            rectTransform.sizeDelta = sizeDelta;
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
        public float minWidth = 0;
        public float maxWidth = 0;
        public bool flexible = false;
        public float extraFlexibleWidth = 0;
        [SerializeField]
        private RectTransform _content;
        private RectTransform _rectTrans;
    }
}
