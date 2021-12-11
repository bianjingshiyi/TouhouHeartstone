using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone
{
    public class CostModifier : IntPropModifier
    {
        public CostModifier() : base(DEFINE.COST, 0)
        {
        }
        public CostModifier(int value) : base(DEFINE.COST, value)
        {
        }
        public CostModifier(int value, bool isSet) : base(DEFINE.COST, value, isSet)
        {
        }
        public CostModifier(int value, CheckConditionDelegate onCheckCondition) : base(DEFINE.COST, value)
        {
            this.onCheckCondition = onCheckCondition;
        }
        public override bool checkCondition(IGame game, Card card)
        {
            if (onCheckCondition != null && !onCheckCondition(game as CardEngine, card))
                return false;
            return base.checkCondition(game, card);
        }
        protected CostModifier(IntPropModifier origin) : base(origin)
        {
        }
        public override PropModifier clone()
        {
            return new CostModifier(this);
        }
        public CheckConditionDelegate onCheckCondition;
    }
}