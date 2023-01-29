using System;
using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using Game;
using TouhouCardEngine;
using UnityEngine;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 私有方法
        /// <summary>
        /// 加载所有事件类型
        /// </summary>
        private void loadEventTypes()
        {
            _eventTypeInfoList.Clear();
            _eventTypeInfoList.AddRange(EventTypeInfoLoader.getOrLoadEventDefines());
            _eventTypeNames.AddRange(EventTypeInfoLoader.getOrLoadEventNames());
        }
        #endregion
        #region 属性字段
        EventTypeInfoCollection _eventTypeInfoList = new EventTypeInfoCollection();
        List<string> _eventTypeNames = new List<string>();
        #endregion
    }
    public class EventTypeInfoLoader
    {

        #region 公有方法
        private static void loadEventTypes()
        {
            _eventDefineList = new List<EventDefine>();
            _eventDefineBeforeTranslations = new Dictionary<string, string>();
            _eventDefineAfterTranslations = new Dictionary<string, string>();
            _eventNames = new List<string>();
            //通过反射加载硬编码的事件类型
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(EventArg))//是EventArg
                        && !type.IsAbstract//不是抽象类
                        )
                    {
                        EventArg eventArg;
                        try
                        {
                            eventArg = Activator.CreateInstance(type) as EventArg;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("无法加载事件类型" + type.FullName + "：" + e);
                            continue;
                        }
                        var eventName = EventHelper.getName(eventArg);
                        if (_hiddenEvents.Contains(eventName)) //隐藏的事件忽略掉。
                            continue;

                        _eventNames.Add(eventName);

                        string editorName;
                        if (!_eventNameTranslations.TryGetValue(eventName, out editorName))
                        {
                            editorName = eventName;
                        }

                        //发生时事件
                        string beforeName = EventHelper.getNameBefore(eventArg);
                        _eventDefineList.Add(new EventDefine()
                        {
                            eventName = beforeName,
                            variableInfos = eventArg.getBeforeEventVarInfos()
                        });
                        _eventDefineBeforeTranslations.Add(beforeName, editorName);

                        //发生后事件
                        string afterName = EventHelper.getNameAfter(eventArg);
                        _eventDefineList.Add(new EventDefine()
                        {
                            eventName = afterName,
                            variableInfos = eventArg.getAfterEventVarInfos()
                        });
                        _eventDefineAfterTranslations.Add(afterName, editorName);
                    }
                }
            }
            _eventDefineList.Sort((a, b) => a.eventName.CompareTo(b.eventName));
            _eventNames.Sort((a, b) => a.CompareTo(b));
            //以后可能有自定义的事件类型，不过先不管
        }
        /// <summary>
        /// 获取所有事件定义（事件时/事件后），如果未加载则进行加载
        /// </summary>
        public static EventDefine[] getOrLoadEventDefines()
        {
            if (_eventDefineList != null)
                return _eventDefineList.ToArray();
            loadEventTypes();
            return _eventDefineList.ToArray();
        }
        /// <summary>
        /// 获取所有事件名，如果未加载则进行加载
        /// </summary>
        public static string[] getOrLoadEventNames()
        {
            if (_eventNames != null)
                return _eventNames.ToArray();
            loadEventTypes();
            return _eventNames.ToArray();
        }
        public static bool isValidEventTypeName(string eventTypeName)
        {
            return FindEventTypeInfo(eventTypeName) != null;
        }
        public static EventDefine FindEventTypeInfo(string eventTypeName)
        {
            if (_eventDefineList == null)
                getOrLoadEventDefines();
            return _eventDefineList.Find(e => e.eventName == eventTypeName || (e.obsoleteNames != null && e.obsoleteNames.Contains(eventTypeName)));
        }
        public static string translateEventTypeNameForEditor(LangManager lang, string eventName)
        {
            if (_eventDefineBeforeTranslations.TryGetValue(eventName, out string beforeEditorName))
            {
                var name = lang._(beforeEditorName);
                return lang._(StringTable.EDITOR_EVENT_NAME_BEFORE_TEMPLATE, name);
            }
            else if (_eventDefineAfterTranslations.TryGetValue(eventName, out string afterEditorName))
            {
                var name = lang._(afterEditorName);
                return lang._(StringTable.EDITOR_EVENT_NAME_AFTER_TEMPLATE, name);
            }
            else if (_eventNameTranslations.TryGetValue(eventName, out string eventEditorName))
            {
                return lang._(eventEditorName);
            }
            return eventName;
        }
        #endregion
        static string[] _hiddenEvents =
        {
            "HealEvent",
            "SecretTriggerEvent",
            "MissionCompleteEvent",
            "ComeOnStageEvent",
            "RebornEvent",
            "RevealEvent",
            "RevealDuelEvent",
            "ChooseOneEvent",
            "SearchPileEvent",
            "ActiveEvent",
            "BeChargedEvent",
            "TestEvent",
            "AddModiEvent",
            "RemoveModiEvent",
            "PropChangeEvent",
            "CommandEvent",
            "TestEvent",
            "ElementMixEvent",
            "LastwordEvent",
        };
        static Dictionary<string, string> _eventNameTranslations = new Dictionary<string, string>()
        {
            { "TurnStartEvent", StringTable.EDITOR_EVENT_NAME_TURN_START },
            { "UseEvent", StringTable.EDITOR_EVENT_NAME_USE },
            { "SetArmorEvent", StringTable.EDITOR_EVENT_NAME_ARMOR_CHANGE },
            { "TurnEndEvent", StringTable.EDITOR_EVENT_NAME_TURN_END },
            { "SilenceEvent", StringTable.EDITOR_EVENT_NAME_SILENCE },
            { "AttackEvent", StringTable.EDITOR_EVENT_NAME_ATTACK },
            { "DamageEvent", StringTable.EDITOR_EVENT_NAME_DAMAGE },
            //{ "HealEvent", StringTable.EDITOR_EVENT_NAME_HEAL },
            { "DeathEvent", StringTable.EDITOR_EVENT_NAME_DEATH },
            //{ "SecreTriggerEvent", StringTable.EDITOR_EVENT_NAME_SECRETRIGGER },
            //{ "MissionCompleteEvent", StringTable.EDITOR_EVENT_NAME_MISSIONCOMPLETE },
            //{ "ComeOnStageEvent", StringTable.EDITOR_EVENT_NAME_COMEONSTAGE },
            { "InitEvent", StringTable.EDITOR_EVENT_NAME_INIT },
            { "StartEvent", StringTable.EDITOR_EVENT_NAME_START },
            { "GameEndEvent", StringTable.EDITOR_EVENT_NAME_GAMEEND },
            { "InitReplaceEvent", StringTable.EDITOR_EVENT_NAME_INITREPLACE },
            { "SetMaxGemEvent", StringTable.EDITOR_EVENT_NAME_SETMAXGEM },
            { "BurnEvent", StringTable.EDITOR_EVENT_NAME_BURN },
            { "EnterFieldEvent", StringTable.EDITOR_EVENT_NAME_ENTERFIELD },
            //{ "ChooseOneEvent", StringTable.EDITOR_EVENT_NAME_CHOOSEONE },
            //{ "RebornEvent", StringTable.EDITOR_EVENT_NAME_REBORN },
            //{ "RevealEvent", StringTable.EDITOR_EVENT_NAME_REVEAL },
            { "SetGemEvent", StringTable.EDITOR_EVENT_NAME_SETGEM },
            { "DrawEvent", StringTable.EDITOR_EVENT_NAME_DRAW },
            { "FatigueEvent", StringTable.EDITOR_EVENT_NAME_FATIGUE },
            //{ "SearchiPileEvent", StringTable.EDITOR_EVENT_NAME_SEARCHPILE },
            //{ "ActiveEvent", StringTable.EDITOR_EVENT_NAME_ACTIVE },
            { "DiscardEvent", StringTable.EDITOR_EVENT_NAME_DISCARD },
            { "CreateTokenEvent", StringTable.EDITOR_EVENT_NAME_CREATETOKEN },
            { "SetDefineEvent", StringTable.EDITOR_EVENT_NAME_SETDEFINE },
            //{ "AddModiEvent", StringTable.EDITOR_EVENT_NAME_ADDMODI },
            //{ "RemoveModiEvent", StringTable.EDITOR_EVENT_NAME_REMOVEMODI },
            //{ "PropChangeEvent", StringTable.EDITOR_EVENT_NAME_PROPCHANGE },
            //{ "CommandEvent", StringTable.EDITOR_EVENT_NAME_COMMAND },
            //{ "TestEvent", StringTable.EDITOR_EVENT_NAME_TEST },
            { "DiscoverEvent", StringTable.EDITOR_EVENT_NAME_DISCOVER },
            //{ "SelectEvent", StringTable.EDITOR_EVENT_NAME_SELECT },
            { "EquipItemEvent", StringTable.EDITOR_EVENT_NAME_EQUIP },
            //{ "ElementEvent", StringTable.EDITOR_EVENT_NAME_ELEMENT },
        };
        static List<EventDefine> _eventDefineList = null;
        static List<string> _eventNames = null;
        static Dictionary<string, string> _eventDefineBeforeTranslations = null;
        static Dictionary<string, string> _eventDefineAfterTranslations = null;
    }
    public class EventTypeInfoCollection : List<EventDefine>
    {
        public string[] getEventVarNames(string eventName)
        {
            EventDefine eventTypeInfo = Find(e => e.eventName == eventName);
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
        public int getEventVarNameIndex(string eventName, string eventVarName)
        {
            EventDefine eventTypeInfo = Find(e => e.eventName == eventName);
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
        public EventVariableInfo getEventVarInfo(string eventName, int index)
        {
            EventDefine eventTypeInfo = Find(e => e.eventName == eventName);
            if (eventTypeInfo.variableInfos != null && index < eventTypeInfo.variableInfos.Length)
            {
                return eventTypeInfo.variableInfos[index];
            }
            return null;
        }
        public int getEventTypeIndex(string eventName)
        {
            return FindIndex(e => e.eventName == eventName);
        }
        public string getEventTypeNameByIndex(int index)
        {
            return this[index].eventName;
        }
        public string[] getEventTypeNames()
        {
            string[] eventNames = new string[Count];
            for (int i = 0; i < eventNames.Length; i++)
            {
                eventNames[i] = this[i].eventName;
            }
            return eventNames;
        }
        public string[] getEventTypeNamesForEditor(LangManager lang)
        {
            string[] eventNames = new string[Count];
            for (int i = 0; i < eventNames.Length; i++)
            {
                eventNames[i] = EventTypeInfoLoader.translateEventTypeNameForEditor(lang, this[i].eventName);
            }
            return eventNames;
        }
    }

}
