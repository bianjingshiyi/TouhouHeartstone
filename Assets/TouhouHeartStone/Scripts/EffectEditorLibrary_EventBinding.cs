using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TouhouCardEngine;

namespace Game
{
    partial class EffectEditorLibrary
    {
        private static Dictionary<int, string[]> eventNodeBindingDic;
        public static bool canActionAddEffect(ActionReference actionRef)
        {
            return getActionBindedEventNames(actionRef) != null;
        }
        public static string[] getActionBindedEventNames(ActionReference actionRef)
        {
            var dic = getOrLoadEventBindingDic();
            if (actionRef.cardPoolId == 0)
            {
                if (dic.TryGetValue(actionRef.defineId, out string[] types))
                {
                    return types;
                }
            }
            return null;
        }
        private static Dictionary<int, string[]> getOrLoadEventBindingDic()
        {
            if (eventNodeBindingDic == null)
            {
                var assemblies = new Assembly[] { typeof(EffectEditorLibrary).Assembly };
                eventNodeBindingDic = loadEventBindingDic(assemblies);
            }
            return eventNodeBindingDic;
        }
        private static Dictionary<int, string[]> loadEventBindingDic(Assembly[] assemblies)
        {
            Dictionary<int, string[]> bindingDict = new Dictionary<int, string[]>();
            var methods = assemblies.SelectMany(a => a.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)));
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<EventNodeAttribute>() is EventNodeAttribute attribute &&
                    method.GetCustomAttribute<ActionNodeMethodAttribute>() is ActionNodeMethodAttribute methodAttribute)
                {
                    bindingDict.Add(methodAttribute.defineId, attribute.types.Select(t => EventHelper.getName(t)).ToArray());
                }
            }
            return bindingDict;
        }
    }

    public class EventNodeAttribute : Attribute
    {
        public EventNodeAttribute(params Type[] types)
        {
            this.types = types;
        }
        public Type[] types;
    }
}
