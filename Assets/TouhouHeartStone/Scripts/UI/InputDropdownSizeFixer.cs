using UnityEngine;

namespace UI
{
    // 因为自定义下拉列表有一个额外的输入框，导致下拉列表的视图变矮，因此需要这个添加额外的高度。
    public class InputDropdownSizeFixer : MonoBehaviour
    {
        #region 私有方法
        #region 生命周期
        private void Start()
        {
            var rectTransform = transform as RectTransform;
            var size = rectTransform.sizeDelta;
            size.y += _extraHeight;
            rectTransform.sizeDelta = size;
        }
        #endregion

        #endregion
        [SerializeField]
        float _extraHeight;
    }
}