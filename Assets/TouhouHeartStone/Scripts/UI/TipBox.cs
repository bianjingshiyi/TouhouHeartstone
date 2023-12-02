using BJSYGameCore.UI;
using TMPro;
using UnityEngine;
namespace UI
{
    public class TipBox : UIObject
    {
        public void display(string msg)
        {
            Text.text = msg;
            display();
        }

        public void setPosition(Vector3 pos)
        {
            rectTransform.position = pos;
        }
        [SerializeField]
        private TextMeshProUGUI _text;
        public TextMeshProUGUI Text => _text;
    }
}
