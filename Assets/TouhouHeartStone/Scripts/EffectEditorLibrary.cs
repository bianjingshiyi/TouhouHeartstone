using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    public enum CardDefineSelect
    {
        Spell,
        Servant,
        Item
    }
    public class EffectEditorLibrary
    {
        //  把这个“玩家”逐步移到下面几个去
        #region 玩家
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
        /// 创建衍生卡
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
