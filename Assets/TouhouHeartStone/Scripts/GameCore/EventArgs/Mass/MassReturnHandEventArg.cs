using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体回手事件
    /// </summary>
    [EventChildren(typeof(THHPile.MoveCardEventArg))]
    public class MassReturnHandEventArg : EventArg, IMassCardEventArg
    {
        ICard[] IMassCardEventArg.getCards()
        {
            return movedCards;
        }
        public override void Record(IGame game, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, cards);
            record.setVar(EVENTVAR.PLAYER, player);
            record.setCardStates(VAR_MOVED_CARDS, movedCards);
        }
        public override EventVariableInfo[] getBeforeEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
            };
        }
        public override EventVariableInfo[] getAfterEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = VAR_MOVED_CARDS, type = typeof(Card[]) },
            };
        }
        public const string VAR_MOVED_CARDS = "移动的卡牌";
        public Card[] cards
        {
            get => getVar<Card[]>(EVENTVAR.CARD);
            set => setVar(EVENTVAR.CARD, value);
        }
        public Player player
        {
            get => getVar<Player>(EVENTVAR.PLAYER);
            set => setVar(EVENTVAR.PLAYER, value);
        }
        public Card[] movedCards
        {
            get => getVar<Card[]>(VAR_MOVED_CARDS);
            set => setVar(VAR_MOVED_CARDS, value);
        }
    }
}
