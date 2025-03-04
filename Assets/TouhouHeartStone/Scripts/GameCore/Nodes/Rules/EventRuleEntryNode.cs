using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class EventRuleEntryNode : Node, IEntryNode
    {
        #region 公有方法
        public EventRuleEntryNode(EventTriggerTime triggerTime) : base()
        {
            this.triggerTime = triggerTime;
        }
        public void setEventDefine(EventDefine define, EventTriggerTimeType type)
        {
            this.define = define;
            this.type = type;
        }
        public virtual void Define()
        {
            DefinitionConditions();
            DefinitionOutputs();
        }
        public override Task<ControlOutput> run(Flow flow)
        {
            foreach (var output in outputPorts.OfType<ValueOutput>())
            {
                flow.setValue(output, flow.env.eventArg.getVar(output.name));
            }
            return Task.FromResult(getOutputPort<ControlOutput>(actionPortName));
        }

        #region 获取端口
        public ValueInput getTriggerCondtionValuePort()
        {
            return getInputPort<ValueInput>(conditionPortName);
        }
        public ControlOutput getActionOutputPort()
        {
            return outputList?.FirstOrDefault() as ControlOutput;
        }
        #endregion

        public override Node Clone()
        {
            EventRuleEntryNode newNode = new EventRuleEntryNode(triggerTime?.Clone());
            newNode.posX = posX;
            newNode.posY = posY;
            if (propDict != null && propDict.Count > 0)
            {
                newNode.propDict = propDict.Clone();
            }
            newNode.graph = graph?.Clone();
            if (inputDefaultValueList != null && inputDefaultValueList.Count > 0)
            {
                foreach (var defaultValue in inputDefaultValueList)
                {
                    newNode.inputDefaultValueList.Add(defaultValue.Clone());
                }
            }
            return newNode;
        }
        #endregion

        #region 私有方法

        private void DefinitionConditions()
        {
            var inputs = new List<IPort>()
            {
                getOrCreateInputPort(conditionPortDefine)
            };

            foreach (var lostPort in inputList.Except(inputs))
            {
                graph.disconnectAll(lostPort);
            }
            inputList.Clear();
            inputList.AddRange(inputs);
        }
        private void DefinitionOutputs()
        {
            var outputs = new List<IPort>();
            foreach (PortDefine portDefine in getOutputDefines())
            {
                outputs.Add(getOrCreateOutputPort(portDefine));
            }
            foreach (var lostPort in outputList.Except(outputs))
            {
                graph.disconnectAll(lostPort);
            }
            outputList.Clear();
            outputList.AddRange(outputs);
        }
        private IEnumerable<PortDefine> getOutputDefines()
        {
            if (define == null)
                yield break;
            yield return actionPortDefine;
            var variableInfos = define.getVariableInfos(type);
            if (variableInfos != null)
            {
                foreach (var info in variableInfos)
                {
                    yield return PortDefine.Value(info.type, info.name);
                }
            }
            yield break;
        }
        #endregion

        public EventDefine define { get; private set; }
        public EventTriggerTimeType type { get; private set; }
        public EventTriggerTime triggerTime { get; private set; }
        public const string actionPortName = "action";
        public const string conditionPortName = "condition";
        public static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        public static PortDefine conditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, Translations.EDITOR_PARAM_NAME_TRIGGER_CONDITION);
    }
}