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
        public override SerializableNode ToSerializableNode()
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
    public class SerializableEventEntryNode : SerializableNode
    {
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

            if (tags != null)
                entry.setTags(tags);
            if (piles != null)
                entry.setPiles(piles);
            return entry;
        }
        public override Node ToNode(ActionGraph graph) => ToActionNode(graph);

        [Obsolete]
        public Dictionary<string, object> consts;
        public EventTriggerTime triggerTime;
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