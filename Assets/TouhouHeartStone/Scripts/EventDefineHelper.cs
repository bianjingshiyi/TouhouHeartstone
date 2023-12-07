using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;

namespace Game
{
    public static class EventDefineHelper
    {
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
        public static EventDefine FindEventDefine(this IEnumerable<EventDefine> list, string eventTypeName)
        {
            return list.FirstOrDefault(e => e.eventName == eventTypeName || (e.obsoleteNames != null && e.obsoleteNames.Contains(eventTypeName)));
        }
        public static EventDefine FindEventDefine(this IEnumerable<EventDefine> list, EventReference eventRef)
        {
            return list.FirstOrDefault(e => (e.cardPoolId == eventRef.cardPoolId && e.eventName == eventRef.eventName) || (e.obsoleteNames != null && e.obsoleteNames.Contains(eventRef.eventName)));
        }
        public static EventTriggerTime getEventTriggerTimeByName(this IEnumerable<EventDefine> list, string triggerTimeName)
        {
            var beforeEventDefine = list.FirstOrDefault(e =>
                EventHelper.getNameBefore(e.eventName) == triggerTimeName ||
                (e.obsoleteNames != null && e.obsoleteNames.Any(n => EventHelper.getNameBefore(n) == triggerTimeName)));

            var afterEventDefine = list.FirstOrDefault(e =>
                EventHelper.getNameAfter(e.eventName) == triggerTimeName ||
                (e.obsoleteNames != null && e.obsoleteNames.Any(n => EventHelper.getNameAfter(n) == triggerTimeName)));

            if (beforeEventDefine != null)
            {
                return new EventTriggerTime(beforeEventDefine, EventTriggerTimeType.Before);
            }
            else if (afterEventDefine != null)
            {
                return new EventTriggerTime(afterEventDefine, EventTriggerTimeType.After);
            }
            return null;
        }
        public static EventTriggerTime[] getEventTriggerTimes(this IEnumerable<EventDefine> list)
        {
            var results = new List<EventTriggerTime>();
            foreach (var eventDefine in list)
            {
                results.Add(new EventTriggerTime(eventDefine, EventTriggerTimeType.Before));
                results.Add(new EventTriggerTime(eventDefine, EventTriggerTimeType.After));
            }
            return results.ToArray();
        }
        public static string[] getEventTypeNamesForEditor(this IEnumerable<EventDefine> list, MainManager main)
        {
            var eventManager = main.eventManager;
            return list.Select(e => eventManager.translateEventDefineNameForEditor(e)).ToArray();
        }
        public static int getEventTriggerTimeIndex(this IEnumerable<EventTriggerTime> list, EventTriggerTime triggerTime)
        {
            int index = 0;
            foreach (var time in list)
            {
                if (time == triggerTime)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
        public static string[] getEventTriggerTimeNames(this IEnumerable<EventTriggerTime> list)
        {
            return list.Select(e => e.getTimeName()).ToArray();
        }
        public static string[] getEventTriggerTimeNamesForEditor(this IEnumerable<EventTriggerTime> list, MainManager main)
        {
            var eventManager = main.eventManager;
            return list.Select(e => eventManager.translateEventTriggerTimeNameForEditor(e)).ToArray();
        }
    }
}
