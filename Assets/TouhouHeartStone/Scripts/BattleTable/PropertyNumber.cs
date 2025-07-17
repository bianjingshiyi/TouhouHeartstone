using TMPro;
using UnityEngine;

namespace Game.BattleTable
{
    public class PropertyNumber : MonoBehaviour
    {
        public void setTextColor(PropertyNumberColor color)
        {
            if (color == PropertyNumberColor.Green)
            {
                setTextGreen();
            }
            else if (color == PropertyNumberColor.Red)
            {
                setTextRed();
            }
            else
            {
                setTextWhite();
            }
        }

        private void setTextWhite()
        {
            _meshRendererController.color = _whiteColor;
        }
        private void setTextGreen()
        {
            _meshRendererController.color = _greenColor;
        }
        private void setTextRed()
        {
            _meshRendererController.color = _redColor;
        }

        public Animator animator => _animator;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private TextMeshPro _text;
        [SerializeField]
        private RendererController _meshRendererController;
        [SerializeField]
        private Color _whiteColor = Color.white;
        [SerializeField]
        private Color _greenColor = Color.green;
        [SerializeField]
        private Color _redColor = Color.red;

        public string text
        {
            get => _text.text;
            set => _text.text = value;
        }
    }

    public enum PropertyNumberColor
    {
        White = 0,
        Green = 1,
        Red = 2
    }
}
