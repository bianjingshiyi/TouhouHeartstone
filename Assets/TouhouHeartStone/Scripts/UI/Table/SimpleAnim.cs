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
        [SerializeField]
        SetColor setColor;
        public void invokeBeforeAnim()
        {
            beforeAnim.Invoke();
            if (setColor.enable)
                setColor.target.color = setColor.color;
        }
        public string animName;
        [SerializeField]
        UnityEvent afterAnim;
        public void invokeAfterAnim()
        {
            afterAnim.Invoke();
        }
    }
    [Serializable]
    public class SetColor
    {
        public bool enable = false;
        public Graphic target;
        public Color color = Color.white;
    }
}
