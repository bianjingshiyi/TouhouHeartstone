using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace TouhouCardEngine
{
    public class PassiveEntryNode : Node, IEntryNode
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
            var card = flow.env.card;
            flow.setValue(getOutputPort<ValueOutput>(cardPortName), card);
            flow.setValue(getOutputPort<ValueOutput>(playerPortName), card?.getOwner());
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
            var tags = getConst<EffectTagCollection>(tagsConstName);
            if (tags == null)
            {
                tags = new EffectTagCollection();
                constList.Add(tagsConstName, tags);
            }
            tags.Clear();
            if (value != null)
                tags.AddRange(value);
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
            if (value != null)
                tags.AddRange(value);
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
            var outputs = new List<IPort>();
            foreach (PortDefine portDefine in GetControlOutputDefines())
            {
                outputs.Add(DefineControlOutput(portDefine));
            }
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
        protected virtual IEnumerable<PortDefine> GetControlOutputDefines()
        {
            yield return actionPortDefine;
            yield return GeneratedPassiveEffect.enableActionPortDefine;
            yield return GeneratedPassiveEffect.disableActionPortDefine;
            yield break;
        }
        protected ValueOutput DefineValueOutput(PortDefine portDefine)
        {
            var existed = outputList.OfType<ValueOutput>().FirstOrDefault(d => d != null && d.define.Equals(portDefine));
            return existed ?? new ValueOutput(this, portDefine);
        }
        protected ControlOutput DefineControlOutput(PortDefine portDefine)
        {
            var exists = outputList.OfType<ControlOutput>().FirstOrDefault(d => d != null && d.define.Equals(portDefine));
            return exists ?? new ControlOutput(this, portDefine);
        }
        public override ISerializableNode ToSerializableNode()
        {
            return new SerializablePassiveEntryNode(this);
        }

        #endregion
        public IEnumerable<string> tags => getConst<EffectTagCollection>(tagsConstName);
        public PileNameCollection piles => getConst<PileNameCollection>(pileListName);

        public const string conditionPortName = "condition";
        public const string actionPortName = "action";
        public const string playerPortName = "玩家";
        public const string cardPortName = "卡牌";
        public const string tagsConstName = "tags";
        public const string pileListName = "pileList";
        public const string triggerConditionPortName = "触发条件";
        public static PortDefine triggerConditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, triggerConditionPortName);
        public static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
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
            consts = eventEntryNode.consts != null ? new Dictionary<string, object>(eventEntryNode.consts) : null;
        }
        public PassiveEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new PassiveEntryNode()
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
        [Obsolete]
        public string[] tags;
        [Obsolete]
        public string[] piles;
    }
}