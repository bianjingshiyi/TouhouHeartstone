using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
namespace TouhouCardEngine
{
    public class IntegerOperationActionDefine : ActionDefine
    {
        public IntegerOperationActionDefine() : base(BuiltinActionDefineID.IntegerOperation, defName, defName)
        {
            type = NodeDefineType.Function;
            inputs = new PortDefine[]
            {
                PortDefine.Value(typeof(IntegerOperator), "operator", isConst: true),
                PortDefine.Value(typeof(int), paramName, "Value", isParams: true),
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(int), resultName, "Value")
            };
            category = EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER;
        }
        public override async Task<ControlOutput> run(Flow flow, Node node)
        {
            var operatorPort = node.getInputPort<ValueInput>("operator");
            var opObj = await flow.getValue(operatorPort);
            IntegerOperator op;

            if (opObj == null)
                op = IntegerOperator.add;
            else if (opObj is IntegerOperator intOp)
                op = intOp;
            else if (opObj is int enumValue)
                op = (IntegerOperator)enumValue;
            else
                op = IntegerOperator.add;

            var ports = node.getParamInputPorts(paramName);

            List<int> numList = new List<int>();
            for (int i = 0; i < ports.Length - 1; i++)
            {
                var port = ports[i];
                var value = await flow.getValue<int>(port);
                numList.Add(value);
            }
            var numbers = numList.ToArray();

            int calced;
            switch (op)
            {
                case IntegerOperator.add:
                    calced = numbers.Sum();
                    break;
                case IntegerOperator.sub:
                    calced = numbers[0];
                    for (int i = 1; i < numbers.Length; i++)
                    {
                        calced -= numbers[i];
                    }
                    break;
                case IntegerOperator.mul:
                    calced = 1;
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        calced *= numbers[i];
                    }
                    break;
                case IntegerOperator.div:
                    calced = numbers[0];
                    for (int i = 1; i < numbers.Length; i++)
                    {
                        calced /= numbers[i];
                    }
                    break;
                case IntegerOperator.mod:
                    calced = numbers[0];
                    for (int i = 1; i < numbers.Length; i++)
                    {
                        calced %= numbers[i];
                    }
                    break;
                default:
                    throw new InvalidOperationException($"未知的操作符{op}");
            }

            var output = node.getOutputPort<ValueOutput>(resultName);
            flow.setValue(output, calced);
            return node.getOutputPort<ControlOutput>(exitPortName);
        }
        public PortDefine[] inputs { get; }
        public PortDefine[] outputs { get; }
        public const string paramName = "arg";
        public const string resultName = "result";
        public const string defName = "IntegerOperation";
        public override IEnumerable<PortDefine> inputDefines => inputs;
        public override IEnumerable<PortDefine> outputDefines => outputs;
    }
    public enum IntegerOperator
    {
        add,
        sub,
        mul,
        div,
        mod
    }
}