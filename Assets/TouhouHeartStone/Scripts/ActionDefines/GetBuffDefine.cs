using System.Collections.Generic;
using System.Threading.Tasks;
using Game;

namespace TouhouCardEngine
{
    public class GetBuffDefineAction : ActionDefine
    {
        public GetBuffDefineAction() : base(BuiltinActionDefineID.GetBuffDefine, defName, defName, "GetBuffDefine")
        {
            type = NodeDefineType.Function;
            _inputs = new PortDefine[]
            {
                PortDefine.Value(typeof(DefineReference), constName, "BuffDefine", isConst: true)
            };
            _outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(BuffDefine), outputName, "BuffDefine")
            };
            category = "Buff";
        }
        private PortDefine[] _inputs;
        private PortDefine[] _outputs;
        public override async Task<ControlOutput> run(Flow flow, Node node)
        {
            CardEngine eng = flow.env.game;
            var defRef = await flow.getValue<DefineReference>(node.getInputPort<ValueInput>(constName));
            var buffDefine = eng.getBuffDefine(defRef.cardPoolId, defRef.defineId);
            flow.setValue(node.getOutputPort<ValueOutput>(outputName), buffDefine);

            return null;
        }
        public const string constName = "buffDefine";
        public const string outputName = "value";
        public const string defName = "获取增益定义";
        public override IEnumerable<PortDefine> inputDefines => _inputs;
        public override IEnumerable<PortDefine> outputDefines => _outputs;
    }
}