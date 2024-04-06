using MessagePack;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableGeneratedRuleEventLogic : SerializableGeneratedRuleLogic
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableGeneratedRuleEventLogic() : base()
        {
        }
        public SerializableGeneratedRuleEventLogic(GeneratedRuleEventLogic logic) : base(logic)
        {
            triggerTime = logic.triggerTime;
        }
        public override GeneratedRuleLogic deserialize(INodeDefiner nodeDefiner)
        {
            var graph = this.graph.toActionGraph(nodeDefiner);
            var logic = new GeneratedRuleEventLogic(triggerTime, graph);
            initLogic(logic);
            return logic;
        }
        [Key(4)]
        public EventTriggerTime triggerTime;
    }
}
