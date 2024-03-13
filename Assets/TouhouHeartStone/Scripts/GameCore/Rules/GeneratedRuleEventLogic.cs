using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GeneratedRuleEventLogic : GeneratedRuleLogic
    {
        #region 公有方法

        #region 构造方法
        public GeneratedRuleEventLogic(EventTriggerTime triggerTime)
        {
            this.triggerTime = triggerTime;
            var graph = new ActionGraph();
            graph.createEventRuleEntryNode(triggerTime);
            this.graph = graph;
        }
        #endregion

        #region 启用/禁用
        public void enable(CardEngine game)
        {
            var trigger = new EventRuleLogicTrigger(game, this);
            game.triggers.register(triggerTime, trigger);
        }
        public void disable(CardEngine game)
        {
            game.triggers.removeAll(triggerTime, t => t is EventRuleLogicTrigger rt && rt.logic == this);
        }
        #endregion

        #region 执行
        public bool checkCondition(CardEngine game, IEventArg arg)
        {
            var entry = entryNode;
            if (entry != null)
            {
                var flowEnv = new FlowEnv(game, null, null, arg, null);
                var flow = new Flow(flowEnv);
                var condition = entry.getTriggerCondtionValuePort();
                if (condition == null || condition.getConnectedOutputPort() == null)
                    return true;
                var task = game.getValue<bool>(flow, condition);
                if (task.IsCompleted)
                {
                    return task.Result;
                }
                throw new InvalidOperationException("不能在条件中调用需要等待的动作");
            }
            return false;
        }
        public Task execute(CardEngine game, IEventArg arg)
        {
            var entry = entryNode;
            if (entry != null)
            {
                var flowEnv = new FlowEnv(game, null, null, arg, null);
                var flow = new Flow(flowEnv);
                return game.runActions(flow, entry.getActionOutputPort());
            }
            return Task.CompletedTask;
        }
        #endregion
        public override SerializableGeneratedRuleLogic serialize()
        {
            return new SerializableGeneratedRuleEventLogic(this);
        }

        #endregion

        #region 私有方法

        #region 构造器
        internal GeneratedRuleEventLogic(EventTriggerTime triggerTime, ActionGraph graph)
        {
            this.triggerTime = triggerTime;
            this.graph = graph;
        }
        #endregion

        #endregion

        public EventRuleEntryNode entryNode => graph?.findEventRuleEntryNode();
        public EventTriggerTime triggerTime { get; private set; }
    }
    [Serializable]
    public class SerializableGeneratedRuleEventLogic : SerializableGeneratedRuleLogic
    {
        public SerializableGeneratedRuleEventLogic(GeneratedRuleEventLogic logic) : base(logic)
        {
            triggerTime = new SerializableEventTriggerTime(logic.triggerTime);
        }
        public override GeneratedRuleLogic deserialize(INodeDefiner nodeDefiner)
        {
            var graph = this.graph.toActionGraph(nodeDefiner);
            var logic = new GeneratedRuleEventLogic(triggerTime.toTriggerTime(), graph);
            initLogic(logic);
            return logic;
        }
        public SerializableEventTriggerTime triggerTime;
    }
}
