using System;
using System.Collections.Generic;
using TouhouCardEngine;

namespace TouhouHeartstone.Frontend
{
    class AutoEventArgs : EventArgs
    {
        public AutoEventArgs(EventWitness witness)
        {
            eventName = witness.eventName;
            foreach (string varName in witness.getVarNames())
            {
                setProp(varName, witness.getVar(varName));
            }
        }
        public AutoEventArgs(string eventName)
        {
            this.eventName = eventName;
        }
        public string eventName { get; }
        Dictionary<string, object> dicProp { get; } = new Dictionary<string, object>();
        public T getProp<T>(string propName)
        {
            if (dicProp.ContainsKey(propName) && dicProp[propName] is T)
                return (T)dicProp[propName];
            return default;
        }
        public void setProp<T>(string propName, T value)
        {
            dicProp[propName] = value;
        }
        public override string ToString()
        {
            return "自定义事件(" + eventName + ")";
        }
    }
}
