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
            inputList.Add(new ValueInput(this, triggerConditionPortDefine));
            inputList.Add(new ValueInput(this, tagsPortDefine));
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
        public int getTargetCheckerIndex(TargetChecker checker)
        {
            return targetCheckerList.IndexOf(checker);
        }
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
            return getInputDefaultValue<EffectTagCollection>(tagsPortName).ToArray();
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
        public override SerializableNode ToSerializableNode()
        {
            return new SerializableActiveEntryNode(this);
        }


        private void DefinitionConditions()
        {
            ValueInput valueCondition()
                 => inputList.OfType<ValueInput>().FirstOrDefault(d => d != null && d.define.Equals(triggerConditionPortDefine)) ??
                 new ValueInput(this, triggerConditionPortDefine);
            ValueInput valueTags()
                 => inputList.OfType<ValueInput>().FirstOrDefault(d => d != null && d.define.Equals(tagsPortDefine)) ??
                 new ValueInput(this, tagsPortDefine);
            ValueInput targetCondition(int paramIndex)
                 => inputList.OfType<ValueInput>().FirstOrDefault(d => d != null && d.define.Equals(targetConditionPortDefine) && d.paramIndex == paramIndex) ??
                 new ValueInput(this, targetConditionPortDefine, paramIndex);


            var inputs = new List<IPort> { valueCondition(), valueTags() };
            for (int i = 0; i < targetCheckerList.Count; i++)
            {
                inputs.Add(targetCondition(i));
            }

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
        public const string playerPortName = "玩家";
        public const string cardPortName = "卡牌";
        public const string targetsPortName = "目标卡牌";
        public const string tagsPortName = "tags";
        private static PortDefine targetConditionPortDefine = PortDefine.Value(typeof(bool), targetConditionName, "目标{0}条件");
        private static PortDefine triggerConditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, "触发条件");
        private static PortDefine tagsPortDefine = PortDefine.Value(typeof(EffectTagCollection), tagsPortName, "标签列表", isConst: true);
        private static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        private static PortDefine playerPortDefine = PortDefine.Value(typeof(Player), playerPortName, "Player");
        private static PortDefine cardPortDefine = PortDefine.Value(typeof(Card), cardPortName, "Card");
        private static PortDefine targetsPortDefine = PortDefine.Value(typeof(Card[]), targetsPortName, "TargetCard");
    }
    public class SerializableActiveEntryNode : SerializableNode
    {
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
            var defaultValues = inputDefaultValues ?? consts;
            InitNode(entry, defaultValues, graph);
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

        [Obsolete]
        public string[] tags;
        [Obsolete]
        public Dictionary<string, object> consts;
        public List<SerializableTargetChecker> targetCheckerList = new List<SerializableTargetChecker>();
    }
}