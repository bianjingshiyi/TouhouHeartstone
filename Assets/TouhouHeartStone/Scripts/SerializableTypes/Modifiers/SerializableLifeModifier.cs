using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    [BsonDiscriminator("LifeModifier")]
    public class SerializableLifeModifier : SerializableIntPropModifier
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableLifeModifier() : base()
        {
        }
        public SerializableLifeModifier(LifeModifier modifier) : base(modifier)
        {
        }
        public override PropModifier deserialize()
        {
            return new LifeModifier(getDefaultValue(), isSet)
            {
                relatedPropName = relatedPropName
            };
        }
    }
}
