using System.Windows.Forms;
using TMPro;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    public class PropertyNumber : MonoBehaviour
    {
        public void setTextColor(PropertyNumberColor color)
        {
            setTextGreen(color == PropertyNumberColor.Green);
            setTextRed(color == PropertyNumberColor.Red);
        }
        public void setTextGreen(bool green)
        {
            _animator.SetBool("isGreen", green);
        }
        public void setTextRed(bool red)
        {
            _animator.SetBool("isRed", red);
        }

        public Animator animator => _animator;
        public SimpleAnim textWhiteAnim;
        public SimpleAnim textGreenAnim;
        public SimpleAnim textRedAnim;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private TextMeshPro _text;

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
