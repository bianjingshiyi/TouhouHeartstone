using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Histories;
using TouhouCardEngine.Interfaces;

namespace Game.BattleTable
{
    public class EventAnimStartData
    {
        public EventAnimStartData(IEventArg arg)
        {
            foreach (var name in arg.getVarNames())
            {
                var value = arg.getVar(name);
                if (value is Card card)
                {
                    if (card == null)
                    {
                        value = new CardState(null, 0);
                    }
                    else
                    {
                        value = new CardState(card, card.getCurrentHistory());
                    }
                }
                else if (value is IEnumerable<Card> cards && value is not Pile)
                {
                    var stateArray = new CardState[cards.Count()];
                    for (int i = 0; i < stateArray.Length; i++)
                    {
                        var element = cards.ElementAt(i);
                        if (element == null)
                        {
                            stateArray[i] = new CardState(null, 0);
                            continue;
                        }
                        stateArray[i] = new CardState(element, element.getCurrentHistory());
                    }
                    value = stateArray;
                }
                _varDict.Add(name, value);
            }
        }
        public object getVar(string name)
        {
            return _varDict[name];
        }
        public T getVar<T>(string name)
        {
            var value = getVar(name);
            if (value is T tValue)
                return tValue;
            return default;
        }
        public void setVar(string name, object value)
        {
            _varDict[name] = value;
        }
        private Dictionary<string, object> _varDict = new Dictionary<string, object>();
    }
}
