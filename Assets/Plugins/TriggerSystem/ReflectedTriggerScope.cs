using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace TriggerSystem
{
    public class ReflectedTriggerAction : TriggerAction
    {
        string name { get; set; }
        public ReflectedTriggerAction(string name)
        {
            this.name = name;
        }
        public override void execute(ReflectedTriggerScope scope)
        {
            ReflectedTriggerActionDefine define = scope.getAction(name);
        }
    }
    public class ReflectedTriggerScope
    {
        Dictionary<string, ReflectedTriggerActionDefine> dicAction { get; set; } = null;
        public ReflectedTriggerScope(UnityEngine.Object obj)
        {
            dicAction = new Dictionary<string, ReflectedTriggerActionDefine>();
            Assembly assembly = obj.GetType().Assembly;
            foreach (Type type in assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    TriggerActionAttribute attribute = method.GetCustomAttribute<TriggerActionAttribute>(false);
                    if (attribute != null)
                    {
                        ReflectedTriggerActionDefine action = new ReflectedTriggerActionDefine(attribute, method);
                        dicAction.Add(action.name, action);
                    }
                }
            }
        }
        public ReflectedTriggerActionDefine[] getActions()
        {
            return dicAction.Values.ToArray();
        }
        public ReflectedTriggerActionDefine getAction(string name)
        {
            if (dicAction.ContainsKey(name))
                return dicAction[name];
            else
                return null;
        }
    }
}