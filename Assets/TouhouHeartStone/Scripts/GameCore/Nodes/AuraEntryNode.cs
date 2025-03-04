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
            var piles = getPiles();
            piles.Clear();
            if (value != null)
                piles.AddRange(value);
        }
        public PileNameRange getPiles()
        {
            var piles = getInputDefaultValue<PileNameCollection>(pileListPortName);
            if (piles is not PileNameRange pileNameRange)
            {
                pileNameRange = piles is IEnumerable<string> strings ? new PileNameRange(strings) : new PileNameRange();
                setInputDefaultValue(pileListPortName, pileNameRange);
            }
            return pileNameRange;
        }
        #endregion

        #region 作用区域
        public void setRanges(params string[] value)
        {
            var ranges = getRanges();
            ranges.Clear();
            if (value != null)
                ranges.AddRange(value);
        }
        public PileNameRange getRanges()
        {
            var ranges = getInputDefaultValue<PileNameCollection>(rangeListPortName);
            if (ranges is not PileNameRange pileNameRange)
            {
                pileNameRange = ranges is IEnumerable<string> strings ? new PileNameRange(strings) : new PileNameRange();
                setInputDefaultValue(rangeListPortName, pileNameRange);
            }
            return pileNameRange;
        }
        #endregion
        public override Node Clone()
        {
            AuraEntryNode newNode = new AuraEntryNode();
            newNode.id = id;
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

        public const string conditionPortName = "condition";
        public const string actionPortName = "action";
        public const string pileListPortName = "pileList";
        public const string rangeListPortName = "rangeList";
        public const string conditionName = "condition";
        public const string buffDefineName = "buffDefine";
        public const string targetCardName = "targetCard";
        public const string targetBuffName = "targetBuff";

        protected static PortDefine conditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, Translations.EDITOR_PARAM_NAME_AFFECT_CONDITION);
        protected static PortDefine buffDefinePortDefine = PortDefine.Value(typeof(BuffDefine), buffDefineName, Translations.EDITOR_PARAM_NAME_BUFF_DEFINE);
        protected static PortDefine pileListPortDefine = PortDefine.Value(typeof(PileNameRange), pileListPortName, Translations.EDITOR_PARAM_NAME_ACTIVE_PILES, isConst: true);
        protected static PortDefine rangeListPortDefine = PortDefine.Value(typeof(PileNameRange), rangeListPortName, Translations.EDITOR_PARAM_NAME_AFFECT_PILES, isConst: true);
        protected static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        protected static PortDefine targetCardPortDefine = PortDefine.Value(typeof(Card), targetCardName, Translations.EDITOR_PARAM_NAME_TARGET_CARD);
        protected static PortDefine targetBuffPortDefine = PortDefine.Value(typeof(Buff), targetBuffName, Translations.EDITOR_PARAM_NAME_TARGET_BUFF);
    }
}