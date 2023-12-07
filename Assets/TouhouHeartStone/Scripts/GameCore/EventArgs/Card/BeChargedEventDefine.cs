using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(CardPropChangeEventDefine))]
    public class BeChargedEventDefine : EventDefine
    {
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(EVENTVAR.CARD, arg);
            record.setVar(VAR_BEFORE_VALUE, arg);
            record.setVar(EVENTVAR.VALUE, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var value = arg.getVar<int>(EVENTVAR.VALUE);
            game.logger.logTrace(toString(arg as EventArg));
            return card.setProp(game, CARD.CHARGED_LEVEL, value);
        }
        public override string toString(EventArg arg)
        {
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var value = arg.getVar<int>(EVENTVAR.VALUE);
            var beforeValue = arg.getVar<int>(VAR_BEFORE_VALUE);
            return $"{card}蓄力层数从{beforeValue}改变为{value}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card, int value)
        {
            var beforeValue = card.getChargedLevel(game);
            var define = game.getEventDefine<BeChargedEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.VALUE, value);
            arg.setVar(VAR_BEFORE_VALUE, beforeValue);
            return game.triggers.doEvent(arg);
        }
        public const string VAR_BEFORE_VALUE = "原数值";

    }
}
