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
            inputs = new PortDefine[1]
            {
                PortDefine.Value(typeof(int), "value", isConst: true)
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(int), "result", "value")
            };
            category = EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER;
        }
        public override async Task<ControlOutput> run(Flow flow, Node node)
        {
            var value = await flow.getValue<int>(node.getInputPort<ValueInput>("value"));
            flow.setValue(node.getOutputPort<ValueOutput>("result"), value);
            return node.getOutputPort<ControlOutput>(exitPortName);
        }
        public const string defName = "IntegerConst";
        private PortDefine[] inputs { get; }
        private PortDefine[] outputs { get; }

        public override IEnumerable<PortDefine> inputDefines => inputs;
        public override IEnumerable<PortDefine> outputDefines => outputs;
    }
}