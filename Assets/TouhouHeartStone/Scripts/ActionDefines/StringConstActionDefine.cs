using System.Collections.Generic;
using System.Threading.Tasks;
using Game;

namespace TouhouCardEngine
{
    public class StringConstActionDefine : ActionDefine
    {
        public StringConstActionDefine() : base(BuiltinActionDefineID.StringConst, defName, defName)
        {
            type = NodeDefineType.Function;
            inputs = new PortDefine[0];
            consts = new PortDefine[1]
            {
                PortDefine.Const(typeof(string), "value", "Value")
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(string), "value", "Value")
            };
            category = EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER;
        }
        public override Task<ControlOutput> run(Flow flow, Node node)
        {
            flow.setValue(node.getOutputPort<ValueOutput>("value"), node.getConst<string>("value") ?? string.Empty);
            return Task.FromResult(node.getOutputPort<ControlOutput>(exitPortName));
        }
        public const string defName = "StringConst";
        public PortDefine[] inputs { get; }
        public PortDefine[] consts { get; }
        public PortDefine[] outputs { get; }

        public override IEnumerable<PortDefine> inputDefines => inputs;

        public override IEnumerable<PortDefine> constDefines => consts;

        public override IEnumerable<PortDefine> outputDefines => outputs;
    }
}