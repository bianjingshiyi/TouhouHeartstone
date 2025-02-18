using System;
using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UnityEngine;

namespace Game
{
    public partial class EventManager : Manager
    {
        #region 公有方法
        /// <summary>
        /// 获取所有事件定义，如果未加载则进行加载
        /// </summary>
        public EventDefine[] getOrLoadBuiltinEventDefines(bool includeHidden = true)
        {
            if (_builtinEventDefineList == null)
                loadBuiltinEvents();
            IEnumerable<EventDefine> list = _builtinEventDefineList;
            if (!includeHidden)
                list = list.Where(e => !isEventHidden(e));
            return list.ToArray();
        }
        public string getAnyEventDefineNameForEditor()
        {
            return game.lang._p(StringTable.CONTEXT_EVENT_NAME, StringTable.EDITOR_EVENT_NAME_ANY);
        }
        public string translateBuiltinEventNameForEditor(string eventName)
        {
            if (_eventNameTranslations.TryGetValue(eventName, out string editorName))
            {
                return game.lang._p(StringTable.CONTEXT_EVENT_NAME, editorName);
            }
            return game.lang._p(StringTable.CONTEXT_EVENT_NAME, eventName);
        }
        public string translateEventDefineNameForEditor(EventDefine define)
        {
            if (define == null)
                return game.lang._p(StringTable.CONTEXT_EVENT_NAME, StringTable.EDITOR_EVENT_NAME_UNKNOWN);
            if (define.cardPoolId == 0)
                return translateBuiltinEventNameForEditor(define.eventName);
            if (define is GeneratedEventDefine gen)
            {
                return game.card.getLocalActionDefine(gen.actionReference).editorName;
            }
            return game.lang._p(StringTable.CONTEXT_EVENT_NAME, define.eventName);
        }
        public string translateEventReferenceNameForEditor(EventReference eventRef)
        {
            if (eventRef == null)
                return game.lang._p(StringTable.CONTEXT_EVENT_NAME, StringTable.EDITOR_EVENT_NAME_UNKNOWN);
            var eventDefine = game.card.getLocalEventDefine(eventRef);
            return translateEventDefineNameForEditor(eventDefine);
        }
        public string translateEventTriggerTimeNameForEditor(EventTriggerTime time)
        {
            var lang = game.lang;
            var name = translateEventReferenceNameForEditor(time.defineRef);
            if (time.type == EventTriggerTimeType.Before)
            {
                return lang._p(StringTable.CONTEXT_EVENT_NAME, StringTable.EDITOR_EVENT_NAME_BEFORE_TEMPLATE, name);
            }
            else if (time.type == EventTriggerTimeType.After)
            {
                return lang._p(StringTable.CONTEXT_EVENT_NAME, StringTable.EDITOR_EVENT_NAME_AFTER_TEMPLATE, name);
            }
            return name;
        }
        public EventDefine[] generateEventDefines(IEnumerable<GeneratedActionDefine> actionDefines)
        {
            var eventActionDefines = actionDefines.Where(a => a.type == NodeDefineType.Event);

            List<EventDefine> results = new List<EventDefine>();
            foreach (var actionDefine in eventActionDefines)
            {
                results.Add(generateEventDefineForActionDefine(actionDefine));
            }
            return results.ToArray();
        }
        public EventDefine generateEventDefineForActionDefine(GeneratedActionDefine actionDefine)
        {
            string eventName = actionDefine.getEventName();
            var actionRef = actionDefine.getReference();
            List<EventReference> childrenList = new List<EventReference>();
            generateActionChildrenEventsNonAlloc(actionDefine, childrenList);
            var define = new GeneratedEventDefine(actionRef)
            {
                cardPoolId = actionDefine.cardPoolId,
                eventName = eventName,
                beforeVariableInfos = actionDefine.getBeforeEventArgVarInfos(),
                afterVariableInfos = actionDefine.getAfterEventArgVarInfos(),
                childrenEvents = childrenList.ToArray(),
            };
            return define;
        }
        public bool isEventHidden(EventDefine define)
        {
            return _hiddenEvents.Contains(define.eventName);
        }
        public EventReference[] getEventReferenceChildren(EventReference eventRef, IEnumerable<ICardPoolReference> poolSet)
        {
            var list = new List<EventReference>()
            {
                eventRef
            };
            getEventReferenceChildrenNonAlloc(eventRef, poolSet, list);
            return list.ToArray();
        }
        public void getEventReferenceChildrenNonAlloc(EventReference eventRef, IEnumerable<ICardPoolReference> poolSet, List<EventReference> list)
        {
            var poolRef = game.card.getCardPoolReferenceInEnv(eventRef.cardPoolId, poolSet);
            var eventDefine = game.card.getEventDefine(poolRef, eventRef.eventName);
            if (eventDefine?.childrenEvents == null)
                return;
            foreach (var childRef in eventDefine.childrenEvents)
            {
                if (list.Contains(childRef))
                    continue;
                list.Add(childRef);
                getEventReferenceChildrenNonAlloc(childRef, poolSet, list);
            }
        }
        #endregion
        #region 私有方法
        protected override void onAwake()
        {
            base.onAwake();
            loadBuiltinEvents();
        }
        private void loadBuiltinEvents()
        {
            _builtinEventDefineList = new List<EventDefine>();
            //通过反射加载硬编码的事件类型
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    // 是EventDefine并且不是抽象类
                    if (type.IsSubclassOf(typeof(EventDefine)) && !type.IsAbstract)
                    {
                        EventDefine eventDefine;
                        try
                        {
                            eventDefine = Activator.CreateInstance(type) as EventDefine;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"无法加载事件类型{type.FullName}：{e}");
                            continue;
                        }
                        var eventName = EventHelper.getName(eventDefine);

                        // 获取子事件。
                        EventReference[] childrenRefs;
                        if (type.getAttribute<EventChildrenAttribute>() is EventChildrenAttribute eventChildren)
                        {
                            childrenRefs = eventChildren.childrenTypes.Select(t => new EventReference(0, EventHelper.getName(t))).ToArray();
                        }
                        else
                        {
                            childrenRefs = Array.Empty<EventReference>();
                        }
                        eventDefine.eventName = eventName;
                        eventDefine.childrenEvents = childrenRefs;

                        _builtinEventDefineList.Add(eventDefine);
                    }
                }
            }
            _builtinEventDefineList.Sort((a, b) => a.eventName.CompareTo(b.eventName));
            //以后可能有自定义的事件类型，不过先不管
        }
        #endregion
        public MainManager game => _game;
        static string[] _hiddenEvents =
        {
            "SecretTriggerEvent",
            "MissionCompleteEvent",
            "RebornEvent",
            "RevealEvent",
            "SelectCardEvent",
            "SelectCardDefineEvent",
            "BeChargedEvent",
            "TestEvent",
            "SetCardPropEvent",
            //"BuffPropertyChangeEvent",
            "ElementMixEvent",
            "UpdateDeathEvent",
            "GeneratedEvent",
            "SetEventVarEvent",
            "AutocastEvent",
            "MassAddBuffEvent",
            "MassDamageEvent",
            "MassDiscardEvent",
            "MassEnterFieldEvent",
            "MassHealEvent",
            "MassMoveCardEvent",
            "MassMoveToHeadEvent",
            "MassReturnHandEvent",
            "MassSealEvent",
            "MassSetCardDefineEvent",
            "MassShuffleToDeckEvent",
            "SplitDamageEvent",
            "ChangeCardVisibilityEvent",
            "ChangeCardPropVisibilityEvent",
            "ChangeBuffPropVisibilityEvent",
            "RevealCardDefineEvent",
            "EffectActivationEvent",
        };
        static Dictionary<string, string> _eventNameTranslations = new Dictionary<string, string>()
        {
            { "AddBuffEvent", StringTable.EDITOR_EVENT_NAME_ADD_BUFF },
            { "RemoveBuffEvent", StringTable.EDITOR_EVENT_NAME_REMOVE_BUFF },
            { "MoveCardEvent", StringTable.EDITOR_EVENT_NAME_MOVE_CARD },
            { "CardPropChangeEvent", StringTable.EDITOR_EVENT_NAME_CARD_PROP_CHANGE },
            { "BuffPropertyChangeEvent", StringTable.EDITOR_EVENT_NAME_BUFF_PROP_CHANGE },
            { "SetCardPropEvent", StringTable.EDITOR_EVENT_NAME_SET_CARD_PROP },
            { "TurnStartEvent", StringTable.EDITOR_EVENT_NAME_TURN_START },
            { "UseEvent", StringTable.EDITOR_EVENT_NAME_USE },
            { "SetArmorEvent", StringTable.EDITOR_EVENT_NAME_ARMOR_CHANGE },
            { "TurnEndEvent", StringTable.EDITOR_EVENT_NAME_TURN_END },
            { "SilenceEvent", StringTable.EDITOR_EVENT_NAME_SILENCE },
            { "AttackEvent", StringTable.EDITOR_EVENT_NAME_ATTACK },
            { "DamageEvent", StringTable.EDITOR_EVENT_NAME_DAMAGE },
            { "HealEvent", StringTable.EDITOR_EVENT_NAME_HEAL },
            { "DeathEvent", StringTable.EDITOR_EVENT_NAME_DEATH },
            { "SecretTriggerEvent", StringTable.EDITOR_EVENT_NAME_SECRETTRIGGER },
            { "MissionCompleteEvent", StringTable.EDITOR_EVENT_NAME_MISSIONCOMPLETE },
            { "ComeOnStageEvent", StringTable.EDITOR_EVENT_NAME_COMEONSTAGE },
            { "InitEvent", StringTable.EDITOR_EVENT_NAME_INIT },
            { "StartEvent", StringTable.EDITOR_EVENT_NAME_START },
            { "GameEndEvent", StringTable.EDITOR_EVENT_NAME_GAMEEND },
            { "InitReplaceEvent", StringTable.EDITOR_EVENT_NAME_INITREPLACE },
            { "SetMaxGemEvent", StringTable.EDITOR_EVENT_NAME_SETMAXGEM },
            { "BurnEvent", StringTable.EDITOR_EVENT_NAME_BURN },
            { "EnterFieldEvent", StringTable.EDITOR_EVENT_NAME_ENTERFIELD },
            { "SelectCardDefineEvent", StringTable.EDITOR_EVENT_NAME_SELECT_CARD_DEFINE },
            { "RebornEvent", StringTable.EDITOR_EVENT_NAME_REBORN },
            { "RevealEvent", StringTable.EDITOR_EVENT_NAME_REVEAL },
            { "RevealDuelEvent", StringTable.EDITOR_EVENT_NAME_REVEALDUEL },
            { "SetGemEvent", StringTable.EDITOR_EVENT_NAME_SETGEM },
            { "DrawEvent", StringTable.EDITOR_EVENT_NAME_DRAW },
            { "FatigueEvent", StringTable.EDITOR_EVENT_NAME_FATIGUE },
            { "SelectCardEvent", StringTable.EDITOR_EVENT_NAME_SELECT_CARD },
            { "DiscardEvent", StringTable.EDITOR_EVENT_NAME_DISCARD },
            { "SetCardDefineEvent", StringTable.EDITOR_EVENT_NAME_SETDEFINE },
            { "ShackleEvent", StringTable.EDITOR_EVENT_NAME_SHACKLE },
            { "EquipItemEvent", StringTable.EDITOR_EVENT_NAME_EQUIP },
            { "BeChargedEvent", StringTable.EDITOR_EVENT_NAME_BE_CHARGED },
            { "ElementMixEvent", StringTable.EDITOR_EVENT_NAME_ELEMENT },
            { "SetEventVarEvent", StringTable.EDITOR_EVENT_NAME_SET_EVENT_VAR },
            { "AutocastEvent", StringTable.EDITOR_EVENT_NAME_AUTOCAST },
            { "MassAddBuffEvent", StringTable.EDITOR_EVENT_NAME_MASS_ADD_BUFF },
            { "MassDamageEvent", StringTable.EDITOR_EVENT_NAME_MASS_DAMAGE },
            { "MassDiscardEvent", StringTable.EDITOR_EVENT_NAME_MASS_DISCARD },
            { "MassEnterFieldEvent", StringTable.EDITOR_EVENT_NAME_MASS_ENTER_FIELD },
            { "MassHealEvent", StringTable.EDITOR_EVENT_NAME_MASS_HEAL },
            { "MassMoveCardEvent", StringTable.EDITOR_EVENT_NAME_MASS_MOVE_CARD },
            { "MassMoveToHeadEvent", StringTable.EDITOR_EVENT_NAME_MASS_MOVE_TO_HEAD },
            { "MassReturnHandEvent", StringTable.EDITOR_EVENT_NAME_MASS_RETURN_TO_HAND },
            { "MassSealEvent", StringTable.EDITOR_EVENT_NAME_MASS_SEAL },
            { "MassSetCardDefineEvent", StringTable.EDITOR_EVENT_NAME_MASS_SET_CARD_DEFINE },
            { "MassShuffleToDeckEvent", StringTable.EDITOR_EVENT_NAME_MASS_SHUFFLE_TO_DECK },
            { "SplitDamageEvent", StringTable.EDITOR_EVENT_NAME_MASS_SPLIT_DAMAGE },
            { "ChangeCardVisibilityEvent", StringTable.EDITOR_EVENT_NAME_CHANGE_CARD_VISIBILITY },
            { "ChangeCardPropVisibilityEvent", StringTable.EDITOR_EVENT_NAME_CHANGE_CARD_PROP_VISIBILITY },
            { "ChangeBuffPropVisibilityEvent", StringTable.EDITOR_EVENT_NAME_CHANGE_BUFF_PROP_VISIBILITY },
            { "RevealCardDefineEvent", StringTable.EDITOR_EVENT_NAME_REVEAL_CARD_DEFINE },
            { "LifestealEvent", StringTable.EDITOR_EVENT_NAME_LIFESTEAL },
            { "EffectTriggerEvent", StringTable.EDITOR_EVENT_NAME_EFFECT_TRIGGER }
        };
        List<EventDefine> _builtinEventDefineList = null;
        [SerializeField]
        MainManager _game;
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
