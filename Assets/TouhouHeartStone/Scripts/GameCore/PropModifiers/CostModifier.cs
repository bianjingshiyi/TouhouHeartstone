using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
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
        //public CostModifier(int value,) : base(nameof(ServantCardDefine.cost), value)
        //{

        //}
        public override bool checkCondition(IGame game, Card card)
        {
            return base.checkCondition(game, card);
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