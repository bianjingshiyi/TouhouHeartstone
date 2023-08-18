using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [AddComponentMenu("Layout/Reversed Horizontal Layout Group", 150)]
    /// <summary>
    /// Layout class for arranging child elements side by side.
    /// </summary>
    public class ReversedHorizontalLayoutGroup : HorizontalLayoutGroup
    {
        protected ReversedHorizontalLayoutGroup()
        { }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            rectChildren.Reverse();
        }
    }
}
