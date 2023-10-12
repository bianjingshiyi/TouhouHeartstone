using System.Collections.Generic;
using System.Linq;
using System;
using BJSYGameCore;
using MukioI18n;
using TouhouCardEngine;
using UnityEngine;
using TouhouHeartstone;
using System.Threading.Tasks;

namespace Game
{
    public class EventManager : Manager
    {
        #region 公有方法
        /// <summary>
        /// 获取所有事件定义（事件时/事件后），如果未加载则进行加载
        /// </summary>
        public EventDefine[] getOrLoadBuiltinEventDefines()
        {
            if (_builtinEventDefineList != null)
                return _builtinEventDefineList.ToArray();
            loadEventTypes();
            return _builtinEventDefineList.ToArray();
        }
        public string translateEventDefineNameForEditor(EventDefine define)
        {
            var lang = game.lang;
            var name = lang._(define.editorName);
            if (define.defineType == EventDefineType.Before)
            {
                return lang._(StringTable.EDITOR_EVENT_NAME_BEFORE_TEMPLATE, name);
            }
            else if (define.defineType == EventDefineType.After)
            {
                return lang._(StringTable.EDITOR_EVENT_NAME_AFTER_TEMPLATE, name);
            }
            return name;
        }
        public EventDefine[] generateEventDefines(IEnumerable<GeneratedActionDefine> actionDefines)
        {
            var eventActionDefines = actionDefines.Where(a => a.type == NodeDefineType.Event);

            List<EventDefine> results = new List<EventDefine>();
            foreach (var actionDefine in eventActionDefines)
            {
                results.AddRange(generateEventDefinesForActionDefine(actionDefine));
            }
            return results.ToArray();
        }
        public static EventDefine[] generateEventDefinesForActionDefine(GeneratedActionDefine actionDefine)
        {
            //发生时事件
            string beforeName = actionDefine.getBeforeEventName();
            var beforeDefine = new EventDefine()
            {
                cardPoolId = actionDefine.cardPoolId,
                eventName = beforeName,
                editorName = actionDefine.editorName,
                defineType = EventDefineType.Before,
                variableInfos = actionDefine.getBeforeEventArgVarInfos()
            };

            //发生后事件
            string afterName = actionDefine.getAfterEventName();
            var afterDefine = new EventDefine()
            {
                cardPoolId = actionDefine.cardPoolId,
                eventName = afterName,
                editorName = actionDefine.editorName,
                defineType = EventDefineType.After,
                variableInfos = actionDefine.getAfterEventArgVarInfos()
            };
            return new EventDefine[] { beforeDefine, afterDefine };
        }
        #endregion
        #region 私有方法
        protected override void onAwake()
        {
            base.onAwake();
            loadEventTypes();
        }
        private void loadEventTypes()
        {
            _builtinEventDefineList = new List<EventDefine>();
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
                        _builtinEventDefineList.Add(new EventDefine()
                        {
                            cardPoolId = 0,
                            eventName = beforeName,
                            editorName = editorName,
                            defineType = EventDefineType.Before,
                            variableInfos = eventArg.getBeforeEventVarInfos()
                        });

                        //发生后事件
                        string afterName = EventHelper.getNameAfter(eventArg);
                        _builtinEventDefineList.Add(new EventDefine()
                        {
                            cardPoolId = 0,
                            eventName = afterName,
                            editorName = editorName,
                            defineType = EventDefineType.After,
                            variableInfos = eventArg.getAfterEventVarInfos()
                        });
                    }
                }
            }
            _builtinEventDefineList.Sort((a, b) => a.eventName.CompareTo(b.eventName));
            _eventNames.Sort((a, b) => a.CompareTo(b));
            //以后可能有自定义的事件类型，不过先不管
        }
        #endregion
        public MainManager game => _game;
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_ADD_BUFF = "添加增益";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_REMOVE_BUFF = "移除增益";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_SHACKLE = "禁锢";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MOVE_CARD = "卡牌移动";
        static string[] _hiddenEvents =
        {
            "AddBuffEvent",
            "RemoveBuffEvent",
            "SecretTriggerEvent",
            "MissionCompleteEvent",
            "ComeOnStageEvent",
            "RebornEvent",
            "RevealEvent",
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
            "UpdateDeathEvent",
            "GeneratedEvent",
        };
        static Dictionary<string, string> _eventNameTranslations = new Dictionary<string, string>()
        {
            { "AddBuffEvent", EDITOR_EVENT_NAME_ADD_BUFF },
            { "RemoveBuffEvent", EDITOR_EVENT_NAME_REMOVE_BUFF },
            { "MoveCardEvent", EDITOR_EVENT_NAME_MOVE_CARD },
            { "TurnStartEvent", StringTable.EDITOR_EVENT_NAME_TURN_START },
            { "UseEvent", StringTable.EDITOR_EVENT_NAME_USE },
            { "SetArmorEvent", StringTable.EDITOR_EVENT_NAME_ARMOR_CHANGE },
            { "TurnEndEvent", StringTable.EDITOR_EVENT_NAME_TURN_END },
            { "SilenceEvent", StringTable.EDITOR_EVENT_NAME_SILENCE },
            { "AttackEvent", StringTable.EDITOR_EVENT_NAME_ATTACK },
            { "DamageEvent", StringTable.EDITOR_EVENT_NAME_DAMAGE },
            { "HealEvent", StringTable.EDITOR_EVENT_NAME_HEAL },
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
            { "RevealDuelEvent", StringTable.EDITOR_EVENT_NAME_REVEALDUEL },
            { "SetGemEvent", StringTable.EDITOR_EVENT_NAME_SETGEM },
            { "DrawEvent", StringTable.EDITOR_EVENT_NAME_DRAW },
            { "FatigueEvent", StringTable.EDITOR_EVENT_NAME_FATIGUE },
            //{ "SearchiPileEvent", StringTable.EDITOR_EVENT_NAME_SEARCHPILE },
            //{ "ActiveEvent", StringTable.EDITOR_EVENT_NAME_ACTIVE },
            { "DiscardEvent", StringTable.EDITOR_EVENT_NAME_DISCARD },
            { "CreateTokenEvent", StringTable.EDITOR_EVENT_NAME_CREATETOKEN },
            { "SetDefineEvent", StringTable.EDITOR_EVENT_NAME_SETDEFINE },
            { "ShackleEvent", EDITOR_EVENT_NAME_SHACKLE },
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
        List<EventDefine> _builtinEventDefineList = null;
        List<string> _eventNames = null;
        [SerializeField]
        MainManager _game;
    }
}
