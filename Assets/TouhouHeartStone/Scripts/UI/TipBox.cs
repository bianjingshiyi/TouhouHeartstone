using UnityEngine;
namespace UI
{
    partial class TipBox
    {
        public void display(string msg, Vector2 position)
        {
            Text.text = msg;
            rectTransform.position = position;
            display();
            //刷新自适应
            asVerticalLayoutGroup.enabled = false;
            asVerticalLayoutGroup.enabled = true;
        }
    }
}
