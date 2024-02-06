using System;
using System.Collections.Generic;
using System.Linq;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Histories;
using TouhouHeartstone;

namespace Game
{
    public partial class EffectEditorLibrary
    {
        /// <summary>
        /// 获取事件记录变量节点的事件参数主键。
        /// </summary>
        public const string EVENT_RECORD_VARIABLE_NODE_EVENT_PARAM_KEY = "record";
        //  事件处理
        [TranslateMsg("编辑器/事件记录_参数名称")]
        public const string EDITOR_TYPE_NAME_EVENT_RECORD = "事件记录";

        #region 事件记录
        [TranslateMsg("编辑器/事件记录_获取变量_节点名称")]
        public const string EDITOR_NODE_EVENT_RECORD_GET_VARIABLE = "获取事件记录变量";
        [TranslateMsg("编辑器/事件记录-节点名称")]
        public const string EDITOR_NODE_NAME_CAST_EVENT_RECORD_TYPE = "转换事件记录类型";
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
        [TranslateMsg("编辑器/事件记录_参数名称")]
        public const string EDITOR_PARAM_NAME_EVENT_RECORDS = "事件记录集合";


        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEventRecordVariable, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_RECORD_GET_VARIABLE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getRecordVariable(CardEngine game,
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)] EventRecord record,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME)] EventVarName varName)
        {
            if (record == null)
                return null;
            var value = record.getVar(varName);
            if (value is CardState state)
            {
                var snapshot = game.snapshotCard(state.card);
                game.triggers.revertChanges(snapshot, state.stateIndex);
                return snapshot;
            }
            return value;
        }
        [Obsolete]
        [THHActionMethod(BuiltinActionDefineID.CastEventRecordType, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_NAME_CAST_EVENT_RECORD_TYPE)]
        [return: ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)]
        public static EventRecord castEventRecordType(
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)] EventRecord eventRecord,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EVENT_TYPE, isConst: true)] EventReference eventReference)
        {
            if (eventRecord == null || eventReference == null)
                return null;
            var define = eventRecord.eventArg.define;
            bool equals = define.cardPoolId == eventReference.cardPoolId && define.eventName == eventReference.eventName;
            return equals ? eventRecord : null;
        }
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.CheckEventRecordType, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_CHECK_EVENT_RECORD_TYPE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool checkEventRecordType(CardEngine game,
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)] EventRecord evt,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EVENT_TYPE, isConst: true)] EventReference argRef)
        {
            if (evt == null || argRef == null)
                return false;
            var define = evt.eventArg.define;
            bool equals = define.cardPoolId == argRef.cardPoolId && define.eventName == argRef.eventName;
            if (equals)
                return true;
            return false;
        }
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEventRecordsThisGame, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_GAME)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT_RECORDS)]
        public static EventRecord[] getEventRecordsThisGame(CardEngine game)
        {
            return game.triggers.getEventRecords().ToArray();
        }

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEventRecordsThisTurn, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_GET_EVENT_RECORDS_THIS_TURN)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EVENT_RECORDS)]
        public static EventRecord[] getEventRecordsThisTurn(CardEngine game)
        {
            return getEventRecordsInTurnsBefore(game, 0, 0);
        }


        [Obsolete]
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
                if (record.eventArg.define is TurnStartEventDefine)
                {
                    turnBeforeCount++;
                }
            }
            return results.ToArray();
        }

        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotOfEvent, EDITOR_TYPE_NAME_EVENT_RECORD, EDITOR_NODE_EVENT_GET_CARD_SNAPSHOT_OF_CARD_ON_EVENT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)]
        public static CardSnapshot getSnapshotOfCardAfterEvent(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_TYPE_NAME_EVENT_RECORD)] EventRecord record)
        {
            if (card == null || record == null)
                return null;
            return game.getCardSnapshotAfterEvent(card, record.eventArg);
        }
        #endregion
    }
}
