using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    public class Event
    {
        public Event(string eventName)
        {
            name = eventName;
        }
        public void copyVars(EventWitness witness)
        {
            foreach (var p in dicVar)
            {
                witness[p.Key] = p.Value;
            }
        }
        public Event parent
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
        Event _parent = null;
        List<Event> childList { get; } = new List<Event>();
        public string name { get; } = null;
        public T getVar<T>(string varName)
        {
            if (dicVar.ContainsKey(varName) && dicVar[varName] is T)
                return (T)dicVar[varName];
            else
                return default(T);
        }
        public void setVar<T>(string varName, T value)
        {
            if (dicVar.ContainsKey(varName))
                dicVar[varName] = value;
            else
                dicVar.Add(varName, value);
        }
        public object this[string varName]
        {
            get { return dicVar.ContainsKey(varName) ? dicVar[varName] : null; }
            set { dicVar[varName] = value; }
        }
        Dictionary<string, object> dicVar { get; } = new Dictionary<string, object>();
        public override string ToString()
        {
            string s = name;
            if (dicVar.Count > 0)
            {
                s += "(";
                foreach (var p in dicVar)
                {
                    if (s[s.Length - 1] == '(')
                        s += p.Key + ":" + p.Value.ToString();
                    else
                        s += "," + p.Key + ":" + p.Value.ToString();
                }
                s += ")";
            }
            if (childList.Count > 0)
            {
                s += "{";
                for (int i = 0; i < childList.Count; i++)
                {
                    s += childList[i].ToString();
                    if (i != childList.Count - 1)
                        s += ";";
                }
                s += "}";
            }
            return s;
        }
    }
}