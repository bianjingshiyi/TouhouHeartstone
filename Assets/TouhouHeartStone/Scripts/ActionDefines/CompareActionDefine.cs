using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game;
namespace TouhouCardEngine
{
    public class CompareActionDefine : ActionDefine
    {
        public CompareActionDefine() : base(BuiltinActionDefineID.Compare, defName, defName)
        {
            type = NodeDefineType.Function;
            inputs = new PortDefine[]
            {
                PortDefine.Value(typeof(CompareOperator), "operator", isConst: true),
                PortDefine.Value(typeof(object), "A"),
                PortDefine.Value(typeof(object), "B"),
            };
            outputs = new PortDefine[]
            {
                PortDefine.Value(typeof(bool), "result")
            };
            category = EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER;
        }
        public override async Task<ControlOutput> run(Flow flow, Node node)
        {
            object opObj = await flow.getValue(node.getInputPort<ValueInput>("operator"));
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
            bool result;
            switch (op)
            {
                case CompareOperator.equals:
                    result = equals(arg0, arg1);
                    break;
                case CompareOperator.unequals:
                    result = !equals(arg0, arg1);
                    break;
                case CompareOperator.greater:
                    {
                        result = arg0 is IComparable cmp1 && arg1 is IComparable cmp2 ?
                                cmp1.CompareTo(cmp2) > 0 :
                                false;
                    }
                    break;
                case CompareOperator.greaterEquals:
                    {
                        result = arg0 is IComparable cmp1 && arg1 is IComparable cmp2 ?
                                cmp1.CompareTo(cmp2) >= 0 :
                                false;
                    }
                    break;
                case CompareOperator.less:
                    {
                        result = arg0 is IComparable cmp1 && arg1 is IComparable cmp2 ?
                                cmp1.CompareTo(cmp2) < 0 :
                                false;
                    }
                    break;
                case CompareOperator.lessEquals:
                    {
                        result = arg0 is IComparable cmp1 && arg1 is IComparable cmp2 ?
                                cmp1.CompareTo(cmp2) <= 0 :
                                false;
                    }
                    break;
                default:
                    throw new InvalidOperationException($"未知的操作符{op}");
            }

            flow.setValue(node.getOutputPort<ValueOutput>("result"), result);
            return node.getOutputPort<ControlOutput>(exitPortName);
        }
        private bool equals(object obj1, object obj2)
        {
            if (obj1 == null)
                return obj2 == null;
            if (oneWayEquals(obj1, obj2) || oneWayEquals(obj2, obj1))
                return true;
            return obj1.Equals(obj2);
        }
        private bool oneWayEquals(object obj1, object obj2)
        {
            if (obj1 is BuffReference buffRef1)
            {
                if (obj2 is BuffDefine buffDef2)
                    return buffRef1.cardPoolId == buffDef2.cardPoolId && buffRef1.defineId == buffDef2.id;
                else if (obj2 is BuffReference defRef2)
                    return buffRef1 == defRef2;
            }
            else if (obj1 is DefineReference defRef1)
            {
                if (obj2 is CardDefine cardDef2)
                    return defRef1.cardPoolId == cardDef2.cardPoolId && defRef1.defineId == cardDef2.id;
                else if (obj2 is DefineReference defRef2)
                    return defRef1 == defRef2;
            }
            return false;
        }
        public const string defName = "Compare";
        public PortDefine[] inputs { get; }
        public PortDefine[] outputs { get; }

        public override IEnumerable<PortDefine> inputDefines => inputs;
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