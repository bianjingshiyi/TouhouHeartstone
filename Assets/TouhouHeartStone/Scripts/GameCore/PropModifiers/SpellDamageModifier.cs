using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class SpellDamageModifier : IntPropModifier
    {
        public SpellDamageModifier(int value) : base(DEFINE.SPELL_DAMAGE, value)
        {
        }
        public SpellDamageModifier(int value, bool isSet) : base(DEFINE.SPELL_DAMAGE, value, isSet)
        {
        }
        public override SerializablePropModifier serialize()
        {
            return new SerializableSpellDamageModifier(this);
        }
    }
    [Serializable]
    [BsonDiscriminator("SpellDamageModifier")]
    public class SerializableSpellDamageModifier : SerializableIntPropModifier
    {
        public SerializableSpellDamageModifier(SpellDamageModifier modifier) : base(modifier)
        {
        }
        public override PropModifier deserialize()
        {
            return new SpellDamageModifier(getDefaultValue(), isSet)
            {
                relatedPropName = relatedPropName
            };
        }
    }
}