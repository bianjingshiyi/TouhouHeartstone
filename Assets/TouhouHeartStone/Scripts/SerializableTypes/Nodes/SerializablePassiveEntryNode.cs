using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializablePassiveEntryNode : SerializableNode
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializablePassiveEntryNode() : base()
        {
        }
        public SerializablePassiveEntryNode(PassiveEntryNode eventEntryNode) : base(eventEntryNode)
        {
        }
        public PassiveEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new PassiveEntryNode()
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
        [IgnoreMember]
        [Obsolete]
        public string[] tags;
        [IgnoreMember]
        [Obsolete]
        public string[] piles;
    }
}
