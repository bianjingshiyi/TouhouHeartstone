using System;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class AttackModifier : IntPropModifier
    {
        public AttackModifier(int value) : base(DEFINE.ATTACK, value)
        {
        }
        public AttackModifier(int value, bool isSet) : base(DEFINE.ATTACK, value, isSet)
        {
        }
        public override SerializablePropModifier serialize()
        {
            return new SerializableAttackModifier(this);
        }
    }
    [Serializable]
    [BsonDiscriminator("AttackModifier")]
    public class SerializableAttackModifier : SerializableIntPropModifier
    {
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