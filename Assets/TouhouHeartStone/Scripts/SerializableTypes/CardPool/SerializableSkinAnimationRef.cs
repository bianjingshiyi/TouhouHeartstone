using MessagePack;
using System;
using TouhouCardEngine;
using UI;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableSkinAnimationRef
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableSkinAnimationRef()
        {
        }
        public SerializableSkinAnimationRef(SkinAnimationRef animRef)
        {
            key = animRef.key;
            this.animRef = animRef.animRef;
            sourcePosition = animRef.sourcePosition;
            targetPosition = animRef.targetPosition;
            sourcePositionVarName = animRef.sourcePositionVarName;
            targetPositionVarName = animRef.targetPositionVarName;
            if (animRef.eventReference != null)
                eventReference = animRef.eventReference;
        }
        public SkinAnimationRef toRef()
        {
            return new SkinAnimationRef(key, animRef)
            {
                eventReference = eventReference,
                sourcePosition = sourcePosition,
                targetPosition = targetPosition,
                sourcePositionVarName = sourcePositionVarName,
                targetPositionVarName = targetPositionVarName
            };
        }
        [Key(0)]
        public int sourcePosition;
        [Key(1)]
        public int targetPosition;
        [Key(2)]
        public string sourcePositionVarName;
        [Key(3)]
        public string targetPositionVarName;
        [Key(4)]
        public string key;
        [Key(5)]
        public DefineReference animRef;
        [Key(6)]
        public EventReference eventReference;
    }
}
