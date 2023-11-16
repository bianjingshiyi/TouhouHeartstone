using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 分配伤害事件
    /// </summary>
    [EventChildren(typeof(THHCard.DamageEventArg))]
    public class SplitDamageEventArg : EventArg, IMassCardEventArg
    {
        ICard[] IMassCardEventArg.getCards()
        {
            return damagedCards;
        }
        public override void Record(IGame game, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, targets);
            record.setCardState(VAR_SOURCE, source);
            record.setVar(EVENTVAR.VALUE, value);
            record.setCardStates(VAR_DAMAGED_CARDS, damagedCards);
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
                new EventVariableInfo() { name = VAR_DAMAGED_CARDS, type = typeof(Card[]) },
            };
        }
        public const string VAR_SOURCE = "伤害源";
        public const string VAR_DAMAGED_CARDS = "击中的卡牌";
        public Card[] targets
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
        public Card[] damagedCards
        {
            get => getVar<Card[]>(VAR_DAMAGED_CARDS);
            set => setVar(VAR_DAMAGED_CARDS, value);
        }
    }
}
