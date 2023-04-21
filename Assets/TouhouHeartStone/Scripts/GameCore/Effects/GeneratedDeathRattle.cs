using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GeneratedDeathRattle : GeneratedEffect
    {
        #region 公有方法
        public GeneratedDeathRattle(ActionGraph graph) : base(graph)
        {
        }
        public GeneratedDeathRattle() : base(null)
        {
        }
        public override void Init()
        {
            var entryNode = graph.createTriggerEntryNode("ActiveEvent");
            entryNode.hideEvents = true;
        }
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            throw new System.NotImplementedException();
        }
        public override Task execute(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            if (activeTrigger != null)
            {
                var flow = new Flow(game, card, buff, eventArg);
                return game.runActions(flow, activeTrigger.getActionOutputPort());
            }
            return Task.CompletedTask;
        }
        #endregion
        #region 属性字段
        public TriggerEntryNode activeTrigger => graph?.findTriggerEntryNode("ActiveEvent");
        #endregion
    }
}