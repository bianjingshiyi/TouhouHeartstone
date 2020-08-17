using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class AttackModifier : IntPropModifier
    {
        public AttackModifier(int value) : base(PROPERTY.ATTACK, value)
        {
        }
        public AttackModifier(int value, bool isSet) : base(PROPERTY.ATTACK, value, isSet)
        {
        }
        protected AttackModifier(IntPropModifier origin) : base(origin)
        {
        }
        public override PropModifier clone()
        {
            return new AttackModifier(this);
        }
    }
}