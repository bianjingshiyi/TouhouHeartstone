using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace TouhouCardEngine
{
    public class PassiveEntryNode : Node, IEntryNode
    {
        #region 公有方法
        public PassiveEntryNode()
        {
            inputList.Add(new ValueInput(this, conditionPortDefine));
            inputList.Add(new ValueInput(this, tagsPortDefine));
            inputList.Add(new ValueInput(this, pileListPortDefine));
            inputList.Add(new ValueInput(this, priorityPortDefine));
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
        #region 获取端口
        public ValueInput getTriggerCondtionValuePort()
        {
            return getInputPort<ValueInput>(conditionPortName);
        }
        public ValueInput getTagsPort()
        {
            return getInputPort<ValueInput>(tagsPortName);
        }
        public ValueInput getPilesPort()
        {
            return getInputPort<ValueInput>(pileListPortName);
        }
        public ControlOutput getActionOutputPort()
        {
            return getOutputPort<ControlOutput>(actionPortName);
        }
        public ControlOutput getEnableActionPort()
        {
            return getOutputPort<ControlOutput>(enableActionName);
        }
        public ControlOutput getDisableActionPort()
        {
            return getOutputPort<ControlOutput>(disableActionName);
        }
        #endregion

        #region 标签
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
        #endregion

        #region 生效区域
        public void setPiles(params string[] value)
        {
            var tags = getInputDefaultValue<PileNameCollection>(pileListPortName);
            if (tags == null)
            {
                tags = new PileNameCollection();
                setInputDefaultValue(pileListPortName, tags);
            }
            tags.Clear();
            if (value != null)
                tags.AddRange(value);
        }
        public PileNameCollection getPiles()
        {
            return getInputDefaultValue<PileNameCollection>(pileListPortName);
        }
        #endregion

        #region 优先级
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
                getOrCreateInputPort(tagsPortDefine),
                getOrCreateInputPort(pileListPortDefine),
                getOrCreateInputPort(priorityPortDefine),
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
            yield return enableActionPortDefine;
            yield return disableActionPortDefine;
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
        public override SerializableNode ToSerializableNode()
        {
            return new SerializablePassiveEntryNode(this);
        }

        #endregion

        public const string conditionPortName = "condition";
        public const string actionPortName = "action";
        public const string playerPortName = "玩家";
        public const string cardPortName = "卡牌";
        public const string tagsPortName = "tags";
        public const string pileListPortName = "pileList";
        public const string priorityPortName = "priority";
        public const string enableActionName = "enableAction";
        public const string disableActionName = "disableAction";

        public const string conditionPortDisplayName = Translations.EDITOR_PARAM_NAME_TRIGGER_CONDITION;
        public const string tagsPortDisplayName = Translations.EDITOR_PARAM_NAME_TAG_LIST;
        public const string pileListPortDisplayName = Translations.EDITOR_PARAM_NAME_ACTIVE_PILES;

        public static PortDefine enableActionPortDefine = PortDefine.Control(enableActionName, Translations.EDITOR_PARAM_NAME_ENABLE_ACTION);
        public static PortDefine disableActionPortDefine = PortDefine.Control(disableActionName, Translations.EDITOR_PARAM_NAME_DISABLE_ACTION);
        public static PortDefine conditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, conditionPortDisplayName);
        public static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        public static PortDefine playerPortDefine = PortDefine.Value(typeof(Player), playerPortName, Translations.EDITOR_PARAM_NAME_PLAYER);
        public static PortDefine cardPortDefine = PortDefine.Value(typeof(Card), cardPortName, Translations.EDITOR_PARAM_NAME_CARD);
        public static PortDefine tagsPortDefine = PortDefine.Value(typeof(EffectTagCollection), tagsPortName, tagsPortDisplayName, isConst: true);
        public static PortDefine pileListPortDefine = PortDefine.Value(typeof(PileNameCollection), pileListPortName, Translations.EDITOR_PARAM_NAME_ACTIVE_PILES, isConst: true);
        public static PortDefine priorityPortDefine = PortDefine.Value(typeof(int), priorityPortName, Translations.EDITOR_PARAM_NAME_PRIORITY, isConst: true);
    }
    [Serializable]
    public class SerializablePassiveEntryNode : SerializableNode
    {
        public SerializablePassiveEntryNode(PassiveEntryNode eventEntryNode) : base(eventEntryNode)
        {
        }
        public PassiveEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new PassiveEntryNode()
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
        [Obsolete]
        public string[] tags;
        [Obsolete]
        public string[] piles;
    }
}