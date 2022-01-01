using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    public class EffectEditorLibrary
    {
        #region 玩家
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DRAW_TARGET_CARD, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static Task drawTargetCard(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]Card targetCard,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)]out HearthStoneRule.DrawEventArg drawEvent,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BURN_EVENT)]out HearthStoneRule.BurnEventArg burnEvent)
        {
            return card.owner.draw(game, targetCard, out drawEvent, out burnEvent);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_CARDS, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Task<Card> selectOneFromManyCards(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count)
        {
            return game.askSelect(card.owner, cards, count);
        }
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
    }
}
