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
}