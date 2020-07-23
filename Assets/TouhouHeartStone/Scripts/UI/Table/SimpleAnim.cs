using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace UI
{
    [Serializable]
    public class SimpleAnim
    {
        [SerializeField]
        UnityEvent beforeAnim;
        public SetColor setColor;
        public void invokeBeforeAnim()
        {
            beforeAnim.Invoke();
            if (setColor.enable)
                setColor.target.color = setColor.color;
        }
        public string animName;
        public UnityEvent afterAnim;
    }
    [Serializable]
    public class SetColor
    {
        public bool enable = false;
        public Graphic target;
        public Color color = Color.white;
    }
}
