using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;

namespace Game
{
    public static class EventDefineHelper
    {
        public static string[] getEventVarNames(this IEnumerable<EventDefine> list, string eventName)
        {
            EventDefine eventTypeInfo = list.FirstOrDefault(e => e.eventName == eventName);
            if (eventTypeInfo.variableInfos != null)
            {
                string[] varNames = new string[eventTypeInfo.variableInfos.Length];
                for (int i = 0; i < eventTypeInfo.variableInfos.Length; i++)
                {
                    varNames[i] = eventTypeInfo.variableInfos[i].name;
                }
                return varNames;
            }
            else
                return null;
        }
        public static int getEventVarNameIndex(this IEnumerable<EventDefine> list, string eventName, string eventVarName)
        {
            EventDefine eventTypeInfo = list.FirstOrDefault(e => e.eventName == eventName);
            if (eventTypeInfo.variableInfos != null)
            {
                for (int i = 0; i < eventTypeInfo.variableInfos.Length; i++)
                {
                    if (eventTypeInfo.variableInfos[i].name == eventVarName)
                        return i;
                }
            }
            return -1;
        }
        public static EventVariableInfo getEventVarInfo(this IEnumerable<EventDefine> list, string eventName, int index)
        {
            EventDefine eventTypeInfo = list.FirstOrDefault(e => e.eventName == eventName);
            if (eventTypeInfo.variableInfos != null && index < eventTypeInfo.variableInfos.Length)
            {
                return eventTypeInfo.variableInfos[index];
            }
            return null;
        }
        public static int getEventTypeIndex(this IEnumerable<EventDefine> list, string eventName)
        {
            int index = 0;
            foreach (var eventDefine in list)
            {
                if (eventDefine.eventName == eventName)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
        public static string getEventTypeNameByIndex(this IEnumerable<EventDefine> list, int index)
        {
            return list.ElementAt(index).eventName;
        }
        public static string[] getEventTypeNames(this IEnumerable<EventDefine> list)
        {
            return list.Select(e => e.eventName).ToArray();
        }
        public static IEnumerable<EventDefine> filterBeforeAfterEventDefines(this IEnumerable<EventDefine> list)
        {
            return list.Where(e => e.defineType != EventDefineType.Event);
        }
        public static IEnumerable<EventDefine> filterMainEventDefines(this IEnumerable<EventDefine> list)
        {
            return list.Where(e => e.defineType == EventDefineType.Event);
        }
        public static EventDefine FindEventDefine(this IEnumerable<EventDefine> list, string eventTypeName)
        {
            return list.FirstOrDefault(e => e.eventName == eventTypeName || (e.obsoleteNames != null && e.obsoleteNames.Contains(eventTypeName)));
        }
        public static string[] getEventTypeNamesForEditor(this IEnumerable<EventDefine> list, MainManager main)
        {
            var eventManager = main.eventManager;
            return list.Select(e => eventManager.translateEventDefineNameForEditor(e)).ToArray();
        }
    }
}
