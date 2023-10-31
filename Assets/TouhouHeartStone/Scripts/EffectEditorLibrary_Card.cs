using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UnityEngine;

namespace Game
{
    partial class EffectEditorLibrary
    {
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_EQUIP_TO_ITEM = "创建衍生卡并装备到道具栏";
        [TranslateMsg("动作图编辑器中动作节点名称之卡牌关键词判断")]
        public const string EDITOR_NODE_IS_KEYWORDS = "卡牌关键词判断";
        [TranslateMsg("动作图编辑器中动作节点名称之获取当前生命值")]
        public const string EDITOR_NODE_NAME_GET_CURRENT_LIFE = "获取当前生命值";
        [TranslateMsg("动作图编辑器中动作节点名称之重置卡牌")]
        public const string EDITOR_NODE_RESET_CARD = "重置卡牌";
        [TranslateMsg("动作图编辑器中动作节点名称之强制攻击目标")]
        public const string EDITOR_NODE_FORCE_ATTACK = "强制攻击目标";
        [TranslateMsg("动作图编辑器中动作节点名称之本回合攻击次数增加一次")]
        public const string EDITOR_NODE_EXTRA_ATTACK = "本回合攻击次数增加一次";
        [TranslateMsg("编辑器/卡牌_获取合法卡牌使用目标")]
        public const string EDITOR_NODE_GET_CARD_TARGETS = "获取合法卡牌使用目标";
        [TranslateMsg("编辑器/卡牌_获取合法卡牌使用目标_返回值_合法目标集合")]
        public const string EDITOR_NODE_GET_CARD_TARGETS_TARGETS = "合法目标集合";
        [TranslateMsg("编辑器/卡牌_获取合法攻击目标")]
        public const string EDITOR_NODE_GET_ATTACK_TARGETS = "获取合法攻击目标";
        [TranslateMsg("编辑器/卡牌_获取合法攻击目标_返回值_合法目标集合")]
        public const string EDITOR_NODE_GET_ATTACK_TARGETS_TARGETS = "合法目标集合";
        [TranslateMsg("编辑器/卡牌-节点名称")]
        public const string EDITOR_NODE_REVEAL = "揭示卡牌";
        [TranslateMsg("编辑器/卡牌-节点名称")]
        public const string EDITOR_NODE_NAME_CARD_TAKE_CONTRL = "获得控制权";
        [TranslateMsg("编辑器/卡牌-节点名称")]
        public const string EDITOR_NODE_IS_CHARGED = "是否已蓄力";
        [TranslateMsg("编辑器/卡牌-节点名称")]
        public const string EDITOR_NODE_GET_CHARGED_LEVEL = "获取蓄力层数";
        [TranslateMsg("编辑器/卡牌-节点名称")]
        public const string EDITOR_NODE_SET_CHARGED_LEVEL = "设置蓄力层数";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_UPDATE_CARDS_STATE = "强制更新所有卡牌状态";
        [TranslateMsg("节点名称")]
        private const string EDITOR_NODE_NAME_IS_DEAD = "是否濒死";
        [TranslateMsg("动作图编辑器中动作节点名称之获取护甲值")]
        public const string EDITOR_NODE_NAME_GET_ARMOR = "获取护甲值";
        [TranslateMsg("动作图编辑器中动作节点名称之获取相邻卡牌")]
        public const string EDITOR_NODE_NAME_GET_NEARBY_CARD = "获取相邻卡牌";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_CARD_PROPERTY = "获取卡牌属性";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_SET_CARD_PROPERTY = "设置卡牌属性";

