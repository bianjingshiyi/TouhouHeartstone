using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using MukioI18n;

namespace Game
{
    partial class EffectEditorLibrary
    {
        /// 获取全部卡牌定义
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ALL_CARD_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine[] getAllCardDefines(CardEngine game)
        {
            return game.getDefines();
        }
        /// 获取单个卡牌定义
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARD_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getCardDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_REFERENCE, true)] CardReference cardRef)
        {
            return game.getDefine(cardRef.cardId);
        }
        /// 获取卡牌定义列表
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARD_DEFINE_ARRAY, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine[] getCardDefineArray(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_REFERENCE, true)] CardReference[] cardRefs)
        {
            if (cardRefs == null || cardRefs.Length < 1)
                return new CardDefine[0];
            CardDefine[] cardDefines = new CardDefine[cardRefs.Length];
            for (int i = 0; i < cardRefs.Length; i++)
            {
                cardDefines[i] = game.getDefine(cardRefs[i].cardId);
            }
            return cardDefines;
        }
        /// 是否具有标签
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CARD_DEFINE_HAS_TAG, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool hasTag(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TAG)] string tag)
        {
            return cardDefine.hasTag(tag);
        }
        ///获取随机卡牌定义
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_RANDOM_CARD_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getRandomCardDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine[] cardDefines)
        {
            if (cardDefines == null || cardDefines.Length < 1)
                return null;
            if (cardDefines.Length == 1)
                return cardDefines[0];
            return cardDefines[game.randomInt(0, cardDefines.Length - 1)];
        }
        /// <summary>
        /// 卡牌类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE__IS_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsCardDefine(
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD)] Card card, 
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD_TYPE, isConst: true)] CardDefineSelect select)
        {
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
        [TranslateMsg("动作图编辑器中动作节点名称之卡牌关键词判断")]
        public const string EDITOR_NODE_IS_KEYWORDS = "卡牌关键词判断";
        [ActionNodeMethod(EDITOR_NODE_IS_KEYWORDS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsKeywords(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_KEYWORDS_TYPE, isConst: true)] string select)
        {
            return card.hasKeyword(game, select);
        }
        /// <summary>
        /// 获取目标卡牌的定义
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        [TranslateMsg("动作图编辑器中动作节点名称之获取目标卡牌的定义")]
        public const string EDITOR_NODE_TARGET_CARD_DEFINE = "获取目标卡牌的定义列表";
        [ActionNodeMethod(EDITOR_NODE_TARGET_CARD_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine[] getTagetCardDefineArray(CardEngine game,
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
        /// 变形为卡牌定义
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之变形为卡牌定义")]
        public const string EDITOR_NODE_CARD_SET_DEFINE = "变形为卡牌定义";
        [TranslateMsg("动作图编辑器中动作节点参数名称之是否重置卡牌")]
        public const string EDITOR_NODE_CARD_IS_RESET = "是否重置卡牌";
        [ActionNodeMethod(EDITOR_NODE_CARD_SET_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> cardSetDefine(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine define,
            [ActionNodeParam(EDITOR_NODE_CARD_IS_RESET, isConst: true)] bool isreset)
        {
            var setDefineEvent = await card.setDefine(game, define);
            if (isreset) { await card.resetCard(game); }
            return setDefineEvent?.card;
        }
        /// <summary>
        /// 获取随机卡牌
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_RANDOM_CARD, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getRandomCard(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            return cards.random(game);
        }
        /// <summary>
        /// 创建衍生物并置入战场
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_FIELD, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToField(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            var createTokenEvent = await player.createToken(game, cardDefine);
            return createTokenEvent?.card;
        }
        /// <summary>
        /// 创建衍生卡并置入手牌
        /// </summary>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_HAND, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToHand(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (player.getHand().isFull)
                return null;
            Card createdCard = game.createCard(cardDefine);
            await player.getHand().add(game, createdCard);
            return createdCard;
        }
        /// <summary>
        /// 创建衍生卡并洗入牌库
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_SHUFFLE_TO_DECK, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardShuffleToDeck(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player, 
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            Card createdCard = game.createCard(cardDefine);

            await player.getDeck().insert(game, createdCard, game.randomInt(0, player.getDeck().count));
            return createdCard;
        }
        /// <summary>
        /// 这张卡牌
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_THIS_CARD, StringTable.EDITOR_NODE_CATEGORY_CARD, "ThisCard")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card thisCard(Card card)
        {
            return card;
        }
        /// <summary>
        /// 获取属性
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PROPERTY, StringTable.EDITOR_NODE_CATEGORY_CARD, "GetProperty")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            return card.getProp(game, propName);
        }
        /// <summary>
        /// 设置属性
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SET_PROPERTY, StringTable.EDITOR_NODE_CATEGORY_CARD, "SetProperty")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROP_CHANGE_EVENT)]
        public static Task<IPropChangeEventArg> setProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            return card.setProp(game, propName, value);
        }
        /// <summary>
        /// 获取花费
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_COST, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCost(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getCost(game);
        }
        /// <summary>
        /// 获取攻击
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ATTACK, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getAttack(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getAttack(game);
        }
        /// <summary>
        /// 获取最大生命值
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_MAX_LIFE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getMaxLife(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getLife(game);
        }
        /// <summary>
        /// 获取当前生命值
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之获取当前生命值")]
        public const string EDITOR_NODE_NAME_GET_CURRENT_LIFE = "获取当前生命值";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_CURRENT_LIFE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getLife(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getCurrentLife(game);
        }
        /// <summary>
        /// 获取一个独一无二的属性名。当你在一段可以重复生效的效果中为卡牌设置额外属性的时候，应该使用这个方法。
        /// 注意，这个方法是为了避免属性名之间的冲突，在不会发生冲突的情况下，输入相同的参数仍然会让这个方法返回相同的属性名。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <param name="eventArg"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME, StringTable.EDITOR_NODE_CATEGORY_CARD, "GetUniquePropertyName")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)]
        public static string getUniquePropName(IBuff buff, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            if (buff != null)
                propName = buff.instanceID + propName;
            return propName;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_OWNER, StringTable.EDITOR_NODE_CATEGORY_CARD, "GetOwner")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]
        public static Player getOwner([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.owner;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_SPELL_DAMAGE, StringTable.EDITOR_NODE_CATEGORY_PLAYER, "GetSpellDamage")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)]
        public static int getSpellDamage(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BASE_DAMAGE)] int baseDamage)
        {
            return player.getSpellDamage(game, baseDamage);
        }
        /// <summary>
        /// 这是不是一张法术牌
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_IS_SPELL, StringTable.EDITOR_NODE_CATEGORY_CARD, "IsSpell")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool isSpell([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.define.type == DEFINE.SPELL;
        }
        /// <summary>
        /// 是否是衍生牌
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_IS_TOKEN, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool isToken([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            return cardDefine.isToken();
        }
        /// <summary>
        /// 对目标造成固定的法术伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="damage"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_TARGETS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg> dealSpecificSpellDamageToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            return targets.damage(game, card, card.owner.getSpellDamage(game, damage));
        }
        /// <summary>
        /// 造成法术伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="damage"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        [TranslateMsg("动作图编辑器中动作节点名称之造成法术伤害")]
        public const string EDITOR_NODE_NAME_DEAL_SPELL_DAMAGE = "造成法术伤害";
        [ActionNodeMethod(EDITOR_NODE_NAME_DEAL_SPELL_DAMAGE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg> dealSpellDamageToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: false)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            return targets.damage(game, card, card.owner.getSpellDamage(game, damage));
        }
        /// <summary>
        /// 造成固定法术伤害并分配给随机目标
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="damage"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_RANDOM_TARGETS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealSpecificSpellDamageToRandomTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            return targets.damageByRandom(game, card, getSpellDamage(game, card.owner, damage));
        }
        /// <summary>
        /// 对固定数量的随机敌人造成固定伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="count"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg> dealSpecificDamageToSpecificNumberOfRandomTarget(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            return targets.damageByRandomCount(game, card, damage, count);
        }
        /// <summary>
        /// 对固定数量的随机敌人造成固定法术伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="count"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg> dealSpecificSpellDamageToSpecificNumberOfRandomTarget(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            return targets.damageByRandomCount(game, card, card.owner.getSpellDamage(game, damage), count);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_HEAL_TARGETS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HEAL_EVENT)]
        public static Task<THHCard.HealEventArg> healTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            return targets.heal(game, card, value);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_HEAL_SPECIFIC_AMOUNT_TO_TARGETS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HEAL_EVENT)]
        public static Task<THHCard.HealEventArg> healSpecificAmountToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE, isConst: true)] int value)
        {
            return targets.heal(game, card, value);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ALL_SERVANTS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getAllServants(CardEngine game)
        {
            List<Card> cardList = new List<Card>();
            foreach (var player in game.players)
            {
                cardList.AddRange(player.getField());
            }
            return cardList.ToArray();
        }
        /// <summary>
        /// 获取敌方随从
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ENEMY_SERVANTS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getEnemyServants(CardEngine game, Card card)
        {
            return game.getOpponent(card.owner).getField();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_FRIEND_CHARACTERS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getFriendCharacters(Card card)
        {
            Card master = card.owner.getMaster();
            Card[] servants = card.owner.getField();
            Card[] characters = new Card[servants.Length + 1];
            characters[0] = master;
            Array.Copy(servants, 0, characters, 1, servants.Length);
            return characters;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ENEMY_CHARACTERS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getEnemyCharacters(CardEngine game, Card card)
        {
            Card master = game.getOpponent(card.owner).getMaster();
            Card[] servants = game.getOpponent(card.owner).getField();
            Card[] characters = new Card[servants.Length + 1];
            characters[0] = master;
            Array.Copy(servants, 0, characters, 1, servants.Length);
            return characters;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ALL_CHARACTERS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getAllCharacters(CardEngine game)
        {
            List<Card> cardList = new List<Card>();
            foreach (var player in game.players)
            {
                cardList.Add(player.getMaster());
                cardList.AddRange(player.getField());
            }
            return cardList.ToArray();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_FREEZE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROP_CHANGE_EVENT)]
        public static Task<IPropChangeEventArg> freeze(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.setFreeze(game, true);
        }
        /// <summary>
        /// 沉默目标卡牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SILENCE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_SILENCE_EVENT)]
        public static Task<THHCard.SilenceEventArg> silence(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            return cards.silence(game);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DESTROY, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DEATH_EVENT)]
        public static Task<THHCard.DeathEventArg> destroy(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            return cards.die(game);
        }

        /// <summary>
        /// 获取所有友方随从
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [TranslateMsg("动作图编辑器中动作节点名称之获取友方随从")]
        public const string EDITOR_NODE_NAME_GET_FRIENDLY_SERVANTS = "获取友方随从";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_FRIENDLY_SERVANTS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getFriendlyServants(CardEngine game, Card card)
        {
            {
                Card[] servants = card.owner.getField();
                return servants;
            }
        }
        /// <summary>
        /// 获取护甲
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之获取护甲值")]
        public const string EDITOR_NODE_NAME_GET_ARMOR = "获取护甲值";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_ARMOR, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getArmor(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getArmor(game);
        }
        /// <summary>
        /// 增加固定数量的护甲
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之增加固定数量护甲")]
        public const string EDITOR_NODE_ADD_AMOUNT_OF_ARMOR = "增加固定数量护甲";
        [ActionNodeMethod(EDITOR_NODE_ADD_AMOUNT_OF_ARMOR, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static async Task addArmorToTarget(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            await player.getMaster().gainArmor(game, count);
        }
        /// <summary>
        /// 增加护甲
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之增加护甲")]
        public const string EDITOR_NODE_ADD_ARMOR = "增加护甲";
        [ActionNodeMethod(EDITOR_NODE_ADD_ARMOR, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static async Task addAmountOfArmorToTarget(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
             [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            await player.getMaster().gainArmor(game, count);
        }
        /// <summary>
        /// 失去护甲
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之失去护甲")]
        public const string EDITOR_NODE_LOSE_ARMOR = "失去护甲";
        [ActionNodeMethod(EDITOR_NODE_LOSE_ARMOR, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static async Task loseArmor(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            await player.getMaster().loseArmor(game, count);
        }
        /// <summary>
        /// 获取延迟区
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之获取玩家延迟区")]
        public const string EDITOR_NODE_NAME_GET_HEAD = "获取玩家延迟区";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_HEAD, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getHead(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getHead();
        }
        /// <summary>
        /// 获取技能
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之获取玩家技能")]
        public const string EDITOR_NODE_NAME_GET_SKILL = "获取玩家技能";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_SKILL, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getSkill(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getSkill();
        }
        /// <summary>
        /// 复原技能
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之复原技能")]
        public const string EDITOR_NODE_RESET_SKILL = "复原技能";
        [ActionNodeMethod(EDITOR_NODE_RESET_SKILL, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static async Task resetSkill(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player.getSkill().isUsed(game))
            {
                await player.getSkill().setUsed(game, false);
            }
        }
        /// <summary>
        /// 获取道具
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之获取玩家道具")]
        public const string EDITOR_NODE_NAME_GET_ITEM = "获取玩家道具";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_ITEM, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getItem(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getItem();
        }
        /// <summary>
        /// 摧毁道具
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之摧毁道具")]
        public const string EDITOR_NODE_DESTROY_ITEM = "摧毁道具";
        [ActionNodeMethod(EDITOR_NODE_DESTROY_ITEM, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static async Task destroyItem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            await player.destroyItem(game);
        }
        /// <summary>
        /// 获取相邻卡牌(战场或者墓地)
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之获取相邻卡牌")]
        public const string EDITOR_NODE_NAME_GET_NEARBY_CARD = "获取相邻卡牌";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_NEARBY_CARD, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getNearbyCards(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_NODE_CATEGORY_CARD)] Card card)
        {
            return card.getNearbyCards();
        }
        /// <summary>
        /// 获取当前灵力值
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [TranslateMsg("动作图编辑器中动作节点名称之获取当前灵力值")]
        public const string EDITOR_NODE_NAME_GET_GEM = "获取当前灵力值";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_GEM, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getGem(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getGem();
        }
        /// <summary>
        /// 获取灵力上限
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [TranslateMsg("动作图编辑器中动作节点名称之获取灵力上限")]
        public const string EDITOR_NODE_NAME_GET_MAX_GEM = "获取灵力上限";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_MAX_GEM, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getMaxGem(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getMaxGem();
        }
        /// <summary>
        /// 设置当前灵力值
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之设置当前灵力值")]
        public const string EDITOR_NODE_ADD_AMOUNT_OF_GEM = "设置当前灵力值";
        [ActionNodeMethod(EDITOR_NODE_ADD_AMOUNT_OF_GEM, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static async Task setGem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            await player.setGem(game, count);
        }
        /// <summary>
        /// 增加当前灵力值
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之增加当前灵力值")]
        public const string EDITOR_NODE_ADD_GEM = "增加当前灵力值";
        [ActionNodeMethod(EDITOR_NODE_ADD_GEM, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static async Task addGem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            await player.setGem(game, player.getGem() + count);
        }
        /// <summary>
        /// 设置灵力上限
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之设置灵力上限")]
        public const string EDITOR_NODE_SET_MAX_GEM = "设置灵力上限";
        [ActionNodeMethod(EDITOR_NODE_SET_MAX_GEM, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static async Task setMaxGem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            await player.setMaxGem(game, count);
        }
        /// <summary>
        /// 增加灵力上限
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之增加灵力上限")]
        public const string EDITOR_NODE_ADD_MAX_GEM = "增加灵力上限";
        [ActionNodeMethod(EDITOR_NODE_ADD_MAX_GEM, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static async Task addMaxGem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            await player.setMaxGem(game, player.getMaxGem() + count);
        }
        /// <summary>
        /// 触发卡牌宣言
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之触发卡牌宣言")]
        public const string EDITOR_NODE_TRIGGER_BATTLECRY = "触发卡牌宣言";
        [ActionNodeMethod(EDITOR_NODE_TRIGGER_BATTLECRY, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        public static async Task triggerActiveEffect(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            foreach (var card in cards)
            {
                await card.activeEffect(game, card.getOwner(), targets);
            }
        }
        /// <summary>
        /// 触发卡牌遗言
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之触发卡牌遗言")]
        public const string EDITOR_NODE_TRIGGER_DEATHRATTLE = "触发卡牌遗言";
        [ActionNodeMethod(EDITOR_NODE_TRIGGER_DEATHRATTLE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        public static async Task triggerDeathRattles(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            foreach (var card in cards)
            {
                //await card.triggerDeathRattles(game, card.getOwner(), targets);
                await card.triggerDeathRattles(game);
            }
        }
        //[TranslateMsg("动作图编辑器中动作节点名称之获取最低属性的卡牌")]
        //public const string EDITOR_NODE_NAME_GET_MIN_CARD = "获取最低属性的卡牌";
        ///// <summary>
        ///// 获取一项属性值最低的卡牌。
        ///// </summary>
        //[ActionNodeMethod(EDITOR_NODE_NAME_GET_MIN_CARD, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        //[return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        //public static Card getMinPropCard(CardEngine game,
        //    [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)]Card[] cards,
        //    [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)]string propName)
        //{
        //    return cards.OrderBy(c =>
        //    {
        //        object prop = c.getProp(game, propName);
        //        if (prop is int intProp)
        //        {
        //            return intProp;
        //        }
        //        return 0;
        //    }).FirstOrDefault();
        //}

        //[TranslateMsg("动作图编辑器中动作节点名称之获取最高属性的卡牌")]
        //public const string EDITOR_NODE_NAME_GET_MAX_CARD = "获取最高属性的卡牌";
        ///// <summary>
        ///// 获取一项属性值最高的卡牌。
        ///// </summary>
        //[ActionNodeMethod(EDITOR_NODE_NAME_GET_MAX_CARD, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        //[return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        //public static Card getMaxPropCard(CardEngine game,
        //    [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Card[] cards,
        //    [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        //{
        //    return cards.OrderBy(c =>
        //    {
        //        object prop = c.getProp(game, propName);
        //        if (prop is int intProp)
        //        {
        //            return intProp;
        //        }
        //        return 0;
        //    }).LastOrDefault();
        //}
        /// <summary>
        /// X到Y之间随机整数
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之X到Y之间的随机数")]
        public const string EDITOR_NODE_NAME_GET_RANDOM_INT = "X到Y之间的随机整数";
        public const string EDITOR_NODE_NAME_GET_INT_X = "整数X";
        public const string EDITOR_NODE_NAME_GET_INT_Y = "整数Y";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_RANDOM_INT, StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int randomInt(CardEngine game,
           [ActionNodeParam(EDITOR_NODE_NAME_GET_INT_X, isConst: false)] int countX,
           [ActionNodeParam(EDITOR_NODE_NAME_GET_INT_Y, isConst: false)] int countY)
        {
            return game.randomInt(countX,countY);
        }
        /// <summary>
        /// 强制攻击目标
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之强制攻击目标")]
        public const string EDITOR_NODE_FORCE_ATTACK = "强制攻击目标";
        public const string EDITOR_NODE_FORCE_ATTACKER = "发动攻击的卡牌";
        [ActionNodeMethod(EDITOR_NODE_FORCE_ATTACK, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        public static async Task forceAttackTargets(CardEngine game,
            [ActionNodeParam(EDITOR_NODE_FORCE_ATTACKER)]Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            foreach (var card in cards)
            {
                foreach (var target in targets)
                { 
                    await card.forceAttack(game, card.getOwner(), target);
                }
            }
        }
        /// <summary>
        /// 本回合增加一次攻击次数
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之本回合攻击次数增加一次")]
        public const string EDITOR_NODE_EXTRA_ATTACK = "本回合攻击次数增加一次";
        [ActionNodeMethod(EDITOR_NODE_EXTRA_ATTACK, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        public static async Task extraAttack(CardEngine game,
            [ActionNodeParam(EDITOR_NODE_FORCE_ATTACKER)]Card[] cards)
        {
            foreach (var card in cards)
            {
                //await card.setAttackTimes(game, 0);
                await card.setAttackTimes(game, card.getAttackTimes(game) - 1);
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



 

