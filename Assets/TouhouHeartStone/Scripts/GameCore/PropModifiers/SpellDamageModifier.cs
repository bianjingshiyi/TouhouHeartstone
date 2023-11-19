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
        protected SpellDamageModifier(IntPropModifier origin) : base(origin)
        {
        }
    }
}