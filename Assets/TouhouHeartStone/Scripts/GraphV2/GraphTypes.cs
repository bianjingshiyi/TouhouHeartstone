using System;
using System.Linq;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UnityEngine;

namespace Game
{
    public static class GraphTypes
    {
        #region 公有方法

        #region 类型
        public static GraphTypeInfo[] getAllTypeInfos()
        {
            return _graphTypeInfos;
        }
        public static Type getTypeByName(string name)
        {
            for (int i = 0; i < _graphTypeInfos.Length; i++)
            {
                var type = _graphTypeInfos[i].type;
                if (type.Name == name || type.FullName == name)
                {
                    return type;
                }
                // 检查数组
                var arrayType = type.MakeArrayType();
                if (arrayType.Name == name || arrayType.FullName == name)
                {
                    return arrayType;
                }
            }
            throw new ArgumentException($"不存在名为{name}的类型");
        }
        public static Color getColorByType(Type type)
        {
            for (int i = 0; i < _graphTypeInfos.Length; i++)
            {
                if (_graphTypeInfos[i].type == type ||
                    (_graphTypeInfos[i].type != typeof(object) && _graphTypeInfos[i].type.IsAssignableFrom(type)))
                {
                    return _graphTypeInfos[i].color;
                }
            }
            return Color.red;
        }

        #endregion

        #region 端口类型
        public static GraphTypeInfo[] getAllPortTypeInfos()
        {
            return _graphTypeInfos.Where(i => i.portType).ToArray();
        }
        public static Type getPortTypeByName(string typeName)
        {
            for (int i = 0; i < _graphTypeInfos.Length; i++)
            {
                var info = _graphTypeInfos[i];
                if (info.portType && (info.type.Name == typeName || info.type.FullName == typeName))
                {
                    return info.type;
                }
            }
            return null;
        }
        public static int getPortTypeIndexByName(string typeName)
        {
            int index = -1;
            for (int i = 0; i < _graphTypeInfos.Length; i++)
            {
                var info = _graphTypeInfos[i];
                if (info.portType)
                    index++;
                if (info.type.Name == typeName || info.type.FullName == typeName)
                    return index;
            }
            return index;
        }
        public static Type getPortTypeAt(int index)
        {
            int count = 0;
            for (int i = 0; i < _graphTypeInfos.Length; i++)
            {
                var info = _graphTypeInfos[i];
                if (info.portType)
                {
                    if (count == index)
                        return info.type;
                    else
                        count++;
                }
            }
            throw new IndexOutOfRangeException($"无法找到索引{index}对应变量类型");
        }
        #endregion

        #endregion

