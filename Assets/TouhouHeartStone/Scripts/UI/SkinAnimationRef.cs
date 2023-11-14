using System;
using TouhouCardEngine;

namespace UI
{
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

    [Serializable]
    public class SerializableSkinAnimationRef
    {
        public SerializableSkinAnimationRef(SkinAnimationRef animRef)
        {
            sourcePosition = animRef.sourcePosition;
            targetPosition = animRef.targetPosition;
            key = animRef.key;
            this.animRef = animRef.animRef;
        }
        public SkinAnimationRef toRef()
        {
            return new SkinAnimationRef(key, animRef)
            {
                sourcePosition = sourcePosition,
                targetPosition = targetPosition
            };
        }
        public int sourcePosition;
        public int targetPosition;
        public string key;
        public DefineReference animRef;
    }
}
