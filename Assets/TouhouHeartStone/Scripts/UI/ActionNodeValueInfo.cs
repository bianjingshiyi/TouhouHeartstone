using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using TouhouCardEngine;

namespace UI
{
    public struct ActionNodeValueInfo
    {
        public ActionNodeValueInfo(string name, ActionNodeValueType type, object value) : this(name, type, value, null, type)
        {
        }
        public ActionNodeValueInfo(string displayName, string name, ActionNodeValueType type, object value) : this(displayName, name, type, value, null, type)
        {
        }
        public ActionNodeValueInfo(string name, ActionNodeValueType type, object value, string[] typeNames, ActionNodeValueType assignType) : this(name, name, type, value, typeNames, assignType)
        {
        }
        public ActionNodeValueInfo(string displayName, string name, ActionNodeValueType type, object value, string[] typeNames, ActionNodeValueType assignType) : this(displayName, name, type, value, null, null)
        {
            this.assignableTypeNames = typeNames;
            assignableTypeValues = assignType.getAssignableValueTypes();
        }
        public ActionNodeValueInfo(string name, ActionNodeValueType type, object value, string[] optionalNames, object[] optionalValues) :
            this(name, name, type, value, optionalNames, optionalValues)
        {
        }
        public ActionNodeValueInfo(string displayName, string name, ActionNodeValueType type, object value, string[] optionalNames, object[] optionalValues)
        {
            this.displayName = displayName;
            this.name = name;
            this.type = type;
            this.value = value;
            this.optionalNames = optionalNames;
            this.optionalValues = optionalValues;
            assignableTypeNames = null;
            assignableTypeValues = null;
            isArray = false;
        }
        public string displayName;
        public string name;
        public ActionNodeValueType type;
        public object value;
        public string[] optionalNames;
        public object[] optionalValues;
        public string[] assignableTypeNames;
        public ActionNodeValueType[] assignableTypeValues;
        public bool isArray;
    }
    public static class NodeValueTypeHelper
    {
        public static bool tryGetValueTypeByType(this Type type, out ActionNodeValueType valueType)
        {
            if (typeDict.TryGetValue(type, out valueType))
            {
                return true;
            }
            if (type.IsEnum)
            {
                valueType = ActionNodeValueType.enumValue;
                return true;
            }
            valueType = ActionNodeValueType.none;
            return false;
        }
        public static ActionNodeValueType getNodeValueType(this Type type)
        {
            if (tryGetValueTypeByType(type, out var valueType))
            {
                return valueType;
            }
            return ActionNodeValueType.none;
        }
        public static ActionNodeValueType[] getAssignableValueTypes(this ActionNodeValueType type)
        {
            switch (type)
            {
                case ActionNodeValueType.any:
                    return new ActionNodeValueType[]
                    {
                        ActionNodeValueType.boolean,
                        ActionNodeValueType.integer,
                        ActionNodeValueType.text,
                        ActionNodeValueType.cardReference,
                    };
            }
            return null;
        }

        public static object getDefaultValue(this ActionNodeValueType valueType, bool isArray)
        {
            if (!defaultValues.TryGetValue(valueType, out var func))
                return null;
            var value = func();
            if (!isArray)
            {
                // 不是数组
                return value;
            }

            // 数组
            Type type = null;
            foreach (var pair in typeDict)
            {
                if (pair.Value == valueType)
                {
                    type = pair.Key;
                    break;
                }
            }
            if (type == null)
                return null;
            var array = Array.CreateInstance(type, 1);
            array.SetValue(value, 0);
            return array;
        }
        public static ActionNodeValueType getAssignableValueTypeByValue(this ActionNodeValueType assignType, object value)
        {
            var childTypes = getAssignableValueTypes(assignType);
            if (childTypes != null && childTypes.Length > 0)
            {
                if (childTypes.Length > 1 && value != null)
                {
                    var valType = getNodeValueType(value.GetType());
                    if (childTypes.Contains(valType))
                        return valType;
                }
                return childTypes[0];
            }
            return assignType;
        }

