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
    public class SerializableAuraEntryNode : SerializableNode
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableAuraEntryNode()
        {
        }
        public SerializableAuraEntryNode(AuraEntryNode entryNode) : base(entryNode)
        {
        }
        public AuraEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new AuraEntryNode()
            {
                id = id,
                posX = posX,
                posY = posY
            };
            IEnumerable<InputDefaultValue> defaultValues =
                inputDefaultValues?.Select(dv => dv.deserialize())
                ?? consts?.Select(c => new InputDefaultValue(c.Key, -1, c.Value));
            InitNode(entry, graph, defaultValues);
            // 将PileNameCollection更新为PileNameRange
            var pileNameCollection = entry.getPiles();
            if (pileNameCollection != null && pileNameCollection is not PileNameRange)
            {
                entry.setPiles(pileNameCollection.ToArray());
            }

            if (piles != null)
                entry.setPiles(piles);
            if (ranges != null)
                entry.setRanges(ranges);
            return entry;
        }
        public override Node ToNode(ActionGraph graph) => ToActionNode(graph);
        [IgnoreMember]
        [Obsolete]
        public Dictionary<string, object> consts;
        [IgnoreMember]
        [Obsolete]
        public string[] piles;
        [IgnoreMember]
        [Obsolete]
        public string[] ranges;
    }
}
