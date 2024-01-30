using System;
using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using Controllers;
using MukioI18n;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game
{
    public class EditorManager : Manager
    {
        #region 公有方法

        #region 类型
        public string[] getEditorPortTypeNames()
        {
            return GraphTypes.getAllPortTypeInfos().Select(i => game.langManager.getString(i.typeName)).ToArray();
        }
        public Color getColorByType(Type type) => GraphTypes.getColorByType(type);
        #endregion

        #region 值类型
        public string getDefaultValueTypeName()
        {
            return game.lang._(EDITOR_TYPE_NAME_UNKNOWN);
        }
        public string getNameByValueType(ActionNodeValueType type)
        {
            if (valueTypeNames.TryGetValue(type, out var str))
            {
                return game.lang._(str);
            }
            return getDefaultValueTypeName();
        }
        public ValueTypeNameMap[] getValueTypeNameMaps()
        {
            return valueTypeNames.Select(p => new ValueTypeNameMap(p.Key, game.lang._(p.Value))).ToArray();
        }
        #endregion

        #region 卡牌类型
        public string[] getCardTypes()
        {
            return cardTypes.Keys.ToArray();
        }
        public string[] getCardTypeNames()
        {
            return cardTypes.Values.Select(v => game.lang._(v)).ToArray();
        }
        public string getCardTypeName(string value)
        {
            if (cardTypes.TryGetValue(value, out var name))
            {
                return game.lang._(name);
            }
            return value;
        }
        #endregion

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

        #region 编辑器环境
        public void reloadCardPoolEnv(CardPoolInfo info)
        {
            clearCardPoolEnv();
            cardPoolEnv.Add(info.ToReference());
            if (info.dependencyRefs != null)
                cardPoolEnv.AddRange(info.dependencyRefs);
        }
        public void clearCardPoolEnv()
        {
            cardPoolEnv.Clear();
        }
        public ICardPoolReference[] getEnvCardPoolRefs()
        {
            return cardPoolEnv.ToArray();
        }
        #endregion

        #region 牌堆
        public string[] getAllPileNames()
        {
            return piles.Select(p => game.lang._(p.Value)).ToArray();
        }
        public string[] getAllPileValues()
        {
            return piles.Select(p => p.Key).ToArray();
        }
        public string getPileNameByValue(string value)
        {
            if (piles.TryGetValue(value, out var name))
            {
                return game.lang._(name);
            }
            return game.lang._(EDITOR_PILE_NAME_UNKNOWN);
        }
        #endregion

        #region 自定义属性
        public static ActionNodeValueType[] getCustomPropTypes()
        {
            return customPropTypes;
        }
        public string[] getCustomPropNames()
        {
            return customPropTypes.Select(t => getNameByValueType(t)).ToArray();
        }
        public static bool isTypeCustomProp(ActionNodeValueType type)
        {
            return customPropTypes.Contains(type);
        }
        #endregion

        #endregion
        #region 私有方法
        #endregion
        #region 属性字段
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

        [TranslateMsg("未知牌堆名称")]
        public const string EDITOR_PILE_NAME_UNKNOWN = "未知牌堆";
        public MainManager game
        {
            get { return _game; }
            set { _game = value; }
        }
        public string categoryFilter { get; set; }
        public bool[] outputFilter { get; set; }
        public string searchFilter { get; set; }

        private List<ICardPoolReference> cardPoolEnv = new List<ICardPoolReference>();

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

        private static readonly ActionNodeValueType[] customPropTypes = new ActionNodeValueType[]
        {
            ActionNodeValueType.integer,
            ActionNodeValueType.boolean,
            ActionNodeValueType.text,
            ActionNodeValueType.cardReference,
            ActionNodeValueType.pileNameCollection,
            ActionNodeValueType.keywords,
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
            KEYWORD.UNPLAYABLE,
            KEYWORD.CAN_BE_CHARGED,
            DEFINE.SPELL_DAMAGE,
            DEFINE.DAMAGE_REDUCE,
        };

        private static readonly Dictionary<string, string> piles = new Dictionary<string, string>
        {
            { PILE.FIELD, StringTable.GAME_FIELD },
            { PILE.HAND, StringTable.GAME_HAND },
            { PILE.DECK, StringTable.GAME_DECK },
            { PILE.GRAVE, StringTable.GAME_GRAVE},
            { PILE.MASTER, StringTable.GAME_MASTER },
            { PILE.SKILL, StringTable.GAME_SKILL },
            { PILE.ITEM, StringTable.GAME_ITEM },
            { PILE.HEAD, StringTable.GAME_DELAY }
        };
        private static readonly Dictionary<string, string> cardTypes = new Dictionary<string, string>
        {
            { DEFINE.SERVANT, StringTable.GAME_CARD_TYPE_SERVANT },
            { DEFINE.SPELL, StringTable.GAME_CARD_TYPE_SPELL },
            { DEFINE.ITEM, StringTable.GAME_CARD_TYPE_ITEM },
            { DEFINE.MASTER, StringTable.GAME_CARD_TYPE_MASTER },
            { DEFINE.SKILL, StringTable.GAME_CARD_TYPE_SKILL },
        };
        #endregion
    }
}
