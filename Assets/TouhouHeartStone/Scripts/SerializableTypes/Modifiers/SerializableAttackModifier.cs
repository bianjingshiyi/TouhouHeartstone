using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    [BsonDiscriminator("AttackModifier")]
    public class SerializableAttackModifier : SerializableIntPropModifier
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableAttackModifier() : base()
        {
        }
        public SerializableAttackModifier(AttackModifier modifier) : base(modifier)
        {
        }
        public override PropModifier deserialize()
        {
            return new AttackModifier(getDefaultValue(), isSet)
            {
                relatedPropName = relatedPropName
            };
        }
    }
}
