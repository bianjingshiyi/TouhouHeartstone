using System;
using System.Linq;
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
        /// <summary>
        /// 以前序优先的方式遍历整个结构并执行参数动作。参数动作的返回值表示是否已经对该节点完成了处理，如果返回值为真，则放弃对该节点的子节点的遍历。
        /// </summary>
        /// <param name="action"></param>
        [Obsolete("foreachDo方法被废弃")]
        public void foreachDo(Func<EventWitness, bool> action)
        {
            for (int i = 0; i < before.Count; i++)
            {
                before[i].foreachDo(action);
            }
            if (!action(this))
            {
                for (int i = 0; i < child.Count; i++)
                {
                    child[i].foreachDo(action);
                }
            }
            for (int i = 0; i < after.Count; i++)
            {
                after[i].foreachDo(action);
            }
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
        public object getVar(string varName)
        {
            if (dicVar.ContainsKey(varName))
                return dicVar[varName];
            throw new KeyNotFoundException("没有找到变量" + varName);
        }
        public T getVar<T>(string varName)
        {
            if (dicVar.ContainsKey(varName) && dicVar[varName] is T)
                return (T)dicVar[varName];
            throw new KeyNotFoundException("没有找到变量" + varName);
        }
        public void setVar<T>(string varName, T value)
        {
            dicVar[varName] = value;
        }
        [Obsolete("因为索引器的类型并不严谨，请使用getVar与setVar作为替代。")]
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