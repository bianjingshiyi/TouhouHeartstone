using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    public abstract class Event
    {
        public Event(string eventName)
        {
            name = eventName;
        }
        public virtual void execute(CardEngine engine)
        {

        }
        public Event parent
        {
            get { return _parent; }
            set
            {
                if (_parent != null)
                {
                    _parent.before.Remove(this);
                    _parent.child.Remove(this);
                    _parent.after.Remove(this);
                }
                _parent = value;
                if (_parent != null)
                {
                    if (_parent.phase == EventPhase.before)
                        _parent.before.Add(this);
                    else if (_parent.phase == EventPhase.after)
                        _parent.after.Add(this);
                    else
                        _parent.child.Add(this);
                }
            }
        }
        Event _parent = null;
        public EventPhase phase { get; set; } = EventPhase.before;
        public List<Event> before { get; } = new List<Event>();
        public List<Event> child { get; } = new List<Event>();
        public List<Event> after { get; } = new List<Event>();
        public string name { get; } = null;
        public string[] getVarNames()
        {
            return dicVar.Keys.ToArray();
        }
        public T getProp<T>(string varName)
        {
            if (dicVar.ContainsKey(varName) && dicVar[varName] is T)
                return (T)dicVar[varName];
            else
                return default(T);
        }
        public void setProp<T>(string propName, T value)
        {
            if (dicVar.ContainsKey(propName))
                dicVar[propName] = value;
            else
                dicVar.Add(propName, value);
        }
        public object this[string varName]
        {
            get { return dicVar.ContainsKey(varName) ? dicVar[varName] : null; }
            set { dicVar[varName] = value; }
        }
        Dictionary<string, object> dicVar { get; } = new Dictionary<string, object>();
        public override string ToString()
        {
            string s = string.Empty;
            if (before.Count > 0)
            {
                s += "{";
                for (int i = 0; i < before.Count; i++)
                {
                    s += before[i].ToString();
                    if (i != before.Count - 1)
                        s += ";";
                }
                s += "}";
            }
            s += name;
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
            if (child.Count > 0)
            {
                s += "{";
                for (int i = 0; i < child.Count; i++)
                {
                    s += child[i].ToString();
                    if (i != child.Count - 1)
                        s += ";";
                }
                s += "}";
            }
            if (after.Count > 0)
            {
                s += "{";
                for (int i = 0; i < after.Count; i++)
                {
                    s += after[i].ToString();
                    if (i != after.Count - 1)
                        s += ";";
                }
                s += "}";
            }
            return s;
        }

    }
}