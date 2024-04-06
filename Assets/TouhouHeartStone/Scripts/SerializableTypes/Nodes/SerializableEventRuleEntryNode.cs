using MessagePack;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableEventRuleEntryNode : SerializableNode
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableEventRuleEntryNode()
        {
        }
        public SerializableEventRuleEntryNode(EventRuleEntryNode eventEntryNode) : base(eventEntryNode)
        {
            triggerTime = eventEntryNode.triggerTime;
        }
        public EventRuleEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new EventRuleEntryNode(triggerTime)
            {
                id = id,
                posX = posX,
                posY = posY
            };
            InitNode(entry, graph);
            return entry;
        }
        public override Node ToNode(ActionGraph graph) => ToActionNode(graph);
        [Key(5)]
        public EventTriggerTime triggerTime;
    }
}
