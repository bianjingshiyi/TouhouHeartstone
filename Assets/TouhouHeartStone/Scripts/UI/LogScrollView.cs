using UnityEngine;
using BJSYGameCore.UI;
namespace UI
{
    partial class LogPanel
    {
        [SerializeField]
        Vector2 _minSize;
        public Animation SEAnimation;

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
            if (SEAnimation.isPlaying == false)
                SEAnimation["LogON"].time = SEAnimation["LogON"].length;
            SEAnimation["LogON"].speed = -1;
            SEAnimation.Play("LogON");
            //hide();
            //rectTransform.setWidth(_minSize.x);
        }
        public void expand()
        {
            _isExpanded = true;
            SEAnimation["LogON"].speed = 1;
            SEAnimation.Play("LogON");
            //display();
            //rectTransform.setWidth(LogList.rectTransform.rect.size.x);
        }
    }
}
