using System.Collections.Generic;
using System.Threading.Tasks;
using Game;
namespace TouhouCardEngine
{
    public class IntegerConstActionDefine : ActionDefine
    {
        public IntegerConstActionDefine() : base(BuiltinActionDefineID.IntegerConst, defName, defName)
        {
            type = NodeDefineType.Function;
            inputs = new PortDefine[0];
            consts = new PortDefine[1]
            {
                PortDefine.Const(typeof(int), "value")
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(int), "result", "value")
            };
            category = EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER;
        }
        public override Task<ControlOutput> run(Flow flow, Node node)
        {
            flow.setValue(node.getOutputPort<ValueOutput>("result"), node.getConst<int>("value"));
            return Task.FromResult(node.getOutputPort<ControlOutput>(exitPortName));
        }
        public const string defName = "IntegerConst";
        private PortDefine[] inputs { get; }
        private PortDefine[] consts { get; }
        private PortDefine[] outputs { get; }

        public override IEnumerable<PortDefine> inputDefines => inputs;

        public override IEnumerable<PortDefine> constDefines => consts;

        public override IEnumerable<PortDefine> outputDefines => outputs;
    }
}