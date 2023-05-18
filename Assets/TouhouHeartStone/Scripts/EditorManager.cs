using BJSYGameCore;
using System;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace Game
{
    public class EditorManager : Manager
    {
        #region 公有方法
        public int getEditorVarTypeNameIndex(string typeName)
        {
            int index = -1;
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isVarType)
                    index++;
                if (_editorTypeInfos[i].type.Name == typeName || _editorTypeInfos[i].type.FullName == typeName)
                    return index;
            }
            return index;
        }
        public int getEditorConstTypeNameIndex(string typeName)
        {
            int index = -1;
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isConstType)
                    index++;
                if (_editorTypeInfos[i].type.Name == typeName || _editorTypeInfos[i].type.FullName == typeName)
                    return index;
            }
            return index;
        }
        public Type getEditorVarTypeAt(int index)
        {
            int count = 0;
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isVarType)
                {
                    if (count == index)
                        return _editorTypeInfos[i].type;
                    else
                        count++;
                }
            }
            throw new IndexOutOfRangeException("无法找到索引" + index + "对应变量类型");
        }
        public Type getEditorConstTypeAt(int index)
        {
            int count = 0;
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isConstType)
                {
                    if (count == index)
                        return _editorTypeInfos[i].type;
                    else
                        count++;
                }
            }
            throw new IndexOutOfRangeException("无法找到索引" + index + "对应常量类型");
        }
        public string[] getEditorVarTypeNames()
        {
            List<string> nameList = new List<string>();
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isVarType)
                {
                    nameList.Add(game.langManager.getString(_editorTypeInfos[i].typeName));
                }
            }
            return nameList.ToArray();
        }
        public string[] getEditorConstTypeNames()
        {
            List<string> nameList = new List<string>();
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isConstType)
                {
                    nameList.Add(game.langManager.getString(_editorTypeInfos[i].typeName));
                }
            }
            return nameList.ToArray();
        }
        public Color getColorByType(Type type)
        {
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].type == type ||
                    (_editorTypeInfos[i].type != typeof(object) && _editorTypeInfos[i].type.IsAssignableFrom(type)))
                {
                    return _editorTypeInfos[i].color;
                }
            }
            return Color.red;
        }
        public Type getTypeByName(string name)
        {
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                var type = _editorTypeInfos[i].type;
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
            throw new ArgumentException("不存在名为" + name + "的类型");
        }
        #endregion
        #region 私有方法
        #endregion
        #region 属性字段
        public MainManager game
        {
            get { return _game; }
            set { _game = value; }
        }
        [SerializeField]
        MainManager _game;
        EditorTypeInfo[] _editorTypeInfos = new EditorTypeInfo[]
        {
            #region 基础类型
            new EditorTypeInfo() { type = typeof(object), typeName = StringTable.EDITOR_TYPE_NAME_OBJECT, isVarType = true, color = new Color(1, 1, 1, 1) },
            new EditorTypeInfo() { type = typeof(int), typeName = StringTable.EDITOR_TYPE_NAME_INT, isVarType = true, isConstType = true, color = new Color(.2f, 1, .2f, 1) },
            new EditorTypeInfo() { type = typeof(float), typeName = StringTable.EDITOR_TYPE_NAME_FLOAT, isVarType = true, isConstType = true, color = new Color(.4f, 1, .6f, 1) },
            new EditorTypeInfo() { type = typeof(bool), typeName = StringTable.EDITOR_TYPE_NAME_BOOL, isVarType = true, isConstType = true, color = new Color(.6f, 1, .8f, 1) },
            new EditorTypeInfo() { type = typeof(char), typeName = StringTable.EDITOR_TYPE_NAME_CHAR, isVarType = true, isConstType = true, color = new Color(.5f, .8f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(string), typeName = StringTable.EDITOR_TYPE_NAME_STRING, isVarType = true, isConstType = true, color = new Color(.7f, .8f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(Array), typeName = StringTable.EDITOR_TYPE_NAME_ARRAY, isVarType = true, isConstType = false, color = new Color(.8f, .8f, .8f, 1) },
            #endregion
            #region 引擎类型
            new EditorTypeInfo() { type = typeof(Rule), typeName = StringTable.EDITOR_TYPE_NAME_RULE, isVarType = true, color = new Color(.2f, .2f, .6f, 1) },
            new EditorTypeInfo() { type = typeof(CardEngine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_ENGINE, isVarType = true, color = new Color(.2f, .2f, 1, 1) },
            new EditorTypeInfo() { type = typeof(Player), typeName = StringTable.EDITOR_TYPE_NAME_PLAYER, isVarType = true, color = new Color(.8f, .2f, 1, 1) },
            new EditorTypeInfo() { type = typeof(Pile), typeName = StringTable.EDITOR_TYPE_NAME_PILE, isVarType = true, color = new Color(.6f, .2f, 1, 1) },
            new EditorTypeInfo() { type = typeof(PileNameCollection), typeName = StringTable.EDITOR_TYPE_NAME_PILE_NAME_COLLECTION, isConstType = true, color = new Color(.2f, .1f, .6f, 1) },
            new EditorTypeInfo() { type = typeof(CardDefine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_DEFINE, isVarType = true, color = new Color(.1f, .4f, .8f, 1) },
            new EditorTypeInfo() { type = typeof(DefineReference), typeName = StringTable.EDITOR_TYPE_NAME_CARD_REFERENCE, isConstType = true, color = new Color(.1f, .3f, .6f, 1) },
            new EditorTypeInfo() { type = typeof(Card), typeName = StringTable.EDITOR_TYPE_NAME_CARD, isVarType = true, color = new Color(.2f, .6f, 1, 1) },
            new EditorTypeInfo() { type = typeof(BuffDefine), typeName = StringTable.EDITOR_TYPE_NAME_BUFF_DEFINE, isVarType = true, color = new Color(.1f, .5f, .6f, 1) },
            new EditorTypeInfo() { type = typeof(Buff), typeName = StringTable.EDITOR_TYPE_NAME_BUFF, isVarType = true, color = new Color(.2f, .8f, 1, 1) },
            new EditorTypeInfo() { type = typeof(Effect), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT, isVarType = true, color = new Color(.2f, 1, 1, 1) },
            new EditorTypeInfo() { type = typeof(GeneratedEffect), typeName = StringTable.EDITOR_TYPE_NAME_HALO_EFFECT, isVarType = true, color = new Color(.2f, 1, 1, 1) },
            new EditorTypeInfo() { type = typeof(GeneratedAura), typeName = StringTable.EDITOR_TYPE_NAME_HALO_EFFECT, isVarType = true, color = new Color(.2f, 1, 1, 1) },
            new EditorTypeInfo() { type = typeof(EffectTagCollection), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT_TAG_COLLECTION, isConstType = true, color = new Color(.2f, .5f, .5f, 1) },
            new EditorTypeInfo() { type = typeof(EventArg), typeName = StringTable.EDITOR_TYPE_NAME_EVENT_ARG, isVarType = true, color = new Color(1, 1, .2f, 1) },
            new EditorTypeInfo() { type = typeof(PropModifier), typeName = StringTable.EDITOR_TYPE_NAME_PROP_MODIFIER, isVarType = true, color = new Color(1, .2f, .8f, 1) },
            new EditorTypeInfo() { type = typeof(TriggerGraph), typeName = StringTable.EDITOR_TYPE_NAME_TRIGGER_GRAPH, isVarType = true, color = new Color(1, .8f, .2f, 1) },
            //typeof(TriggerCollection),
            new EditorTypeInfo() { type = typeof(TargetChecker), typeName = StringTable.EDITOR_TYPE_NAME_TARGET_CHECKER, isVarType = true, color = new Color(1, .6f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(ActionValueRef), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_VALUE_REF, isVarType = true, isConstType = true, color = new Color(1, .4f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(ActionDefine), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_DEFINE, isVarType = true, color = new Color(.6f, .1f, .1f, 1) },
            new EditorTypeInfo() { type = typeof(ActionNode), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_NODE, isVarType = true, isConstType = true, color = new Color(1, .2f, .2f, 1) },
            #endregion
        };
        #endregion
        #region 内部类
        [Serializable]
        struct EditorTypeInfo
        {
            public Type type;
            public string typeName;
            public bool isVarType;
            public bool isConstType;
            public Color color;
        }
        #endregion
    }
}
