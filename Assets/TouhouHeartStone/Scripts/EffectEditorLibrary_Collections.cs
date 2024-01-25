using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;

namespace Game
{

    public static partial class EffectEditorLibrary
    {
        #region 集合运算
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CONTAINS = "包含";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_ORDER_BY = "排序";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_ELEMENT_BY_INDEX = "获取第X个元素";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_RANDOM_ELEMENT = "获取集合中的随机元素";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_REVERSE_COLLECTION = "反转集合";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_DISTINCT_COLLECTION = "集合去重";
        [TranslateMsg("编辑器_动作节点_遍历")]
        public const string EDITOR_NODE_NAME_FOREACH = "遍历";

        [TranslateMsg("编辑器_参数名称_降序")]
        public const string EDITOR_PARAM_NAME_DESCENDING = "降序";
        [TranslateMsg("编辑器_参数名称_排序依据")]
        public const string EDITOR_PARAM_NAME_ORDER_KEY = "排序依据";
        [TranslateMsg("编辑器-包含节点-参数名称")]
        public const string EDITOR_PARAM_NAME_CONTAINS_ELEMENT = "是否包含元素";
        [TranslateMsg("动作图编辑器中参数节点名称之匹配的集合")]
        public const string EDITOR_PARAM_NAME_ARRAY_MATCHED = "匹配的集合";
        [TranslateMsg("动作图编辑器中参数节点名称之集合")]
        public const string EDITOR_PARAM_NAME_ARRAY_MISMATCHED = "不匹配的集合";
        [TranslateMsg("编辑器_参数名称_元素位置")]
        public const string EDITOR_PARAM_NAME_INDEX = "元素位置";
        [TranslateMsg("编辑器-集合相减-参数名称")]
        public const string EDITOR_PARAM_NAME_EXCEPTED_COLLECTION = "减去的集合";


