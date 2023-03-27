using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GeneratedBattlecry : GeneratedEffect
    {
        #region 公有方法
        public GeneratedBattlecry(ActionGraph graph) : base(graph)
        {
        }
        public GeneratedBattlecry() : base(null)
        {
        }
        public override void Init()
        {
            var entryNode = graph.createTriggerEntryNode(entryNodeEvent);
            entryNode.hideEvents = true;
        }
        public override Task execute(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            if (activeTrigger != null)
            {
                return game.runActions(card, buff, eventArg, activeTrigger.getActionOutputPort());
            }
            return Task.CompletedTask;
        }
        #endregion
        #region 属性字段
        public TriggerEntryNode activeTrigger => graph?.findTriggerEntryNode(entryNodeEvent);
        public const string entryNodeEvent = "ActiveEvent";
        #endregion
    }
}