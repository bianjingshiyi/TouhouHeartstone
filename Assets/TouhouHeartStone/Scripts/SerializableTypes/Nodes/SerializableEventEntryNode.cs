using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableEventEntryNode : SerializableNode
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableEventEntryNode()
        {
        }
        public SerializableEventEntryNode(EventEntryNode eventEntryNode) : base(eventEntryNode)
        {
            triggerTime = eventEntryNode.triggerTime;
        }
        public PassiveEntryNode ToActionNode(ActionGraph graph)
        {
            EventTriggerTime eventTriggerTime;
            if (triggerTime != null)
            {
                eventTriggerTime = triggerTime;
            }
            else
            {
                eventTriggerTime = EventTriggerTime.fromName(eventCardPoolId, eventName);
            }
            var entry = new EventEntryNode(eventTriggerTime)
            {
                id = id,
                posX = posX,
                posY = posY
            };
            IEnumerable<InputDefaultValue> defaultValues =
                inputDefaultValues?.Select(dv => dv.deserialize())
                ?? consts?.Select(c => new InputDefaultValue(c.Key, -1, c.Value));
            InitNode(entry, graph, defaultValues);
            if (!entry.hasInputDefaultValue(PassiveEntryNode.conditionPortName))
                entry.setInputDefaultValue(PassiveEntryNode.conditionPortName, true);
            // 将PileNameCollection更新为PileNameRange
            var pileNameCollection = entry.getPiles();
            if (pileNameCollection != null && pileNameCollection is not PileNameRange)
            {
                entry.setPiles(pileNameCollection.ToArray());
            }

            if (tags != null)
                entry.setTags(tags);
            if (piles != null)
                entry.setPiles(piles);
            return entry;
        }
        public override Node ToNode(ActionGraph graph) => ToActionNode(graph);
        [IgnoreMember]
        [Obsolete]
        public Dictionary<string, object> consts;
        [Key(5)]
        public EventTriggerTime triggerTime;
        [IgnoreMember]
        [Obsolete]
        public long eventCardPoolId;
        [IgnoreMember]
        [Obsolete]
        public string eventName;
        [IgnoreMember]
        [Obsolete]
        public string[] tags;
        [IgnoreMember]
        [Obsolete]
        public string[] piles;
    }
}
