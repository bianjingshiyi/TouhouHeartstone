using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game
{
    partial class EffectEditorLibrary
    {
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_EQUIP_TO_ITEM = "创建衍生卡并装备到道具栏";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_HEAD = "创建衍生卡并置入延迟区";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_PILE = "创建衍生卡并置入牌堆";
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
        [TranslateMsg("编辑器/卡牌_获取合法攻击目标")]
        public const string EDITOR_NODE_GET_ATTACK_TARGETS = "获取合法攻击目标";
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
        private const string EDITOR_NODE_NAME_UPDATE_CARDS_STATE = "强制更新游戏状态";
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
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_SEAL = "封印";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CLONE_CARD = "复制卡牌";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_SET_CARD_PROP_VISIBILITY = "设置卡牌属性可见性";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_IS_CARD_PROP_VISIBLE_TO = "卡牌属性是否可见";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_SET_CARD_VISIBILITY = "设置卡牌可见性";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_IS_CARD_VISIBLE_TO = "卡牌是否可见";

        [TranslateMsg("是否濒死节点-常量", StringTable.CONTEXT_NODE_PARAM_NAME)]
        private const string EDITOR_PARAM_NAME_CHECK_HEALTH_ZERO = "检查生命值<=0";
        [TranslateMsg("是否濒死节点-常量", StringTable.CONTEXT_NODE_PARAM_NAME)]
        private const string EDITOR_PARAM_NAME_CHECK_DESTROYED = "检查待摧毁";
        [TranslateMsg("是否濒死节点-返回值", StringTable.CONTEXT_NODE_PARAM_NAME)]
        private const string EDITOR_PARAM_NAME_IS_DEAD = "濒死";
        [TranslateMsg("编辑器/参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_FORCE_ATTACKER = "发动攻击的卡牌";
        [TranslateMsg("编辑器/卡牌-是否已蓄力节点-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_IS_CHARGED = "已蓄力";
        [TranslateMsg("编辑器/卡牌-是否已蓄力节点-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_CHARGED_LEVEL = "蓄力层数";
        [TranslateMsg("编辑器节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_VALID_TARGETS = "合法目标集合";

        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        const string EDITOR_PARAM_NAME_CARD_PROP_CHANGE_EVENT = "卡牌属性更改事件";
        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        const string EDITOR_PARAM_NAME_SET_CARD_DEFINE_EVENT = "设置卡牌定义事件";
        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        const string EDITOR_PARAM_NAME_BE_CHARGED_EVENT = "蓄力事件";
        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        private const string EDITOR_PARAM_NAME_POSITION = "位置";
        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        const string EDITOR_PARAM_NAME_CARD_PROP_VISIBILITY_CHANGED_EVENT = "卡牌属性可见性更改事件";
        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        const string EDITOR_PARAM_NAME_CARD_VISIBILITY_CHANGED_EVENT = "卡牌可见性更改事件";

        [TranslateMsg("编辑器-卡牌-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_MASS_DISCARD_EVENT = "群体弃牌事件";
        [TranslateMsg("编辑器-卡牌-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_MASS_HEAL_EVENT = "群体治疗事件";
        [TranslateMsg("编辑器-卡牌-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_MASS_SEAL_EVENT = "群体封印事件";

        /// <summary>
        /// 卡牌类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.IsCardOfType_Obs_CardType, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_CHECK_CARD_TYPE)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsCardDefineObsolete(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_TYPE, isConst: true)] CardDefineSelect select)
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
        /// 卡牌类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [THHActionMethod(BuiltinActionDefineID.IsCardOfType, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_CHECK_CARD_TYPE)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsCardOfType(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_TYPE)] CardType type)
        {
            if (card == null || type == null)
                return false;
            return card.define.type == type.value;
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
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_KEYWORD)] string select)
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
        [EventNode(typeof(EnterFieldEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToField, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_FIELD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToField(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(EDITOR_PARAM_NAME_ENTER_FIELD_EVENT, isOut: true)][NodeParamEventType(typeof(EnterFieldEventDefine))] EventArg outEvent)
        {
            if (player == null || cardDefine == null)
                return null;
            outEvent = await player.createToken(game, cardDefine);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent?.getVar<Card>(EnterFieldEventDefine.VAR_CARD);
        }
        /// <summary>
        /// 创建衍生卡并置入手牌
        /// </summary>
        /// <returns></returns>
        [EventNode(typeof(MoveCardEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToHand, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_HAND)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToHand(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(EDITOR_PARAM_NAME_MOVE_CARD_EVENT, isOut: true)][NodeParamEventType(typeof(MoveCardEventDefine))] EventArg outEvent)
        {
            if (player == null || cardDefine == null)
                return null;
            if (player.getHand().isFull)
                return null;
            Card createdCard = game.createCard(cardDefine);
            outEvent = await player.getHand().add(game, createdCard);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return createdCard;
        }
        /// <summary>
        /// 创建衍生卡并洗入牌库
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [EventNode(typeof(MoveCardEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToDeck, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_SHUFFLE_TO_DECK)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardShuffleToDeck(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(EDITOR_PARAM_NAME_MOVE_CARD_EVENT, isOut: true)][NodeParamEventType(typeof(MoveCardEventDefine))] EventArg outEvent)
        {
            if (player == null || cardDefine == null)
                return null;
            Card createdCard = game.createCard(cardDefine);

            outEvent = await player.getDeck().insert(game, createdCard, game.randomInt(0, player.getDeck().count));
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return createdCard;
        }


        /// <summary>
        /// 创建衍生卡并装备到道具栏
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [EventNode(typeof(EquipItemEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToItem, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_CREATE_TOKEN_CARD_EQUIP_TO_ITEM)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardEquipToItem(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(EDITOR_PARAM_NAME_EQUIP_EVENT, isOut: true)][NodeParamEventType(typeof(EquipItemEventDefine))] EventArg outEvent)
        {
            if (player == null || cardDefine == null)
                return null;
            Card createdCard = game.createCard(cardDefine);

            outEvent = await player.equip(game, null, createdCard);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return createdCard;
        }
        /// <summary>
        /// 创建衍生物并置入延迟区
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [EventNode(typeof(MoveCardEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToHead, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_HEAD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToHead(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(EDITOR_PARAM_NAME_MOVE_CARD_EVENT, isOut: true)][NodeParamEventType(typeof(MoveCardEventDefine))] EventArg outEvent)
        {
            if (player == null || cardDefine == null)
                return null;
            Card createdCard = game.createCard(cardDefine);
            outEvent = await createdCard.moveToHead(game, player);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent?.getVar<Card>(MoveCardEventDefine.VAR_CARD);
        }
        /// <summary>
        /// 创建衍生物并置入牌堆
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [EventNode(typeof(MoveCardEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateTokenToPile, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToPile(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)] Pile pile,
            [ActionNodeParam(EDITOR_PARAM_NAME_MOVE_CARD_EVENT, isOut: true)][NodeParamEventType(typeof(MoveCardEventDefine))] EventArg outEvent,
            [ActionNodeParam(EDITOR_PARAM_NAME_POSITION)] int position = -1)
        {
            if (cardDefine == null)
                return null;
            // 根据卡牌的定义创建一张新的卡牌。
            var newCard = game.createCard(cardDefine);

            // 不移动到任何牌堆，直接返回。
            if (pile == null)
                return newCard;

            // 将卡牌移动到目标牌堆。
            outEvent = await newCard.moveCardFromPileToPile(game, null, pile, position);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return newCard;
        }
        /// <summary>
        /// 复制卡牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [EventNode(typeof(MoveCardEventDefine), typeof(SetCardPropEventDefine), typeof(AddBuffEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.CloneCard, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_CLONE_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Task<Card> cloneCard(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_NAME_TO_PILE)] Pile targetPile,
            [ActionNodeParam(EDITOR_PARAM_NAME_POSITION)] int position = -1)
        {
            if (card == null)
                return null;
            return card.cloneCardTo(game, targetPile, position);
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
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] CardPropertyGetterName propName)
        {
            return card?.getProp(game, propName);
        }
        /// <summary>
        /// 设置卡牌属性
        /// </summary>
        [EventNode(typeof(SetCardPropEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.SetCardProp, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_SET_CARD_PROPERTY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROP_CHANGE_EVENT)]
        public static async Task setCardProp(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] CardPropertySetterName propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value,
            [ActionNodeParam(EDITOR_PARAM_NAME_CARD_PROP_CHANGE_EVENT, isOut: true)][NodeParamEventType(typeof(SetCardPropEventDefine))] EventArg outEvent)
        {
            if (card == null)
                return;
            outEvent = await card.setProp(game, propName, value);
            setOutParam(flow, node, nameof(outEvent), outEvent);
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
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROP_CHANGE_EVENT)][return: NodeParamEventType(typeof(SetCardPropEventDefine))]
        public static Task<EventArg> setPropObsolete(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            if (card != null)
                return card.setProp(game, propName, value);
            return Task.FromResult<EventArg>(null);
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
        /// 丢弃卡牌（群体事件过时）
        /// </summary>
        [Obsolete]
        [EventNode(typeof(DiscardEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.Discard_Obs_Mass, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_DISCARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)]
        public static Task<EventArg[]> discardObsoleteMass(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HANDS)] Card[] hands)
        {
            if (hands == null)
                return null;
            return hands.discardObsoleteMass(game);
        }
        /// <summary>
        /// 丢弃卡牌
        /// </summary>
        [EventNode(typeof(MassDiscardEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.Discard, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_DISCARD)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MASS_DISCARD_EVENT)][return: NodeParamEventType(typeof(MassDiscardEventDefine))]
        public static Task<EventArg> discard(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HANDS)] Card[] hands)
        {
            if (hands == null)
                return null;
            return hands.discard(game);
        }


        /// <summary>
        /// 增加固定数量的护甲
        /// </summary>
        [Obsolete]
        [EventNode(typeof(SetArmorEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddArmor, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_ADD_AMOUNT_OF_ARMOR)]
        public static async Task addArmorToTarget(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_PARAM_NAME_SET_ARMOR_EVENT, isOut: true)][NodeParamEventType(typeof(SetArmorEventDefine))] EventArg outEvent)
        {
            if (player == null)
                return;
            outEvent = await player.getMaster().gainArmor(game, count);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }
        /// <summary>
        /// 增加护甲
        /// </summary>
        [EventNode(typeof(SetArmorEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddAmountOfArmor, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_ADD_ARMOR)]
        public static async Task addAmountOfArmorToTarget(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
             [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(EDITOR_PARAM_NAME_SET_ARMOR_EVENT, isOut: true)][NodeParamEventType(typeof(SetArmorEventDefine))] EventArg outEvent)
        {
            if (player == null)
                return;
            outEvent = await player.getMaster().gainArmor(game, count);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }
        /// <summary>
        /// 失去护甲
        /// </summary>
        [EventNode(typeof(SetArmorEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.LoseArmor, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_LOSE_ARMOR)]
        public static async Task loseArmor(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(EDITOR_PARAM_NAME_SET_ARMOR_EVENT, isOut: true)][NodeParamEventType(typeof(SetArmorEventDefine))] EventArg outEvent)
        {
            if (player == null)
                return;
            outEvent = await player.getMaster().loseArmor(game, count);
            setOutParam(flow, node, nameof(outEvent), outEvent);
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
        public const string EDITOR_NODE_NAME_DEAL_DAMAGE = "造成伤害";
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE = "造成伤害或法伤";
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_SPLIT_TO_TARGETS = "造成伤害或法伤并分配给目标";
        [TranslateMsg("编辑器/伤害分类/节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_TARGETS = "对固定数量的随机目标造成伤害或法伤";
        [TranslateMsg("动作图编辑器中动作节点名称之造成法术伤害")]
        public const string EDITOR_NODE_NAME_DEAL_SPELL_DAMAGE = "造成法术伤害";


        [TranslateMsg("编辑器/伤害分类/节点名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_VALUE_COMPENSATION = "+";
        [TranslateMsg("编辑器/伤害分类/节点名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_DAMAGE_SOURCE = "伤害源";
        [TranslateMsg("编辑器/伤害分类/节点名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_HEAL_SOURCE = "治疗源";

        [TranslateMsg("编辑器-伤害-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_MASS_DAMAGE_EVENT = "群体伤害事件";
        [TranslateMsg("编辑器-伤害-参数名称", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_SPLIT_DAMAGE_EVENT = "分配伤害事件";

        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        private const string EDITOR_PARAM_NAME_ATTACK_EVENT = "攻击事件";
        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        private const string EDITOR_PARAM_NAME_SET_ARMOR_EVENT = "护甲改变事件";
        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        private const string EDITOR_PARAM_NAME_SHACKLE_EVENT = "禁锢事件";
        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        private const string EDITOR_PARAM_NAME_REVEAL_EVENT = "揭示事件";
        [TranslateMsg("节点参数", StringTable.CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_IS_SPELL_DAMAGE = "是否为法术伤害";

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
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        [Obsolete]
        public static Task<EventArg[]> dealDamageOrSpellDamageObsolete(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_PARAM_NAME_IS_SPELL_DAMAGE)] bool damagetype,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int damage2)
        {
            if (targets == null)
                return default;

            var source = damageSource ?? card;
            var dmg = damage + damage2;
            if (damagetype)
            {
                dmg = source.owner.getSpellDamage(game, dmg, source);
            }
            return targets.damageObsolete(game, source, dmg);
        }
        /// <summary>
        /// 造成伤害或法术伤害并分配给目标
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.DealDamageSplitAmong_Obsolete, StringTable.EDITOR_NODE_CATEGORY_CARD, "造成伤害或法伤并分配给目标（过时）", EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_SPLIT_TO_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        [Obsolete]
        public static Task<EventArg[]> dealDamageOrSpellDamageSplitToTargetsObsolete(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_PARAM_NAME_IS_SPELL_DAMAGE)] bool damagetype,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int damage2)
        {
            if (targets == null)
                return Task.FromResult(Array.Empty<EventArg>());

            var source = damageSource ?? card;
            var dmg = damage + damage2;
            if (damagetype)
            {
                dmg = source.owner.getSpellDamage(game, dmg, source);
            }
            return targets.damageByRandomObsolete(game, source, dmg);
        }
        /// <summary>
        /// 对固定数量的随机目标造成伤害或法术伤害
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.DealDamageToRandomTargets_Obsolete, StringTable.EDITOR_NODE_CATEGORY_CARD, "对固定数量的随机目标造成伤害或法伤（过时）", EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        [Obsolete]
        public static Task<EventArg[]> dealDamageOrSpellDamageToSpecificNumberOfTargetsObsolete(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_PARAM_NAME_IS_SPELL_DAMAGE)] bool damagetype,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int count2,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int damage2)
        {
            if (targets == null)
                return default;

            var source = damageSource ?? card;
            var dmg = damage + damage2;
            if (damagetype)
            {
                dmg = source.owner.getSpellDamage(game, dmg, source);
            }
            return targets.damageByRandomCountObsolete(game, card, dmg, count + count2);
        }
        /// <summary>
        /// 造成伤害或法术伤害（群体事件过时）
        /// </summary>
        [Obsolete]
        [EventNode(typeof(DamageEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DealDamageOrSpellDamage_Obs_Mass, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        public static Task<EventArg[]> dealDamageOrSpellDamageObsoleteMass(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_PARAM_NAME_IS_SPELL_DAMAGE)] bool damagetype,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int damage2)
        {
            if (targets == null)
                return default;

            var dmg = damage + damage2;
            if (damagetype && damageSource != null)
            {
                dmg = damageSource.owner.getSpellDamage(game, dmg, damageSource);
            }
            return targets.damageObsolete(game, damageSource, dmg);
        }
        /// <summary>
        /// 造成伤害或法术伤害
        /// </summary>
        [EventNode(typeof(MassDamageEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.DealDamageOrSpellDamage, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MASS_DAMAGE_EVENT)][return: NodeParamEventType(typeof(MassDamageEventDefine))]
        public static Task<EventArg> dealDamageOrSpellDamage(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int damage2,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_PARAM_NAME_IS_SPELL_DAMAGE)] bool damagetype)
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
        /// 造成伤害或法术伤害并分配给目标（群体事件过时）
        /// </summary>
        [Obsolete]
        [EventNode(typeof(DamageEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DealDamageSplitAmong_Obs_Mass, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_SPLIT_TO_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        public static Task<EventArg[]> dealDamageOrSpellDamageSplitToTargetsObsoleteMass(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_PARAM_NAME_IS_SPELL_DAMAGE)] bool damagetype,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int damage2)
        {
            if (targets == null)
                return Task.FromResult(Array.Empty<EventArg>());

            var dmg = damage + damage2;
            if (damagetype && damageSource != null)
            {
                dmg = damageSource.owner.getSpellDamage(game, dmg, damageSource);
            }
            return targets.damageByRandomObsolete(game, damageSource, dmg);
        }
        /// <summary>
        /// 造成伤害或法术伤害并分配给目标
        /// </summary>
        [EventNode(typeof(SplitDamageEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.DealDamageSplitAmong, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_SPLIT_TO_TARGETS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_SPLIT_DAMAGE_EVENT)][return: NodeParamEventType(typeof(SplitDamageEventDefine))]
        public static Task<EventArg> dealDamageOrSpellDamageSplitToTargets(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int damage2,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_PARAM_NAME_IS_SPELL_DAMAGE)] bool damagetype)
        {
            if (targets == null)
                return Task.FromResult<EventArg>(null);

            var dmg = damage + damage2;
            if (damagetype && damageSource != null)
            {
                dmg = damageSource.owner.getSpellDamage(game, dmg, damageSource);
            }
            return targets.damageByRandom(game, damageSource, dmg);
        }
        /// <summary>
        /// 对固定数量的随机目标造成伤害或法术伤害（群体事件过时）
        /// </summary>
        [Obsolete]
        [EventNode(typeof(DamageEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DealDamageToRandomTargets_Obs_Mass, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        public static Task<EventArg[]> dealDamageOrSpellDamageToSpecificNumberOfTargetsObsoleteMass(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_PARAM_NAME_IS_SPELL_DAMAGE)] bool damagetype,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int count2,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int damage2)
        {
            if (targets == null)
                return default;

            var dmg = damage + damage2;
            if (damagetype && damageSource != null)
            {
                dmg = damageSource.owner.getSpellDamage(game, dmg, damageSource);
            }
            return targets.damageByRandomCountObsolete(game, damageSource, dmg, count + count2);
        }
        /// <summary>
        /// 对固定数量的随机目标造成伤害或法术伤害
        /// </summary>
        [EventNode(typeof(MassDamageEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.DealDamageToRandomTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_DEAL_DAMAGE_OR_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_TARGETS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MASS_DAMAGE_EVENT)][return: NodeParamEventType(typeof(MassDamageEventDefine))]
        public static Task<EventArg> dealDamageOrSpellDamageToSpecificNumberOfTargets(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int count2,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(EDITOR_PARAM_NAME_VALUE_COMPENSATION)] int damage2,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_DAMAGE_SOURCE)] Card damageSource,
            [ActionNodeParam(EDITOR_PARAM_NAME_IS_SPELL_DAMAGE)] bool damagetype)
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
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        public static Task<EventArg[]> dealSpecificSpellDamageToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;

            return targets.damageObsolete(game, card, card.owner.getSpellDamage(game, damage));
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
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        public static Task<EventArg[]> dealSpellDamageToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;
            return targets.damageObsolete(game, card, card.owner.getSpellDamage(game, damage));
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
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        public static Task<EventArg[]> dealSpecificSpellDamageToRandomTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;
            return targets.damageByRandomObsolete(game, card, card.owner.getSpellDamage(game, damage));
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
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        public static Task<EventArg[]> dealSpecificDamageToSpecificNumberOfRandomTarget(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;
            return targets.damageByRandomCountObsolete(game, card, damage, count);
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
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)][return: NodeParamEventType(typeof(DamageEventDefine))]
        public static Task<EventArg[]> dealSpecificSpellDamageToSpecificNumberOfRandomTarget(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            if (targets == null)
                return default;
            return targets.damageByRandomCountObsolete(game, card, card.owner.getSpellDamage(game, damage), count);
        }
        #endregion
        /// <summary>
        /// 治疗（群体事件过时）
        /// </summary>
        [Obsolete]
        [EventNode(typeof(HealEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.HealTargets_Obs_Mass, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_HEAL_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HEAL_EVENT)][return: NodeParamEventType(typeof(HealEventDefine))]
        public static Task<EventArg[]> healTargetsObsoleteMass(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (targets == null)
                return Task.FromResult(Array.Empty<EventArg>());
            return targets.healObsolete(game, card, value);
        }
        /// <summary>
        /// 治疗（自定义规则过时）
        /// </summary>
        [Obsolete]
        [EventNode(typeof(MassHealEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.HealTargets_Obs_Rule, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_HEAL_TARGETS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MASS_HEAL_EVENT)][return: NodeParamEventType(typeof(MassHealEventDefine))]
        public static Task<EventArg> healTargetsRuleObsolete(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (targets == null)
                return Task.FromResult<EventArg>(null);
            return targets.heal(game, card, value);
        }
        /// <summary>
        /// 治疗
        /// </summary>
        [EventNode(typeof(MassHealEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.HealTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_HEAL_TARGETS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MASS_HEAL_EVENT)]
        [return: NodeParamEventType(typeof(MassHealEventDefine))]
        public static Task<EventArg> healTargets(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_HEAL_SOURCE)] Card source,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (targets == null)
                return Task.FromResult<EventArg>(null);
            return targets.heal(game, source, value);
        }
        [ObsoletedKeyMethod(ObsoletedActionDefineID.HealSpecificAmountToTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_HEAL_SPECIFIC_AMOUNT_TO_TARGETS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HEAL_EVENT)][return: NodeParamEventType(typeof(HealEventDefine))]
        [Obsolete]
        public static Task<EventArg[]> healSpecificAmountToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (targets == null)
                return Task.FromResult(Array.Empty<EventArg>());
            return targets.healObsolete(game, card, value);
        }


        #region 获取场上的卡牌
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetAllServants, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_ALL_SERVANTS, "获取所有随从")]
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
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_GET_FRIENDLY_SERVANTS = "获取友方仆从";
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetFriendlyServants, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_GET_FRIENDLY_SERVANTS, "获取友方随从")]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEnemyServants, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_GET_ENEMY_SERVANTS, "获取敌方随从")]
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
        [EventNode(typeof(ShackleEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.Shackle, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_FREEZE)]
        public static async Task freeze(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_NAME_SHACKLE_EVENT, isOut:true)][NodeParamEventType(typeof(ShackleEventDefine))] EventArg outEvent)
        {
            if (card == null)
                return;
            outEvent = await card.shackle(game);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }
        /// <summary>
        /// 沉默目标卡牌（群体事件过时）
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        [Obsolete]
        [EventNode(typeof(SilenceEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.Seal_Obs_Mass, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_SILENCE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_SILENCE_EVENT)][return: NodeParamEventType(typeof(SilenceEventDefine))]
        public static Task<EventArg[]> silenceObsoleteMass(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return Task.FromResult(Array.Empty<EventArg>());
            return cards.silenceObsoleteMass(game);
        }
        /// <summary>
        /// 封印
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        [EventNode(typeof(MassSealEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.Seal, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_SEAL)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_MASS_SEAL_EVENT)][return: NodeParamEventType(typeof(MassSealEventDefine))]
        public static Task<EventArg> seal(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return Task.FromResult<EventArg>(null);
            return cards.silence(game);
        }
        [EventNode(typeof(SetCardPropEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.Kill, StringTable.EDITOR_NODE_CATEGORY_CARD, StringTable.EDITOR_NODE_NAME_DESTROY)]
        public static async Task destroy(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(EDITOR_PARAM_NAME_CARD_PROP_CHANGE_EVENT, isOut: true)][NodeParamEventType(typeof(SetCardPropEventDefine))] EventArg[] outEvent)
        {
            if (cards == null)
                return;
            outEvent = await cards.kill(game);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.ForceUpdateCardsState, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_UPDATE_CARDS_STATE)]
        public static Task forceUpdateCardsState(CardEngine game)
        {
            return game.updateGameState();
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.IsDead, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_IS_DEAD)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_IS_DEAD)]
        public static bool isDead(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_NAME_CHECK_HEALTH_ZERO)] bool checkHealth,
            [ActionNodeParam(EDITOR_PARAM_NAME_CHECK_DESTROYED)] bool checkKilled)
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
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
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
                await card.triggerBattlecries(game, targets);
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
                if (card.isSpell())
                {
                    if (card.isTrap(game) || card.hasKeyword(game, KEYWORD.MISSION))
                    {
                        var player = card.getOwner();
                        await player.tryEnterHead(game, card.pile, card);
                    }
                    else
                    {
                        await card.triggerActiveEffect(game, targets);
                    }
                }
                else if (card.isSkill())
                {
                    await card.triggerActiveEffect(game, targets);
                }
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
        [EventNode(typeof(AttackEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.ForceAttack, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_FORCE_ATTACK)]
        public static async Task forceAttackTargets(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(EDITOR_PARAM_NAME_FORCE_ATTACKER)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(EDITOR_PARAM_NAME_ATTACK_EVENT, isOut: true)][NodeParamEventType(typeof(AttackEventDefine))] EventArg[] outEvent)
        {
            if (cards == null || targets == null)
                return;
            var eventList = new List<EventArg>(); 
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                foreach (var target in targets)
                {
                    if (target == null)
                        continue;
                    eventList.Add(await card.forceAttack(game, target));
                }
            }
            outEvent = eventList.ToArray();
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }
        /// <summary>
        /// 本回合增加一次攻击次数
        /// </summary>
        [EventNode(typeof(SetCardPropEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddAttackChance, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_EXTRA_ATTACK)]
        public static async Task extraAttack(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(EDITOR_PARAM_NAME_FORCE_ATTACKER)] Card[] cards,
            [ActionNodeParam(EDITOR_PARAM_NAME_CARD_PROP_CHANGE_EVENT, isOut: true)][NodeParamEventType(typeof(SetCardPropEventDefine))] EventArg[] outEvent)
        {
            if (cards == null)
                return;
            var eventList = new List<EventArg>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                eventList.Add(await card.setAttackTimes(game, card.getAttackTimes(game) - 1));
            }
            outEvent = eventList.ToArray();
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }
        /// <summary>
        /// 获取卡牌合法目标集合
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetValidCardTargets, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_GET_CARD_TARGETS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_VALID_TARGETS)]
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
        [return: ActionNodeParam(EDITOR_PARAM_NAME_VALID_TARGETS)]
        public static Card[] getAllLegalAttackTargets(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return Array.Empty<Card>();
            var legalTargets = game.players
                .SelectMany(p => p.getField().Append(p.getMaster()))
                .Where(c => card.isAttackable(game, c, out _))
                .ToArray();

            return legalTargets;
        }


        /// <summary>
        /// 揭示卡牌
        /// </summary>
        [EventNode(typeof(RevealEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.RevealCard, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_REVEAL)]
        public static async Task revealCard(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_NAME_REVEAL_EVENT, isOut: true)][NodeParamEventType(typeof(RevealEventDefine))] EventArg outEvent)
        {
            if (card == null)
                return;
            outEvent = await game.Reveal(card);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }
        /// <summary>
        /// 获得控制权
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [EventNode(typeof(MoveCardEventDefine), typeof(SetCardPropEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.TakeControl, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_CARD_TAKE_CONTRL)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task takeControl(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_NAME_MOVE_CARD_EVENT, isOut: true)][NodeParamEventType(typeof(MoveCardEventDefine))] EventArg outEvent,
            [ActionNodeParam(EDITOR_PARAM_NAME_POSITION)] int position = -1)
        {
            if (card == null)
                return;
            if (player == null)
                return;
            outEvent = await card.takeControl(game, player, position);
            setOutParam(flow, node, nameof(outEvent), outEvent);
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
        [EventNode(typeof(BeChargedEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetChargedLevel, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_SET_CHARGED_LEVEL)]
        public static async Task setChargedLevel(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_NAME_CHARGED_LEVEL)] int level,
            [ActionNodeParam(EDITOR_PARAM_NAME_BE_CHARGED_EVENT, isOut: true)][NodeParamEventType(typeof(BeChargedEventDefine))] EventArg outEvent)
        {
            if (card == null)
                return;
            outEvent = await card.setChargeLevel(game, level);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }


        [EventNode(typeof(ChangeCardPropVisibilityEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.SetCardPropVisibility, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_SET_CARD_PROP_VISIBILITY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task setCardPropVisibility(Flow flow, ActionNode node, CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_PARAM_NAME_VISIBLE)] bool visible,
            [ActionNodeParam(EDITOR_PARAM_NAME_CARD_PROP_VISIBILITY_CHANGED_EVENT, isOut: true)][NodeParamEventType(typeof(ChangeCardPropVisibilityEventDefine))] EventArg[] outEvent)
        {
            if (cards == null || player == null || propName == null)
                return;
            var list = new List<EventArg>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                list.Add(await card.setPropInvisibleTo(game, propName, player, !visible));
            }
            outEvent = list.ToArray();
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }

        [THHActionMethod(BuiltinActionDefineID.IsCardPropVisibleTo, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_IS_CARD_PROP_VISIBLE_TO)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_VISIBLE)]
        public static bool isCardPropVisibleTo(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (card == null || player == null || propName == null)
                return false;
            return !card.isPropInvisibleTo(propName, player);
        }

        [EventNode(typeof(ChangeCardVisibilityEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.SetCardVisibility, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_SET_CARD_VISIBILITY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task setCardVisibility(Flow flow, ActionNode node, CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_PARAM_NAME_VISIBLE)] bool visible,
            [ActionNodeParam(EDITOR_PARAM_NAME_CARD_VISIBILITY_CHANGED_EVENT, isOut: true)][NodeParamEventType(typeof(ChangeCardVisibilityEventDefine))] EventArg[] outEvent)
        {
            if (cards == null || player == null)
                return;
            var list = new List<EventArg>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                list.Add(await card.setCardVisibleTo(game, player, visible));
            }
            outEvent = list.ToArray();
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }

        [THHActionMethod(BuiltinActionDefineID.IsCardVisibleTo, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_IS_CARD_VISIBLE_TO)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_VISIBLE)]
        public static bool isCardVisibleTo(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (card == null || player == null)
                return false;
            return card.isCardVisibleTo(player);
        }

        [Obsolete]
        public static readonly Dictionary<int, string> obsoleteConstNodeDict = new Dictionary<int, string>()
        {
            { BuiltinActionDefineID.BooleanConst, "value" },
            { BuiltinActionDefineID.IntegerConst, "value" },
            { BuiltinActionDefineID.StringConst, "value" },
            { BuiltinActionDefineID.GetCardDefine, "cardRef" },
            { BuiltinActionDefineID.GetBuffDefine, GetBuffDefineAction.constName },
        };
    }
}





