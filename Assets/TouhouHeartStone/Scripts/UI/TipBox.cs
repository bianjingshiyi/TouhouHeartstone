using BJSYGameCore.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class TipBox : UIObject
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
        [SerializeField]
        private TextMeshProUGUI _text;
        public TextMeshProUGUI Text => _text;
    }
}
