using System;
using System.Collections.Generic;
using System.Linq;

namespace TouhouCardEngine
{
    public class EventEntryNode : PassiveEntryNode, IDefineNode<EventDefine>
    {
        #region 公有方法
        public EventEntryNode(string eventName) : base()
        {
            this.eventName = eventName;
        }
        public override ISerializableNode ToSerializableNode()
        {
            return new SerializableEventEntryNode(this);
        }
        protected override IEnumerable<PortDefine> GetValueOutputDefines()
        {
            if (define?.variableInfos != null)
            {
                foreach (var info in define.variableInfos)
                {
                    yield return PortDefine.Value(info.type, info.name);
                }
            }
            yield break;
        }
        protected override IEnumerable<PortDefine> GetControlOutputDefines()
        {
            yield return actionPortDefine;
            yield break;
        }
        public void setPiles(params string[] value)
        {
            var tags = getConst<PileNameCollection>(pileListName);
            if (tags == null)
            {
                tags = new PileNameCollection();
                constList.Add(pileListName, tags);
            }
            tags.Clear();
            tags.AddRange(value);
        }

        #endregion
        public EventDefine define { get; set; }
        public string eventName;
        public PileNameCollection piles => getConst<PileNameCollection>(pileListName);
        public const string pileListName = "pileList";
    }
    [Serializable]
    public class SerializableEventEntryNode : ISerializableNode
    {
        public SerializableEventEntryNode(EventEntryNode eventEntryNode)
        {
            id = eventEntryNode.id;
            eventName = eventEntryNode.eventName;
            posX = eventEntryNode.posX;
            posY = eventEntryNode.posY;
            tags = eventEntryNode.tags?.ToArray() ?? Array.Empty<string>();
            piles = eventEntryNode.piles?.ToArray() ?? Array.Empty<string>();
        }
        public PassiveEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new EventEntryNode(eventName)
            {
                id = id,
                posX = posX,
                posY = posY
            };
            entry.setTags(tags);
            entry.setPiles(piles);
            entry.graph = graph;
            return entry;
        }
        Node ISerializableNode.ToActionNode(ActionGraph graph) => ToActionNode(graph);

        public int id;
        public float posX;
        public float posY;
        public string eventName;
        public string[] tags;
        public string[] piles;
    }
}