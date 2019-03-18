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
        public void copyVars(CardEngine game, EventWitness witness)
        {
            foreach (var p in dicVar)
            {
                //基本类型
                if (p.Value is int ||
                    p.Value is int[] ||
                    p.Value is float ||
                    p.Value is float[] ||
                    p.Value is bool ||
                    p.Value is bool[] ||
                    p.Value is string ||
                    p.Value is string[])
                    witness[p.Key] = p.Value;
                else if (p.Value is Card)
                {
                    witness[p.Key + "_id"] = (p.Value as Card).id;
                    witness[p.Key + "_define_id"] = (p.Value as Card).define.id;
                }
                else if (p.Value is Card[])
                {
                    witness[p.Key + "_id"] = (p.Value as Card[]).Select(e => { return e.id; }).ToArray();
                    witness[p.Key + "_define_id"] = (p.Value as Card[]).Select(e => { return e.define.id; }).ToArray();
                }
                else if (p.Value is Pile)
                {
                    witness[p.Key + "_owner_index"] = Array.IndexOf(game.getPlayers(), (p.Value as Pile).owner);
                    witness[p.Key + "_name"] = (p.Value as Pile).name;
                }
                else if (p.Value is Pile[])
                {
                    witness[p.Key + "_owner_index"] = (p.Value as Pile[]).Select(e => { return Array.IndexOf(game.getPlayers(), e); }).ToArray();
                    witness[p.Key + "_name"] = (p.Value as Pile[]).Select(e => { return e.name; }).ToArray();
                }
                else if (p.Value is Player)
                    witness[p.Key + "_index"] = Array.IndexOf(game.getPlayers(), p.Value);
                else if (p.Value is Player[])
                    witness[p.Key + "_index"] = (p.Value as Player[]).Select(e => { return Array.IndexOf(game.getPlayers(), e); }).ToArray();
                else
                    throw new NotSupportedException("Event无法将" + p.Value.GetType().Name + "类型的属性写入Witness！");
            }
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