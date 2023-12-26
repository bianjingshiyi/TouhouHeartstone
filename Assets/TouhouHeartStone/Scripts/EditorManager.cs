using System;
using System.Collections.Generic;
using BJSYGameCore;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Card = TouhouCardEngine.Card;

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
            throw new IndexOutOfRangeException($"无法找到索引{index}对应变量类型");
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
            throw new ArgumentException($"不存在名为{name}的类型");
        }
        public string getNameByValueType(ActionNodeValueType type)
        {
            if (valueTypeNames.TryGetValue(type, out var str))
            {
                return game.lang._(str);
            }
            return game.lang._(EDITOR_TYPE_NAME_UNKNOWN);
        }
        public string getRuleLogicTypeEditorName(GameRuleLogicType type)
        {
            string str = null;
            switch (type)
            {
                case GameRuleLogicType.GameStart:
                    str = RULE_NAME_POST_GAME_START;
                    break;
                case GameRuleLogicType.PlayerEnter:
                    str = RULE_NAME_POST_PLAYER_ENTER;
                    break;
                case GameRuleLogicType.PlayerExit:
                    str = RULE_NAME_POST_PLAYER_EXIT;
                    break;
                case GameRuleLogicType.GameEnd:
                    str = RULE_NAME_POST_GAME_END;
                    break;
            }
            if (str != null)
            {
                return game.lang._(str);
            }
            return game.lang._(RULE_NAME_UNKNOWN);
        }
        #endregion
        #region 私有方法
        #endregion
        #region 属性字段
        [TranslateMsg("编辑器类型名称")]
        public const string EDITOR_TYPE_NAME_CARD_PROP_GETTER_NAME = "卡牌属性名（获取）";
        [TranslateMsg("编辑器类型名称")]
        public const string EDITOR_TYPE_NAME_CARD_PROP_SETTER_NAME = "卡牌属性名（设置）";
        [TranslateMsg("编辑器类型名称")]
        public const string EDITOR_TYPE_NAME_CARD_DEFINE_PROP_GETTER_NAME = "卡牌定义属性名（获取）";
        [TranslateMsg("编辑器类型名称")]
        public const string EDITOR_TYPE_NAME_EFFECT_TAG = "效果标签";
        [TranslateMsg("编辑器类型名称")]
        public const string EDITOR_TYPE_NAME_EVENT_VAR_NAME = "事件变量名";
        [TranslateMsg("编辑器类型名称")]
        public const string EDITOR_TYPE_NAME_PILE_NAME = "牌堆名";
        [TranslateMsg("编辑器类型名称")]
        public const string EDITOR_TYPE_NAME_BUFF_REFERENCE = "增益定义引用";
        [TranslateMsg("编辑器类型名称")]
        public const string EDITOR_TYPE_NAME_UNKNOWN = "未知类型";

        [TranslateMsg("游戏规则逻辑类型")]
        public const string RULE_NAME_POST_GAME_START = "游戏开始后";
        [TranslateMsg("游戏规则逻辑类型")]
        public const string RULE_NAME_POST_PLAYER_ENTER = "玩家加入后";
        [TranslateMsg("游戏规则逻辑类型")]
        public const string RULE_NAME_POST_PLAYER_EXIT = "玩家离开后";
        [TranslateMsg("游戏规则逻辑类型")]
        public const string RULE_NAME_POST_GAME_END = "游戏结束后";
        [TranslateMsg("游戏规则逻辑类型")]
        public const string RULE_NAME_UNKNOWN = "未知规则";
        public MainManager game
        {
            get { return _game; }
            set { _game = value; }
        }
        public string categoryFilter { get; set; }
        public bool[] outputFilter { get; set; }
        public string searchFilter { get; set; }
        [SerializeField]
        MainManager _game;
        static readonly Dictionary<ActionNodeValueType, string> valueTypeNames = new Dictionary<ActionNodeValueType, string>()
        {
            { ActionNodeValueType.any, StringTable.EDITOR_TYPE_NAME_OBJECT },
            { ActionNodeValueType.boolean, StringTable.EDITOR_TYPE_NAME_BOOL },
            { ActionNodeValueType.integer, StringTable.EDITOR_TYPE_NAME_INT },
            { ActionNodeValueType.text, StringTable.EDITOR_TYPE_NAME_STRING },
            { ActionNodeValueType.cardReference, StringTable.EDITOR_TYPE_NAME_CARD_REFERENCE },
            { ActionNodeValueType.buffReference, EDITOR_TYPE_NAME_BUFF_REFERENCE },
            { ActionNodeValueType.pileNameCollection, StringTable.EDITOR_TYPE_NAME_PILE },
            { ActionNodeValueType.keywords, StringTable.EDITOR_TYPE_NAME_KEYWORDS_TYPE }
        };
        static EditorTypeInfo[] _editorTypeInfos = new EditorTypeInfo[]
        {
            #region 基础类型
            new EditorTypeInfo() { type = typeof(object), typeName = StringTable.EDITOR_TYPE_NAME_OBJECT, isVarType = true, color = new Color(1, 1, 1, 1) },
            new EditorTypeInfo() { type = typeof(int), typeName = StringTable.EDITOR_TYPE_NAME_INT, isVarType = true, color = new Color(.2f, 1, .2f, 1) },
            new EditorTypeInfo() { type = typeof(float), typeName = StringTable.EDITOR_TYPE_NAME_FLOAT, isVarType = true, color = new Color(.4f, 1, .6f, 1) },
            new EditorTypeInfo() { type = typeof(bool), typeName = StringTable.EDITOR_TYPE_NAME_BOOL, isVarType = true, color = new Color(.6f, 1, .8f, 1) },
            new EditorTypeInfo() { type = typeof(char), typeName = StringTable.EDITOR_TYPE_NAME_CHAR, isVarType = true, color = new Color(.5f, .8f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(string), typeName = StringTable.EDITOR_TYPE_NAME_STRING, isVarType = true, color = new Color(.7f, .8f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(Array), typeName = StringTable.EDITOR_TYPE_NAME_ARRAY, isVarType = true, color = new Color(.8f, .8f, .8f, 1) },
            #endregion
            #region 引擎类型
            new EditorTypeInfo() { type = typeof(Player), typeName = StringTable.EDITOR_TYPE_NAME_PLAYER, isVarType = true, color = new Color(.8f, .2f, 1, 1) },
            new EditorTypeInfo() { type = typeof(Pile), typeName = StringTable.EDITOR_TYPE_NAME_PILE, isVarType = true, color = new Color(.6f, .2f, 1, 1) },
            new EditorTypeInfo() { type = typeof(CardDefine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_DEFINE, isVarType = true, color = new Color(.1f, .4f, .8f, 1) },
            new EditorTypeInfo() { type = typeof(Card), typeName = StringTable.EDITOR_TYPE_NAME_CARD, isVarType = true, color = new Color(.2f, .6f, 1, 1) },
            new EditorTypeInfo() { type = typeof(CardSnapshot), typeName = EffectEditorLibrary.EDITOR_VAR_TYPE_CARD_SNAPSHOT, isVarType = true, color = new Color(.5f, .6f, 7f, 1) },
            new EditorTypeInfo() { type = typeof(BuffDefine), typeName = StringTable.EDITOR_TYPE_NAME_BUFF_DEFINE, isVarType = true, color = new Color(.1f, .5f, .6f, 1) },
            new EditorTypeInfo() { type = typeof(Buff), typeName = StringTable.EDITOR_TYPE_NAME_BUFF, isVarType = true, color = new Color(.2f, .8f, 1, 1) },
            new EditorTypeInfo() { type = typeof(IEffect), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT, isVarType = true, color = new Color(.2f, 1, 1, 1) },
            
            // 事件
            new EditorTypeInfo() { type = typeof(EventArg), typeName = StringTable.EDITOR_TYPE_NAME_EVENT_ARG, isVarType = true, color = new Color(1, 1, .2f, 1) },
            new EditorTypeInfo() { type = typeof(EventRecord), typeName = EffectEditorLibrary.EDITOR_TYPE_NAME_EVENT_RECORD, isVarType = true, color = new Color(.9f, .9f, .4f, 1) },
            #endregion

            #region 类型代理
            new EditorTypeInfo() { type = typeof(CardPropertyGetterName), typeName = EDITOR_TYPE_NAME_CARD_PROP_GETTER_NAME, color = new Color(.7f, .8f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(CardPropertySetterName), typeName = EDITOR_TYPE_NAME_CARD_PROP_SETTER_NAME, color = new Color(.7f, .8f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(CardDefinePropertyGetterName), typeName = EDITOR_TYPE_NAME_CARD_DEFINE_PROP_GETTER_NAME, color = new Color(.7f, .8f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(EffectTag), typeName = EDITOR_TYPE_NAME_EFFECT_TAG, color = new Color(.7f, .8f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(EventVarName), typeName = EDITOR_TYPE_NAME_EVENT_VAR_NAME, color = new Color(.7f, .8f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(PileName), typeName = EDITOR_TYPE_NAME_PILE_NAME, color = new Color(.7f, .8f, .2f, 1) },

            new EditorTypeInfo() { type = typeof(EventReference), typeName = EffectEditorLibrary.EDITOR_TYPE_NAME_EVENT_TYPE, isVarType = true, color = new Color(1, 1, .2f, 1) },
            #endregion

            #region 隐藏类型
            new EditorTypeInfo() { type = typeof(Rule), typeName = StringTable.EDITOR_TYPE_NAME_RULE, color = new Color(.2f, .2f, .6f, 1) },
            new EditorTypeInfo() { type = typeof(CardEngine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_ENGINE, color = new Color(.2f, .2f, 1, 1) },
            new EditorTypeInfo() { type = typeof(DefineReference), typeName = StringTable.EDITOR_TYPE_NAME_CARD_REFERENCE, color = new Color(.1f, .3f, .6f, 1) },
            new EditorTypeInfo() { type = typeof(PileNameCollection), typeName = StringTable.EDITOR_TYPE_NAME_PILE_NAME_COLLECTION, color = new Color(.2f, .1f, .6f, 1) },
            new EditorTypeInfo() { type = typeof(EffectTagCollection), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT_TAG_COLLECTION, color = new Color(.2f, .5f, .5f, 1) },

            new EditorTypeInfo() { type = typeof(PropModifier), typeName = StringTable.EDITOR_TYPE_NAME_PROP_MODIFIER, color = new Color(1, .2f, .8f, 1) },
            new EditorTypeInfo() { type = typeof(TriggerGraph), typeName = StringTable.EDITOR_TYPE_NAME_TRIGGER_GRAPH, color = new Color(1, .8f, .2f, 1) },
            //typeof(TriggerCollection),
            new EditorTypeInfo() { type = typeof(TargetChecker), typeName = StringTable.EDITOR_TYPE_NAME_TARGET_CHECKER, color = new Color(1, .6f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(NodeValueRef), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_VALUE_REF, color = new Color(1, .4f, .2f, 1) },
            new EditorTypeInfo() { type = typeof(ActionDefine), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_DEFINE, color = new Color(.6f, .1f, .1f, 1) },
            new EditorTypeInfo() { type = typeof(ActionNode), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_NODE, color = new Color(1, .2f, .2f, 1) },
            
            #endregion

        };

        public static readonly List<string> editorKeywords = new List<string>
        {
            KEYWORD.TAUNT,
            KEYWORD.CHARGE,
            KEYWORD.RUSH,
            KEYWORD.SHIELD,
            KEYWORD.STEALTH,
            KEYWORD.PERMANENTSTEALTH,
            KEYWORD.DRAIN,
            KEYWORD.POISONOUS,
            KEYWORD.ELUSIVE,
            KEYWORD.FROZEN,
            KEYWORD.UNIQUE,
            KEYWORD.AUTOCAST,
            KEYWORD.CANTATTACK,
            KEYWORD.WINDFURY,
            KEYWORD.IMMUNITY,
            KEYWORD.MISSION,
            KEYWORD.PERMANENT_FROZEN,
            KEYWORD.CAN_BE_CHARGED,
            DEFINE.SPELL_DAMAGE,
            DEFINE.DAMAGE_REDUCE,
        };
        #endregion
        #region 内部类
        [Serializable]
        struct EditorTypeInfo
        {
            public int id;
            public Type type;
            public string typeName;
            public bool isVarType;
            public Color color;
        }
        #endregion
    }
}
