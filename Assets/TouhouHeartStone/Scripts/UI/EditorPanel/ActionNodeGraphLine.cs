using UnityEngine;

namespace UI
{
    class ActionNodeGraphLine : MonoBehaviour
    {
        /// <summary>
        /// 设置连线的位置
        /// </summary>
        /// <param name="from">连线相对根的局部坐标</param>
        /// <param name="to">连线相对根的局部坐标</param>
        public void setPosition(Vector2 from, Vector2 to)
        {
            RectTransform rectTransform = transform as RectTransform;
            rectTransform.anchoredPosition = from;
            rectTransform.right = (to - from).normalized;
            rectTransform.sizeDelta = new Vector2(Vector2.Distance(from, to), rectTransform.sizeDelta.y);
        }
    }
}