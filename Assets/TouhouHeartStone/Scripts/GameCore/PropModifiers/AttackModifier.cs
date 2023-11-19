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
        protected AttackModifier(IntPropModifier origin) : base(origin)
        {
        }
    }
}