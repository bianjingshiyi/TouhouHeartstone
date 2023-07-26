using TMPro;
using UnityEngine;

namespace Game.BattleTable
{
    public class PropertyNumber : MonoBehaviour
    {
        public void setTextColor(PropertyNumberColor color)
        {
            _animator.SetBool("WhiteText", color == PropertyNumberColor.White);
            _animator.SetBool("GreenText", color == PropertyNumberColor.Green);
            _animator.SetBool("RedText", color == PropertyNumberColor.Red);
        }
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
