using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;

namespace TouhouCardEngine
{
    public class AuraDataDefine : ActionDefine
    {
        public AuraDataDefine() : base(defName)
        {
            _inputs = new PortDefine[2]
            {
                PortDefine.Value(typeof(BuffDefine), buffDefineName, "BuffDefine"),
                PortDefine.Value(typeof(bool), conditionName, "AffectCondition")
            };
            _consts = new PortDefine[]
            {
                PortDefine.Value(typeof(PileNameCollection), pileListName, "PileList"),
                PortDefine.Value(typeof(PileNameCollection), rangeListName, "RangeList")
            };
            _outputs = new PortDefine[]
            {
                PortDefine.Control(updateActionName, string.Empty),
                PortDefine.Value(typeof(Card), targetCardName, "TargetCard"),
                PortDefine.Value(typeof(Buff), targetBuffName, "TargetBuff")
            };
            hideDocument = true;
            disableMenu = true;
            disableCreate = true;
        }
        public override Task<ControlOutput> run(Flow flow, Node node)
        {
            return Task.FromResult(node.getOutputPort<ControlOutput>(updateActionName));
        }
        public const string pileListName = "pileList";
        public const string rangeListName = "rangeList";
        public const string conditionName = "condition";
        public const string buffDefineName = "buffDefine";
        public const string updateActionName = "updateAction";
        public const string targetCardName = "targetCard";
        public const string targetBuffName = "targetBuff";
        public const string defName = "AuraData";
        private PortDefine[] _inputs;
        private PortDefine[] _consts;
        private PortDefine[] _outputs;
        public override IEnumerable<PortDefine> inputDefines => _inputs;
        public override IEnumerable<PortDefine> constDefines => _consts;
        public override IEnumerable<PortDefine> outputDefines => _outputs;
    }
}