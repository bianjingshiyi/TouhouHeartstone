using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class AuraEntryNode : Node, IEntryNode
    {
        #region 公有方法
        public AuraEntryNode()
        {
            inputList.Add(new ValueInput(this, buffDefinePortDefine));
            inputList.Add(new ValueInput(this, conditionPortDefine));
            inputList.Add(new ValueInput(this, pileListPortDefine));
            inputList.Add(new ValueInput(this, rangeListPortDefine));
            outputList.Add(new ControlOutput(this, actionPortDefine));
            outputList.Add(new ValueOutput(this, targetCardPortDefine));
            outputList.Add(new ValueOutput(this, targetBuffPortDefine));
        }
        public override Task<ControlOutput> run(Flow flow)
        {
            foreach (var output in outputPorts.OfType<ValueOutput>())
            {
                flow.setValue(output, flow.env.GetArgument(output.name));
            }
            return Task.FromResult(getOutputPort<ControlOutput>(actionPortName));
        }

        #region 获取输入端口
        public ValueInput getTriggerCondtionValuePort()
        {
            return getInputPort<ValueInput>(conditionPortName);
        }
        public ControlOutput getActionOutputPort()
        {
            return outputList?.FirstOrDefault() as ControlOutput;
        }
        public ValueInput getPilesPort()
        {
            return getInputPort<ValueInput>(pileListPortName);
        }
        #endregion

        #region 生效区域
        public void setPiles(params string[] value)
        {
            var piles = getInputDefaultValue<PileNameCollection>(pileListPortName);
            if (piles == null)
            {
                piles = new PileNameCollection();
                setInputDefaultValue(pileListPortName, piles);
            }
            piles.Clear();
            if (value != null)
                piles.AddRange(value);
        }
        public PileNameCollection getPiles()
        {
            return getInputDefaultValue<PileNameCollection>(pileListPortName);
        }
        #endregion

        #region 作用区域
        public void setRanges(params string[] value)
        {
            var ranges = getInputDefaultValue<PileNameCollection>(rangeListPortName);
            if (ranges == null)
            {
                ranges = new PileNameCollection();
                setInputDefaultValue(rangeListPortName, ranges);
            }
            ranges.Clear();
            if (value != null)
                ranges.AddRange(value);
        }
        public PileNameCollection getRanges()
        {
            return getInputDefaultValue<PileNameCollection>(rangeListPortName);
        }
        #endregion

        public override SerializableNode ToSerializableNode()
        {
            return new SerializableAuraEntryNode(this);
        }

        #endregion

        public const string conditionPortName = "condition";
        public const string actionPortName = "action";
        public const string pileListPortName = "pileList";
        public const string rangeListPortName = "rangeList";
        public const string conditionName = "condition";
        public const string buffDefineName = "buffDefine";
        public const string targetCardName = "targetCard";
        public const string targetBuffName = "targetBuff";
        protected static PortDefine conditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, "作用条件");
        protected static PortDefine buffDefinePortDefine = PortDefine.Value(typeof(BuffDefine), buffDefineName, "增益定义");
        protected static PortDefine pileListPortDefine = PortDefine.Value(typeof(PileNameCollection), pileListPortName, "生效区域", isConst: true);
        protected static PortDefine rangeListPortDefine = PortDefine.Value(typeof(PileNameCollection), rangeListPortName, "作用区域", isConst: true);
        protected static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        protected static PortDefine targetCardPortDefine = PortDefine.Value(typeof(Card), targetCardName, "目标卡牌");
        protected static PortDefine targetBuffPortDefine = PortDefine.Value(typeof(Buff), targetBuffName, "目标增益");
    }


    [Serializable]
    public class SerializableAuraEntryNode : SerializableNode
    {
        public SerializableAuraEntryNode(AuraEntryNode entryNode) : base(entryNode)
        {
        }
        public AuraEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new AuraEntryNode()
            {
                id = id,
                posX = posX,
                posY = posY
            };
            var defaultValues = inputDefaultValues ?? consts;
            InitNode(entry, defaultValues, graph);
            if (piles != null)
                entry.setPiles(piles);
            if (ranges != null)
                entry.setRanges(ranges);
            return entry;
        }
        public override Node ToNode(ActionGraph graph) => ToActionNode(graph);

        [Obsolete]
        public Dictionary<string, object> consts;
        [Obsolete]
        public string[] piles;
        [Obsolete]
        public string[] ranges;
    }
}