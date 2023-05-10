using System.Collections.Generic;
using System.Threading.Tasks;

namespace TouhouCardEngine
{
    public class EventEffectData : ActionDefine
    {
        public EventEffectData() : base(defName)
        {
            _inputs = new PortDefine[0];
            _consts = new PortDefine[]
            {
                PortDefine.Value(typeof(PileNameCollection), pileListName, "PileList")
            };
            _outputs = new PortDefine[0];
        }
        public override Task<ControlOutput> run(Flow flow, Node node)
        {
            return Task.FromResult<ControlOutput>(null);
        }

        private PortDefine[] _inputs;
        private PortDefine[] _consts;
        private PortDefine[] _outputs;
        public const string pileListName = "pileList";
        public const string defName = "EventEffectData";
        public override IEnumerable<PortDefine> inputDefines => _inputs;
        public override IEnumerable<PortDefine> constDefines => _consts;
        public override IEnumerable<PortDefine> outputDefines => _outputs;
    }
}