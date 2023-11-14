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
        public static string getNodeKey(Node node)
        {
            return node.getConst<string>(NODE_CONST_KEY);
        }
        public static void setNodeKey(Node node, string value)
        {
            node.setConst(NODE_CONST_KEY, value);
        }
        public string key;
        public int sourcePosition;
        public int targetPosition;
        public EventReference eventReference;
        public DefineReference animRef;
        public const string NODE_CONST_KEY = "EDITOR_ANIMATION_REF";
    }

    [Serializable]
    public class SerializableSkinAnimationRef
    {
        public SerializableSkinAnimationRef(SkinAnimationRef animRef)
        {
            key = animRef.key;
            this.animRef = animRef.animRef;
            sourcePosition = animRef.sourcePosition;
            targetPosition = animRef.targetPosition;
            if (animRef.eventReference != null)
                eventReference = animRef.eventReference;
        }
        public SkinAnimationRef toRef()
        {
            return new SkinAnimationRef(key, animRef)
            {
                eventReference = eventReference,
                sourcePosition = sourcePosition,
                targetPosition = targetPosition
            };
        }
        public int sourcePosition;
        public int targetPosition;
        public string key;
        public DefineReference animRef;
        public EventReference eventReference;
    }
}
