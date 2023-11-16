using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体伤害事件
    /// </summary>
    [EventChildren(typeof(THHCard.DamageEventArg))]
    public class MassDamageEventArg : EventArg, IMassCardEventArg
    {
        ICard[] IMassCardEventArg.getCards()
        {
            return cards;
        }
        public override void Record(IGame game, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, cards);
            record.setCardState(VAR_SOURCE, source);
            record.setVar(EVENTVAR.VALUE, value);
        }
        public override EventVariableInfo[] getBeforeEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_SOURCE, type = typeof(Card) },
                new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) },
            };
        }
        public override EventVariableInfo[] getAfterEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_SOURCE, type = typeof(Card) },
                new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) },
            };
        }
        public const string VAR_SOURCE = "伤害源";
        public Card[] cards
        {
            get => getVar<Card[]>(EVENTVAR.CARD);
            set => setVar(EVENTVAR.CARD, value);
        }
        public Card source
        {
            get => getVar<Card>(VAR_SOURCE);
            set => setVar(VAR_SOURCE, value);
        }
        public int value
        {
            get => getVar<int>(EVENTVAR.VALUE);
            set => setVar(EVENTVAR.VALUE, value);
        }
    }
}
