using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
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
        [TranslateMsg("编辑器/事件记录_前X回合开始_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT_START_BEFORE = "前X回合开始";
        [TranslateMsg("编辑器/事件记录_前Y回合结束_参数名称")]
        public const string EDITOR_PARAM_NAME_TURN_COUNT_END_BEFORE = "前Y回合结束";

        #region 事件记录

        [TranslateMsg("编辑器/事件记录_获取当前回合数_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_TURN_COUNT_OBSOLETE = "获取当前回合数（过时）";
        [TranslateMsg("编辑器/事件记录_获取当前回合数_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_TURN_COUNT = "获取当前回合数";
        [TranslateMsg("编辑器/事件记录_获取变量_节点名称")]
        public const string EDITOR_NODE_EVENT_RECORD_GET_VARIABLE = "获取事件记录变量";
        [TranslateMsg("编辑器/事件记录_事件记录类型判断_节点名称")]
        public const string EDITOR_NODE_EVENT_CHECK_EVENT_RECORD_TYPE = "事件记录类型判断";
        [TranslateMsg("编辑器/事件记录_获取本局游戏事件记录_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_GAME = "获取本局游戏事件记录";
        [TranslateMsg("编辑器/事件记录_获取本回合事件记录_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_TURN = "获取本回合事件记录";
        [TranslateMsg("编辑器/事件记录_获取前X-Y回合内的事件记录_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_EVENT_RECORDS_IN_TURNS_BEFORE = "获取前X-Y回合内的事件记录";
        [TranslateMsg("编辑器/事件记录_获取卡牌在某事件时的快照_节点名称")]
        public const string EDITOR_NODE_EVENT_GET_CARD_SNAPSHOT_OF_CARD_ON_EVENT = "获取卡牌在某事件时的快照";

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetTurnCount_Obsolete, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_GET_TURN_COUNT_OBSOLETE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT)]
        public static int getTurnCountObsolete(CardEngine game)
        {
            return game.getTurnCount() - 1;
        }
        [THHActionMethod(BuiltinActionDefineID.GetTurnCount, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_GET_TURN_COUNT)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT)]
        public static int getTurnCount(CardEngine game)
        {
            return game.getTurnCount();
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEventRecordVariable, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_RECORD_GET_VARIABLE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getRecordVariable(CardEngine game,
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)] EventRecord record,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME, true)] string varName)
        {
            if (record == null)
                return null;
            var value = record.getVar(varName);
            if (value is CardState state)
            {
                var snapshot = game.snapshotCard(state.card);
                state.card.revertToHistory(snapshot, state.stateIndex);
                return snapshot;
            }
            return value;
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.CheckEventRecordType, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_CHECK_EVENT_RECORD_TYPE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool checkEventRecordType(CardEngine game,
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)] EventRecord evt,
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_TYPE, isConst: true)] EventArgReference argRef)
        {
            if (evt == null || argRef == null)
                return false;
            if (game.triggers.getName(evt.eventArg) == argRef.value)
                return true;
            return false;
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEventRecordsThisGame, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_GAME)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT_RECORDS)]
        public static EventRecord[] getEventRecordsThisGame(CardEngine game)
        {
            return game.triggers.getEventRecords().ToArray();
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEventRecordsThisTurn, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_TURN)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT_RECORDS)]
        public static EventRecord[] getEventRecordsThisTurn(CardEngine game)
        {
            return getEventRecordsInTurnsBefore(game, 0, 0);
        }


        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEventRecordsInTurnsBefore, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_GET_EVENT_RECORDS_IN_TURNS_BEFORE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT_RECORDS)]
        public static EventRecord[] getEventRecordsInTurnsBefore(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT_START_BEFORE)] int farther,
            [ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT_END_BEFORE)] int nearer)
        {
            var records = game.triggers.getEventRecords();
            var results = new List<EventRecord>();
            int turnBeforeCount = 0;
            for (int i = records.Length - 1; i >= 0; i--)
            {
                var record = records[i];
                if (turnBeforeCount >= nearer)
                    results.Insert(0, record);
                if (turnBeforeCount > farther)
                    break;
                if (record.eventArg is HearthStoneRule.TurnStartEventArg)
                {
                    turnBeforeCount++;
                }
            }
            return results.ToArray();
        }


        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotOfEvent, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_GET_CARD_SNAPSHOT_OF_CARD_ON_EVENT)]
        [return: ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)]
        public static CardSnapshot getSnapshotOfCardAfterEvent(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)] EventRecord record)
        {
            if (card == null || record == null)
                return null;
            return game.getCardSnapshotOfEvent(card, record);
        }
        #endregion
    }
    [Serializable]
    public class EventArgReference : NodeConstProxyString
    {
        [Obsolete("请使用value。该属性仅用于序列化兼容，之后会移除该属性")]
        [BsonElement]
        [BsonIgnoreIfNull]
        public string eventName
        {
            get => null;
            set => this.value = value;
        }
        public EventArgReference(string value) : base(value)
        {

        }
        public static explicit operator EventArgReference(string str)
        {
            return new EventArgReference(str);
        }
    }
}
