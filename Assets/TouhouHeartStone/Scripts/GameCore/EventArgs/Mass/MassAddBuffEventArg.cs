using System;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体添加增益事件
    /// </summary>
    [EventChildren(typeof(Card.AddBuffEventArg))]
    public class MassAddBuffEventArg : EventArg, IMassCardEventArg
    {
        ICard[] IMassCardEventArg.getCards()
        {
            return cards;
        }
        public override void Record(IGame game, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, cards);
            record.setVar(VAR_BUFF_DEFINE, buffDefine);
            record.setVar(VAR_BUFF, buffs);
        }
        public override EventVariableInfo[] getBeforeEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_BUFF_DEFINE, type = typeof(BuffDefine) },
            };
        }
        public override EventVariableInfo[] getAfterEventVarInfos()
        {
            return new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_BUFF_DEFINE, type = typeof(BuffDefine) },
                new EventVariableInfo() { name = VAR_BUFF, type = typeof(Buff[]) },
            };
        }
        public const string VAR_BUFF = "增益";
        public const string VAR_BUFF_DEFINE = "增益定义";
        public Card[] cards
        {
            get => getVar<Card[]>(EVENTVAR.CARD);
            set => setVar(EVENTVAR.CARD, value);
        }
        public BuffDefine buffDefine
        {
            get => getVar<BuffDefine>(VAR_BUFF);
            set => setVar(VAR_BUFF, value);
        }
        public Buff[] buffs
        {
            get => getVar<Buff[]>(VAR_BUFF);
            set => setVar(VAR_BUFF, value);
        }
    }
}
