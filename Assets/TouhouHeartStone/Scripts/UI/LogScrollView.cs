using UnityEngine;
namespace UI
{
    partial class LogScrollView
    {
        [SerializeField]
        Vector2 _minSize;
        partial void onAwake()
        {
            _minSize = rectTransform.rect.size;
        }
        [SerializeField]
        bool _isExpanded = false;
        public void expandOrShrink()
        {
            if (!_isExpanded)
                expand();
            else
                shrink();
        }
        public void shrink()
        {
            _isExpanded = false;
            rectTransform.setWidth(_minSize.x);
        }
        public void expand()
        {
            _isExpanded = true;
            rectTransform.setWidth(LogList.rectTransform.rect.size.x);
        }
    }
}
