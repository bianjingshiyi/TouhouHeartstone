using System.Collections.Generic;
using TouhouCardEngine.Interfaces;

namespace Game.BattleTable
{
    public class EventAnimStartData
    {
        public EventAnimStartData(IEventArg arg)
        {
            eventArg = arg;
            foreach (var name in arg.getVarNames())
            {
                var value = arg.getVar(name);
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
        public IEventArg eventArg;
        private Dictionary<string, object> _varDict = new Dictionary<string, object>();
    }
}
