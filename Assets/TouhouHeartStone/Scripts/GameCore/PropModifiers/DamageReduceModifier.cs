using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class DamageReduceModifier : IntPropModifier
    {
        public DamageReduceModifier(int value) : base(nameof(ServantCardDefine.damageReduce), value)
        {
        }
        public DamageReduceModifier(int value, bool isSet) : base(nameof(ServantCardDefine.damageReduce), value, isSet)
        {
        }
        protected DamageReduceModifier(IntPropModifier origin) : base(origin)
        {
        }
        public override PropModifier clone()
        {
            return new DamageReduceModifier(this);
        }
    }
}