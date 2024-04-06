using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class ActiveEntryNode : Node, ITargetCheckerNode, IEntryNode
    {
        #region 公有方法
        public ActiveEntryNode(int id)
        {
            this.id = id;
            inputList.Add(new ValueInput(this, conditionPortDefine));
            inputList.Add(new ValueInput(this, tagsPortDefine));
            inputList.Add(new ValueInput(this, priorityPortDefine));
            outputList.Add(new ControlOutput(this, actionPortDefine));
        }
        public override Task<ControlOutput> run(Flow flow)
        {
            foreach (var output in outputPorts.OfType<ValueOutput>())
            {
                flow.setValue(output, flow.env.eventArg.getVar(output.name));
            }
            return Task.FromResult(getOutputPort<ControlOutput>(actionPortName));
        }
        public void Define()
        {
            DefinitionConditions();
            DefinitionOutputs();
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
        public ValueInput getTargetConditionPort(int index)
        {
            return getInputPorts<ValueInput>().FirstOrDefault(p => p.name == targetConditionName && p.paramIndex == index);
        }
        public ValueInput getTagsPort()
        {
            return getInputPort<ValueInput>(tagsPortName);
        }
        public ValueInput[] getTargetConditionPorts()
        {
            return getInputPorts<ValueInput>().Where(p => p.name == targetConditionName).ToArray();
        }
        #endregion

        #region 目标
        public int getTargetCheckerIndex(TargetChecker checker)
        {
            return targetCheckerList.IndexOf(checker);
        }
        public void AddTargetChecker(TargetChecker checker)
        {
            InsertTargetChecker(targetCheckerList.Count, checker);
        }
        public void InsertTargetChecker(int index, TargetChecker checker)
        {
            checker.node = this;
            targetCheckerList.Insert(index, checker);
            DefinitionConditions();
        }
        public void RemoveTargetChecker(int index)
        {
            var checker = targetCheckerList[index];
            checker.node = null;
            targetCheckerList.Remove(checker);
            DefinitionConditions();
        }
        #endregion

        #region 值
        public void setTags(params string[] value)
        {
            var tags = getInputDefaultValue<EffectTagCollection>(tagsPortName);
            if (tags == null)
            {
                tags = new EffectTagCollection();
                setInputDefaultValue(tagsPortName, tags);
            }
            tags.Clear();
            if (value != null)
                tags.AddRange(value);
        }
        public string[] getTags()
        {
            return getInputDefaultValue<EffectTagCollection>(tagsPortName)?.ToArray();
        }
        public int getPriorty()
        {
            return getInputDefaultValue<int>(priorityPortName);
        }
        #endregion

        private void DefinitionConditions()
        {
            var inputs = new List<IPort> 
            { 
                getOrCreateInputPort(conditionPortDefine),
            };
            for (int i = 0; i < targetCheckerList.Count; i++)
            {
                inputs.Add(getOrCreateParamsValueInput(targetConditionPortDefine, i));
            }
            inputs.Add(getOrCreateInputPort(tagsPortDefine));
            inputs.Add(getOrCreateInputPort(priorityPortDefine));

            foreach (var lostPort in inputList.Except(inputs))
            {
                graph.disconnectAll(lostPort);
            }
            inputList.Clear();
            inputList.AddRange(inputs);
        }
        private void DefinitionOutputs()
        {
            ValueOutput valueOutput(PortDefine portDefine)
                 => outputList.OfType<ValueOutput>().FirstOrDefault(d => d != null && d.define.Equals(portDefine)) ??
                 new ValueOutput(this, portDefine);
            ControlOutput controlOutput()
                 => outputList.OfType<ControlOutput>().FirstOrDefault() ??
                 new ControlOutput(this, actionPortDefine);


            var outputs = new List<IPort>()
            {
                controlOutput(),
                valueOutput(playerPortDefine),
                valueOutput(cardPortDefine),
                valueOutput(targetsPortDefine)
            };

            foreach (var lostPort in outputList.Except(outputs))
            {
                graph.disconnectAll(lostPort);
            }
            outputList.Clear();
            outputList.AddRange(outputs);
        }

        #endregion
        public List<TargetChecker> targetCheckerList { get; private set; } = new List<TargetChecker>();

        public const string conditionPortName = "condition";
        public const string targetConditionName = "targetCondition";
        public const string actionPortName = "action";
        public const string playerPortName = EffectTriggerEventDefine.VAR_PLAYER;
        public const string cardPortName = EffectTriggerEventDefine.VAR_CARD;
        public const string targetsPortName = EffectTriggerEventDefine.VAR_TARGETS;
        public const string tagsPortName = "tags";
        public const string priorityPortName = "priority";

        private static PortDefine conditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, Translations.EDITOR_PARAM_NAME_TRIGGER_CONDITION);
        private static PortDefine targetConditionPortDefine = PortDefine.Value(typeof(bool), targetConditionName, Translations.EDITOR_PARAM_NAME_TARGET_CONDITION);
        private static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        private static PortDefine playerPortDefine = PortDefine.Value(typeof(Player), playerPortName, Translations.EDITOR_PARAM_NAME_PLAYER);
        private static PortDefine cardPortDefine = PortDefine.Value(typeof(Card), cardPortName, Translations.EDITOR_PARAM_NAME_CARD);
        private static PortDefine targetsPortDefine = PortDefine.Value(typeof(Card[]), targetsPortName, Translations.EDITOR_PARAM_NAME_TARGET_CARD);
        private static PortDefine tagsPortDefine = PortDefine.Value(typeof(EffectTagCollection), tagsPortName, Translations.EDITOR_PARAM_NAME_TAG_LIST, isConst: true);
        private static PortDefine priorityPortDefine = PortDefine.Value(typeof(int), priorityPortName, Translations.EDITOR_PARAM_NAME_PRIORITY, isConst: true);
    }
}