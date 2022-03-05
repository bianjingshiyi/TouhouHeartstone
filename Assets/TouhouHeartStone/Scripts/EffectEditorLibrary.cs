using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game
{
    public enum CardDefineSelect
    {
        Spell,
        Servant,
        Item
    }
    public static class EffectEditorLibrary
    {
        //  把这个“玩家”逐步移到下面几个去
        #region 玩家
        /// <summary>
        /// 获取玩家属性
        /// </summary>
        /// <param name="player"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PLAYER_PROP, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getPlayerProp(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)]string propName)
        {
            return player.getProp(propName);
        }
        /// <summary>
        /// 设置玩家属性
        /// </summary>
        /// <param name="player"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SET_PLAYER_PROP, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static void setPlayerProp(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]object value)
        {
            player.setProp(propName, value);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_MASTER, StringTable.EDITOR_NODE_CATEGORY_PLAYER, "GetMaster")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getMaster([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getMaster();
        }
        /// <summary>
        /// 获取玩家手牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PLAYER_HAND_CARDS, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getHandCards([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]Player player)
        {
            return player.getHand();
        }
        #endregion

        //  有关全局的操作，比如说获取发现池，获取卡牌
        #region 全局

        #endregion

        //  有关日志的一些操作，比如说获取出牌记录，获取事件发生次数 
        #region 日志

        #endregion

        //  有关牌堆的操作，比如说洗牌，获取特定牌等等
        #region 牌堆
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARDS_IN_DECK_WITH_CONDITION, StringTable.EDITOR_NODE_CATEGORY_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> getCardsInDeckWithCondition(CardEngine game, Card card,
        [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION, isConst: true)] ActionValueRef condition)
        {
            Pile deck = card.owner.getDeck();
            List<Card> cardList = new List<Card>();
            for (int i = 0; i < deck.count; i++)
            {
                bool result = await game.getActionReturnValueAsync<bool>(deck[i], null, null, condition.action, condition.index);
                if (result)
                {
                    cardList.Add(deck[i]);
                }
            }
            return cardList.ToArray();
        }
        #endregion

        //  卡牌（包括随从）的操作，比如addbuff，attack
        #region 卡牌
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARD_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getCardDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_REFERENCE, true)] CardReference cardRef)
        {
            return game.getDefine(cardRef.cardId);
        }
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
        [return: ActionNodeParam("Value")]
        public static bool IsCardDefine([ActionNodeParam("Card")] Card card, [ActionNodeParam("Type", isConst: true)] CardDefineSelect select)
        {
            switch (select)
            {
                case CardDefineSelect.Spell:
                    return card.isSpell();
                case CardDefineSelect.Servant:
                    return card.isServant();
                default:
                    return card.isItem();
            }
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
        /// 创建衍生卡并置入手牌
        /// </summary>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_HAND, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToHand(CardEngine game, Card card, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            Player player = card.getOwner();
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
        /// <param name="card"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_SHUFFLE_TO_DECK, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardShuffleToDeck(CardEngine game, Card card, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            Player player = card.owner;
            Card createdCard = game.createCard(cardDefine);
            await player.getDeck().insert(game, createdCard, game.randomInt(0, player.getDeck().count));
            return createdCard;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_THIS_CARD, StringTable.EDITOR_NODE_CATEGORY_CARD, "ThisCard")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card thisCard(Card card)
        {
            return card;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PROPERTY, StringTable.EDITOR_NODE_CATEGORY_CARD, "GetProperty")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            return card.getProp(game, propName);
        }
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
        /// 对固定数量的随机敌人造成法术伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="count"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg> dealSpecificSpellDamageToSpecificNumberOfRandomTarget(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)]int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)]int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)]Card[] targets)
        {
            return targets.damageByRandomCount(game, card, card.owner.getSpellDamage(game, damage), count);
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
        #endregion
        #region 增益
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_BUFF_DEFINE, StringTable.EDITOR_NODE_CATEGORY_BUFF, "GetBuffDefine")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)]
        public static BuffDefine getBuffDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ID, isConst: true)]int id)
        {
            return game.getBuffDefine(id);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_THIS_BUFF, StringTable.EDITOR_NODE_CATEGORY_BUFF, "ThisBuff")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)]
        public static Buff thisBuff(Buff buff)
        {
            return buff;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_ADD_BUFF, StringTable.EDITOR_NODE_CATEGORY_BUFF, "AddBuff")]
        public static Task addBuff(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)]BuffDefine buffDefine)
        {
            return card.addBuff(game, new GeneratedBuff(buffDefine));
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_REMOVE_BUFF, StringTable.EDITOR_NODE_CATEGORY_BUFF, "RemoveBuff")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_IS_BUFF_REMOVED)]
        public static Task<bool> removeBuff(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)]Buff buff)
        {
            return card.removeBuff(game, buff);
        }
        #endregion
        //  玩家的行动，比如抽一张牌，获得一张牌
        #region 行动
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DRAW_TARGET_CARD, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static Task drawTargetCard(CardEngine game, Card card,
        [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card targetCard,
        [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)] out HearthStoneRule.DrawEventArg drawEvent,
        [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BURN_EVENT)] out HearthStoneRule.BurnEventArg burnEvent)
        {
            return card.owner.draw(game, targetCard, out drawEvent, out burnEvent);
        }

        /// <summary>
        /// 简单抽牌
        /// 抽牌行动的简单实现，抽一张牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="player"></param>
        /// <param name="drawEvent"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__NAME, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        public static Task simpleDrawCard(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__DRAW_EVENT)] out HearthStoneRule.DrawEventArg drawEvent)
        {
            return player.draw(game, out drawEvent);
        }


        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_CARDS, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Task<Card> selectOneFromManyCards(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count)
        {
            return game.askSelect(card.owner, cards, count);
        }
        #endregion

        /// <summary>
        /// 简单召唤召唤物节点
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="player"></param>
        /// <param name="define"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE__SIMPLE_CREATE_TOKEN__NAME, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static Task simpleCreateToken(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_NODE__SIMPLE_CREATE_TOKEN__PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_NODE__SIMPLE_CREATE_TOKEN__DEFINE)] CardDefine define)
        {
            return player.createToken(game, define);
        }
    }
}
