using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    partial class TipBox
    {
        public void display(string msg, Vector3 position)
        {
            Text.text = msg;
            rectTransform.position = position;
            display();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        public void updatePos(Vector3 pos)
        {
            rectTransform.position = pos;
        }
    }
}
