using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class SetEventVarEventArg : EventArg
    {
        public SetEventVarEventArg()
        {

        }
        public SetEventVarEventArg(IEventArg eventArg, string varName, object afterValue)
        {
            this.eventArg = eventArg;
            this.varName = varName;
            this.afterValue = afterValue;
        }
        public override void Record(IGame game, EventRecord record)
        {
            record.setVar(VAR_EVENT, eventArg);
            record.setVar(VAR_VAR_NAME, varName);
            var value = afterValue;
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
        public IEventArg eventArg
        {
            get => getVar<IEventArg>(VAR_EVENT);
            set => setVar(VAR_EVENT, value);
        }
        public string varName
        {
            get => getVar<string>(VAR_VAR_NAME);
            set => setVar(VAR_VAR_NAME, value);
        }
        public object afterValue
        {
            get => getVar<object>(VAR_AFTER_VALUE);
            set => setVar(VAR_AFTER_VALUE, value);
        }
        public const string VAR_EVENT = "事件";
        public const string VAR_VAR_NAME = "变量名";
        public const string VAR_BEFORE_VALUE = "原值";
        public const string VAR_AFTER_VALUE = "值";
    }
}
