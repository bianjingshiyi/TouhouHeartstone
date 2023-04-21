using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;

namespace TouhouCardEngine
{
    public class ActiveEntryNode : Node, ITargetCheckerNode
    {
        #region 公有方法
        public ActiveEntryNode(int id)
        {
            this.id = id;
            inputList.Add(new ValueInput(this, triggerConditionPortDefine));
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
            var tags = getConst<EffectTagCollection>(tagsConstName);
            if (tags == null)
            {
                tags = new EffectTagCollection();
                constList.Add(tagsConstName, tags);
            }
            tags.Clear();
            tags.AddRange(value);
        }
        public void AddTargetChecker(TargetChecker checker)
        {
            checker.node = this;
            targetCheckerList.Add(checker);
            DefinitionConditions();
        }
        public void RemoveTargetChecker(int index)
        {
            var checker = targetCheckerList[index];
            checker.node = null;
            targetCheckerList.Remove(checker);
            DefinitionConditions();
        }
        public override ISerializableNode ToSerializableNode()
        {
            return new SerializableActiveEntryNode(this);
        }


        private void DefinitionConditions()
        {
            ValueInput valueCondition()
                 => inputList.OfType<ValueInput>().FirstOrDefault(d => d != null && d.define.Equals(triggerConditionPortDefine)) ??
                 new ValueInput(this, triggerConditionPortDefine);
            ValueInput targetCondition(int paramIndex)
                 => inputList.OfType<ValueInput>().FirstOrDefault(d => d != null && d.define.Equals(targetConditionPortDefine) && d.paramIndex == paramIndex) ??
                 new ValueInput(this, targetConditionPortDefine, paramIndex);


            var inputs = new List<IPort> { valueCondition() };
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
        public IEnumerable<string> tags => getConst<EffectTagCollection>(tagsConstName);

        public const string conditionPortName = "condition";
        public const string targetConditionName = "targetCondition";
        public const string actionPortName = "action";
        public const string playerPortName = "玩家";
        public const string cardPortName = "卡牌";
        public const string targetsPortName = "目标卡牌";
        public const string tagsConstName = "tags";
        private static PortDefine targetConditionPortDefine = PortDefine.Value(typeof(bool), targetConditionName, "目标{0}条件");
        private static PortDefine triggerConditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, "触发条件");
        private static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        private static PortDefine playerPortDefine = PortDefine.Value(typeof(Player), playerPortName, "Player");
        private static PortDefine cardPortDefine = PortDefine.Value(typeof(Card), cardPortName, "Card");
        private static PortDefine targetsPortDefine = PortDefine.Value(typeof(Card[]), targetsPortName, "TargetCard");
    }
    public class SerializableActiveEntryNode : ISerializableNode
    {
        public SerializableActiveEntryNode(ActiveEntryNode activeEntry)
        {
            id = activeEntry.id;
            tags = activeEntry.tags.ToArray();
            targetCheckerList = activeEntry.targetCheckerList.ConvertAll(t => new SerializableTargetChecker(t));
            posX = activeEntry.posX;
            posY = activeEntry.posY;
        }
        public ActiveEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new ActiveEntryNode(id)
            {
                posX = posX,
                posY = posY,
            };
            entry.graph = graph;
            entry.setTags(tags);
            foreach (var seri in targetCheckerList)
            {
                var checker = seri.toTargetChecker();
                entry.AddTargetChecker(checker);
            }
            return entry;
        }
        Node ISerializableNode.ToActionNode(ActionGraph graph) => ToActionNode(graph);

        public string[] tags;
        public int id;
        public float posX;
        public float posY;
        public List<SerializableTargetChecker> targetCheckerList = new List<SerializableTargetChecker>();
    }
}