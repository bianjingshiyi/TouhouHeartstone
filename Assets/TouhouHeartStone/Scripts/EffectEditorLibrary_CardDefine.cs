using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    partial class EffectEditorLibrary
    {
        [TranslateMsg("编辑器-节点类型-卡牌定义")]
        private const string EDITOR_NODE_CATEGORY_CARD_DEFINE = "卡牌定义";


        /// 获取全部卡牌定义
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ALL_CARD_DEFINE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine[] getAllCardDefines(CardEngine game)
        {
            return game.getDefines();
        }


        /// 获取单个卡牌定义
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARD_DEFINE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getCardDefine(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_REFERENCE, true)] DefineReference cardRef)
        {
            if (cardRef == null)
                return null;
            return game.getDefine(cardRef.cardPoolId, cardRef.defineId);
        }


        /// 获取卡牌定义列表
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARD_DEFINE_ARRAY, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
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
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CARD_DEFINE_HAS_TAG, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool cardDefineHasTag(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TAG)] string tag)
        {
            if (cardDefine == null)
                return false;
            return cardDefine.hasTag(tag);
        }


        ///获取随机卡牌定义
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_RANDOM_CARD_DEFINE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getRandomCardDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine[] cardDefines)
        {
            if (cardDefines == null || cardDefines.Length < 1)
                return null;
            if (cardDefines.Length == 1)
                return cardDefines[0];
            return cardDefines[game.randomInt(0, cardDefines.Length - 1)];
        }


        [TranslateMsg("编辑器-动作节点-卡牌定义类型判断")]
        private const string EDITOR_NODE_NAME_IS_CARD_DEFINE_OF_TYPE = "卡牌定义类型判断";
        /// <summary>
        /// 卡牌定义类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ActionNodeMethod(EDITOR_NODE_NAME_IS_CARD_DEFINE_OF_TYPE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
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


        [TranslateMsg("编辑器-动作节点-卡牌定义关键词判断")]
        public const string EDITOR_NODE_CARD_DEFINE_HAS_KEYWORDS = "卡牌定义关键词判断";
        /// <summary>
        /// 卡牌关键词判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_HAS_KEYWORDS, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
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
        [TranslateMsg("动作图编辑器中动作节点名称之获取目标卡牌的定义")]
        public const string EDITOR_NODE_TARGET_CARD_DEFINE = "获取目标卡牌的定义列表";
        [ActionNodeMethod(EDITOR_NODE_TARGET_CARD_DEFINE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
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
        [TranslateMsg("动作图编辑器中动作节点名称之获取单张卡牌的定义")]
        public const string EDITOR_NODE_SPECIFIC_CARD_DEFINE = "获取单张卡牌的定义";
        [ActionNodeMethod(EDITOR_NODE_SPECIFIC_CARD_DEFINE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getSpecificCardDefine(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card?.define;
        }


        /// <summary>
        /// 变形为卡牌定义
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之变形为卡牌定义")]
        public const string EDITOR_NODE_CARD_SET_DEFINE = "变形为卡牌定义";
        [TranslateMsg("动作图编辑器中动作节点参数名称之是否重置卡牌")]
        public const string EDITOR_NODE_CARD_IS_RESET = "是否重置卡牌";
        [ActionNodeMethod(EDITOR_NODE_CARD_SET_DEFINE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> cardSetDefine(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine define,
            [ActionNodeParam(EDITOR_NODE_CARD_IS_RESET, isConst: true)] bool isreset)
        {
            if (define == null)
                return card;
            var setDefineEvent = await card.setDefine(game, define);
            if (isreset) { await card.resetCard(game); }
            return setDefineEvent?.card;
        }


        [TranslateMsg("编辑器-动作节点-获取卡牌定义属性")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_DEFINE = "获取卡牌定义属性";
        /// <summary>
        /// 获取属性
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_GET_DEFINE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getCardDefineProp(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            return card?.getProp<object>(propName);
        }
        //[TranslateMsg("编辑器-动作节点-设置卡牌定义属性")]
        //public const string EDITOR_NODE_CARD_DEFINE_SET_DEFINE = "设置卡牌定义属性";
        /// <summary>
        /// 设置属性
        /// </summary>
        //[ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_SET_DEFINE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        //[return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROP_CHANGE_EVENT)]
        //public static void setCardDefineProp(
        //    [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card,
        //    [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
        //    [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        //{
        //    card.setProp(propName, value);
        //}
        [TranslateMsg("编辑器-动作节点-获取卡牌定义花费")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_COST = "获取卡牌定义花费";
        /// <summary>
        /// 获取花费
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_GET_COST, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardDefineCost(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            if (card == null)
                return 0;
            return card.getCost();
        }
        [TranslateMsg("编辑器-动作节点-获取卡牌定义攻击力")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_ATTACK = "获取卡牌定义攻击力";
        /// <summary>
        /// 获取攻击
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_GET_ATTACK, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardDefineAttack(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            if (card == null)
                return 0;
            return card.getAttack();
        }
        [TranslateMsg("编辑器-动作节点-获取卡牌定义生命值")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_LIFE = "获取卡牌定义生命值";
        /// <summary>
        /// 获取最大生命值
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_GET_LIFE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardDefineMaxLife(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            if (card == null)
                return 0;
            return card.getLife();
        }

        [TranslateMsg("编辑器-动作节点-获取卡牌定义法术伤害")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_SPELL_DAMAGE = "获取卡牌定义法术伤害";
        [ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_GET_SPELL_DAMAGE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
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
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_IS_TOKEN, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
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
        [TranslateMsg("编辑器-动作节点-获取英雄牌技能")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_SKILL = "获取英雄牌技能";
        [ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_GET_SKILL, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
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
        [TranslateMsg("编辑器-动作节点-获取卡牌定义对应角色")]
        public const string EDITOR_NODE_CARD_DEFINE_GET_CHARACTER = "获取卡牌定义对应角色";
        [ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_GET_CHARACTER, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
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

        [TranslateMsg("编辑器-动作节点-卡牌是否可收藏")]
        public const string EDITOR_NODE_CARD_DEFINE_COLLECTIBLE = "卡牌是否可收藏";
        [TranslateMsg("编辑器-参数类型-可收藏")]
        public const string EDITOR_NODE_PARAM_NAME_COLLECTIBLE = "可收藏";
        [ActionNodeMethod(EDITOR_NODE_CARD_DEFINE_COLLECTIBLE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        [return: ActionNodeParam(EDITOR_NODE_PARAM_NAME_COLLECTIBLE)]
        public static bool isCardDefineCollectible(
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine card)
        {
            return !card.isMaster() && !card.isSkill() && !card.isToken();
        }
        /// <summary>
        /// 触发卡牌宣言
        /// </summary>
        [TranslateMsg("编辑器-动作节点-触发卡牌定义宣言")]
        public const string EDITOR_NODE_TRIGGER_CARD_DEFINE_BATTLECRY = "触发卡牌定义宣言";
        [ActionNodeMethod(EDITOR_NODE_TRIGGER_CARD_DEFINE_BATTLECRY, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
        public static async Task triggerActiveEffect(CardEngine game,
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
        /// 触发卡牌遗言
        /// </summary>
        [TranslateMsg("编辑器-动作节点-触发卡牌定义遗言")]
        public const string EDITOR_NODE_TRIGGER_CARD_DEFINE_DEATHRATTLE = "触发卡牌定义遗言";
        [ActionNodeMethod(EDITOR_NODE_TRIGGER_CARD_DEFINE_DEATHRATTLE, EDITOR_NODE_CATEGORY_CARD_DEFINE)]
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





