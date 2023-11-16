using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体移动事件
    /// </summary>
    [EventChildren(typeof(THHPile.MoveCardEventArg))]
    public class MassMoveCardEventArg : EventArg, IMassCardEventArg
    {
        ICard[] IMassCardEventArg.getCards()
        {
            return movedCards;
        }
        public override void Record(IGame game, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, cards);
            record.setVar(VAR_TO_PILE, to);
            record.setCardStates(VAR_MOVED_CARDS, movedCards);
        }
        public override EventVariableInfo[] getBeforeEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_TO_PILE, type = typeof(Pile) },
            };
        }
        public override EventVariableInfo[] getAfterEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_TO_PILE, type = typeof(Pile) },
                new EventVariableInfo() { name = VAR_MOVED_CARDS, type = typeof(Card[]) },
            };
        }
        public const string VAR_MOVED_CARDS = "移动的卡牌";
        public const string VAR_TO_PILE = "目标牌堆";
        public Card[] cards
        {
            get => getVar<Card[]>(EVENTVAR.CARD);
            set => setVar(EVENTVAR.CARD, value);
        }
        public Pile to
        {
            get => getVar<Pile>(VAR_TO_PILE);
            set => setVar(VAR_TO_PILE, value);
        }
        public Card[] movedCards
        {
            get => getVar<Card[]>(VAR_MOVED_CARDS);
            set => setVar(VAR_MOVED_CARDS, value);
        }
    }
}
