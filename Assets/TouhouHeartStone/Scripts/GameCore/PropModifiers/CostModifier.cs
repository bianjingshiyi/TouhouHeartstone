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
        public CostModifier(int value, CheckConditionDelegate onCheckCondition) : base(nameof(ServantCardDefine.cost), value)
        {
            _onCheckCondition = onCheckCondition;
        }
        CheckConditionDelegate _onCheckCondition;
        public override bool checkCondition(IGame game, Card card)
        {
            if (_onCheckCondition != null && !_onCheckCondition(game as THHGame, card))
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
    }
}