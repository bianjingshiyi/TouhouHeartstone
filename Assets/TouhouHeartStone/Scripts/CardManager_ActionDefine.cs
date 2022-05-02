using BJSYGameCore;
using System;
using System.Collections.Generic;
using TouhouCardEngine;
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
                if (_editorTypeInfos[i].type.Name == typeName)
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
                if (_editorTypeInfos[i].type.Name == typeName)
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
        public Color getColorByTypeName(string typeName)
        {
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].type.Name == typeName)
                {
                    return _editorTypeInfos[i].color;
                }
            }
            return Color.red;
        }
        #endregion
        #region 属性字段
        public MainManager game
        {
            get { return _game; }
            set { _game = value; }
        }
        EditorTypeInfo[] _editorTypeInfos = new EditorTypeInfo[]
        {
            #region 基础类型
            new EditorTypeInfo() { type = typeof(object), typeName = StringTable.EDITOR_TYPE_NAME_OBJECT, isVarType = true },
            new EditorTypeInfo() { type = typeof(int), typeName = StringTable.EDITOR_TYPE_NAME_INT, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(float), typeName = StringTable.EDITOR_TYPE_NAME_FLOAT, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(bool), typeName = StringTable.EDITOR_TYPE_NAME_BOOL, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(char), typeName = StringTable.EDITOR_TYPE_NAME_CHAR, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(string), typeName = StringTable.EDITOR_TYPE_NAME_STRING, isVarType = true, isConstType = true },
            #endregion
            #region 引擎类型
            new EditorTypeInfo() { type = typeof(Rule), typeName = StringTable.EDITOR_TYPE_NAME_RULE, isVarType = true },
            new EditorTypeInfo() { type = typeof(CardEngine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_ENGINE, isVarType = true },
            new EditorTypeInfo() { type = typeof(Player), typeName = StringTable.EDITOR_TYPE_NAME_PLAYER, isVarType = true },
            new EditorTypeInfo() { type = typeof(Pile), typeName = StringTable.EDITOR_TYPE_NAME_PILE, isVarType = true },
            new EditorTypeInfo() { type = typeof(PileNameCollection), typeName = StringTable.EDITOR_TYPE_NAME_PILE_NAME_COLLECTION, isConstType = true },
            new EditorTypeInfo() { type = typeof(CardDefine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_DEFINE, isVarType = true },
            new EditorTypeInfo() { type = typeof(CardReference), typeName = StringTable.EDITOR_TYPE_NAME_CARD_REFERENCE, isConstType = true },
            new EditorTypeInfo() { type = typeof(Card), typeName = StringTable.EDITOR_TYPE_NAME_CARD, isVarType = true },
            new EditorTypeInfo() { type = typeof(BuffDefine), typeName = StringTable.EDITOR_TYPE_NAME_BUFF_DEFINE, isVarType = true },
            new EditorTypeInfo() { type = typeof(Buff), typeName = StringTable.EDITOR_TYPE_NAME_BUFF, isVarType = true },
            new EditorTypeInfo() { type = typeof(Effect), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT, isVarType = true },
            new EditorTypeInfo() { type = typeof(EffectTagCollection), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT_TAG_COLLECTION, isConstType = true },
            new EditorTypeInfo() { type = typeof(EventArg), typeName = StringTable.EDITOR_TYPE_NAME_EVENT_ARG, isVarType = true },
            new EditorTypeInfo() { type = typeof(PropModifier), typeName = StringTable.EDITOR_TYPE_NAME_PROP_MODIFIER, isVarType = true },
            new EditorTypeInfo() { type = typeof(TriggerGraph), typeName = StringTable.EDITOR_TYPE_NAME_TRIGGER_GRAPH, isVarType = true },
            //typeof(TriggerCollection),
            new EditorTypeInfo() { type = typeof(TargetChecker), typeName = StringTable.EDITOR_TYPE_NAME_TARGET_CHECKER, isVarType = true },
            new EditorTypeInfo() { type = typeof(ActionValueRef), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_VALUE_REF, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(ActionDefine), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_DEFINE, isVarType = true },
            new EditorTypeInfo() { type = typeof(ActionNode), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_NODE, isVarType = true, isConstType = true },
            #endregion
        };
        [SerializeField]
        MainManager _game;
        #endregion
        #region 内部类
        class EditorTypeInfo
        {
            public Type type;
            public string typeName;
            public bool isVarType;
            public bool isConstType;
            public Color color;
        }
        #endregion
    }
    partial class CardManager
    {
        public SerializableActionDefine createActionDefine(int cardPoolId, string name)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            //分配Id
            int id = 0;
            for (int i = 0; i < cardPool.actionDefineList.Count; i++)
            {
                if (cardPool.actionDefineList[i].id > id)
                {
                    id = cardPool.actionDefineList[i].id;
                }
            }
            id++;
            SerializableActionDefine actionDefine = new SerializableActionDefine()
            {
                id = id,
                name = name
            };
            cardPool.actionDefineList.Add(actionDefine);
            return actionDefine;
        }
        public bool removeActionDefine(int cardPoolId, SerializableActionDefine actionDefine)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            return cardPool.actionDefineList.Remove(actionDefine);
        }
        public bool getActionDefine(int cardPoolId, int id, out SerializableActionDefine actionDefine)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            actionDefine = cardPool.actionDefineList.Find(a => a.id == id);
            return actionDefine != null;
        }
        public SerializableActionDefine[] getActionDefines(int cardPoolId)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            return cardPool.actionDefineList.ToArray();
        }
    }
}
