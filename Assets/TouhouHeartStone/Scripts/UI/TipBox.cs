using UnityEngine;
namespace UI
{
    partial class TipBox
    {
        public void display(string msg, Vector2 position)
        {
            Text.text = msg;
            display();
            rectTransform.position = position;
        }
    }
}
