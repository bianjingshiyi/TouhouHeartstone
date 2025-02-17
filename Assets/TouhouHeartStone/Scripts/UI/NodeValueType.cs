using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using TouhouCardEngine;
using TouhouHeartstone;

namespace UI
{
    public struct NodeValueType
    {
        public ActionNodeValueType type;
        public int arrayLevel;
        public NodeValueType(ActionNodeValueType type) : this(type, 0)
        {
        }
        public NodeValueType(ActionNodeValueType type, int arrayLevel)
        {
            this.type = type;
            this.arrayLevel = arrayLevel;
        }
        public bool isArray() => arrayLevel > 0;
        public override bool Equals(object obj)
        {
            if (obj is NodeValueType other)
            {
                return type == other.type && arrayLevel == other.arrayLevel;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            var hash = type.GetHashCode();
            hash = hash * 31 + arrayLevel.GetHashCode();
            return hash;
        }
        public static bool operator ==(NodeValueType lhs, NodeValueType rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(NodeValueType lhs, NodeValueType rhs)
        {
            return !(lhs == rhs);
        }
    }
    [Serializable]
    public struct ValueTypeNameMap
    {
        public ActionNodeValueType type;
        public string name;
        public ValueTypeNameMap(ActionNodeValueType type, string name)
        {
            this.type = type;
            this.name = name;
        }
    }
    public static class NodeValueTypeHelper
    {
        public static bool tryGetValueTypeByType(this Type type, out NodeValueType valueType)
        {
            if (type == null)
            {
                valueType = default;
                return false;
            }
            if (typeDict.TryGetValue(type, out var t))
            {
                valueType = new NodeValueType(t);
                return true;
            }
            if (type.IsEnum)
            {
                valueType = new NodeValueType(ActionNodeValueType.enumValue);
                return true;
            }
            if (type.IsArray)
            {
                if (tryGetValueTypeByType(type.GetElementType(), out valueType))
                {
                    valueType.arrayLevel++;
                    return true;
                }
            }
            valueType = default;
            return false;
        }
        public static NodeValueType getNodeValueType(this Type type)
        {
            if (tryGetValueTypeByType(type, out var valueType))
            {
                return valueType;
            }
            return default;
        }
        public static NodeValueType getElementType(this NodeValueType type)
        {
            if (!type.isArray())
                throw new InvalidOperationException("无法获取非数组类型的元素类型。");
            return new NodeValueType(type.type, type.arrayLevel - 1);
        }
        public static NodeValueType[] getAssignableValueTypes(this NodeValueType type)
        {
            if (type.isArray())
                return null;
            return getAssignableValueTypes(type.type);
        }

        public static NodeValueType[] getAssignableValueTypes(ActionNodeValueType valueType)
        {
            switch (valueType)
            {
                case ActionNodeValueType.any:
                    return new NodeValueType[]
                    {
                        new NodeValueType(ActionNodeValueType.boolean),
                        new NodeValueType(ActionNodeValueType.integer),
                        new NodeValueType(ActionNodeValueType.text),
                        new NodeValueType(ActionNodeValueType.keyword),
                        new NodeValueType(ActionNodeValueType.cardTag),
                        new NodeValueType(ActionNodeValueType.cardReference),
                        new NodeValueType(ActionNodeValueType.cardPoolReference),
                        new NodeValueType(ActionNodeValueType.buffReference),
                    };
            }
            return null;
        }

        public static object getDefaultValue(this NodeValueType valueType)
        {
            if (!defaultValues.TryGetValue(valueType.type, out var constructor))
                return null;
            if (valueType.isArray())
            {
                // 数组
                Type type = null;
                foreach (var pair in typeDict)
                {
                    if (pair.Value == valueType.type)
                    {
                        type = pair.Key;
                        break;
                    }
                }
                if (type == null)
                    return null;
                var array = Array.CreateInstance(type, 0);
                return array;
            }
            else
            {
                return constructor();
            }

        }
        public static NodeValueType getAssignableValueTypeByValue(this NodeValueType assignType, object value)
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
            { typeof(CardPoolIDProxy), ActionNodeValueType.cardPoolReference },
            { typeof(BuffReference), ActionNodeValueType.buffReference },
            { typeof(PileName), ActionNodeValueType.pileName },
            { typeof(PileNameCollection), ActionNodeValueType.pileNameCollection },
            { typeof(PileNameRange), ActionNodeValueType.pileNameRange },
            { typeof(KeywordName), ActionNodeValueType.keyword },
            { typeof(CardTagName), ActionNodeValueType.cardTag },
            { typeof(KeywordCollection), ActionNodeValueType.keywordCollection },
            { typeof(CardTagCollection), ActionNodeValueType.cardTagCollection },
            { typeof(EventArgReference), ActionNodeValueType.eventReference },
            { typeof(EventReference), ActionNodeValueType.eventReference },
            { typeof(EventTriggerTime), ActionNodeValueType.eventTriggerTime },
            { typeof(CardPropertySetterName), ActionNodeValueType.cardPropertyNameSet },
            { typeof(CardPropertyGetterName), ActionNodeValueType.cardPropertyNameGet },
            { typeof(CardDefinePropertyGetterName), ActionNodeValueType.cardDefinePropertyNameGet },
            { typeof(EventVarName), ActionNodeValueType.eventVarName },
            { typeof(TypeName), ActionNodeValueType.type },
            { typeof(CardType), ActionNodeValueType.cardType },
            { typeof(CardDefine), ActionNodeValueType.cardReference },
            { typeof(BuffDefine), ActionNodeValueType.buffReference },
            { typeof(Pair), ActionNodeValueType.pair }
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
            { ActionNodeValueType.cardPoolReference, () => new CardPoolIDProxy(0) },
            { ActionNodeValueType.buffReference, () => new BuffReference(0, 0) },
            { ActionNodeValueType.pileName, () => new PileName(string.Empty) },
            { ActionNodeValueType.pileNameCollection, () => new PileNameCollection() },
            { ActionNodeValueType.pileNameRange, () => new PileNameRange() },
            { ActionNodeValueType.keyword, () => new KeywordName(string.Empty) },
            { ActionNodeValueType.cardTag, () => new CardTagName(string.Empty) },
            { ActionNodeValueType.keywordCollection, () => new KeywordCollection() },
            { ActionNodeValueType.cardTagCollection, () => new CardTagCollection() },
            { ActionNodeValueType.eventReference, () => new EventReference(0, string.Empty) },
            { ActionNodeValueType.eventTriggerTime, () => new EventTriggerTime(new EventReference(0, string.Empty), EventTriggerTimeType.Before) },
            { ActionNodeValueType.cardPropertyNameSet, () => new CardPropertySetterName(string.Empty) },
            { ActionNodeValueType.cardPropertyNameGet, () => new CardPropertyGetterName(string.Empty) },
            { ActionNodeValueType.cardDefinePropertyNameGet, () => new CardDefinePropertyGetterName(string.Empty)  },
            { ActionNodeValueType.eventVarName, () => new EventVarName(string.Empty) },
            { ActionNodeValueType.type, () => new TypeName(typeof(object).Name) },
            { ActionNodeValueType.cardType, () => new CardType(string.Empty) },
            { ActionNodeValueType.pair, () => new Pair() }
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
        /// 牌堆名称集（包括任意牌堆值）
        /// </summary>
        pileNameRange,
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
        /// 卡池引用
        /// </summary>
        cardPoolReference,
        /// <summary>
        /// 关键词
        /// </summary>
        keyword,
        /// <summary>
        /// 卡牌标签
        /// </summary>
        cardTag,
        /// <summary>
        /// 关键词列表
        /// </summary>
        keywordCollection,
        /// <summary>
        /// 卡牌标签列表
        /// </summary>
        cardTagCollection,
        /// <summary>
        /// 单个效果标签
        /// </summary>
        effectTag,
        /// <summary>
        /// 事件引用
        /// </summary>
        eventReference,
        /// <summary>
        /// 事件触发时机
        /// </summary>
        eventTriggerTime,
        /// <summary>
        /// 卡牌属性（可设置）
        /// </summary>
        cardPropertyNameSet,
        /// <summary>
        /// 卡牌属性（可读取）
        /// </summary>
        cardPropertyNameGet,
        /// <summary>
        /// 卡牌定义属性（可设置）
        /// </summary>
        cardDefinePropertyNameSet,
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
        /// <summary>
        /// 类型
        /// </summary>
        type,
        /// <summary>
        /// 卡牌类型
        /// </summary>
        cardType,
        /// <summary>
        /// 映射
        /// </summary>
        pair,
    }
}
