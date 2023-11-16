using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体变形事件
    /// </summary>
    [EventChildren(typeof(Card.SetDefineEventArg))]
    public class MassSetCardDefineEventArg : EventArg, IMassCardEventArg
    {
        ICard[] IMassCardEventArg.getCards()
        {
            return card;
        }
        public override void Record(IGame game, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, card);
            record.setVar(VAR_AFTER_DEFINE, cardDefine);
        }
        public override EventVariableInfo[] getBeforeEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_AFTER_DEFINE, type = typeof(CardDefine) },
            };
        }
        public override EventVariableInfo[] getAfterEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_AFTER_DEFINE, type = typeof(CardDefine) },
            };
        }
        public const string VAR_AFTER_DEFINE = "目标卡牌定义";
        public Card[] card
        {
            get => getVar<Card[]>(EVENTVAR.CARD);
            set => setVar(EVENTVAR.CARD, value);
        }
        public CardDefine cardDefine
        {
            get => getVar<CardDefine>(VAR_AFTER_DEFINE);
            set => setVar(VAR_AFTER_DEFINE, value);
        }
    }
}
