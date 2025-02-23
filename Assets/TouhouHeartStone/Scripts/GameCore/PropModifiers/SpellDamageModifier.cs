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
        public override PropModifier Clone()
        {
            return new SpellDamageModifier(defaultValue, isSet);
        }
    }
}