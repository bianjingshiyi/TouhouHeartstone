using Game.Animations;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using UI;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableSkinAnimationRef : IComparable<SerializableSkinAnimationRef>
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
            colorModifies = animRef.colorModifies?.Select(d => new SerializableColorModifyData(d))?.ToArray();
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
                targetPositionVarName = targetPositionVarName,
                colorModifies = colorModifies?.Select(d => d.deserialize())?.ToList() ?? new List<ColorModifyData>(),
            };
        }

        public int CompareTo(SerializableSkinAnimationRef other)
        {
            if (other == null)
                return -1; // 任何对象都排在null前面

            int result = key.CompareTo(other.key);
            if (result != 0)
                return result;

            return eventReference.CompareTo(other.eventReference);
        }

        public int CompareTo(SerializableSkinAnimationRef other, StringComparer comparer)
        {
            if (other == null)
                return -1; // 任何对象都排在null前面

            int result = key.CompareTo(other.key);
            if (result != 0)
                return result;

            return eventReference.CompareTo(other.eventReference, comparer);
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
        [Key(7)]
        public SerializableColorModifyData[] colorModifies;
    }
}
