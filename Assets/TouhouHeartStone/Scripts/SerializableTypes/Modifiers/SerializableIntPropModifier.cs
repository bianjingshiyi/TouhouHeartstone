using MessagePack;
using System;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public abstract class SerializableIntPropModifier : SerializablePropModifier<int>
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableIntPropModifier() : base()
        {
        }
        public SerializableIntPropModifier(IntPropModifier modifier) : base(modifier)
        {
            isSet = modifier.isSet;
        }
        [Key(3)]
        public bool isSet;
        [IgnoreMember]
        [Obsolete]
        public int minValue;
        [IgnoreMember]
        [Obsolete]
        public int maxValue;
    }
}
