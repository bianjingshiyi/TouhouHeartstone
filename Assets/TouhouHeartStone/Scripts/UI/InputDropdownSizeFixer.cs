using UnityEngine;

namespace UI
{
    // 因为自定义下拉列表有一个额外的输入框，导致下拉列表的视图变矮，因此需要这个添加额外的高度。
    // 有的地方因为下拉列表过窄，导致显示的内容不全。也可以使用该组件进行修复。
    public class InputDropdownSizeFixer : MonoBehaviour
    {
        #region 私有方法
        #region 生命周期
        private void Start()
        {
            var parentSize = GetParentSize();
            var size = _targetTransform.sizeDelta;
            size.x = Mathf.Max(_minWidth, size.x + parentSize.x) - parentSize.x;
            size.y += _extraHeight;
            _targetTransform.sizeDelta = size;
        }
        #endregion
        private Vector2 GetParentSize()
        {
            RectTransform rectTransform = _targetTransform.parent as RectTransform;
            if (!rectTransform)
            {
                return Vector2.zero;
            }

            return rectTransform.rect.size;
        }
        #endregion
        [SerializeField]
        float _extraHeight;
        [SerializeField]
        float _minWidth;
        [SerializeField]
        RectTransform _targetTransform;
    }
}