        #region 属性字段
        private static GraphTypeInfo[] _graphTypeInfos = new GraphTypeInfo[]
        {
            #region 基础类型
            new GraphTypeInfo() { type = typeof(object), typeName = StringTable.EDITOR_TYPE_NAME_OBJECT, portType = true, color = new Color(1, 1, 1, 1) },
            new GraphTypeInfo() { type = typeof(int), typeName = StringTable.EDITOR_TYPE_NAME_INT, portType = true, color = new Color(.2f, 1, .2f, 1) },
            new GraphTypeInfo() { type = typeof(bool), typeName = StringTable.EDITOR_TYPE_NAME_BOOL, portType = true, color = new Color(.6f, 1, .8f, 1) },
            new GraphTypeInfo() { type = typeof(string), typeName = StringTable.EDITOR_TYPE_NAME_STRING, portType = true, color = new Color(.7f, .8f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(Array), typeName = StringTable.EDITOR_TYPE_NAME_ARRAY, portType = true, color = new Color(.8f, .8f, .8f, 1) },
            // 特殊的类型，暂时不能作为端口变量使用
            new GraphTypeInfo() { type = typeof(float), typeName = StringTable.EDITOR_TYPE_NAME_FLOAT, color = new Color(.4f, 1, .6f, 1) },
            new GraphTypeInfo() { type = typeof(char), typeName = StringTable.EDITOR_TYPE_NAME_CHAR, color = new Color(.5f, .8f, .2f, 1) },
            #endregion
            #region 引擎类型
            new GraphTypeInfo() { type = typeof(Player), typeName = StringTable.EDITOR_TYPE_NAME_PLAYER, portType = true, color = new Color(.8f, .2f, 1, 1) },
            new GraphTypeInfo() { type = typeof(Pile), typeName = StringTable.EDITOR_TYPE_NAME_PILE, portType = true, color = new Color(.6f, .2f, 1, 1) },
            new GraphTypeInfo() { type = typeof(CardDefine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_DEFINE, portType = true, color = new Color(.1f, .4f, .8f, 1) },
            new GraphTypeInfo() { type = typeof(Card), typeName = StringTable.EDITOR_TYPE_NAME_CARD, portType = true, color = new Color(.2f, .6f, 1, 1) },
            new GraphTypeInfo() { type = typeof(CardSnapshot), typeName = StringTable.EDITOR_TYPE_NAME_CARD_SNAPSHOT, portType = true, color = new Color(.5f, .6f, 7f, 1) },
            new GraphTypeInfo() { type = typeof(BuffDefine), typeName = StringTable.EDITOR_TYPE_NAME_BUFF_DEFINE, portType = true, color = new Color(.1f, .5f, .6f, 1) },
            new GraphTypeInfo() { type = typeof(Buff), typeName = StringTable.EDITOR_TYPE_NAME_BUFF, portType = true, color = new Color(.2f, .8f, 1, 1) },
            new GraphTypeInfo() { type = typeof(Effect), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT, portType = true, color = new Color(.2f, 1, 1, 1) },
            
            // 事件
            new GraphTypeInfo() { type = typeof(EventArg), typeName = StringTable.EDITOR_TYPE_NAME_EVENT_ARG, portType = true, color = new Color(1, 1, .2f, 1) },
            new GraphTypeInfo() { type = typeof(EventRecord), typeName = StringTable.EDITOR_TYPE_NAME_EVENT_RECORD, color = new Color(.9f, .9f, .4f, 1) },
            #endregion

            #region 类型代理
            new GraphTypeInfo() { type = typeof(CardPropertyGetterName), typeName = StringTable.EDITOR_TYPE_NAME_CARD_PROP_GETTER_NAME, color = new Color(.7f, .8f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(CardPropertySetterName), typeName = StringTable.EDITOR_TYPE_NAME_CARD_PROP_SETTER_NAME, color = new Color(.7f, .8f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(CardDefinePropertyGetterName), typeName = StringTable.EDITOR_TYPE_NAME_CARD_DEFINE_PROP_GETTER_NAME, color = new Color(.7f, .8f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(EffectTag), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT_TAG, color = new Color(.7f, .8f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(EventVarName), typeName = StringTable.EDITOR_TYPE_NAME_EVENT_VAR_NAME, color = new Color(.7f, .8f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(PileName), typeName = StringTable.EDITOR_TYPE_NAME_PILE_NAME, color = new Color(.7f, .8f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(KeywordName), typeName = StringTable.EDITOR_TYPE_NAME_KEYWORD, color = new Color(.7f, .8f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(CardTagName), typeName = StringTable.EDITOR_TYPE_NAME_CARD_TAG, color = new Color(.7f, .8f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(CardType), typeName = StringTable.EDITOR_TYPE_NAME_CARD_TYPE, color = new Color(.7f, .8f, .2f, 1) },

            new GraphTypeInfo() { type = typeof(EventReference), typeName = StringTable.EDITOR_TYPE_NAME_EVENT_TYPE, color = new Color(1, 1, .2f, 1) },
            new GraphTypeInfo() { type = typeof(CardPoolIDProxy), typeName = StringTable.EDITOR_TYPE_NAME_CARD_POOL_REFERENCE, color = new Color(.1f, .5f, .1f, 1) },
            #endregion

            #region 隐藏类型
            new GraphTypeInfo() { type = typeof(Rule), typeName = StringTable.EDITOR_TYPE_NAME_RULE, color = new Color(.2f, .2f, .6f, 1) },
            new GraphTypeInfo() { type = typeof(CardEngine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_ENGINE, color = new Color(.2f, .2f, 1, 1) },
            new GraphTypeInfo() { type = typeof(DefineReference), typeName = StringTable.EDITOR_TYPE_NAME_CARD_REFERENCE, color = new Color(.1f, .3f, .6f, 1) },
            new GraphTypeInfo() { type = typeof(PileNameCollection), typeName = StringTable.EDITOR_TYPE_NAME_PILE_NAME_COLLECTION, color = new Color(.2f, .1f, .6f, 1) },
            new GraphTypeInfo() { type = typeof(PileNameRange), typeName = StringTable.EDITOR_TYPE_NAME_PILE_NAME_RANGE, color = new Color(.2f, .1f, .6f, 1) },
            new GraphTypeInfo() { type = typeof(EffectTagCollection), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT_TAG_COLLECTION, color = new Color(.2f, .5f, .5f, 1) },

            new GraphTypeInfo() { type = typeof(PropModifier), typeName = StringTable.EDITOR_TYPE_NAME_PROP_MODIFIER, color = new Color(1, .2f, .8f, 1) },
            new GraphTypeInfo() { type = typeof(TriggerGraph), typeName = StringTable.EDITOR_TYPE_NAME_TRIGGER_GRAPH, color = new Color(1, .8f, .2f, 1) },
            //typeof(TriggerCollection),
            new GraphTypeInfo() { type = typeof(TargetChecker), typeName = StringTable.EDITOR_TYPE_NAME_TARGET_CHECKER, color = new Color(1, .6f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(NodeValueRef), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_VALUE_REF, color = new Color(1, .4f, .2f, 1) },
            new GraphTypeInfo() { type = typeof(ActionDefine), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_DEFINE, color = new Color(.6f, .1f, .1f, 1) },
            new GraphTypeInfo() { type = typeof(ActionNode), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_NODE, color = new Color(1, .2f, .2f, 1) },
            
            #endregion

        };
        #endregion
    }
    public struct GraphTypeInfo
    {
        public Type type;
        public string typeName;
        public bool portType;
        public Color color;
    }
}
