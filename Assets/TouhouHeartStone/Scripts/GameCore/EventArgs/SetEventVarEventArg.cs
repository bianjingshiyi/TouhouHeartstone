using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class SetEventVarEventDefine : EventDefine
    {
        public static Task<EventArg> doEvent(CardEngine game, IEventArg eventArg, string varName, object afterValue)
        {
            var beforeValue = eventArg.getVar(varName);
            var define = game.getEventDefine<SetEventVarEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_EVENT, eventArg);
            arg.setVar(VAR_VAR_NAME, varName);
            arg.setVar(VAR_BEFORE_VALUE, beforeValue);
            arg.setVar(VAR_AFTER_VALUE, afterValue);
            return game.triggers.doEvent(arg);
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_EVENT, arg);
            record.setVar(VAR_VAR_NAME, arg);
            var value = arg.getVar<object>(VAR_AFTER_VALUE);
            if (value is Card card)
            {
                record.setCardState(VAR_AFTER_VALUE, card);
            }
            else if (value is Card[] cardArray)
            {
                record.setCardStates(VAR_AFTER_VALUE, cardArray);
            }
            else
            {
                record.setVar(VAR_AFTER_VALUE, value);
            }
        }

        public override Task execute(IEventArg arg)
        {
            var eventArg = arg.getVar<IEventArg>(VAR_EVENT);
            var varName = arg.getVar<string>(VAR_VAR_NAME);
            var afterValue = arg.getVar<object>(VAR_AFTER_VALUE);
            eventArg.setVar(varName, afterValue);
            return Task.CompletedTask;
        }

        public override string toString(EventArg arg)
        {
            var eventArg = arg.getVar<IEventArg>(VAR_EVENT);
            var varName = arg.getVar<string>(VAR_VAR_NAME);
            var beforeValue = arg.getVar<object>(VAR_BEFORE_VALUE);
            var afterValue = arg.getVar<object>(VAR_AFTER_VALUE);
            return $"将事件({eventArg})的变量{varName}从{beforeValue}变为{afterValue}";
        }
        public const string VAR_EVENT = EVENTVAR.EVENT;
        public const string VAR_VAR_NAME = EVENTVAR.EVENT_VAR_NAME;
        public const string VAR_BEFORE_VALUE = EVENTVAR.BEFORE_VALUE;
        public const string VAR_AFTER_VALUE = EVENTVAR.VALUE;
    }
}
