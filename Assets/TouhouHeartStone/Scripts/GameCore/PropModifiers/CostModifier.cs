using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class CostModifier : IntPropModifier
    {
        public CostModifier(int value) : base(nameof(ServantCardDefine.cost), value)
        {
        }
        public CostModifier(int value, bool isSet) : base(nameof(ServantCardDefine.cost), value, isSet)
        {
        }
        protected CostModifier(IntPropModifier origin) : base(origin)
        {
        }
        public override PropModifier clone()
        {
            return new CostModifier(this);
        }
    }
}