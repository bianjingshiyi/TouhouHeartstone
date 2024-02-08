using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(SetCardPropEventDefine))]
    public class BeChargedEventDefine : EventDefine
    {
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(VAR_CARD, arg);
            record.setVar(VAR_BEFORE_VALUE, arg);
            record.setVar(VAR_VALUE, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(VAR_CARD);
            var value = arg.getVar<int>(VAR_VALUE);
            game.logger.logTrace(toString(arg as EventArg));
            return card.setProp(game, CARD.CHARGED_LEVEL, value);
        }
        public override string toString(EventArg arg)
        {
            var card = arg.getVar<Card>(VAR_CARD);
            var value = arg.getVar<int>(VAR_VALUE);
            var beforeValue = arg.getVar<int>(VAR_BEFORE_VALUE);
            return $"{card}蓄力层数从{beforeValue}改变为{value}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card, int value)
        {
            var beforeValue = card.getChargedLevel(game);
            var define = game.getEventDefine<BeChargedEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD, card);
            arg.setVar(VAR_VALUE, value);
            arg.setVar(VAR_BEFORE_VALUE, beforeValue);
            return game.triggers.doEvent(arg);
        }

        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_VALUE = EVENTVAR.VALUE;
        public const string VAR_BEFORE_VALUE = EVENTVAR.BEFORE_VALUE;
    }
}
