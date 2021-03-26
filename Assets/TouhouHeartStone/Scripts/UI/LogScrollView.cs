using UnityEngine;
using BJSYGameCore.UI;
namespace UI
{
    partial class LogScrollView
    {
        [SerializeField]
        Vector2 _minSize;
        partial void onAwake()
        {

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
            animator.speed = -1;
            animator.Play("LogON");
            //hide();
            //rectTransform.setWidth(_minSize.x);
        }
        public void expand()
        {
            _isExpanded = true;
            animator.speed = 1;
            animator.Play("LogON");
            //display();
            //rectTransform.setWidth(LogList.rectTransform.rect.size.x);
        }
    }
}