        private static readonly Dictionary<Type, ActionNodeValueType> typeDict = new Dictionary<Type, ActionNodeValueType>()
        {
            { typeof(object), ActionNodeValueType.any },
            { typeof(int), ActionNodeValueType.integer },
            { typeof(float), ActionNodeValueType.real },
            { typeof(bool), ActionNodeValueType.boolean },
            { typeof(string), ActionNodeValueType.text },
            { typeof(EffectTag), ActionNodeValueType.effectTag },
            { typeof(EffectTagCollection), ActionNodeValueType.effectTagCollection },
            { typeof(DefineReference), ActionNodeValueType.cardReference },
            { typeof(BuffReference), ActionNodeValueType.buffReference },
            { typeof(DefineReference[]), ActionNodeValueType.cardReferenceCollection },
            { typeof(PileName), ActionNodeValueType.pileName },
            { typeof(PileNameCollection), ActionNodeValueType.pileNameCollection },
            { typeof(KeywordCollection), ActionNodeValueType.keywords },
            { typeof(EventArgReference), ActionNodeValueType.eventReference },
            { typeof(EventReferenceProxy), ActionNodeValueType.eventReference },
            { typeof(CardPropertySetterName), ActionNodeValueType.cardPropertyNameSet },
            { typeof(CardPropertyGetterName), ActionNodeValueType.cardPropertyNameGet },
            { typeof(CardDefinePropertyGetterName), ActionNodeValueType.cardDefinePropertyNameGet },
            { typeof(EventVarName), ActionNodeValueType.eventVarName },

            { typeof(CardDefine), ActionNodeValueType.cardReference },
            { typeof(BuffDefine), ActionNodeValueType.buffReference },
        };
        private static readonly Dictionary<ActionNodeValueType, Func<object>> defaultValues = new Dictionary<ActionNodeValueType, Func<object>>()
        {
            { ActionNodeValueType.any, () => false},
            { ActionNodeValueType.integer, () => 0},
            { ActionNodeValueType.real, () => 0f },
            { ActionNodeValueType.boolean, () => false },
            { ActionNodeValueType.text, () => string.Empty },
            { ActionNodeValueType.effectTag, () => new EffectTag(string.Empty) },
            { ActionNodeValueType.effectTagCollection, () => new EffectTagCollection() },
            { ActionNodeValueType.cardReference, () => new DefineReference() },
            { ActionNodeValueType.cardReferenceCollection, () => new DefineReference[0] },
            { ActionNodeValueType.buffReference, () => new BuffReference(0, 0) },
            { ActionNodeValueType.pileName, () => new PileName(string.Empty) },
            { ActionNodeValueType.pileNameCollection, () => new PileNameCollection() },
            { ActionNodeValueType.keywords, () => new KeywordCollection() },
            { ActionNodeValueType.eventReference, () => new EventReferenceProxy(new EventReference(0, string.Empty)) },
            { ActionNodeValueType.cardPropertyNameSet, () => new CardPropertySetterName(string.Empty) },
            { ActionNodeValueType.cardPropertyNameGet, () => new CardPropertyGetterName(string.Empty) },
            { ActionNodeValueType.cardDefinePropertyNameGet, () => new CardDefinePropertyGetterName(string.Empty)  },
            { ActionNodeValueType.eventVarName, () => new EventVarName(string.Empty) },
        };

    }
    public enum ActionNodeValueType
    {
        /// <summary>
        /// 无
        /// </summary>
        none,
        /// <summary>
        /// 任意类型
        /// </summary>
        any,
        /// <summary>
        /// 整数
        /// </summary>
        integer,
        /// <summary>
        /// 实数
        /// </summary>
        real,
        /// <summary>
        /// 真值
        /// </summary>
        boolean,
        /// <summary>
        /// 字符串
        /// </summary>
        text,
        /// <summary>
        /// 枚举值
        /// </summary>
        enumValue,
        /// <summary>
        /// 牌堆名称集
        /// </summary>
        pileNameCollection,
        /// <summary>
        /// 标签集合
        /// </summary>
        effectTagCollection,
        /// <summary>
        /// 动作节点
        /// </summary>
        actionNode,
        /// <summary>
        /// 卡牌引用
        /// </summary>
        cardReference,
        /// <summary>
        /// 卡牌引用集合
        /// </summary>
        cardReferenceCollection,
        /// <summary>
        /// 关键词
        /// </summary>
        keywords,
        /// <summary>
        /// 单个效果标签
        /// </summary>
        effectTag,
        /// <summary>
        /// 事件引用
        /// </summary>
        eventReference,
        /// <summary>
        /// 卡牌属性（可设置）
        /// </summary>
        cardPropertyNameSet,
        /// <summary>
        /// 卡牌属性（可读取）
        /// </summary>
        cardPropertyNameGet,
        /// <summary>
        /// 卡牌定义属性（可读取）
        /// </summary>
        cardDefinePropertyNameGet,
        /// <summary>
        /// 牌堆名称
        /// </summary>
        pileName,
        /// <summary>
        /// 事件变量名
        /// </summary>
        eventVarName,
        /// <summary>
        /// 增益引用
        /// </summary>
        buffReference,
    }
}
