using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game;
namespace TouhouCardEngine
{
    public class WhereActionDefine : ActionDefine
    {
        public WhereActionDefine() : base(BuiltinActionDefineID.Where, StringTable.EDITOR_NODE_NAME_WHERE)
        {
            type = NodeDefineType.Function;
            inputs = new PortDefine[]
            {
                PortDefine.Value(typeof(object[]), "array", StringTable.EDITOR_PARAM_NAME_COLLECTION),
                PortDefine.Value(typeof(NodeValueRef<bool>), "condition", StringTable.EDITOR_PARAM_NAME_CONDITION)
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(object), "element", StringTable.EDITOR_PARAM_NAME_ELEMENT),
                PortDefine.Value(typeof(object[]), "return", EffectEditorLibrary.EDITOR_PARAM_NAME_ARRAY_MATCHED),
            };
            category = StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION;
        }
        public override async Task<ControlOutput> run(Flow flow, Node node)
        {
            var result = await runAction(flow, node);
            flow.setValue(node.getOutputPort<ValueOutput>("return"), result);
            return node.getOutputPort<ControlOutput>(exitPortName);
        }
        private async Task<object[]> runAction(Flow flow, Node node)
        {
            var arrayPort = node.getInputPort<ValueInput>("array");
            var array = await flow.getValue<object[]>(arrayPort);
            if (array == null)
                return Array.Empty<object>();

            var conditionPort = node.getInputPort<ValueInput>("condition");
            var elementPort = node.getOutputPort<ValueOutput>("element");
            ValueOutput conditionOutput = conditionPort?.getConnectedOutputPort();
            if (conditionOutput == null)
                return array;
            var condition = NodeValueRef.FromType<bool>(conditionOutput);

            List<object> objList = new List<object>();

            foreach (var obj in array)
            {
                flow.setValue(elementPort, obj);
                flow.EnterScope();
                bool result = await condition.getValue(flow);
                flow.ExitScope();

                if (result)
                {
                    objList.Add(obj);
                }
            }
            return objList.ToArray();
        }
        public const string defName = "Where";
        public PortDefine[] inputs { get; }
        public PortDefine[] outputs { get; }

        public override IEnumerable<PortDefine> inputDefines => inputs;
        public override IEnumerable<PortDefine> outputDefines => outputs;
    }
}