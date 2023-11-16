using System.Collections.Generic;
using System.Threading.Tasks;
using Game;

namespace TouhouCardEngine
{
    public class BooleanConstActionDefine : ActionDefine
    {
        public BooleanConstActionDefine() : base(BuiltinActionDefineID.BooleanConst, defName, defName)
        {
            type = NodeDefineType.Function;
            inputs = new PortDefine[0];
            consts = new PortDefine[1]
            {
                PortDefine.Const(typeof(bool), "value", "Value")
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(bool), "return", "Value")
            };
            category = EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER;
        }

        public override Task<ControlOutput> run(Flow flow, Node node)
        {
            flow.setValue(node.getOutputPort<ValueOutput>("return"), node.getConst<bool>("value"));
            return Task.FromResult(node.getOutputPort<ControlOutput>(exitPortName));
        }
        public const string defName = "BooleanConst";

        public override IEnumerable<PortDefine> inputDefines => inputs;
        public override IEnumerable<PortDefine> constDefines => consts;
        public override IEnumerable<PortDefine> outputDefines => outputs;
        private PortDefine[] inputs { get; }
        private PortDefine[] consts { get; }
        private PortDefine[] outputs { get; }
    }
}