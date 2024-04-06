using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    [BsonDiscriminator("DamageReduceModifier")]
    public class SerializableDamageReduceModifier : SerializableIntPropModifier
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableDamageReduceModifier() : base()
        {
        }
        public SerializableDamageReduceModifier(DamageReduceModifier modifier) : base(modifier)
        {
        }
        public override PropModifier deserialize()
        {
            return new DamageReduceModifier(getDefaultValue(), isSet)
            {
                relatedPropName = relatedPropName
            };
        }
    }
}
