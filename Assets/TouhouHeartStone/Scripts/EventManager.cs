using System;
using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
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
        public string translateBuiltinEventNameForEditor(string eventName)
        {
            if (_eventNameTranslations.TryGetValue(eventName, out string editorName))
            {
                return game.lang._(editorName);
            }
            return game.lang._(eventName);
        }
        public string translateEventDefineNameForEditor(EventDefine define)
        {
            if (define == null)
                return game.lang._(EDITOR_NODE_NAME_UNKNOWN_EVENT);
            if (define.cardPoolId == 0)
                return translateBuiltinEventNameForEditor(define.eventName);
            if (define is GeneratedEventDefine gen)
            {
                return game.card.getLocalActionDefine(gen.actionReference).editorName;
            }
            return game.lang._(define.eventName);
        }
        public string translateEventReferenceNameForEditor(EventReference eventRef)
        {
            if (eventRef == null)
                return game.lang._(EDITOR_NODE_NAME_UNKNOWN_EVENT);
            var eventDefine = game.card.getLocalEventDefine(eventRef);
            return translateEventDefineNameForEditor(eventDefine);
        }
        public string translateEventTriggerTimeNameForEditor(EventTriggerTime time)
        {
            var lang = game.lang;
            var name = translateEventReferenceNameForEditor(time.defineRef);
            if (time.type == EventTriggerTimeType.Before)
            {
                return lang._(StringTable.EDITOR_EVENT_NAME_BEFORE_TEMPLATE, name);
            }
            else if (time.type == EventTriggerTimeType.After)
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
        [TranslateMsg("未知事件")]
        public const string EDITOR_NODE_NAME_UNKNOWN_EVENT = "未知事件";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_ADD_BUFF = "添加增益";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_REMOVE_BUFF = "移除增益";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_SHACKLE = "禁锢";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MOVE_CARD = "卡牌移动";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_CARD_PROP_CHANGE = "卡牌属性变动";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_CHANGE_CARD_VISIBILITY = "卡牌可见性变动";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_CHANGE_CARD_PROP_VISIBILITY = "卡牌属性可见性变动";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_CHANGE_BUFF_PROP_VISIBILITY = "增益属性可见性变动";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_REVEAL_CARD_DEFINE = "展示卡牌定义";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_SET_CARD_PROP = "修改卡牌属性";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_BE_CHARGED = "蓄力";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_LASTWORD = "遗言";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_SET_EVENT_VAR = "设置事件变量";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_AUTOCAST = "自动施放";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_ADD_BUFF = "群体添加增益";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_DAMAGE = "群体伤害";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_DISCARD = "群体弃牌";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_ENTER_FIELD = "群体入场";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_HEAL = "群体治疗";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_MOVE_CARD = "群体移动卡牌";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_MOVE_TO_HEAD = "群体移动至延迟区";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_RETURN_TO_HAND = "群体移回手牌";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_SEAL = "群体封印";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_SET_CARD_DEFINE = "群体变形";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_SHUFFLE_TO_DECK = "群体洗入牌库";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_MASS_SPLIT_DAMAGE = "分配伤害";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_SELECT_CARD = "选择卡牌";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_SELECT_CARD_DEFINE = "选择卡牌定义";
        [TranslateMsg("事件名称")]
        public const string EDITOR_EVENT_NAME_LIFESTEAL = "吸血";
        static string[] _hiddenEvents =
        {
            "SecretTriggerEvent",
            "MissionCompleteEvent",
            "RebornEvent",
            "RevealEvent",
            "SelectCardEvent",
            "SelectCardDefineEvent",
            "ActiveEvent",
            "BeChargedEvent",
            "TestEvent",
            "CardPropChangeEvent",
            "SetCardPropEvent",
            "BuffPropertyChangeEvent",
            "EffectTriggerEvent",
            "ElementMixEvent",
            "LastwordEvent",
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
        };
        static Dictionary<string, string> _eventNameTranslations = new Dictionary<string, string>()
        {
            { "AddBuffEvent", EDITOR_EVENT_NAME_ADD_BUFF },
            { "RemoveBuffEvent", EDITOR_EVENT_NAME_REMOVE_BUFF },
            { "MoveCardEvent", EDITOR_EVENT_NAME_MOVE_CARD },
            { "CardPropChangeEvent", EDITOR_EVENT_NAME_CARD_PROP_CHANGE },
            { "SetCardPropEvent", EDITOR_EVENT_NAME_SET_CARD_PROP },
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
            { "SelectCardDefineEvent", EDITOR_EVENT_NAME_SELECT_CARD_DEFINE },
            { "RebornEvent", StringTable.EDITOR_EVENT_NAME_REBORN },
            { "RevealEvent", StringTable.EDITOR_EVENT_NAME_REVEAL },
            { "RevealDuelEvent", StringTable.EDITOR_EVENT_NAME_REVEALDUEL },
            { "SetGemEvent", StringTable.EDITOR_EVENT_NAME_SETGEM },
            { "DrawEvent", StringTable.EDITOR_EVENT_NAME_DRAW },
            { "FatigueEvent", StringTable.EDITOR_EVENT_NAME_FATIGUE },
            { "SelectCardEvent", EDITOR_EVENT_NAME_SELECT_CARD },
            { "ActiveEvent", StringTable.EDITOR_EVENT_NAME_ACTIVE },
            { "DiscardEvent", StringTable.EDITOR_EVENT_NAME_DISCARD },
            { "CreateTokenEvent", StringTable.EDITOR_EVENT_NAME_CREATETOKEN },
            { "SetCardDefineEvent", StringTable.EDITOR_EVENT_NAME_SETDEFINE },
            { "ShackleEvent", EDITOR_EVENT_NAME_SHACKLE },
            { "AddModiEvent", StringTable.EDITOR_EVENT_NAME_ADDMODI },
            { "RemoveModiEvent", StringTable.EDITOR_EVENT_NAME_REMOVEMODI },
            { "PropChangeEvent", StringTable.EDITOR_EVENT_NAME_PROPCHANGE },
            { "CommandEvent", StringTable.EDITOR_EVENT_NAME_COMMAND },
            { "TestEvent", StringTable.EDITOR_EVENT_NAME_TEST },
            { "DiscoverEvent", StringTable.EDITOR_EVENT_NAME_DISCOVER },
            { "SelectEvent", StringTable.EDITOR_EVENT_NAME_SELECT },
            { "EquipItemEvent", StringTable.EDITOR_EVENT_NAME_EQUIP },
            { "BeChargedEvent", EDITOR_EVENT_NAME_BE_CHARGED },
            { "ElementMixEvent", StringTable.EDITOR_EVENT_NAME_ELEMENT },
            { "LastwordEvent", EDITOR_EVENT_NAME_LASTWORD },
            { "SetEventVarEvent", EDITOR_EVENT_NAME_SET_EVENT_VAR },
            { "AutocastEvent", EDITOR_EVENT_NAME_AUTOCAST },
            { "MassAddBuffEvent", EDITOR_EVENT_NAME_MASS_ADD_BUFF },
            { "MassDamageEvent", EDITOR_EVENT_NAME_MASS_DAMAGE },
            { "MassDiscardEvent", EDITOR_EVENT_NAME_MASS_DISCARD },
            { "MassEnterFieldEvent", EDITOR_EVENT_NAME_MASS_ENTER_FIELD },
            { "MassHealEvent", EDITOR_EVENT_NAME_MASS_HEAL },
            { "MassMoveCardEvent", EDITOR_EVENT_NAME_MASS_MOVE_CARD },
            { "MassMoveToHeadEvent", EDITOR_EVENT_NAME_MASS_MOVE_TO_HEAD },
            { "MassReturnHandEvent", EDITOR_EVENT_NAME_MASS_RETURN_TO_HAND },
            { "MassSealEvent", EDITOR_EVENT_NAME_MASS_SEAL },
            { "MassSetCardDefineEvent", EDITOR_EVENT_NAME_MASS_SET_CARD_DEFINE },
            { "MassShuffleToDeckEvent", EDITOR_EVENT_NAME_MASS_SHUFFLE_TO_DECK },
            { "SplitDamageEvent", EDITOR_EVENT_NAME_MASS_SPLIT_DAMAGE },
            { "ChangeCardVisibilityEvent", EDITOR_EVENT_NAME_CHANGE_CARD_VISIBILITY },
            { "ChangeCardPropVisibilityEvent", EDITOR_EVENT_NAME_CHANGE_CARD_PROP_VISIBILITY },
            { "ChangeBuffPropVisibilityEvent", EDITOR_EVENT_NAME_CHANGE_BUFF_PROP_VISIBILITY },
            { "RevealCardDefineEvent", EDITOR_EVENT_NAME_REVEAL_CARD_DEFINE },
            { "LifestealEvent", EDITOR_EVENT_NAME_LIFESTEAL }
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
