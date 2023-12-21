using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class EventRuleLogicTrigger : Trigger
    {
        public EventRuleLogicTrigger(CardEngine game, GeneratedRuleEventLogic logic)
        {
            this.game = game;
            this.logic = logic;
        }
        public override bool checkCondition(IEventArg arg)
        {
            return logic.checkCondition(game, arg);
        }
        public override Task invoke(IEventArg arg)
        {
            return logic.execute(game, arg);
        }
        public CardEngine game;
        public GeneratedRuleEventLogic logic;
    }
}