        [TranslateMsg("是否濒死节点-常量")]
        private const string EDITOR_PARAM_NAME_CHECK_HEALTH_ZERO = "检查生命值<=0";
        [TranslateMsg("是否濒死节点-常量")]
        private const string EDITOR_PARAM_NAME_CHECK_DESTROYED = "检查待摧毁";
        [TranslateMsg("是否濒死节点-返回值")]
        private const string EDITOR_PARAM_NAME_IS_DEAD = "濒死";
        [TranslateMsg("编辑器/参数名称")]
        public const string EDITOR_PARAM_NAME_FORCE_ATTACKER = "发动攻击的卡牌";
        [TranslateMsg("编辑器/卡牌-是否已蓄力节点-参数名称")]
        public const string EDITOR_PARAM_NAME_IS_CHARGED = "已蓄力";
        [TranslateMsg("编辑器/卡牌-是否已蓄力节点-参数名称")]
        public const string EDITOR_PARAM_NAME_CHARGED_LEVEL = "蓄力层数";
        [TranslateMsg("节点参数 isSet")]
        const string EDITOR_PARAM_IS_SET = "设置";

        /// <summary>
        /// 卡牌类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.IsCardOfType, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE__IS_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsCardDefine(
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD)] Card card,
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.CardHasKeyword, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_IS_KEYWORDS)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsKeywords(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_KEYWORDS_TYPE, isConst: true)] string select)
        {
            if (card == null)
                return false;
            return card.hasKeyword(game, select);
        }
        /// <summary>
        /// 获取随机卡牌
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(ObsoletedActionDefineID.GetRandomCard, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_RANDOM_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        [Obsolete]
        public static Card getRandomCard(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            return cards?.random(game);
        }

        #region 创建衍生卡
        /// <summary>
        /// 创建衍生物并置入战场
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToField, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_FIELD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToField(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (player == null || cardDefine == null)
                return null;
            var createTokenEvent = await player.createToken(game, cardDefine);
            return createTokenEvent?.card;
        }
        /// <summary>
        /// 创建衍生卡并置入手牌
        /// </summary>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToHand, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_HAND)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToHand(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (player == null || cardDefine == null)
                return null;
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToDeck, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_SHUFFLE_TO_DECK)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardShuffleToDeck(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (player == null || cardDefine == null)
                return null;
            Card createdCard = game.createCard(cardDefine);

            await player.getDeck().insert(game, createdCard, game.randomInt(0, player.getDeck().count));
            return createdCard;
        }


        /// <summary>
        /// 创建衍生卡并装备到道具栏
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToItem, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_CREATE_TOKEN_CARD_EQUIP_TO_ITEM)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardEquipToItem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (player == null || cardDefine == null)
                return null;
            Card createdCard = game.createCard(cardDefine);

            await player.equip(game, null, createdCard);
            return createdCard;
        }
        #endregion

        /// <summary>
        /// 这张卡牌
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.ThisCard, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_THIS_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card thisCard(Card card)
        {
            return card;
        }
        /// <summary>
        /// 获取卡牌属性
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.GetCardProp, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_GET_CARD_PROPERTY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getCardProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] CardPropertyGetterName propName)
        {
            return card?.getProp(game, propName);
        }
        /// <summary>
        /// 设置卡牌属性
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.SetCardProp, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_SET_CARD_PROPERTY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROP_CHANGE_EVENT)]
        public static Task setCardProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] CardPropertySetterName propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            if (card != null)
                return card.setProp(game, propName, value);
            return Task.CompletedTask;
        }
        /// <summary>
        /// 获取属性
        /// </summary>
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardProp_Obsolete, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_PROPERTY, "GetProperty")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getPropObsolete(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            return card?.getProp(game, propName);
        }
        /// <summary>
        /// 设置属性
        /// </summary>
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetCardProp_Obsolete, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_SET_PROPERTY, "SetProperty")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROP_CHANGE_EVENT)]
        public static Task<IPropChangeEventArg> setPropObsolete(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            if (card != null)
                return card.setProp(game, propName, value);
            return Task.FromResult<IPropChangeEventArg>(null);
        }
        /// <summary>
        /// 获取花费
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardCost, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_COST)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCost(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return 0;
            return card.getCost(game);
        }
        /// <summary>
        /// 获取攻击
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardAttack, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_ATTACK)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getAttack(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return 0;
            return card.getAttack(game);
        }
        /// <summary>
        /// 获取最大生命值
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardMaxLife, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_MAX_LIFE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getMaxLife(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return 0;
            return card.getLife(game);
        }
        /// <summary>
        /// 获取当前生命值
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardCurrentLife, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_GET_CURRENT_LIFE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getLife(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return 0;
            return card.getCurrentLife(game);
        }

        /// <summary>
        /// 丢弃卡牌
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.Discard, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_DISCARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)]
        public static Task<THHCard.DiscardEventArg[]> discard(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HANDS)] Card[] hands)
        {
            if (hands == null)
                return null;
            return hands.discard(game);
        }


        /// <summary>
        /// 增加固定数量的护甲
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddArmor, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_ADD_AMOUNT_OF_ARMOR)]
        public static async Task addArmorToTarget(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player != null)
                await player.getMaster().gainArmor(game, count);
        }
        /// <summary>
        /// 增加护甲
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddAmountOfArmor, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_ADD_ARMOR)]
        public static async Task addAmountOfArmorToTarget(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
             [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            if (player != null)
                await player.getMaster().gainArmor(game, count);
        }
        /// <summary>
        /// 失去护甲
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.LoseArmor, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_LOSE_ARMOR)]
        public static async Task loseArmor(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            if (player != null)
                await player.getMaster().loseArmor(game, count);
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetUniquePropertyName, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)]
        public static string getUniquePropName(IBuff buff, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            if (buff != null)
                propName = buff.instanceID + propName;
            return propName;
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardOwner, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_OWNER, "GetOwner")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]
        public static Player getOwner([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card?.owner;
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSpellDamage, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_GET_CARD_SPELL_DAMAGE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardSpellDamage(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return 0;
            return card.getSpellDamage(game);
        }
        /// <summary>
        /// 这是不是一张法术牌
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(ObsoletedActionDefineID.IsSpellCard, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_IS_SPELL, "IsSpell")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        [Obsolete]
        public static bool isSpell([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return false;
            return card.define.type == DEFINE.SPELL;
        }
        #region 伤害类节点
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_NODE_IS_SPELL_DAMAGE = "是否为法术伤害";
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_TYPE_NAME_VALUE_COMPENSATION = "+";
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_DAMAGE = "造成伤害";
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE = "造成伤害或法伤";
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_SPLIT_TO_TARGETS = "造成伤害或法伤并分配给目标";
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_TARGETS = "对固定数量的随机目标造成伤害或法伤";
        [TranslateMsg("动作图编辑器中动作节点名称之造成法术伤害")]
        public const string EDITOR_NODE_NAME_DEAL_SPELL_DAMAGE = "造成法术伤害";
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_PARAM_NAME_DAMAGE_SOURCE = "伤害源";

        [ObsoletedKeyMethod(BuiltinActionDefineID.Damage, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE, "Damage")]
        [Obsolete]
        public static Task damage(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] this Card card, CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card source,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int value)
        {
            if (card == null)
                return default;
            return card.damage(game, source, value);
        }
        /// <summary>
        /// 造成伤害或法术伤害
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.DealDamageOrSpellDamage_Obsolete, StringTable.EDITOR_NODE_CATEGORY_CARD, "造成伤害或法伤（过时）", EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        [Obsolete]
        public static Task<THHCard.DamageEventArg[]> dealDamageOrSpellDamageObsolete(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: false)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_NODE_IS_SPELL_DAMAGE, isConst: true)] bool damagetype,
            [ActionNodeParam(EDITOR_TYPE_NAME_VALUE_COMPENSATION, isConst: true)] int damage2)
        {
            if (targets == null)
                return default;

            var source = damageSource ?? card;
            var dmg = damage + damage2;
            if (damagetype)
            {
                dmg = source.owner.getSpellDamage(game, dmg, source);
            }
            return targets.damage(game, source, dmg);
        }
        /// <summary>
        /// 造成伤害或法术伤害并分配给目标
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.DealDamageSplitAmong_Obsolete, StringTable.EDITOR_NODE_CATEGORY_CARD, "造成伤害或法伤并分配给目标（过时）", EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_SPLIT_TO_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        [Obsolete]
        public static Task<THHCard.DamageEventArg[]> dealDamageOrSpellDamageSplitToTargetsObsolete(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: false)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_NODE_IS_SPELL_DAMAGE, isConst: true)] bool damagetype,
            [ActionNodeParam(EDITOR_TYPE_NAME_VALUE_COMPENSATION, isConst: true)] int damage2)
        {
            if (targets == null)
                return Task.FromResult(Array.Empty<THHCard.DamageEventArg>());

            var source = damageSource ?? card;
            var dmg = damage + damage2;
            if (damagetype)
            {
                dmg = source.owner.getSpellDamage(game, dmg, source);
            }
            return targets.damageByRandom(game, source, dmg);
        }
        /// <summary>
        /// 对固定数量的随机目标造成伤害或法术伤害
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.DealDamageToRandomTargets_Obsolete, StringTable.EDITOR_NODE_CATEGORY_CARD, "对固定数量的随机目标造成伤害或法伤（过时）", EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        [Obsolete]
        public static Task<THHCard.DamageEventArg[]> dealDamageOrSpellDamageToSpecificNumberOfTargetsObsolete(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: false)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_NODE_IS_SPELL_DAMAGE, isConst: true)] bool damagetype,
            [ActionNodeParam(EDITOR_TYPE_NAME_VALUE_COMPENSATION, isConst: true)] int count2,
            [ActionNodeParam(EDITOR_TYPE_NAME_VALUE_COMPENSATION, isConst: true)] int damage2)
        {
            if (targets == null)
                return default;

            var source = damageSource ?? card;
            var dmg = damage + damage2;
            if (damagetype)
            {
                dmg = source.owner.getSpellDamage(game, dmg, source);
            }
            return targets.damageByRandomCount(game, card, dmg, count + count2);
        }
        /// <summary>
        /// 造成伤害或法术伤害
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.DealDamageOrSpellDamage, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealDamageOrSpellDamage(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: false)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_NODE_IS_SPELL_DAMAGE, isConst: true)] bool damagetype,
            [ActionNodeParam(EDITOR_TYPE_NAME_VALUE_COMPENSATION, isConst: true)] int damage2)
        {
            if (targets == null)
                return default;

            var dmg = damage + damage2;
            if (damagetype && damageSource != null)
            {
                dmg = damageSource.owner.getSpellDamage(game, dmg, damageSource);
            }
            return targets.damage(game, damageSource, dmg);
        }
        /// <summary>
        /// 造成伤害或法术伤害并分配给目标
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.DealDamageSplitAmong, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_SPLIT_TO_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealDamageOrSpellDamageSplitToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: false)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_NODE_IS_SPELL_DAMAGE, isConst: true)] bool damagetype,
            [ActionNodeParam(EDITOR_TYPE_NAME_VALUE_COMPENSATION, isConst: true)] int damage2)
        {
            if (targets == null)
                return Task.FromResult(Array.Empty<THHCard.DamageEventArg>());

            var dmg = damage + damage2;
            if (damagetype && damageSource != null)
            {
                dmg = damageSource.owner.getSpellDamage(game, dmg, damageSource);
            }
            return targets.damageByRandom(game, damageSource, dmg);
        }
        /// <summary>
        /// 对固定数量的随机目标造成伤害或法术伤害
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.DealDamageToRandomTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealDamageOrSpellDamageToSpecificNumberOfTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: false)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_NODE_IS_SPELL_DAMAGE, isConst: true)] bool damagetype,
            [ActionNodeParam(EDITOR_TYPE_NAME_VALUE_COMPENSATION, isConst: true)] int count2,
            [ActionNodeParam(EDITOR_TYPE_NAME_VALUE_COMPENSATION, isConst: true)] int damage2)
        {
            if (targets == null)
                return default;

            var dmg = damage + damage2;
            if (damagetype && damageSource != null)
            {
                dmg = damageSource.owner.getSpellDamage(game, dmg, damageSource);
            }
            return targets.damageByRandomCount(game, damageSource, dmg, count + count2);
        }
        /// <summary>
        /// 对目标造成固定的法术伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="damage"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        [Obsolete]
        [ObsoletedKeyMethod(ObsoletedActionDefineID.DealSpecificSpellDamageToTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealSpecificSpellDamageToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;

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
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DealSpellDamageToTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_SPELL_DAMAGE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealSpellDamageToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: false)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;
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
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DealSpecificSpellDamageToRandomTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_RANDOM_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealSpecificSpellDamageToRandomTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;
            return targets.damageByRandom(game, card, card.owner.getSpellDamage(game, damage));
        }
        /// <summary>
        /// 对固定数量的随机敌人造成固定伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="count"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        [Obsolete]
        [ObsoletedKeyMethod(ObsoletedActionDefineID.DealSpecificDamageToSpecificNumberOfRandomTarget, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealSpecificDamageToSpecificNumberOfRandomTarget(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;
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
        [Obsolete]
        [ObsoletedKeyMethod(ObsoletedActionDefineID.DealSpecificSpellDamageToSpecificNumberOfRandomTarget, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealSpecificSpellDamageToSpecificNumberOfRandomTarget(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;
            return targets.damageByRandomCount(game, card, card.owner.getSpellDamage(game, damage), count);
        }
        #endregion
        /// <summary>
        /// 治疗
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.HealTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_HEAL_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HEAL_EVENT)]
        public static Task<THHCard.HealEventArg[]> healTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (targets == null)
                return Task.FromResult(Array.Empty<THHCard.HealEventArg>());
            return targets.heal(game, card, value);
        }
        [ObsoletedKeyMethod(ObsoletedActionDefineID.HealSpecificAmountToTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_HEAL_SPECIFIC_AMOUNT_TO_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HEAL_EVENT)]
        [Obsolete]
        public static Task<THHCard.HealEventArg[]> healSpecificAmountToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE, isConst: true)] int value)
        {
            if (targets == null)
                return Task.FromResult(Array.Empty<THHCard.HealEventArg>());
            return targets.heal(game, card, value);
        }


        #region 获取场上的卡牌
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetAllServants, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_ALL_SERVANTS)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetAllCharacters, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_ALL_CHARACTERS)]
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
        /// <summary>
        /// 获取所有友方随从
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [TranslateMsg("动作图编辑器中动作节点名称之获取友方随从")]
        public const string EDITOR_NODE_NAME_GET_FRIENDLY_SERVANTS = "获取友方随从";
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetFriendlyServants, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_GET_FRIENDLY_SERVANTS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getFriendlyServants(CardEngine game, Card card)
        {
            Card[] servants = card.owner.getField();
            return servants;
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetFriendlyCharacters, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_FRIEND_CHARACTERS)]
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
        /// <summary>
        /// 获取敌方随从
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEnemyServants, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_ENEMY_SERVANTS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getEnemyServants(CardEngine game, Card card)
        {
            return game.getOpponent(card.owner).getField();
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEnemyCharacters, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_ENEMY_CHARACTERS)]
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
        #endregion
        [ObsoletedKeyMethod(BuiltinActionDefineID.Shackle, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_FREEZE)]
        public static Task freeze(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return Task.CompletedTask;
            return card.setFreeze(game, true);
        }
        /// <summary>
        /// 沉默目标卡牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.Seal, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_SILENCE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_SILENCE_EVENT)]
        public static Task<THHCard.SilenceEventArg[]> silence(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return Task.FromResult(Array.Empty<THHCard.SilenceEventArg>());
            return cards.silence(game);
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.Kill, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_DESTROY)]
        public static Task destroy(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return Task.CompletedTask;
            return cards.kill(game);
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.ForceUpdateCardsState, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_UPDATE_CARDS_STATE)]
        public static Task forceUpdateCardsState(CardEngine game)
        {
            return game.updateDeath();
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.IsDead, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_IS_DEAD)]
        [return : ActionNodeParam(EDITOR_PARAM_NAME_IS_DEAD)]
        public static bool isDead(CardEngine game, 
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_NAME_CHECK_HEALTH_ZERO, isConst: true)] bool checkHealth,
            [ActionNodeParam(EDITOR_PARAM_NAME_CHECK_DESTROYED, isConst: true)] bool checkKilled)
        {
            if (card == null)
                return false;
            return card.isDead(game, checkHealth, checkKilled);
        }
        /// <summary>
        /// 获取护甲
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetArmor, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_GET_ARMOR)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getArmor(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return 0;
            return card.getArmor(game);
        }
        /// <summary>
        /// 获取相邻卡牌(战场或者墓地)
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetNearbyCards, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_GET_NEARBY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getNearbyCards(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_NODE_CATEGORY_CARD)] Card card)
        {
            if (card == null)
                return Array.Empty<Card>();
            return card.getNearbyCards();
        }


        #region 宣言/遗言
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_CARD_HAS_BATTLECRY = "卡牌具有宣言";
        [TranslateMsg("动作图编辑器中动作节点名称之触发卡牌宣言")]
        public const string EDITOR_NODE_TRIGGER_BATTLECRY = "触发卡牌宣言";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_CARD_HAS_DEATHRATTLE = "卡牌具有遗言";
        [TranslateMsg("动作图编辑器中动作节点名称之触发卡牌遗言")]
        public const string EDITOR_NODE_TRIGGER_DEATHRATTLE = "触发卡牌遗言";
        [TranslateMsg("动作图编辑器中动作节点名称之触发法术或技能卡牌效果")]
        public const string EDITOR_NODE_TRIGGER_SPELL_CARD_EFFECT = "触发法术或技能卡牌效果";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_CARD_IS_TRAP = "卡牌是否是陷阱";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_GET_CARD_PILE = "获取卡牌所在牌堆";
        /// <summary>
        /// 卡牌具有宣言
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CardHasBattlecry, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_CARD_HAS_BATTLECRY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool cardHasBattlecry(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return false;
            return card.hasBattlecry(game);
        }
        /// <summary>
        /// 触发卡牌宣言
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.TriggerCardBattlecries, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_TRIGGER_BATTLECRY)]
        public static async Task triggerBattlecries(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (cards == null)
                return;
            foreach (var card in cards)
            {
                if (card == null || (!card.hasBattlecry(game)))
                    continue;
                await card.triggerBattlecries(game, card.getOwner(), targets);
            }
        }

        /// <summary>
        /// 卡牌具有遗言
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CardHasDeathrattle, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_CARD_HAS_DEATHRATTLE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool cardHasDeathrattle(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return false;
            return card.hasDeathRattle(game);
        }
        /// <summary>
        /// 触发卡牌遗言
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.TriggerCardDeathrattles, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_TRIGGER_DEATHRATTLE)]
        public static async Task triggerDeathRattles(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return;
            foreach (var card in cards)
            {
                if (card == null || (!card.hasDeathRattle(game)))
                    continue;
                await card.triggerDeathRattles(game);
            }
        }
        /// <summary>
        /// 触发法术或技能卡牌效果
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.TriggerCardActiveEffect, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_TRIGGER_SPELL_CARD_EFFECT)]
        public static async Task triggerActiveEffect(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (cards == null)
                return;
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                else if (card.isSpell() || card.isSkill())
                    await card.activeEffect(game, card.getOwner(), targets);
            }
        }
        /// <summary>
        /// 卡牌是否是陷阱
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.CardIsTrap, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_CARD_IS_TRAP)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool cardIsTrap(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return false;
            return card.isTrap(game);
        }
        /// <summary>
        /// 获取卡牌所在牌堆
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.GetCardPile, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_GET_CARD_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)]
        public static Pile getCardPile(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return null;
            return card.pile;
        }
        #endregion


        /// <summary>
        /// 重置卡牌
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.ResetCard, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_RESET_CARD)]

        public static async Task resetCards(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return;
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                await card.resetCard(game);
            }
        }
        /// <summary>
        /// 强制攻击目标
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.ForceAttack, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_FORCE_ATTACK)]
        public static async Task forceAttackTargets(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_FORCE_ATTACKER)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (cards == null || targets == null)
                return;
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                foreach (var target in targets)
                {
                    if (target == null)
                        continue;
                    await card.forceAttack(game, card.getOwner(), target);
                }
            }
        }
        /// <summary>
        /// 本回合增加一次攻击次数
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddAttackChance, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_EXTRA_ATTACK)]
        public static async Task extraAttack(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_FORCE_ATTACKER)]Card[] cards)
        {
            if (cards == null)
                return;
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                await card.setAttackTimes(game, card.getAttackTimes(game) - 1);
            }
        }
        /// <summary>
        /// 获取卡牌合法目标集合
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetValidCardTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_GET_CARD_TARGETS)]
        [return: ActionNodeParam(EDITOR_NODE_GET_CARD_TARGETS_TARGETS)]
        public static Card[] getAllLegalCardTargets(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return Array.Empty<Card>();
            var legalTargets = card.getAvaliableTargets(game).Where(c => c != card).ToArray();

            return legalTargets;
        }
        /// <summary>
        /// 获取攻击目标集合
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetValidAttackTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_GET_ATTACK_TARGETS)]
        [return: ActionNodeParam(EDITOR_NODE_GET_ATTACK_TARGETS_TARGETS)]
        public static Card[] getAllLegalAttackTargets(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return Array.Empty<Card>();
            var oppo = game.getOpponent(card.getOwner());
            var legalTargets = oppo.getField().ToList().Append(oppo.getMaster())
                .Where(c => !c.isStealth(game)).ToArray();

            return legalTargets;
        }


        /// <summary>
        /// 揭示卡牌
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.RevealCard, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_REVEAL)]
        public static void revealCard(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return;
            game.Reveal(card);
        }
        /// <summary>
        /// 获得控制权
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.TakeControl, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_CARD_TAKE_CONTRL)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task takeControl(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card card)
        {
            if (card == null)
                return;
            if (player == null)
                return;
            await card.takeControl(game, player, -1);// TODO：ActionNodeParam不吃默认值，先填-1，以后有时间了做对仆从位置有关的兼容
        }


        /// <summary>
        /// 是否已蓄力
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.IsCharged, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_IS_CHARGED)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_IS_CHARGED)]
        public static bool isCharged(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return false;
            return card.isCharged(game);
        }

        /// <summary>
        /// 获取蓄力层数
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetChargedLevel, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_GET_CHARGED_LEVEL)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_CHARGED_LEVEL)]
        public static int getChargedLevel(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return 0;
            return card.getChargedLevel(game);
        }
        /// <summary>
        /// 设置蓄力层数
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetChargedLevel, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_SET_CHARGED_LEVEL)]
        public static async void setChargedLevel(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_NAME_CHARGED_LEVEL)] int level)
        {
            if (card == null)
                return;
            await card.setChargeLevel(game, level);
        }


        [ObsoletedKeyMethod(ObsoletedActionDefineID.AddIntModifier, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_ADD_INT_MODIFIER)]
        [return: ActionNodeParam(StringTable.EDITOR_NODE_PARAM_NAME_MODIFIER)]
        [Obsolete]
        public static IntPropModifier addIntModifier(IGame game, 
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value,
            [ActionNodeParam(EDITOR_PARAM_IS_SET)] bool isSet)
        {
            IntPropModifier modifier = new IntPropModifier(propName, value, isSet);
            card.addModifier(game, modifier);
            return modifier;
        }
        [ObsoletedKeyMethod(ObsoletedActionDefineID.RemoveModifier, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_REMOVE_MODIFIER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_REMOVE_MODIFIER_EVENT)]
        [Obsolete]
        public static Task<IRemoveModiEventArg> removeModifier(IGame game, 
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card, 
            [ActionNodeParam(StringTable.EDITOR_NODE_PARAM_NAME_MODIFIER)] PropModifier modifier)
        {
            return card.removeModifier(game, modifier);
        }
    }
}





