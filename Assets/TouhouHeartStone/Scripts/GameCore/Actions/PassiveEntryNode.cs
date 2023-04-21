using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;

namespace TouhouCardEngine
{
    public class PassiveEntryNode : Node
    {
        #region 公有方法
        public PassiveEntryNode()
        {
            inputList.Add(new ValueInput(this, triggerConditionPortDefine));
            outputList.Add(new ControlOutput(this, actionPortDefine));
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
        public ValueInput getTriggerCondtionValuePort()
        {
            return getInputPort<ValueInput>(conditionPortName);
        }
        public ControlOutput getActionOutputPort()
        {
            return outputList?.FirstOrDefault() as ControlOutput;
        }
        public void setTags(params string[] value)
        {
            var tags = getConst<List<string>>(tagsConstName);
            if (tags == null)
            {
                constList.Add(tagsConstName, new List<string>(value));
                return;
            }
            tags.Clear();
            tags.AddRange(tags);
        }


        private void DefinitionConditions()
        {
            ValueInput valueCondition()
                 => inputList.OfType<ValueInput>().FirstOrDefault(d => d != null && d.define.Equals(triggerConditionPortDefine)) ??
                 new ValueInput(this, triggerConditionPortDefine);


            var inputs = new List<IPort> { valueCondition() };

            foreach (var lostPort in inputList.Except(inputs))
            {
                graph.disconnectAll(lostPort);
            }
            inputList.Clear();
            inputList.AddRange(inputs);
        }
        private void DefinitionOutputs()
        {
            var outputs = new List<IPort>()
            {
                DefineControlOutput()
            };
            foreach (PortDefine portDefine in GetValueOutputDefines())
            {
                outputs.Add(DefineValueOutput(portDefine));
            }

            foreach (var lostPort in outputList.Except(outputs))
            {
                graph.disconnectAll(lostPort);
            }
            outputList.Clear();
            outputList.AddRange(outputs);
        }
        protected virtual IEnumerable<PortDefine> GetValueOutputDefines()
        {
            yield return playerPortDefine;
            yield return cardPortDefine;
            yield break;
        }
        protected ValueOutput DefineValueOutput(PortDefine portDefine)
        {
            var existed = outputList.OfType<ValueOutput>().FirstOrDefault(d => d != null && d.define.Equals(portDefine));
            return existed ?? new ValueOutput(this, portDefine);
        }
        protected ControlOutput DefineControlOutput()
        {
            var exists = outputList.OfType<ControlOutput>().FirstOrDefault();
            return exists ?? new ControlOutput(this, actionPortDefine);
        }
        public override ISerializableNode ToSerializableNode()
        {
            return new SerializablePassiveEntryNode(this);
        }

        #endregion
        public IEnumerable<string> tags => getConst<List<string>>(tagsConstName);

        public const string conditionPortName = "condition";
        public const string actionPortName = "action";
        public const string playerPortName = "player";
        public const string cardPortName = "card";
        public const string tagsConstName = "tags";
        protected static PortDefine triggerConditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, "触发条件");
        protected static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        protected static PortDefine playerPortDefine = PortDefine.Value(typeof(Player), playerPortName, "Player");
        protected static PortDefine cardPortDefine = PortDefine.Value(typeof(Card), cardPortName, "Card");
    }
    [Serializable]
    public class SerializablePassiveEntryNode : ISerializableNode
    {
        public SerializablePassiveEntryNode(PassiveEntryNode eventEntryNode)
        {
            id = eventEntryNode.id;
            posX = eventEntryNode.posX;
            posY = eventEntryNode.posY;
            tags = eventEntryNode.tags.ToArray();
        }
        public PassiveEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new PassiveEntryNode()
            {
                id = id,
                posX = posX,
                posY = posY
            };
            entry.setTags(tags);
            entry.graph = graph;
            return entry;
        }
        Node ISerializableNode.ToActionNode(ActionGraph graph) => ToActionNode(graph);

        public int id;
        public float posX;
        public float posY;
        public string[] tags;
    }
}