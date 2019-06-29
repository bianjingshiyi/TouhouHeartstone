using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public abstract class EventWitness : IWitness
    {
        public EventWitness(string eventName)
        {
            this.eventName = eventName;
        }
        public void setParent(EventWitness value)
        {
            if (parent != null)
            {
                parent.before.Remove(this);
                parent.child.Remove(this);
                parent.after.Remove(this);
            }
            parent = value;
            if (parent != null)
                parent.child.Add(this);
        }
        public void setParent(EventWitness value, EventPhase phase)
        {
            if (parent != null)
            {
                parent.before.Remove(this);
                parent.child.Remove(this);
                parent.after.Remove(this);
            }
            parent = value;
            if (parent != null)
            {
                if (phase == EventPhase.before)
                    parent.before.Add(this);
                else if (phase == EventPhase.after)
                    parent.after.Add(this);
                else
                    parent.child.Add(this);
            }
        }
        public EventWitness parent { get; private set; } = null;
        public List<EventWitness> before { get; } = new List<EventWitness>();
        public List<EventWitness> child { get; } = new List<EventWitness>();
        public List<EventWitness> after { get; } = new List<EventWitness>();
        public string[] getVarNames()
        {
            return dicVar.Keys.ToArray();
        }
        public object getVar(string varName, bool throwException = true)
        {
            if (dicVar.ContainsKey(varName))
                return dicVar[varName];
            if (throwException)
                throw new KeyNotFoundException("没有找到变量" + varName);
            else
                return null;
        }
        public T getVar<T>(string varName, bool throwException = true)
        {
            if (dicVar.ContainsKey(varName) && dicVar[varName] is T)
                return (T)dicVar[varName];
            if (throwException)
                throw new KeyNotFoundException("没有找到变量" + varName);
            else
                return default;
        }
        public void setVar<T>(string varName, T value)
        {
            dicVar[varName] = value;
        }
        public int number { get; set; }
        public string eventName { get; }
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
            s += eventName;
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