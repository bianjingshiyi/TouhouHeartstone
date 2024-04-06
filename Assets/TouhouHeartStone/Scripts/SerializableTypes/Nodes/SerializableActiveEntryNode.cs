using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    public class SerializableActiveEntryNode : SerializableNode
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableActiveEntryNode() : base()
        {
        }
        public SerializableActiveEntryNode(ActiveEntryNode activeEntry) : base(activeEntry)
        {
            targetCheckerList = activeEntry.targetCheckerList.ConvertAll(t => new SerializableTargetChecker(t));
        }
        public ActiveEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new ActiveEntryNode(id)
            {
                posX = posX,
                posY = posY,
            };
            IEnumerable<InputDefaultValue> defaultValues =
                inputDefaultValues?.Select(dv => dv.deserialize())
                ?? consts?.Select(c => new InputDefaultValue(c.Key, -1, c.Value));
            InitNode(entry, graph, defaultValues);
            if (!entry.hasInputDefaultValue(ActiveEntryNode.conditionPortName))
                entry.setInputDefaultValue(ActiveEntryNode.conditionPortName, true);

            if (tags != null)
                entry.setTags(tags);
            foreach (var seri in targetCheckerList)
            {
                var checker = seri.toTargetChecker();
                entry.AddTargetChecker(checker);
            }
            return entry;
        }
        public override Node ToNode(ActionGraph graph) => ToActionNode(graph);
        [IgnoreMember]
        [Obsolete]
        public string[] tags;
        [IgnoreMember]
        [Obsolete]
        public Dictionary<string, object> consts;
        [Key(5)]
        public List<SerializableTargetChecker> targetCheckerList = new List<SerializableTargetChecker>();
    }
}
