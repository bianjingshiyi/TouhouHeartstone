using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    [BsonDiscriminator("CustomPropertyModifier")]
    public class SerializableCustomPropertyModifier : SerializablePropModifier<object>
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableCustomPropertyModifier() : base()
        {
        }
        public SerializableCustomPropertyModifier(CustomPropertyModifier modifier) : base(modifier)
        {
            isSet = modifier.isSet;
        }
        public override PropModifier deserialize()
        {
            return new CustomPropertyModifier(propertyName, getDefaultValue(), isSet)
            {
                relatedPropName = relatedPropName
            };
        }
        [Key(3)]
        public bool isSet;
    }
}
