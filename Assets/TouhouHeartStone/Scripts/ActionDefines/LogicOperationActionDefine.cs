using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
namespace TouhouCardEngine
{
    public class LogicOperationActionDefine : ActionDefine
    {
        public LogicOperationActionDefine() : base((int)BuiltinActionDefineID.LogicOperation, "LogicOperation")
        {
            inputs = new PortDefine[]
            {
                enterPortDefine,
                PortDefine.Value(typeof(bool), "value", isParams: true)
            };
            consts = new PortDefine[1]
            {
                PortDefine.Const(typeof(LogicOperator), "operator")
            };
            outputs = new PortDefine[]
            {
                exitPortDefine,
                PortDefine.Value(typeof(bool), "result")
            };
            obsoleteNames = new string[] { "LogicOperation" };
        }
        public override async Task<ControlOutput> run(Flow flow, Node node)
        {
            var opObj = node.getConst("operator");
            LogicOperator op;

            if (opObj == null)
                // 因为LogicOperator的默认值是not（为0），所以在常量值为null时，将操作符默认值设置为not。
                op = LogicOperator.not;
            else if (opObj is LogicOperator lgcOp)
                op = lgcOp;
            else if (opObj is int enumValue)
                op = (LogicOperator)enumValue;
            else
                op = LogicOperator.and;

            var argPorts = node.getParamInputPorts("value").Select(port => port.getConnectedOutputPort()).Where(p => p != null).ToArray();
            bool result = false;
            if (argPorts != null && argPorts.Length > 0)
            {
                switch (op)
                {
                    case LogicOperator.not:
                        result = !await flow.getValue<bool>(argPorts[0]);
                        break;
                    case LogicOperator.and:
                        result = true;
                        foreach (var port in argPorts)
                        {
                            if (!await flow.getValue<bool>(port))
                            {
                                result = false;
                                break;
                            }
                        }
                        break;
                    case LogicOperator.or:
                        result = false;
                        foreach (var port in argPorts)
                        {
                            if (await flow.getValue<bool>(port))
                            {
                                result = true;
                                break;
                            }
                        }
                        break;
                    default:
                        throw new InvalidOperationException("未知的操作符" + op);
                }
            }
            flow.setValue(node.getOutputPort<ValueOutput>("result"), result);
            return node.getOutputPort<ControlOutput>(exitPortName);
        }
        public PortDefine[] inputs { get; }
        public PortDefine[] consts { get; }
        public PortDefine[] outputs { get; }

        public override IEnumerable<PortDefine> inputDefines => inputs;

        public override IEnumerable<PortDefine> constDefines => consts;

        public override IEnumerable<PortDefine> outputDefines => outputs;
    }
    public enum LogicOperator
    {
        not,
        and,
        or
    }
}