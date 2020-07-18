using System;
using UnityEngine.Events;
namespace UI
{
    [Serializable]
    public class SimpleAnim
    {
        public UnityEvent beforeAnim;
        public string animName;
        public UnityEvent afterAnim;
    }
}
