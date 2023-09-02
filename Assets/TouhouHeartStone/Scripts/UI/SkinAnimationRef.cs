using System;
using TouhouCardEngine;

namespace UI
{
    [Serializable]
    public class SkinAnimationRef
    {
        /// <summary>
        /// 构造一个特效引用。
        /// </summary>
        public SkinAnimationRef(string key, DefineReference animRef)
        {
            this.key = key;
            this.animRef = animRef;
        }
        public SkinAnimationRef(SkinAnimationRef other)
        {
            key = other.key;
            animRef = other.animRef;
        }
        public int sourcePosition;
        public int targetPosition;
        public string key;
        public DefineReference animRef;
        public const string NODE_CONST_KEY = "EDITOR_ANIMATION_REF";
    }
}
