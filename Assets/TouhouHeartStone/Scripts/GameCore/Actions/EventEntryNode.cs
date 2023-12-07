using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class EventEntryNode : PassiveEntryNode, IEntryNode
    {
        #region 公有方法
        public EventEntryNode(EventTriggerTime triggerTime) : base()
        {
            this.triggerTime = triggerTime;
        }
        public void setEventDefine(EventDefine define, EventTriggerTimeType type)
        {
            this.define = define;
            this.type = type;
        }
        public override Task<ControlOutput> run(Flow flow)
        {
            foreach (var output in outputPorts.OfType<ValueOutput>())
            {
                flow.setValue(output, flow.env.eventArg.getVar(output.name));
            }
            return Task.FromResult(getOutputPort<ControlOutput>(actionPortName));
        }
        public override ISerializableNode ToSerializableNode()
        {
            return new SerializableEventEntryNode(this);
        }
        protected override IEnumerable<PortDefine> GetValueOutputDefines()
        {
            if (define == null)
                yield break;
            var variableInfos = define.getVariableInfos(triggerTime.type);
            if (variableInfos != null)
            {
                foreach (var info in variableInfos)
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

        #endregion
        public EventDefine define { get; private set; }
        public EventTriggerTimeType type { get; private set; }
        public EventTriggerTime triggerTime { get; private set; }
    }
    [Serializable]
    public class SerializableEventEntryNode : ISerializableNode
    {
        public SerializableEventEntryNode(EventEntryNode eventEntryNode)
        {
            id = eventEntryNode.id;
            triggerTime = new SerializableEventTriggerTime(eventEntryNode.triggerTime);
            posX = eventEntryNode.posX;
            posY = eventEntryNode.posY;
            consts = eventEntryNode.consts != null ? new Dictionary<string, object>(eventEntryNode.consts) : null;
        }
        public PassiveEntryNode ToActionNode(ActionGraph graph)
        {
            EventTriggerTime eventTriggerTime;
            if (triggerTime != null)
            {
                eventTriggerTime = triggerTime.toTriggerTime();
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
            if (consts != null)
            {
                foreach (var pair in consts)
                {
                    entry.setConst(pair.Key, pair.Value);
                }
            }
            if (tags != null)
                entry.setTags(tags);
            if (piles != null)
                entry.setPiles(piles);
            entry.graph = graph;
            return entry;
        }
        Node ISerializableNode.ToActionNode(ActionGraph graph) => ToActionNode(graph);

        public int id;
        public float posX;
        public float posY;
        public Dictionary<string, object> consts;
        public SerializableEventTriggerTime triggerTime;
        [Obsolete]
        public long eventCardPoolId;
        [Obsolete]
        public string eventName;
        [Obsolete]
        public string[] tags;
        [Obsolete]
        public string[] piles;
    }
}