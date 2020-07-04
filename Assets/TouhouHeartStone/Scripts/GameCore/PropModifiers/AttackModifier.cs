using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class AttackModifier : IntPropModifier
    {
        public AttackModifier(int value) : base(nameof(ServantCardDefine.attack), value)
        {
        }
        public AttackModifier(int value, bool isSet) : base(nameof(ServantCardDefine.attack), value, isSet)
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