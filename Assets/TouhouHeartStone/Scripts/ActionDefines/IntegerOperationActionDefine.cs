using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
namespace TouhouCardEngine
{
    public class IntegerOperationActionDefine : ActionDefine
    {
        public IntegerOperationActionDefine() : base(BuiltinActionDefineID.IntegerOperation, "IntegerOperation")
        {
            type = NodeDefineType.Function;
            inputs = new PortDefine[]
            {
                PortDefine.Value(typeof(int), paramName, "Value", isParams: true)
            };
            consts = new PortDefine[]
            {
                PortDefine.Const(typeof(IntegerOperator), "operator")
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(int), resultName, "Value")
            };
            obsoleteNames = new string[] { "IntegerOperation" };
            category = EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER;
        }
        public override async Task<ControlOutput> run(Flow flow, Node node)
        {
            var opObj = node.getConst("operator");
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
            foreach (var port in ports)
            {
                if (port.getConnectedOutputPort() == null)
                    continue;
                var value = await flow.getValue<int>(port);
                numList.Add((int)value);
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
        public PortDefine[] consts { get; }
        public PortDefine[] outputs { get; }
        public const string paramName = "arg";
        public const string resultName = "result";
        public override IEnumerable<PortDefine> inputDefines => inputs;

        public override IEnumerable<PortDefine> constDefines => consts;

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