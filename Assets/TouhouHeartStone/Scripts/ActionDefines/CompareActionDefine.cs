using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game;
namespace TouhouCardEngine
{
    public class CompareActionDefine : ActionDefine
    {
        public CompareActionDefine() : base(BuiltinActionDefineID.Compare, "Compare")
        {
            type = NodeDefineType.Function;
            inputs = new PortDefine[]
            {
                PortDefine.Value(typeof(object), "A"),
                PortDefine.Value(typeof(object), "B")
            };
            consts = new PortDefine[1]
            {
                PortDefine.Const(typeof(CompareOperator), "operator")
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(bool), "result")
            };
            obsoleteNames = new string[] { "Compare" };
            category = EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER;
        }
        public override async Task<ControlOutput> run(Flow flow, Node node)
        {
            object opObj = node.getConst("operator");
            CompareOperator op;

            if (opObj == null)
                op = CompareOperator.equals;
            else if (opObj is CompareOperator cmpOp)
                op = cmpOp;
            else if (opObj is int enumValue)
                op = (CompareOperator)enumValue;
            else
                op = CompareOperator.equals;

            var arg0 = await flow.getValue(node.getInputPort<ValueInput>("A"));
            var arg1 = await flow.getValue(node.getInputPort<ValueInput>("B"));
            object[] args = new object[] { arg0, arg1 };
            bool result;
            switch (op)
            {
                case CompareOperator.equals:
                    result = args[0] != null ? args[0].Equals(args[1]) : args[1] == null;
                    break;
                case CompareOperator.unequals:
                    result = args[0] != null ? !args[0].Equals(args[1]) : args[1] != null;
                    break;
                case CompareOperator.greater:
                    {
                        result = args[0] is IComparable cmp1 && args[1] is IComparable cmp2 ?
                                cmp1.CompareTo(cmp2) > 0 :
                                false;
                    }
                    break;
                case CompareOperator.greaterEquals:
                    {
                        result = args[0] is IComparable cmp1 && args[1] is IComparable cmp2 ?
                                cmp1.CompareTo(cmp2) >= 0 :
                                false;
                    }
                    break;
                case CompareOperator.less:
                    {
                        result = args[0] is IComparable cmp1 && args[1] is IComparable cmp2 ?
                                cmp1.CompareTo(cmp2) < 0 :
                                false;
                    }
                    break;
                case CompareOperator.lessEquals:
                    {
                        result = args[0] is IComparable cmp1 && args[1] is IComparable cmp2 ?
                                cmp1.CompareTo(cmp2) <= 0 :
                                false;
                    }
                    break;
                default:
                    throw new InvalidOperationException("未知的操作符" + op);
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
    public enum CompareOperator
    {
        equals,
        unequals,
        greater,
        greaterEquals,
        less,
        lessEquals
    }
}