        /// <summary>
        /// 创建集合
        /// </summary>
        /// <param name="elements">元素列表</param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateArray, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_CREATE_ARRAY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)]
        public static Array createArray([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isParams: true)] object[] elements)
        {
            if (elements == null)
                return Array.Empty<object>();
            return elements;
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddElementToArray, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_ADD_ELEMENT_TO_ARRAY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)]
        public static Array addElementToArray(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isParams: true)] object[] elements)
        {
            if (array == null)
                array = Array.Empty<object>();

            object[] newArray = new object[array.Length + elements.Length];
            Array.Copy(array, 0, newArray, 0, array.Length);

            if (elements == null)
                return newArray;
            Array.Copy(elements, 0, newArray, array.Length, elements.Length);
            return newArray;
        }
        /// <summary>
        /// 筛选（过时）
        /// </summary>
        [Obsolete]
        [THHActionMethod(BuiltinActionDefineID.Where_Obsolete, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, "筛选（过时）", StringTable.EDITOR_NODE_NAME_WHERE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_ARRAY_MATCHED)]
        public static async Task<object[]> where(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] ControlInput condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return Array.Empty<object>();

            List<object> objList = new List<object>();
            List<object> negList = new List<object>();

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault();

            for (int i = 0; i < array.Length; i++)
            {
                object obj = array.GetValue(i);

                flow.setValue(elementPort, obj);


                bool result = true;
                if (resultPort != null)
                {
                    flow.EnterScope();
                    result = await game.getValue<bool>(flow, resultPort);
                    flow.ExitScope();
                }

                if (result)
                {
                    objList.Add(obj);
                }
                else
                {
                    negList.Add(obj);
                }
            }
            return objList.ToArray();
        }
        /// <summary>
        /// 筛选
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.Where, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_WHERE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_ARRAY_MATCHED)]
        public static async Task<object[]> where(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] NodeValueRef<bool> condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return Array.Empty<object>();

            List<object> objList = new List<object>();
            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));

            foreach (var obj in array)
            {
                flow.setValue(elementPort, obj);
                bool result = true;
                if (condition.outputPort != null)
                {
                    flow.EnterScope();
                    result = await condition.getValue(flow);
                    flow.ExitScope();
                }

                if (result)
                {
                    objList.Add(obj);
                }
            }
            return objList.ToArray();
        }
        /// <summary>
        /// 计算集合中元素数量
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCount, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_GET_COUNT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)]
        public static int getCount([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] collection)
        {
            if (collection == null)
                return 0;
            return collection.Length;
        }

        /// <summary>
        /// 包含
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.Contains, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_CONTAINS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_CONTAINS_ELEMENT)]
        public static bool contains([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] collection,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)] object element)
        {
            if (collection == null)
                return false;
            return collection.Contains(element);
        }

        /// <summary>
        /// 根据排序依据排序。（过时）
        /// </summary>
        [Obsolete]
        [THHActionMethod(BuiltinActionDefineID.OrderBy_Obsolete, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, "排序（过时）", EDITOR_NODE_NAME_ORDER_BY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array orderBy(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(EDITOR_PARAM_NAME_DESCENDING)] bool descending,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_ORDER_KEY)] ControlInput condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return Array.Empty<object>();
            if (condition == null)
                return array;

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault();

            object func(object obj)
            {
                flow.setValue(elementPort, obj);
                object value = null;
                if (resultPort != null)
                {
                    flow.EnterScope();
                    value = game.getValue<object>(flow, resultPort).Result;
                    flow.ExitScope();
                }
                return value;
            }
            if (!descending)
            {
                return array.OrderBy(func).ToArray();
            }
            else
            {
                return array.OrderByDescending(func).ToArray();
            }
        }

        /// <summary>
        /// 根据排序依据排序。
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.OrderBy, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_ORDER_BY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array orderBy(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(EDITOR_PARAM_NAME_DESCENDING)] bool descending,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_ORDER_KEY)] NodeValueRef<object> condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return Array.Empty<object>();
            if (condition == null)
                return array;

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));

            object func(object obj)
            {
                flow.setValue(elementPort, obj);
                object value = null;
                if (condition.outputPort != null)
                {
                    flow.EnterScope();
                    value = condition.getValue(flow).Result;
                    flow.ExitScope();
                }
                return value;
            }
            if (!descending)
            {
                return array.OrderBy(func).ToArray();
            }
            else
            {
                return array.OrderByDescending(func).ToArray();
            }
        }

        /// <summary>
        /// 获取第X个元素。
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetElementByIndex, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_GET_ELEMENT_BY_INDEX)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)]
        public static object getElementByIndex(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_INDEX)] int index)
        {
            if (index >= 0 && index < array.Length)
                return array[index];
            return null;
        }
        /// <summary>
        /// 遍历集合
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.ForeachAction, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_FOREACH)]
        public static async Task foreachAction(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION)] ControlInput action,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return;
            if (action == null)
                return;
            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));
            foreach (object ele in array)
            {
                var scope = flow.EnterScope();
                setScopeLooping(scope);

                flow.setValue(elementPort, ele);
                await game.runActions(flow, action);

                int flowState = getScopeLoopState(scope);
                flow.ExitScope();

                if (flowState == SCOPE_LOOP_STATE_BROKEN)
                    break;
            }
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetRandomElement, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_GET_RANDOM_ELEMENT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)]
        public static object getRandomElement(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] collection)
        {
            if (collection == null || collection.Length <= 0)
                return null;
            if (collection.Length == 1)
                return collection[0];
            return collection[game.randomInt(0, collection.Length - 1)];
        }
        /// <summary>
        /// 反转集合
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.ReverseCollection, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_REVERSE_COLLECTION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static object[] reverseCollection(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array)
        {
            if (array == null)
                return null;
            return array.Reverse().ToArray();
        }
        /// <summary>
        /// 集合去重
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.Distinct, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_DISTINCT_COLLECTION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array distinct(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array)
        {
            if (array == null)
                return null;

            return array.Distinct().ToArray();
        }
        /// <summary>
        /// 集合相减
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.Except, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_EXCEPT, "Except")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array except(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_EXCEPTED_COLLECTION)] object[] excepted)
        {
            if (array == null)
                return null;
            if (excepted == null)
                return array;
            return array.Except(excepted).ToArray();
        }

        #region 整数相关
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_SUM = "求和";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_MIN = "获取最小值";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_MAX = "获取最大值";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_AVERAGE = "获取平均值";

        [TranslateMsg("编辑器-求和-参数名称")]
        public const string EDITOR_PARAM_NAME_SUM = "和";
        [TranslateMsg("编辑器-求和-参数名称")]
        public const string EDITOR_PARAM_NAME_VALUE_TO_BE_SUMMED = "参与求和的值";
        [TranslateMsg("编辑器-获取最小值-参数名称")]
        public const string EDITOR_PARAM_NAME_MIN_VALUE = "最小值";
        [TranslateMsg("编辑器-获取最大值-参数名称")]
        public const string EDITOR_PARAM_NAME_MAX_VALUE = "最大值";
        [TranslateMsg("编辑器-获取最大值-参数名称")]
        public const string EDITOR_PARAM_NAME_AVERAGE = "平均值";

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Sum, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_SUM)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_SUM)]
        public static int sum(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] array)
        {
            if (array == null)
                return 0;
            var numbers = array.OfType<int>();
            if (numbers.Count() <= 0)
                return 0;
            return numbers.Sum();
        }
        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Min, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_MIN)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MIN_VALUE)]
        public static int min(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] array)
        {
            if (array == null)
                return 0;
            var numbers = array.OfType<int>();
            if (numbers.Count() <= 0)
                return 0;
            return numbers.Min();
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Max, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_MAX)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MAX_VALUE)]
        public static int max(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] array)
        {
            if (array == null)
                return 0;
            var numbers = array.OfType<int>();
            if (numbers.Count() <= 0)
                return 0;
            return numbers.Max();
        }
        /// <summary>
        /// 获取平均值
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Average, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_AVERAGE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_AVERAGE)]
        public static int average(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] array)
        {
            if (array == null)
                return 0;
            var numbers = array.OfType<int>();
            if (numbers.Count() <= 0)
                return 0;
            return (int)numbers.Average();
        }
        #endregion
        #region 集合操作
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CONCAT = "集合相加";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_UNION = "获取并集";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_INTERSECT = "获取交集";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_IS_SUBSET = "是否是某集合的子集";

        [TranslateMsg("编辑器-集合相加-参数名称")]
        public const string EDITOR_PARAM_NAME_CONCATING_COLLECTION = "相加的集合";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_COLLECTION_A = "集合A";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_COLLECTION_B = "集合B";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_SUBSET = "子集";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_IS_SUBSET = "是否是子集";

        /// <summary>
        /// 集合相加
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Concat, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_CONCAT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array concat(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] first,
            [ActionNodeParam(EDITOR_PARAM_NAME_CONCATING_COLLECTION)] object[] second)
        {
            if (first == null || second == null)
                return first ?? second ?? Array.Empty<object>();
            return first.Concat(second).ToArray();
        }
        /// <summary>
        /// 获取并集
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Union, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_UNION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array union(
            [ActionNodeParam(EDITOR_PARAM_NAME_COLLECTION_A)] object[] first,
            [ActionNodeParam(EDITOR_PARAM_NAME_COLLECTION_B)] object[] second)
        {
            if (first == null || second == null)
                return first ?? second ?? Array.Empty<object>();
            return first.Union(second).ToArray();
        }
        /// <summary>
        /// 获取交集
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Intersect, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_INTERSECT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array intersect(
            [ActionNodeParam(EDITOR_PARAM_NAME_COLLECTION_A)] object[] first,
            [ActionNodeParam(EDITOR_PARAM_NAME_COLLECTION_B)] object[] second)
        {
            if (first == null || second == null)
                return first ?? second ?? Array.Empty<object>();
            return first.Intersect(second).ToArray();
        }
        /// <summary>
        /// 是否是某集合的子集
        /// </summary>
        /// <param name="set"></param>
        /// <param name="subset"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.IsSubset, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_IS_SUBSET)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_IS_SUBSET)]
        public static bool isSubset(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] set,
            [ActionNodeParam(EDITOR_PARAM_NAME_SUBSET)] object[] subset)
        {
            if (subset == null) // 空集是所有集合的子集
                return true;
            if (set == null) // 空集没有除了它本身之外的子集
                return false;
            return set.isSubset(subset);
        }
        #endregion
        #region 逻辑运算
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_ALL = "是否所有元素都满足条件";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_ANY = "是否有任意元素满足条件";
        /// <summary>
        /// 是否所有元素都满足条件
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.All, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_ALL)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool all(Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] NodeValueRef<bool> condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null || condition?.outputPort == null)
                return false;

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));

            bool func(object obj)
            {
                flow.EnterScope();
                flow.setValue(elementPort, obj);
                bool value = condition.getValue(flow).Result;
                flow.ExitScope();
                return value;
            }
            return array.All(func);
        }
        /// <summary>
        /// 是否有任意元素满足条件
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Any, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_ANY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool any(Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] NodeValueRef<bool> condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null || condition?.outputPort == null)
                return false;

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));

            bool func(object obj)
            {
                flow.EnterScope();
                flow.setValue(elementPort, obj);
                bool value = condition.getValue(flow).Result;
                flow.ExitScope();
                return value;
            }
            return array.Any(func);
        }
        #endregion
        #region 获取范围内元素
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_FIRST = "获取第一个元素";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_LAST = "获取最后一个元素";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_TAKE = "获取集合内前X个元素";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_SKIP = "获取集合内前X个元素之外的元素";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_TAKE_WHILE = "获取集合内元素直到条件不成立";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_SKIP_WHILE = "条件不成立后获取剩余元素";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_RANDOM_TAKE = "获取集合内的随机X个元素";

        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_COUNT_X = "数量X";
        /// <summary>
        /// 获取第一个元素
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.First, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_FIRST)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)]
        public static object first(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array)
        {
            if (array == null)
                return null;
            return array.FirstOrDefault();
        }
        /// <summary>
        /// 获取最后一个元素
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Last, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_LAST)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)]
        public static object last(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array)
        {
            if (array == null)
                return null;
            return array.LastOrDefault();
        }
        /// <summary>
        /// 获取集合内前X个元素
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Take, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_TAKE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array take(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_COUNT_X)] int count)
        {
            if (array == null)
                return Array.Empty<object>();
            return array.Take(count).ToArray();
        }
        /// <summary>
        /// 获取集合内前X个元素之外的元素
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Skip, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_SKIP)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array skip(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_COUNT_X)] int count)
        {
            if (array == null)
                return Array.Empty<object>();
            return array.Skip(count).ToArray();
        }
        /// <summary>
        /// 获取集合内结果直到条件不成立
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.TakeWhile, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_TAKE_WHILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array takeWhile(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] NodeValueRef<bool> condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return Array.Empty<object>();
            if (condition?.outputPort == null)
                return array;

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));

            bool predicate(object obj)
            {
                flow.EnterScope();
                flow.setValue(elementPort, obj);
                bool value = condition.getValue(flow).Result;
                flow.ExitScope();
                return value;
            }
            return array.TakeWhile(predicate).ToArray();
        }
        /// <summary>
        /// 条件不成立后获取剩余结果
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.SkipWhile, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_SKIP_WHILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array skipWhile(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] NodeValueRef<bool> condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return Array.Empty<object>();
            if (condition?.outputPort == null)
                return array;

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));

            bool predicate(object obj)
            {
                flow.EnterScope();
                flow.setValue(elementPort, obj);
                bool value = condition.getValue(flow).Result;
                flow.ExitScope();
                return value;
            }
            return array.SkipWhile(predicate).ToArray();
        }
        /// <summary>
        /// 获取集合内的随机X个元素
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.RandomTake, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_RANDOM_TAKE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array randomTake(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_COUNT_X)] int count)
        {
            if (array == null)
                return Array.Empty<object>();
            return array.randomTake(game, count).ToArray();
        }

        #endregion
        #region 其他
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_SELECT = "获取集合内所有元素的某项属性";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_RANDOMIZE = "打乱集合内元素顺序";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CONTENT_EQUALS = "集合内容是否相同";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_INDEX_OF_ELEMENT = "获取元素在集合中的位置";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_MERGE = "合并多个集合";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CAST_COLLECTION_TYPE = "转换集合类型"; 

        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_SELECTOR = "选择的属性";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_IGNORE_ORDER = "忽略顺序";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_MANY_COLLECTIONS = "多个集合";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_MERGED_COLLECTION = "合并的集合";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_SPECIFIC_TYPE_COLLECTION = "特定类型的集合";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_TYPE = "类型";

        /// <summary>
        /// 获取集合内所有元素的某项属性
        /// </summary>
        /// <param name="array"></param>
        /// <param name="selector"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Select, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_SELECT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array select(Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_SELECTOR)] NodeValueRef<object> selector,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null || selector?.outputPort == null)
                return Array.Empty<object>();

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));

            object select(object obj)
            {
                flow.EnterScope();
                flow.setValue(elementPort, obj);
                object value = selector.getValue(flow).Result;
                flow.ExitScope();
                return value;
            }
            return array.Select(select).ToArray();
        }

        /// <summary>
        /// 打乱集合内的元素顺序
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Randomize, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_RANDOMIZE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array randomize(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array)
        {
            if (array == null)
                return Array.Empty<object>();
            return array.shuffle(game).ToArray();
        }
        /// <summary>
        /// 集合内容是否相同
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.ContentEquals, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_CONTENT_EQUALS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool contentEquals(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_IGNORE_ORDER)] bool ignoreOrder,
            [ActionNodeParam(EDITOR_PARAM_NAME_COLLECTION_A)] object[] arrayA,
            [ActionNodeParam(EDITOR_PARAM_NAME_COLLECTION_B)] object[] arrayB)
        {
            if (arrayA == null && arrayB == null) // 两个都是空集
                return true;
            if (arrayA == null || arrayB == null) // 其中一个是空集
                return false;
            if (!ignoreOrder)
                return arrayA.SequenceEqual(arrayB);
            return arrayA.Count() == arrayB.Count() && arrayA.isSubset(arrayB); // 数量相同，并且其中一个是另一个的子集
        }
        /// <summary>
        /// 获取元素在集合中的位置
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="node"></param>
        /// <param name="array"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.IndexOf, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_GET_INDEX_OF_ELEMENT)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_INDEX)]
        public static int indexOf(Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Array array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)] object element)
        {
            if (array == null || element == null)
                return -1;
            return Array.IndexOf(array, element);
        }
        /// <summary>
        /// 合并多个集合
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="node"></param>
        /// <param name="array"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Merge, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_MERGE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MERGED_COLLECTION)]
        public static object[] merge(
            [ActionNodeParam(EDITOR_PARAM_NAME_MANY_COLLECTIONS)] Array[] collections)
        {
            if (collections == null)
                return null;
            var list = new List<object>();
            foreach (var array in collections)
            {
                foreach (var element in array)
                {
                    list.Add(element);
                }
            }
            return list.ToArray();
        }


        /// <summary>
        /// 转换集合类型
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.CastCollectionType, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_CAST_COLLECTION_TYPE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_SPECIFIC_TYPE_COLLECTION)]
        public static Array castCollectionType(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Array array,
            [ActionNodeParam(EDITOR_PARAM_NAME_TYPE, isConst: true)] TypeName typeName)
        {
            if (array == null || typeName == null)
                return null;
            var type = GraphTypes.getTypeByName(typeName);
            if (type == null)
                return null;
            var newArray = Array.CreateInstance(type, array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                var value = array.GetValue(i);
                if (value == null)
                    continue;
                var valueType = value.GetType();
                if (type.IsAssignableFrom(valueType))
                {
                    newArray.SetValue(value, i);
                }
            }
            return newArray;
        }
        #endregion
        #endregion

    }
}
