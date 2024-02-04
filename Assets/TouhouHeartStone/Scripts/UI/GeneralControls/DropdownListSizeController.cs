using UnityEngine;

namespace UI
{
    [ExecuteAlways]
    public class DropdownListSizeController : MonoBehaviour
    {
        private void Update()
        {
            var parent = rectTransform.parent as RectTransform;

            var anchorMin = rectTransform.anchorMin;
            var anchorMax = rectTransform.anchorMax;

            var anchorSize = parent.rect.size * (anchorMax - anchorMin);

            var sizeDelta = rectTransform.sizeDelta;
            if (minWidth > 0)
                sizeDelta.x = Mathf.Max(0, minWidth - anchorSize.x);
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
        private RectTransform _rectTrans;
    }
}
