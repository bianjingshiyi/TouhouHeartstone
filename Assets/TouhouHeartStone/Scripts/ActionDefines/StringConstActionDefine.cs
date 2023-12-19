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
            inputs = new PortDefine[]
            {
                PortDefine.Value(typeof(string), "value", "Value", isConst: true)
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(string), "value", "Value")
            };
            obsoleteMsg = "";
            category = EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER;
        }
        public override async Task<ControlOutput> run(Flow flow, Node node)
        {
            var valuePort = node.getInputPort<ValueInput>("value");
            var value = await flow.getValue<string>(valuePort) ?? string.Empty;
            flow.setValue(node.getOutputPort<ValueOutput>("value"), value);
            return node.getOutputPort<ControlOutput>(exitPortName);
        }
        public const string defName = "StringConst";
        public PortDefine[] inputs { get; }
        public PortDefine[] outputs { get; }

        public override IEnumerable<PortDefine> inputDefines => inputs;

        public override IEnumerable<PortDefine> outputDefines => outputs;
    }
}