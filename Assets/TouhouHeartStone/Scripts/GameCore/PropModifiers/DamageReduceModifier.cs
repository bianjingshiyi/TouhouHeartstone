using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class DamageReduceModifier : IntPropModifier
    {
        public DamageReduceModifier(int value) : base(DEFINE.DAMAGE_REDUCE, value)
        {
        }
        public DamageReduceModifier(int value, bool isSet) : base(DEFINE.DAMAGE_REDUCE, value, isSet)
        {
        }
        public override SerializablePropModifier serialize()
        {
            return new SerializableDamageReduceModifier(this);
        }
    }
    [Serializable]
    [BsonDiscriminator("DamageReduceModifier")]
    public class SerializableDamageReduceModifier : SerializableIntPropModifier
    {
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