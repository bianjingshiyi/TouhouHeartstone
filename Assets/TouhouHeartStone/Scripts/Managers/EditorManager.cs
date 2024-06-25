using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using MukioI18n;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game
{
    public partial class EditorManager : Manager
    {
        #region 公有方法

        #region 类型
        public string[] getEditorPortTypeNames()
        {
            return GraphTypes.getAllPortTypeInfos().Select(i => game.lang._p(StringTable.CONTEXT_EDITOR_TYPE, i.typeName)).ToArray();
        }
        public Color getColorByType(Type type) => GraphTypes.getColorByType(type);
        #endregion

        #region 值类型
        public string getDefaultValueTypeName()
        {
            return game.lang._p(StringTable.CONTEXT_EDITOR_TYPE, StringTable.EDITOR_TYPE_NAME_UNKNOWN);
        }
        public string getNameByValueType(ActionNodeValueType type)
        {
            if (valueTypeNames.TryGetValue(type, out var str))
            {
                return game.lang._p(StringTable.CONTEXT_EDITOR_TYPE, str);
            }
            return getDefaultValueTypeName();
        }
        public ValueTypeNameMap[] getValueTypeNameMaps()
        {
            return valueTypeNames.Select(p => new ValueTypeNameMap(p.Key, game.lang._p(StringTable.CONTEXT_EDITOR_TYPE, p.Value))).ToArray();
        }
        #endregion

        #region 卡牌类型
        public string[] getCardTypes()
        {
            return cardTypes.Keys.ToArray();
        }
        public string[] getCardTypeNames()
        {
            return cardTypes.Values.Select(v => game.lang._p(StringTable.CONTEXT_CARD_TYPE_NAME, v)).ToArray();
        }
        public string getCardTypeName(string value)
        {
            if (cardTypes.TryGetValue(value, out var name))
            {
                return game.lang._p(StringTable.CONTEXT_CARD_TYPE_NAME, name);
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

        public CardPropertyNameItem[] getTranslatedCardProperties()
        {
            return cardProps.Select(i =>
            {
                i.name = game.lang._p(StringTable.CONTEXT_CARD_PROPERTY, i.name);
                return i;
            }).ToArray();
        }

        #region 牌堆
        public string getAnyPileName()
        {
            return game.lang._(StringTable.PILE_NAME_ANY, StringTable.CONTEXT_PILE_NAME);
        }
        public string[] getAllPileNames()
        {
            return piles.Select(p => game.lang._p(StringTable.CONTEXT_PILE_NAME, p.Value)).ToArray();
        }
        public string[] getAllPileValues()
        {
            return piles.Select(p => p.Key).ToArray();
        }
        public string getPileNameByValue(string value)
        {
            if (piles.TryGetValue(value, out var name))
            {
                return game.lang._p(StringTable.CONTEXT_PILE_NAME, name);
            }
            return game.lang._p(StringTable.CONTEXT_PILE_NAME, EDITOR_PILE_NAME_UNKNOWN);
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
        private void Awake()
        {
            Awake_Nodes();
        }
        #endregion
        #region 属性字段

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

        [SerializeField]
        MainManager _game;
        static readonly Dictionary<ActionNodeValueType, string> valueTypeNames = new Dictionary<ActionNodeValueType, string>()
        {
            { ActionNodeValueType.any, StringTable.EDITOR_TYPE_NAME_OBJECT },
            { ActionNodeValueType.boolean, StringTable.EDITOR_TYPE_NAME_BOOL },
            { ActionNodeValueType.integer, StringTable.EDITOR_TYPE_NAME_INT },
            { ActionNodeValueType.text, StringTable.EDITOR_TYPE_NAME_STRING },
            { ActionNodeValueType.keyword, StringTable.EDITOR_TYPE_NAME_KEYWORD },
            { ActionNodeValueType.cardTag, StringTable.EDITOR_TYPE_NAME_CARD_TAG },
            { ActionNodeValueType.cardReference, StringTable.EDITOR_TYPE_NAME_CARD_REFERENCE },
            { ActionNodeValueType.cardPoolReference, StringTable.EDITOR_TYPE_NAME_CARD_POOL_REFERENCE },
            { ActionNodeValueType.buffReference, StringTable.EDITOR_TYPE_NAME_BUFF_REFERENCE },
            { ActionNodeValueType.pileNameCollection, StringTable.EDITOR_TYPE_NAME_PILE },
            { ActionNodeValueType.keywordCollection, StringTable.EDITOR_TYPE_NAME_KEYWORD_COLLECTION },
            { ActionNodeValueType.cardTagCollection, StringTable.EDITOR_TYPE_NAME_CARD_TAG_COLLECTION }
        };

        private static readonly ActionNodeValueType[] customPropTypes = new ActionNodeValueType[]
        {
            ActionNodeValueType.integer,
            ActionNodeValueType.boolean,
            ActionNodeValueType.text,
            ActionNodeValueType.keyword,
            ActionNodeValueType.cardTag,
            ActionNodeValueType.cardReference,
            ActionNodeValueType.cardPoolReference,
            ActionNodeValueType.pileNameCollection,
            ActionNodeValueType.keywordCollection,
            ActionNodeValueType.cardTagCollection,
            ActionNodeValueType.buffReference,
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
        };
        public static readonly List<string> editorCardTags = new List<string>
        {
            TAG.FAIRY,
            TAG.MACHINE,
            TAG.DEMON,
            TAG.ELEMENT,
            TAG.BEAST,
            TAG.GOD,
            TAG.GHOST,
        };
        public static readonly List<CardPropertyNameItem> cardProps = new List<CardPropertyNameItem>
        {
            new CardPropertyNameItem(CARD.ISDEAD, StringTable.CARD_PROP_NAME_ISDEAD, true, true, false, false),
            new CardPropertyNameItem(CARD.CURRENT_LIFE, StringTable.CARD_PROP_NAME_CURRENT_LIFE, true, true, false, false),
            new CardPropertyNameItem(CARD.ARMOR, StringTable.CARD_PROP_NAME_ARMOR, true, true, false, false),
            new CardPropertyNameItem(CARD.SILENCE, StringTable.CARD_PROP_NAME_SILENCED, true, true, false, false),
            new CardPropertyNameItem(CARD.ISREADY, StringTable.CARD_PROP_NAME_ISREADY, true, true, false, false),
            new CardPropertyNameItem(CARD.ATTACK_TIMES, StringTable.CARD_PROP_NAME_ATTACK_TIMES, true, true, false, false),
            new CardPropertyNameItem(CARD.HAS_STEALTH, StringTable.CARD_PROP_NAME_HAS_STEALTH, true, true, false, false),
            new CardPropertyNameItem(CARD.HAS_SHIELD, StringTable.CARD_PROP_NAME_HAS_BARRIER, true, true, false, false),
            new CardPropertyNameItem(CARD.IS_FROZEN, StringTable.CARD_PROP_NAME_IS_SHACKLED, true, true, false, false),

            // 只读
            new CardPropertyNameItem(DEFINE.ATTACK, StringTable.CARD_PROP_NAME_ATTACK),
            new CardPropertyNameItem(DEFINE.COST, StringTable.CARD_PROP_NAME_COST),
            new CardPropertyNameItem(DEFINE.LIFE, StringTable.CARD_PROP_NAME_LIFE),
            new CardPropertyNameItem(DEFINE.DAMAGE_REDUCE, StringTable.CARD_PROP_NAME_DAMAGE_REDUCE),
            new CardPropertyNameItem(DEFINE.SPELL_DAMAGE, StringTable.CARD_PROP_NAME_SPELL_DAMAGE),
        };

        private static readonly Dictionary<string, string> piles = new Dictionary<string, string>
        {
            { PILE.INIT, StringTable.PILE_NAME_INIT },
            { PILE.WARP, StringTable.PILE_NAME_WARP },
            { PILE.FIELD, StringTable.PILE_NAME_FIELD },
            { PILE.HAND, StringTable.PILE_NAME_HAND },
            { PILE.DECK, StringTable.PILE_NAME_DECK },
            { PILE.GRAVE, StringTable.PILE_NAME_GRAVE},
            { PILE.MASTER, StringTable.PILE_NAME_MASTER },
            { PILE.SKILL, StringTable.PILE_NAME_SKILL },
            { PILE.ITEM, StringTable.PILE_NAME_ITEM },
            { PILE.HEAD, StringTable.PILE_NAME_DELAY }
        };
        private static readonly Dictionary<string, string> cardTypes = new Dictionary<string, string>
        {
            { DEFINE.SERVANT, StringTable.CARD_TYPE_SERVANT },
            { DEFINE.SPELL, StringTable.CARD_TYPE_SPELL },
            { DEFINE.ITEM, StringTable.CARD_TYPE_ITEM },
            { DEFINE.MASTER, StringTable.CARD_TYPE_MASTER },
            { DEFINE.SKILL, StringTable.CARD_TYPE_SKILL },
        };
        #endregion
    }
}
