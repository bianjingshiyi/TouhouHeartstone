using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public class EventWitness : IWitness
    {
        public EventWitness(string eventName)
        {
            this.eventName = eventName;
        }
        public EventWitness parent
        {
            get { return _parent; }
            set
            {
                if (_parent != null)
                    _parent.childList.Remove(this);
                _parent = value;
                if (_parent != null)
                    _parent.childList.Add(this);
            }
        }
        EventWitness _parent = null;
        List<EventWitness> childList { get; } = new List<EventWitness>();
        public object this[string varName]
        {
            get { return dicVar.ContainsKey(varName) ? dicVar[varName] : null; }
            set
            {
                if (dicVar.ContainsKey(varName))
                    dicVar[varName] = varName;
                else
                    dicVar.Add(varName, value);
            }
        }
        public int number { get; set; }
        public string eventName { get; }
        Dictionary<string, object> dicVar { get; } = new Dictionary<string, object>();
    }
}