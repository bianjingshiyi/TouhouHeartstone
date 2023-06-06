using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TouhouCardEngine
{
    public class AuraEntryNode : Node
    {
        #region 公有方法
        public AuraEntryNode()
        {
            inputList.Add(new ValueInput(this, buffDefinePortDefine));
            inputList.Add(new ValueInput(this, conditionPortDefine));
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
        public ValueInput getTriggerCondtionValuePort()
        {
            return getInputPort<ValueInput>(conditionPortName);
        }
        public ControlOutput getActionOutputPort()
        {
            return outputList?.FirstOrDefault() as ControlOutput;
        }
        public void setPiles(params string[] value)
        {
            var piles = getConst<PileNameCollection>(pileListName);
            if (piles == null)
            {
                piles = new PileNameCollection();
                constList.Add(pileListName, piles);
            }
            piles.Clear();
            piles.AddRange(value);
        }
        public PileNameCollection getPiles()
        {
            var piles = getConst<PileNameCollection>(pileListName);
            if (piles == null)
            {
                piles = new PileNameCollection();
                constList.Add(pileListName, piles);
            }
            return piles;
        }
        public void setRanges(params string[] value)
        {
            var piles = getConst<PileNameCollection>(rangeListName);
            if (piles == null)
            {
                piles = new PileNameCollection();
                constList.Add(rangeListName, piles);
            }
            piles.Clear();
            piles.AddRange(value);
        }
        public PileNameCollection getRanges()
        {
            var piles = getConst<PileNameCollection>(rangeListName);
            if (piles == null)
            {
                piles = new PileNameCollection();
                constList.Add(rangeListName, piles);
            }
            return piles;
        }
        public override ISerializableNode ToSerializableNode()
        {
            return new SerializableAuraEntryNode(this);
        }

        #endregion

        public const string conditionPortName = "condition";
        public const string actionPortName = "action";
        public const string pileListName = "pileList";
        public const string rangeListName = "rangeList";
        public const string conditionName = "condition";
        public const string buffDefineName = "buffDefine";
        public const string targetCardName = "targetCard";
        public const string targetBuffName = "targetBuff";
        protected static PortDefine conditionPortDefine = PortDefine.Value(typeof(bool), conditionPortName, "作用条件");
        protected static PortDefine buffDefinePortDefine = PortDefine.Value(typeof(BuffDefine), buffDefineName, "增益定义");
        protected static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        protected static PortDefine targetCardPortDefine = PortDefine.Value(typeof(Card), targetCardName, "目标卡牌");
        protected static PortDefine targetBuffPortDefine = PortDefine.Value(typeof(Buff), targetBuffName, "目标增益");
    }


    [Serializable]
    public class SerializableAuraEntryNode : ISerializableNode
    {
        public SerializableAuraEntryNode(AuraEntryNode entryNode)
        {
            id = entryNode.id;
            posX = entryNode.posX;
            posY = entryNode.posY;
            piles = entryNode.getPiles().ToArray();
            ranges = entryNode.getRanges()?.ToArray();
        }
        public AuraEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new AuraEntryNode()
            {
                id = id,
                posX = posX,
                posY = posY
            };
            entry.setPiles(piles);
            entry.setRanges(ranges);
            entry.graph = graph;
            return entry;
        }
        Node ISerializableNode.ToActionNode(ActionGraph graph) => ToActionNode(graph);

        public int id;
        public float posX;
        public float posY;
        public string[] piles;
        public string[] ranges;
    }
}