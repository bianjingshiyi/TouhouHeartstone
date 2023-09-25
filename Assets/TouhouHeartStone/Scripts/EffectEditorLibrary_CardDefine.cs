using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    partial class EffectEditorLibrary
    {
        [TranslateMsg("编辑器-节点类型-卡牌定义")]
        public const string EDITOR_NODE_CATEGORY_CARD_DEFINE = "卡牌定义";
        [TranslateMsg("编辑器-动作节点-卡牌定义类型判断")]
        private const string EDITOR_NODE_NAME_IS_CARD_DEFINE_OF_TYPE = "卡牌定义类型判断";
        [TranslateMsg("编辑器-动作节点-卡牌定义关键词判断")]
        public const string EDITOR_NODE_CARD_DEFINE_HAS_KEYWORDS = "卡牌定义关键词判断";
        [TranslateMsg("动作图编辑器中动作节点名称之获取目标卡牌的定义")]
        public const string EDITOR_NODE_TARGET_CARD_DEFINE = "获取目标卡牌的定义列表";
        [TranslateMsg("动作图编辑器中动作节点名称之获取单张卡牌的定义")]
        public const string EDITOR_NODE_SPECIFIC_CARD_DEFINE = "获取单张卡牌的定义";
        [TranslateMsg("动作图编辑器中动作节点名称之变形为卡牌定义")]
        public const string EDITOR_NODE_CARD_SET_DEFINE = "变形为卡牌定义";
        [TranslateMsg("动作图编辑器中动作节点参数名称之是否重置卡牌")]
        public const string EDITOR_NODE_CARD_IS_RESET = "是否重置卡牌";
        [TranslateMsg("编辑器-动作节点-获取卡牌定义属性（过时）")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_PROP_OBSOLETE = "获取卡牌定义属性（过时）";
        [TranslateMsg("编辑器-动作节点-获取卡牌定义属性")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_PROP = "获取卡牌定义属性";
        [TranslateMsg("编辑器-动作节点-获取卡牌定义花费")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_COST = "获取卡牌定义花费";
        [TranslateMsg("编辑器-动作节点-获取卡牌定义攻击力")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_ATTACK = "获取卡牌定义攻击力";
        [TranslateMsg("编辑器-动作节点-获取卡牌定义生命值")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_LIFE = "获取卡牌定义生命值";
        [TranslateMsg("编辑器-动作节点-获取卡牌定义法术伤害")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_SPELL_DAMAGE = "获取卡牌定义法术伤害";
        [TranslateMsg("编辑器-动作节点-获取英雄牌技能")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_SKILL = "获取英雄牌技能";
        [TranslateMsg("编辑器-动作节点-获取卡牌定义对应角色")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_CHARACTER = "获取卡牌定义对应角色";
        [TranslateMsg("编辑器-动作节点-卡牌是否可收藏")]
        public const string EDITOR_NODE_CARD_DEFINE_COLLECTIBLE = "卡牌是否可收藏";
        [TranslateMsg("编辑器-参数类型-可收藏")]
        public const string EDITOR_NODE_PARAM_NAME_COLLECTIBLE = "可收藏";
        [TranslateMsg("编辑器-动作节点-触发卡牌定义宣言")]
        public const string EDITOR_NODE_TRIGGER_CARD_DEFINE_BATTLECRY = "触发卡牌定义宣言";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_CARD_DEFINE_HAS_BATTLECRY = "卡牌定义具有宣言";
        [TranslateMsg("编辑器-动作节点-触发卡牌定义遗言")]
        public const string EDITOR_NODE_TRIGGER_CARD_DEFINE_DEATHRATTLE = "触发卡牌定义遗言";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_CARD_DEFINE_HAS_DEATHRATTLE = "卡牌定义具有遗言";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_CARD_DEFINE_IS_TRAP = "卡牌定义是否是陷阱";
        [TranslateMsg("编辑器-动作节点-触发法术或技能卡牌定义效果")]
        public const string EDITOR_NODE_TRIGGER_SPELL_CARD_DEFINE = "触发法术或技能卡牌定义效果";
        [TranslateMsg("编辑器-节点名称-获取卡牌定义的标签列表")]
        public const string EDITOR_NODE_NAME_GET_CARD_DEFINE_TAG_LIST = "获取卡牌定义的标签列表";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_GET_CARD_DEFINE_TARGETS = "获取卡牌定义的合法使用目标";
        [TranslateMsg("编辑器-参数名称-标签列表")]
        public const string EDITOR_PARAM_NAME_TAG_LIST = "标签列表";
        [TranslateMsg("编辑器-参数名称-源卡牌")]
        public const string EDITOR_PARAM_NAME_SOURCE_CARD = "源卡牌";


        /// 获取全部卡牌定义
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetAllCardDefines, EDITOR_NODE_CATEGORY_CARD_DEFINE, StringTable.EDITOR_NODE_NAME_GET_ALL_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine[] getAllCardDefines(CardEngine game)
        {
            return game.getDefines();
        }


        /// 获取单个卡牌定义
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefine, EDITOR_NODE_CATEGORY_CARD_DEFINE, StringTable.EDITOR_NODE_NAME_GET_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getCardDefine(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_REFERENCE, true)] DefineReference cardRef)
        {
            if (cardRef == null)
                return null;
            return game.getDefine(cardRef.cardPoolId, cardRef.defineId);
        }


        /// 获取卡牌定义列表
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineArray, EDITOR_NODE_CATEGORY_CARD_DEFINE, StringTable.EDITOR_NODE_NAME_GET_CARD_DEFINE_ARRAY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine[] getCardDefineArray(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_REFERENCE, true)] DefineReference[] cardRefs)
        {
            if (cardRefs == null || cardRefs.Length < 1)
                return new CardDefine[0];
            CardDefine[] cardDefines = new CardDefine[cardRefs.Length];
            for (int i = 0; i < cardRefs.Length; i++)
            {
                cardDefines[i] = game.getDefine(cardRefs[i].cardPoolId, cardRefs[i].defineId);
            }
            return cardDefines;
        }


        /// 是否具有标签
        [ObsoletedKeyMethod(BuiltinActionDefineID.CardDefineHasTag, EDITOR_NODE_CATEGORY_CARD_DEFINE, StringTable.EDITOR_NODE_NAME_CARD_DEFINE_HAS_TAG)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool cardDefineHasTag(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TAG)] string tag)
        {
            if (cardDefine == null)
                return false;
            return cardDefine.hasTag(tag);
        }

        /// 获取卡牌定义标签列表
        [THHActionMethod(BuiltinActionDefineID.GetCardDefineTagList, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_NAME_GET_CARD_DEFINE_TAG_LIST)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_TAG_LIST)]
        public static string[] getCardDefineTagList(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (cardDefine == null)
                return Array.Empty<string>();
            return cardDefine.getTags();
        }

        ///获取随机卡牌定义
        [ObsoletedKeyMethod(ObsoletedActionDefineID.GetRandomCardDefine, EDITOR_NODE_CATEGORY_CARD_DEFINE, StringTable.EDITOR_NODE_NAME_GET_RANDOM_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        [Obsolete]
        public static CardDefine getRandomCardDefine(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine[] cardDefines)
        {
            if (cardDefines == null || cardDefines.Length < 1)
                return null;
            if (cardDefines.Length == 1)
                return cardDefines[0];
            return cardDefines[game.randomInt(0, cardDefines.Length - 1)];
        }


        /// <summary>
        /// 卡牌定义类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.IsCardDefineOfType, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_NAME_IS_CARD_DEFINE_OF_TYPE)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsCardDefineOfType(
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD_DEFINE)] CardDefine card,
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD_TYPE, isConst: true)] CardDefineSelect select)
        {
            if (card == null)
                return false;
            switch (select)
            {
                case CardDefineSelect.Spell:
                    return card.isSpell();
                case CardDefineSelect.Servant:
                    return card.isServant();
                case CardDefineSelect.Master:
                    return card.isMaster();
                case CardDefineSelect.Skill:
                    return card.isSkill();
                default:
                    return card.isItem();
            }
        }


        /// <summary>
        /// 卡牌关键词判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CardDefineHasKeyword, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_HAS_KEYWORDS)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool CardDefineHasKeywords(
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD_DEFINE)] CardDefine card,
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_KEYWORDS_TYPE, isConst: true)] string select)
        {
            if (card == null)
                return false;
            return card.hasKeyword(select);
        }


        /// <summary>
        /// 获取目标卡牌的定义列表
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefinesOfCardArray, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_TARGET_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine[] getTagetCardDefineArray(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null || cards.Length < 1)
                return null;
            CardDefine[] cardDefines = new CardDefine[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                cardDefines[i] = cards[i].define;
            }
            return cardDefines;
        }


        /// <summary>
        /// 获取单张卡牌的定义
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineOfCard, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_SPECIFIC_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getSpecificCardDefine(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card?.define;
        }


        /// <summary>
        /// 变形为卡牌定义
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetCardDefineOfCard, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_SET_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> cardSetDefine(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine define,
            [ActionNodeParam(EDITOR_NODE_CARD_IS_RESET, isConst: true)] bool isreset)
        {
            if (define == null)
                return card;
            var setDefineEvent = await card.setDefine(game, define, isreset);
            return setDefineEvent?.card;
        }


        /// <summary>
        /// 获取属性
        /// </summary>
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineProp_Obsolete, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_GET_PROP_OBSOLETE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getCardDefineProp(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            return card?.getProp<object>(propName);
        }
        /// <summary>
        /// 获取属性
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.GetCardDefineProp, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_GET_PROP)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getCardDefinePropByPropertyName(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] CardDefinePropertyGetterName propName)
        {
            return card?.getProp<object>(propName);
        }
        /// <summary>
        /// 获取花费
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineCost, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_GET_COST)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardDefineCost(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            if (card == null)
                return 0;
            return card.getCost();
        }
        /// <summary>
        /// 获取攻击
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineAttack, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_GET_ATTACK)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardDefineAttack(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            if (card == null)
                return 0;
            return card.getAttack();
        }
        /// <summary>
        /// 获取最大生命值
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineMaxLife, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_GET_LIFE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardDefineMaxLife(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            if (card == null)
                return 0;
            return card.getLife();
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineSpellDamage, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_GET_SPELL_DAMAGE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)]
        public static int getCardDefineSpellDamage(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            if (card == null)
                return 0;
            return card.getSpellDamage();
        }
        /// <summary>
        /// 是否是衍生牌
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.IsCardDefineToken, EDITOR_NODE_CATEGORY_CARD_DEFINE, StringTable.EDITOR_NODE_NAME_IS_TOKEN)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool isToken([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (cardDefine == null)
                return false;
            return cardDefine.isToken();
        }
        /// <summary>
        /// 获取技能
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineSkill, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_GET_SKILL)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getSkill(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            var skillRef = card?.getSkillRef();
            if (skillRef == null)
                return null;
            try
            {
                return game.getDefine(skillRef);
            }
            catch (UnknowDefineException)
            {
                return null;
            }
        }
        /// <summary>
        /// 获取对应角色（职业）
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineCharacter, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_GET_CHARACTER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getCardDefineCharacter(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            var characterRef = card?.getCharacterRef();
            if (characterRef == null)
                return null;
            try
            {
                return game.getDefine(characterRef);
            }
            catch (UnknowDefineException)
            {
                return null;
            }
        }

        [ObsoletedKeyMethod(ObsoletedActionDefineID.IsCardDefineCollectible, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_COLLECTIBLE)]
        [return: ActionNodeParam(EDITOR_NODE_PARAM_NAME_COLLECTIBLE)]
        [Obsolete]
        public static bool isCardDefineCollectible(
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            return !card.isMaster() && !card.isSkill() && !card.isToken();
        }
        /// <summary>
        /// 触发卡牌宣言
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.TriggerCardDefineBattlecries, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_TRIGGER_CARD_DEFINE_BATTLECRY)]
        public static async Task triggerCardDefineBattlecry(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (cards == null)
                return;
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                await card.triggerCardDefineBattlecries(game, card.getOwner(), cardDefine, targets);
            }
        }
        /// <summary>
        /// 卡牌定义具有宣言
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CardDefineHasBattlecry, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_HAS_BATTLECRY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool cardDefineHasBattlecry(
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (cardDefine == null)
                return false;
            return cardDefine.hasBattlecry();
        }
        /// <summary>
        /// 触发卡牌遗言
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.TriggerCardDefineDeathrattles, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_TRIGGER_CARD_DEFINE_DEATHRATTLE)]
        public static async Task triggerCardDefineDeathRattles(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return;
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                await card.triggerCardDefineDeathRattles(game, cardDefine);
            }
        }
        /// <summary>
        /// 卡牌定义具有遗言
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CardDefineHasDeathrattle, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_HAS_DEATHRATTLE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool cardDefineHasDeathrattle(
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (cardDefine == null)
                return false;
            return cardDefine.hasDeathRattles();
        }
        /// <summary>
        /// 触发法术或技能卡牌定义效果
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.TriggerCardDefineActiveEffect, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_TRIGGER_SPELL_CARD_DEFINE)]
        public static async Task activeSpellCardDefineEffect(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (cards == null)
                return;
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                await card.activeSpellCardDefineEffect(game, card.getOwner(), cardDefine, targets);
            }
        }
        /// <summary>
        /// 卡牌定义是否是陷阱
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.CardDefineIsTrap, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_CARD_DEFINE_IS_TRAP)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool cardDefineIsTrap(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (cardDefine == null)
                return false;
            return cardDefine.isTrap();
        }
        /// <summary>
        /// 获取卡牌定义合法目标集合
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.GetValidCardDefineTargets, EDITOR_NODE_CATEGORY_CARD_DEFINE, EDITOR_NODE_GET_CARD_DEFINE_TARGETS)]
        [return: ActionNodeParam(EDITOR_NODE_GET_CARD_TARGETS_TARGETS)]
        public static Card[] getAllLegalCardDefineTargets(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(EDITOR_PARAM_NAME_SOURCE_CARD)] Card source)
        {
            if (cardDefine == null || source == null)
                return Array.Empty<Card>();
            return cardDefine.getAvaliableTargets(game, source);
        }
    }
    public enum CardDefineSelect
    {
        Spell,
        Servant,
        Item,
        Master,
        Skill
    }
}





