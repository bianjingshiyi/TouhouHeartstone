using System.Threading.Tasks;
using System;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体治疗事件
    /// </summary>
    [EventChildren(typeof(HealEventDefine))]
    public class MassHealEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassHealEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_SOURCE, type = typeof(Card) },
                new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_SOURCE, type = typeof(Card) },
                new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, arg);
            record.setCardState(VAR_SOURCE, arg);
            record.setVar(EVENTVAR.VALUE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var source = arg.getVar<Card>(VAR_SOURCE);
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var value = arg.getVar<int>(EVENTVAR.VALUE);
            foreach (var card in cards)
            {
                await card.heal(game, source, value);
            }
        }
        public override string toString(EventArg arg)
        {
            var source = arg.getVar<Card>(VAR_SOURCE);
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var value = arg.getVar<int>(EVENTVAR.VALUE);
            var cardString = string.Join<Card>(",", cards);
            return $"{source} 为 {cardString} 治疗 {value} 点生命值";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card source, Card[] targets, int value)
        {
            var define = game.getEventDefine<MassHealEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_SOURCE, source);
            arg.setVar(EVENTVAR.CARD, targets);
            arg.setVar(EVENTVAR.VALUE, value);
            return game.triggers.doEvent(arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg eventArg)
        {
            return eventArg.getVar<Card[]>(EVENTVAR.CARD);
        }
        public const string VAR_SOURCE = "治疗源";
    }
}
