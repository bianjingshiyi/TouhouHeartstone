using System;
using System.Collections.Generic;
using System.Linq;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    public partial class EffectEditorLibrary
    {
        /// <summary>
        /// 设置/获取事件变量节点的事件参数主键。
        /// </summary>
        public const string EVENT_VARIABLE_NODE_EVENT_PARAM_KEY = "eventArg";
        /// <summary>
        /// 设置/获取事件变量节点的变量名称参数主键。
        /// </summary>
        public const string EVENT_VARIABLE_NODE_EVENT_NAME_PARAM_KEY = "varName";
        /// <summary>
        /// 转化事件类型节点的事件类型参数主键。
        /// </summary>
        public const string EVENT_CAST_EVENT_TYPE_NODE_TYPE_REF_PARAM_KEY = "eventReference";
        /// <summary>
        /// 转化事件类型节点的事件类型参数主键。
        /// </summary>
        public const string EVENT_CAST_EVENT_TYPE_NODE_CAST_EVENT_PARAM_KEY = MethodActionDefine.returnValueName;
        //  事件处理
        [TranslateMsg("编辑器/事件_事件_参数名称")]
        public const string EDITOR_PARAM_NAME_EVENT = "事件";
        [TranslateMsg("编辑器/事件_事件类型_参数名称")]
        public const string EDITOR_PARAM_NAME_EVENT_TYPE = "事件类型";
        [TranslateMsg("编辑器/事件_事件集合_参数名称")]
        public const string EDITOR_PARAM_NAME_EVENTS = "事件集合";
        [TranslateMsg("编辑器/事件_回合数_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT = "回合数";
        [TranslateMsg("编辑器/参数名称")]
        public const string EDITOR_PARAM_NAME_VARIABLE_NAME = "变量名";
        [TranslateMsg("编辑器/参数名称")]
        public const string EDITOR_PARAM_NAME_REPEAT_TIMES = "重复次数";

        [TranslateMsg("编辑器/事件_集合名称")]
        public const string EDITOR_NODE_EVENT = "事件";
        [TranslateMsg("编辑器/事件_当前事件_节点名称")]
        public const string EDITOR_NODE_EVENT_THIS_EVENT = "当前事件";
        [TranslateMsg("编辑器/事件_阻止事件_节点名称")]
        public const string EDITOR_NODE_EVENT_STOP_EVENT = "阻止事件";
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_NAME_CAST_EVENT_TYPE = "转换事件类型";
        [TranslateMsg("编辑器/事件_更改攻击目标_节点名称")]
        public const string EDITOR_NODE_EVENT_CHANGE_ATTACK_TARGET = "更改攻击目标";
        [TranslateMsg("编辑器/事件_更改使用目标_节点名称")]
        public const string EDITOR_NODE_EVENT_CHANGE_USE_TARGET = "更改使用目标";
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_TARGET = "伤害/更改受伤卡牌";
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_SOURCE = "伤害/更改伤害源";
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_VALUE = "伤害/更改伤害值";
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_HEAL_CHANGE_VALUE = "治疗/更改治疗值";
        [TranslateMsg("编辑器/事件_参数名称")]
        public const string EDITOR_NODE_EVENT_TYPE = "事件类型";
        [TranslateMsg("编辑器/事件_前X回合开始_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT_START_BEFORE = "前X回合开始";
        [TranslateMsg("编辑器/事件_前Y回合结束_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT_END_BEFORE = "前Y回合结束";
        [TranslateMsg("编辑器/事件_获取当前回合数_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_TURN_COUNT_OBSOLETE = "获取当前回合数（过时）";
        [TranslateMsg("编辑器/事件_获取当前回合数_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_TURN_COUNT = "获取当前回合数";
        [TranslateMsg("编辑器/事件_事件类型判断_节点名称")]
        public const string EDITOR_NODE_EVENT_CHECK_EVENT_TYPE = "事件类型判断";
        [TranslateMsg("编辑器/事件_获取本局游戏事件_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_EVENTS_THIS_GAME = "获取本局游戏事件";
        [TranslateMsg("编辑器/事件_获取本回合事件_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_EVENTS_THIS_TURN = "获取本回合事件";
        [TranslateMsg("编辑器/事件_获取前X-Y回合内的事件_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_EVENTS_IN_TURNS_BEFORE = "获取前X-Y回合内的事件";
        [TranslateMsg("编辑器/事件_获取卡牌在某事件前的快照_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_CARD_SNAPSHOT_OF_CARD_BEFORE_EVENT = "获取卡牌在某事件前的快照";
        [TranslateMsg("编辑器/事件_获取卡牌在某事件后的快照_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_CARD_SNAPSHOT_OF_CARD_AFTER_EVENT = "获取卡牌在某事件后的快照";

        [TranslateMsg("编辑器/节点名称")]
        public const string EDITOR_NODE_GET_EVENT_REPEAT_TIME = "获取事件重复次数";
        [TranslateMsg("编辑器/节点名称")]
        public const string EDITOR_NODE_SET_EVENT_REPEAT_TIME = "设置事件重复次数";

        #region 事件

        [ObsoletedKeyMethod(BuiltinActionDefineID.ThisEvent, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_THIS_EVENT)]
        [return: ActionNodeParam(EDITOR_NODE_EVENT)]
        public static EventArg thisEvent(EventArg eventArg)
        {
            return eventArg;
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEventVariable, EDITOR_NODE_EVENT, StringTable.EDITOR_NODE_NAME_GET_VARIABLE, "GetVariable")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getVariable(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME)] EventVarName varName)
        {
            if (eventArg == null || varName == null)
                return null;
            return eventArg.getVar(varName);
        }

        [THHActionMethod(BuiltinActionDefineID.CastEventType, EDITOR_NODE_EVENT, EDITOR_NODE_NAME_CAST_EVENT_TYPE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT)]
        public static EventArg castEventType(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(EDITOR_NODE_EVENT_TYPE, isConst: true)] EventReference eventReference)
        {
            if (eventArg == null || eventReference == null)
                return null;
            var define = eventArg.define;
            bool equals = define.cardPoolId == eventReference.cardPoolId && define.eventName == eventReference.eventName;
            return equals ? eventArg : null;
        }

        [EventNode(typeof(SetEventVarEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetEventVariable, EDITOR_NODE_EVENT, StringTable.EDITOR_NODE_NAME_SET_VARIABLE, "SetVariable")]
        public static void setVariable(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME)] EventVarName varName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            if (eventArg == null || varName == null)
                return;
            eventArg.changeEventVar(game, varName, value);
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.StopEvent, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_STOP_EVENT)]
        public static void stopEvent(
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg)
        {
            if (eventArg == null)
                return;
            eventArg.isCanceled = true;
        }

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.ChangeAttackTarget, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_CHANGE_ATTACK_TARGET)]
        public static void changeAttackTarget(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card target)
        {
            if (eventArg.define is AttackEventDefine attackArg)
            {
                eventArg.changeEventVar(game, EVENTVAR.TARGET_CARD, target);
            }
        }

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.ChangeUseTarget, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_CHANGE_USE_TARGET)]
        public static void changeUseTargets(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Card[] target)
        {
            if (eventArg.getVar<Card[]>(EVENTVAR.TARGETS).Length > 0)
                eventArg.changeEventVar(game, EVENTVAR.TARGETS, target);
        }

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DamageEvent_ChangeTarget, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_DAMAGE_CHANGE_TARGET)]
        public static void damageEvent_changeTarget(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card target)
        {
            if (eventArg.define is DamageEventDefine)
            {
                eventArg.changeEventVar(game, EVENTVAR.CARD, target);
            }
        }

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DamageEvent_ChangeSource, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_DAMAGE_CHANGE_SOURCE)]
        public static void damageEvent_changeSource(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card source)
        {
            if (eventArg.define is DamageEventDefine)
            {
                eventArg.changeEventVar(game, DamageEventDefine.VAR_DAMAGE_SOURCE, source);
            }
        }

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DamageEvent_ChangeValue, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_DAMAGE_CHANGE_VALUE)]
        public static void damageEvent_changeValue(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (eventArg.define is DamageEventDefine)
            {
                eventArg.changeEventVar(game, DamageEventDefine.VAR_DAMAGE_VALUE, value);
            }
        }

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.HealEvent_ChangeValue, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_HEAL_CHANGE_VALUE)]
        public static void healEvent_changeValue(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (eventArg.define is HealEventDefine)
            {
                eventArg.changeEventVar(game, EVENTVAR.VALUE, value);
            }
        }

        [THHActionMethod(BuiltinActionDefineID.CheckEventType, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_CHECK_EVENT_TYPE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool checkEventType(
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg,
            [ActionNodeParam(EDITOR_NODE_EVENT_TYPE, isConst: true)] EventReference argRef)
        {
            if (eventArg == null || argRef == null)
                return false;
            var eventDefine = eventArg.define;
            var eventReference = argRef;
            if (eventDefine.cardPoolId == eventReference.cardPoolId && eventDefine.eventName == eventReference.eventName)
                return true;
            return false;
        }
        [THHActionMethod(BuiltinActionDefineID.GetEventsThisGame, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_GET_EVENTS_THIS_GAME)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENTS)]
        public static EventArg[] getEventsThisGame(CardEngine game)
        {
            return game.triggers.getRecordedEvents().OfType<EventArg>().ToArray();
        }

        [THHActionMethod(BuiltinActionDefineID.GetEventsThisTurn, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_GET_EVENTS_THIS_TURN)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENTS)]
        public static EventArg[] getEventsThisTurn(CardEngine game)
        {
            return getEventsInTurnsBefore(game, 0, 0);
        }


        [THHActionMethod(BuiltinActionDefineID.GetEventsInTurnsBefore, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_GET_EVENTS_IN_TURNS_BEFORE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENTS)]
        public static EventArg[] getEventsInTurnsBefore(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT_START_BEFORE)] int farther,
            [ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT_END_BEFORE)] int nearer)
        {
            var events = game.triggers.getRecordedEvents();
            var results = new List<EventArg>();
            int turnBeforeCount = 0;
            for (int i = events.Length - 1; i >= 0; i--)
            {
                var arg = events[i];
                if (arg is not EventArg eventArg)
                    continue;
                if (turnBeforeCount >= nearer)
                    results.Insert(0, eventArg);
                if (turnBeforeCount > farther)
                    break;
                if (eventArg.define is TurnStartEventDefine)
                {
                    turnBeforeCount++;
                }
            }
            return results.ToArray();
        }

        [THHActionMethod(BuiltinActionDefineID.GetCardSnapshotBeforeEvent, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_GET_CARD_SNAPSHOT_OF_CARD_BEFORE_EVENT)]
        [return: ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)]
        public static CardSnapshot getSnapshotOfCardBeforeEvent(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg)
        {
            if (card == null || eventArg == null)
                return null;
            return game.getCardSnapshotBeforeEvent(card, eventArg);
        }

        [THHActionMethod(BuiltinActionDefineID.GetCardSnapshotAfterEvent, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_GET_CARD_SNAPSHOT_OF_CARD_AFTER_EVENT)]
        [return: ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)]
        public static CardSnapshot getSnapshotOfCardAfterEvent(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg)
        {
            if (card == null || eventArg == null)
                return null;
            return game.getCardSnapshotAfterEvent(card, eventArg);
        }

        [THHActionMethod(BuiltinActionDefineID.GetEventRepeatTimes, EDITOR_NODE_EVENT, EDITOR_NODE_GET_EVENT_REPEAT_TIME)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_REPEAT_TIMES)]
        public static int setEventRepeatTime(
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg)
        {
            if (eventArg == null)
                return 0;
            return eventArg.repeatTime;
        }
        [THHActionMethod(BuiltinActionDefineID.SetEventRepeatTimes, EDITOR_NODE_EVENT, EDITOR_NODE_SET_EVENT_REPEAT_TIME)]
        public static void setEventRepeatTime(
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg,
            [ActionNodeParam(EDITOR_PARAM_NAME_REPEAT_TIMES)] int times)
        {
            if (eventArg == null)
                return;
            eventArg.repeatTime = times;
        }
        #endregion

        #region 回合数

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetTurnCount_Obsolete, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_GET_TURN_COUNT_OBSOLETE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT)]
        public static int getTurnCountObsolete(CardEngine game)
        {
            return game.getTurnCount() - 1;
        }
        [THHActionMethod(BuiltinActionDefineID.GetTurnCount, EDITOR_NODE_EVENT, EDITOR_NODE_EVENT_GET_TURN_COUNT)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT)]
        public static int getTurnCount(CardEngine game)
        {
            return game.getTurnCount();
        }

        #endregion
    }
    public class EventRecordNodeConversion
    {
        public int source;
        public int target;
        public string sourceInput;
        public string targetInput;
        public string sourceOutput;
        public string targetOutput;
    }
}
