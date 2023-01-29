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
        //  事件处理
        [TranslateMsg("编辑器/事件记录_参数名称")]
        public const string EDITOR_TYPE_NAME_EVENT_RECORD = "事件记录";
        [TranslateMsg("编辑器/事件记录_参数名称")]
        public const string EDITOR_TYPE_NAME_EVENT_TYPE = "事件类型";
        [TranslateMsg("编辑器/事件记录_X回合开始_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT_START = "X回合开始";
        [TranslateMsg("编辑器/事件记录_Y回合结束_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT_END = "Y回合结束";
        [TranslateMsg("编辑器/事件记录_前X回合开始_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT_START_BEFORE = "前X回合开始";
        [TranslateMsg("编辑器/事件记录_前Y回合结束_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT_END_BEFORE = "前Y回合结束";

        #region 事件记录

        [TranslateMsg("编辑器/事件记录_获取当前回合数_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_TURN_COUNT = "获取当前回合数";
        [ActionNodeMethod(EDITOR_NODE_EVENT_GET_TURN_COUNT, EDITOR_TYPE_NAME_EVENT_RECORD)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT)]
        public static int getTurnCount(CardEngine game)
        {
            return game.getTurnCount();
        }

        [TranslateMsg("编辑器/事件记录_获取变量_节点名称")]
        public const string EDITOR_NODE_EVENT_RECORD_GET_VARIABLE = "获取事件记录变量";
        [ActionNodeMethod(EDITOR_NODE_EVENT_RECORD_GET_VARIABLE, EDITOR_TYPE_NAME_EVENT_RECORD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getRecordVariable(
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)] EventRecord record,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, true)] string varName)
        {
            return record.getVar(varName);
        }
        [TranslateMsg("编辑器/事件记录_事件记录类型判断_节点名称")]
        public const string EDITOR_NODE_EVENT_CHECK_EVENT_RECORD_TYPE = "事件记录类型判断";
        [ActionNodeMethod(EDITOR_NODE_EVENT_CHECK_EVENT_RECORD_TYPE, EDITOR_TYPE_NAME_EVENT_RECORD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool checkEventRecordType(CardEngine game,
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)] EventRecord evt,
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_TYPE, isConst: true)] EventArgReference argRef)
        {
            if (argRef == null)
                return false;
            if (game.triggers.getName(evt.eventArg) == argRef.eventName)
                return true;
            return false;
        }
        [TranslateMsg("编辑器/事件记录_获取本局游戏事件记录_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_GAME = "获取本局游戏事件记录";
        [ActionNodeMethod(EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_GAME, EDITOR_TYPE_NAME_EVENT_RECORD)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT_RECORDS)]
        public static EventRecord[] getEventRecordsThisGame(CardEngine game)
        {
            return game.triggers.getEventRecords().ToArray();
        }


        [TranslateMsg("编辑器/事件记录_获取本回合事件记录_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_TURN = "获取本回合事件记录";
        [ActionNodeMethod(EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_TURN, EDITOR_TYPE_NAME_EVENT_RECORD)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT_RECORDS)]
        public static EventRecord[] getEventRecordsThisTurn(CardEngine game)
        {
            return getEventRecordsInTurnsBefore(game, 0, 0);
        }

        [TranslateMsg("编辑器/事件记录_获取前X-Y回合内的事件记录_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_EVENT_RECORDS_IN_TURNS_BEFORE = "获取前X-Y回合内的事件记录";
        [ActionNodeMethod(EDITOR_NODE_EVENT_GET_EVENT_RECORDS_IN_TURNS_BEFORE, EDITOR_TYPE_NAME_EVENT_RECORD)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT_RECORDS)]
        public static EventRecord[] getEventRecordsInTurnsBefore(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT_START_BEFORE)] int start,
            [ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT_END_BEFORE)] int end)
        {
            var records = game.triggers.getEventRecords();
            var results = new List<EventRecord>();
            int turnBeforeCount = 0;
            for (int i = records.Length - 1; i >= 0; i--)
            {
                var record = records[i];
                if (turnBeforeCount >= start)
                    results.Insert(0, record);
                if (turnBeforeCount > end)
                    break;
                if (record.eventArg is HearthStoneRule.TurnStartEventArg)
                {
                    turnBeforeCount++;
                }
            }
            return results.ToArray();
        }
        #endregion
    }
    [Serializable]
    public class EventArgReference
    {
        public string eventName;
        public EventArgReference(string name)
        {
            eventName = name;
        }
        public override bool Equals(object obj)
        {
            if (obj is EventArgReference eventArg)
            {
                return eventName == eventArg.eventName;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return eventName.GetHashCode();
        }
        public override string ToString()
        {
            return eventName;
        }
        public static bool operator ==(EventArgReference lhs, EventArgReference rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }
            return lhs.Equals(rhs);
        }
        public static bool operator !=(EventArgReference lhs, EventArgReference rhs)
        {
            if (lhs is null)
            {
                return !(rhs is null);
            }
            return !lhs.Equals(rhs);
        }
        public static implicit operator EventArgReference(string str)
        {
            return new EventArgReference(str);
        }
    }
}
