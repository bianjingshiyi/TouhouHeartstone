using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;

namespace Game
{
    [Union(0, typeof(SerializableCustomPropertyModifier))]
    [Union(1, typeof(SerializableKeywordsModifier))]
    [Union(2, typeof(SerializableAttackModifier))]
    [Union(3, typeof(SerializableDamageReduceModifier))]
    [Union(4, typeof(SerializableCostModifier))]
    [Union(5, typeof(SerializableLifeModifier))]
    [Union(6, typeof(SerializableSpellDamageModifier))]
    [Union(7, typeof(SerializableTagModifier))]
    [MessagePackObject]
    [Serializable]
    public abstract class SerializablePropModifier
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializablePropModifier()
        {
        }
        public SerializablePropModifier(PropModifier modifier)
        {
            relatedPropName = modifier.relatedPropName;
            propertyName = modifier.propertyName;
        }
        public abstract PropModifier deserialize();
        [Key(0)]
        public string relatedPropName;
        [Key(1)]
        public string propertyName;
    }
    [MessagePackObject]
    [Serializable]
    public abstract class SerializablePropModifier<T> : SerializablePropModifier
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializablePropModifier() : base()
        {
        }
        public SerializablePropModifier(PropModifier<T> modifier) : base(modifier)
        {
            defaultValue = modifier.defaultValue;
        }
        protected T getDefaultValue()
        {
            var defValue = default(T);
            if (Equals(defaultValue, defValue) && !Equals(value, defValue))
            {
                return value;
            }
            return defaultValue;
        }
        [Key(2)]
        public T defaultValue;
        [IgnoreMember]
        [Obsolete]
        [BsonIgnoreIfDefault]
        public T value;
    }
}
