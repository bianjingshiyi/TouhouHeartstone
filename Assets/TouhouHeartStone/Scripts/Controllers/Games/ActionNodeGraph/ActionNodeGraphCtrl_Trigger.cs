using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 公有方法
        /// <summary>
        /// 打开一个触发器的动作节点图
        /// </summary>
        /// <param name="trigger"></param>
        public void openTriggerGraph(long cardPoolId, TriggerGraph trigger, ActionNodeGraphInfo graphInfo)
        {
            display(new DisplayInfo(cardPoolId, trigger, null, graphInfo));
        }
        #endregion
        #region 私有方法
        #region 事件回调
        private void onChangeTriggerTypeIndex(ActionNodeItem item, int typeIndex)
        {
            var actionNode = getActionNode(item);
            if (actionNode is TriggerEntryNode trigger)
            {
                trigger.eventName = _eventTypeInfoList.getEventTypeNameByIndex(typeIndex);
                disconnectNodeAll(trigger);
                item.setOutputs(getTriggerOutputViewData(trigger));
                OnEdited();
            }
        }
        /// <summary>
        /// 当用户点击新增目标按钮，为当前触发器新建目标检查器，然后在UI列表中新增项目
        /// </summary>
        private void onClickAddTargetCheckerButton(ActionNodeItem item)
        {
            var actionNode = getActionNode(item);
            if (actionNode is TriggerEntryNode trigger)
            {
                var index = trigger.targetCheckerList.Count;
                trigger.AddTargetChecker(new TargetChecker(string.Empty, index, string.Empty));
                item.updateTargetCheckerList(getTranslatedTargetCheckerTypes(), getTriggerTargetCheckerViewData(trigger));
                OnEdited();
            }
        }
        private void onTargetCheckerTypeValueChanged(ActionNodeItem item, int index, int value)
        {
            var actionNode = getActionNode(item);
            if (actionNode is TriggerEntryNode trigger)
            {
                trigger.targetCheckerList[index].targetType = _targetCheckerTypes[value];
                OnEdited();
            }
        }
        /// <summary>
        /// 当用户点击删除目标按钮，为当前触发器删除目标检查器，然后在UI列表中也删除对应索引项目
        /// </summary>
        /// <param name="index"></param>
        private void onClickRemoveTargetCheckerButton(ActionNodeItem item, int index)
        {
            var actionNode = getActionNode(item);
            if (actionNode is TriggerEntryNode trigger)
            {
                trigger.RemoveTargetChecker(index);
                item.updateTargetCheckerList(getTranslatedTargetCheckerTypes(), getTriggerTargetCheckerViewData(trigger));
                OnEdited();
            }
        }
        private void onTargetCheckerErrorTipValueChanged(ActionNodeItem item, int index, string value)
        {
            var actionNode = getActionNode(item);
            if (actionNode is TriggerEntryNode trigger)
            {
                trigger.targetCheckerList[index].errorTip = value;
                OnEdited();
            }
        }
        #endregion
        /// <summary>
        /// 加载所有事件类型
        /// </summary>
        private void loadEventTypes()
        {
            _eventTypeInfoList.Clear();
            _eventTypeInfoList.AddRange(EventTypeInfoLoader.getOrLoadEventTypes());
        }
        private (int typeIndex, string name, string errorTip)[] getTriggerTargetCheckerViewData(TriggerEntryNode trigger)
        {
            var targetList = trigger.targetCheckerList;
            var targetCheckers = new (int typeIndex, string name, string errorTip)[targetList.Count];
            for (int i = 0; i < targetCheckers.Length; i++)
            {
                var targetChecker = targetList[i];
                targetCheckers[i].typeIndex = Array.IndexOf(_targetCheckerTypes, targetChecker.targetType);
                if (targetCheckers[i].typeIndex < 0)
                {
                    targetList[i].targetType = DEFINE.TARGET_TYPE_SERVANT;
                    targetCheckers[i].typeIndex = Array.IndexOf(_targetCheckerTypes, DEFINE.TARGET_TYPE_SERVANT);
                }
                targetCheckers[i].name = "目标" + (i + 1);
                targetChecker.errorTip = targetList[i].errorTip;
            }
            return targetCheckers;
        }
        private (string name, Color color)[] getTriggerInputViewData(TriggerEntryNode trigger)
        {
            (string name, Color color)[] inputs = new (string name, Color color)[1 + trigger.targetCheckerList.Count];
            inputs[0] = (
                game.langManager.getString(StringTable.EDITOR_PARAM_NAME_TRIGGER_CONDITION),
                game.editor.getColorByType(typeof(bool)));
            for (int i = 0; i < trigger.targetCheckerList.Count; i++)
            {
                inputs[1 + i] = (
                    string.Format(game.langManager.getString(StringTable.EDITOR_PARAM_NAME_TARGET_CONDITION), 1 + i),
                    game.editor.getColorByType(typeof(bool)));
            }
            return inputs;
        }
        /// <summary>
        /// 获取当前触发器的输出值的名称。
        /// </summary>
        /// <returns></returns>
        private (string name, Color color)[] getTriggerOutputViewData(TriggerEntryNode trigger)
        {
            int typeIndex = _eventTypeInfoList.getEventTypeIndex(trigger.eventName);
            EventDefine eventTypeInfo = _eventTypeInfoList[typeIndex];
            if (eventTypeInfo.variableInfos != null)
            {
                (string name, Color color)[] viewData = new (string name, Color color)[eventTypeInfo.variableInfos.Length + 1];
                //动作输出
                viewData[0] = (string.Empty, game.editor.getColorByType(typeof(ActionNode)));
                for (int i = 0; i < eventTypeInfo.variableInfos.Length; i++)
                {
                    //爱，这个名字要弄成中文可麻烦了，我总不能把字段的名称都搞成中文吧？
                    //不过考虑到自定义的时候玩家也不可能都写英文，所以这倒也不是不合理。
                    viewData[i + 1] = (game.langManager.getString(eventTypeInfo.variableInfos[i].name),
                        game.editor.getColorByType(eventTypeInfo.variableInfos[i].type));
                }
                return viewData;
            }
            else
            {
                //只有一个动作输出
                return new (string name, Color color)[]
                {
                    (string.Empty, game.editor.getColorByType(typeof(ActionNode)))
                };
            }
        }
        #endregion
        #region 属性字段
        EventTypeInfoCollection _eventTypeInfoList = new EventTypeInfoCollection();
        #endregion
    }
    public class EventTypeInfoLoader
    {

        #region 公有方法
        /// <summary>
        /// 获取所有事件类型，如果未加载则进行加载
        /// </summary>
        public static EventDefine[] getOrLoadEventTypes()
        {
            if (_eventTypeInfoList != null)
                return _eventTypeInfoList.ToArray();
            _eventTypeInfoList = new List<EventDefine>();
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
                        //发生时事件
                        _eventTypeInfoList.Add(new EventDefine()
                        {
                            eventName = EventHelper.getNameBefore(eventArg),
                            variableInfos = eventArg.getBeforeEventVarInfos()
                        });
                        //发生后事件
                        _eventTypeInfoList.Add(new EventDefine()
                        {
                            eventName = EventHelper.getNameAfter(eventArg),
                            variableInfos = eventArg.getAfterEventVarInfos()
                        });
                    }
                }
            }
            //一些无法通过反射加载的事件类型
            _eventTypeInfoList.Add(new EventDefine()
            {
                eventName = "ActiveEvent",
                editorName = StringTable.EDITOR_EVENT_NAME_ACTIVE,
                variableInfos = new EventVariableInfo[]
                {
                    new EventVariableInfo() { type = typeof(Player), name = "玩家"},
                    new EventVariableInfo() { type = typeof(Card), name = "卡牌" },
                    new EventVariableInfo() { type = typeof(Card), name = "目标卡牌" },
                }
            });
            _eventTypeInfoList.Find(e => e.eventName == "BeforeTurnStartEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_TURN_START;
            _eventTypeInfoList.Find(e => e.eventName == "AfterTurnStartEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_TURN_START;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeUseEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_USE;
            _eventTypeInfoList.Find(e => e.eventName == "AfterUseEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_USE;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeSetArmorEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_ARMOR_CHANGE;
            _eventTypeInfoList.Find(e => e.eventName == "AfterSetArmorEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_ARMOR_CHANGE;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeTurnEndEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_TURN_END;
            _eventTypeInfoList.Find(e => e.eventName == "AfterTurnEndEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_TURN_END;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeSilenceEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_SILENCE;
            _eventTypeInfoList.Find(e => e.eventName == "AfterSilenceEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_SILENCE;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeAttackEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_ATTACK;
            _eventTypeInfoList.Find(e => e.eventName == "AfterAttackEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_ATTACK;
            //_eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeDamageEvent");
            //_eventTypeInfoList.RemoveAll(e => e.eventName == "AfterDamageEvent");
            _eventTypeInfoList.Find(e => e.eventName == "BeforeDamageEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_DAMAGE;
            _eventTypeInfoList.Find(e => e.eventName == "AfterDamageEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_DAMAGE;
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeHealEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterHealEvent");
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeHealEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_HEAL;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterHealEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_HEAL;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeDeathEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_DEATH;
            _eventTypeInfoList.Find(e => e.eventName == "AfterDeathEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_DEATH;
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeSecretTriggerEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterSecretTriggerEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeMissionCompleteEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterMissionCompleteEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeComeOnStageEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterComeOnStageEvent");
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeSecreTriggerEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_SECRETRIGGER;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterSecreTriggerEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_SECRETRIGGER;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeMissionCompleteEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_MISSIONCOMPLETE;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterMissionCompleteEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_MISSIONCOMPLETE;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeComeOnStageEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_COMEONSTAGE;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterComeOnStageEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_COMEONSTAGE;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeInitEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_INIT;
            _eventTypeInfoList.Find(e => e.eventName == "AfterInitEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_INIT;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeStartEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_START;
            _eventTypeInfoList.Find(e => e.eventName == "AfterStartEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_START;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeGameEndEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_GAMEEND;
            _eventTypeInfoList.Find(e => e.eventName == "AfterGameEndEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_GAMEEND;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeInitReplaceEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_INITREPLACE;
            _eventTypeInfoList.Find(e => e.eventName == "AfterInitReplaceEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_INITREPLACE;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeSetMaxGemEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_SETMAXGEM;
            _eventTypeInfoList.Find(e => e.eventName == "AfterSetMaxGemEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_SETMAXGEM;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeBurnEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_BURN;
            _eventTypeInfoList.Find(e => e.eventName == "AfterBurnEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_BURN;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeEnterFieldEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_ENTERFIELD;
            _eventTypeInfoList.Find(e => e.eventName == "AfterEnterFieldEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_ENTERFIELD;
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeRebornEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterRebornEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeRevealEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterRevealEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeRevealDuelEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterRevealDuelEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeChooseOneEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterChooseOneEvent");
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeChooseOneEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_CHOOSEONE;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterChooseOneEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_CHOOSEONE;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeRebornEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_REBORN;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterRebornEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_REBORN;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeRevealEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_REVEAL;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterRevealEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_REVEAL;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeSetGemEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_SETGEM;
            _eventTypeInfoList.Find(e => e.eventName == "AfterSetGemEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_SETGEM;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeDrawEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_DRAW;
            _eventTypeInfoList.Find(e => e.eventName == "AfterDrawEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_DRAW;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeFatigueEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_FATIGUE;
            _eventTypeInfoList.Find(e => e.eventName == "AfterFatigueEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_FATIGUE;
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeSearchPileEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterSearchPileEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeActiveEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterActiveEvent");
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeSearchiPileEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_SEARCHPILE;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterSearchiPileEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_SEARCHPILE;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeActiveEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_ACTIVE;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterActiveEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_ACTIVE;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeDiscardEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_DISCARD;
            _eventTypeInfoList.Find(e => e.eventName == "AfterDiscardEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_DISCARD;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeCreateTokenEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_CREATETOKEN;
            _eventTypeInfoList.Find(e => e.eventName == "AfterCreateTokenEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_CREATETOKEN;
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeBeChargedEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterBeChargedEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeTestEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterTestEvent");
            _eventTypeInfoList.Find(e => e.eventName == "BeforeSetDefineEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_SETDEFINE;
            _eventTypeInfoList.Find(e => e.eventName == "AfterSetDefineEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_SETDEFINE;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeAddModiEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_ADDMODI;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterAddModiEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_ADDMODI;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeRemoveModiEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_REMOVEMODI;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterRemoveModiEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_REMOVEMODI;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforePropChangeEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_PROPCHANGE;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterPropChangeEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_PROPCHANGE;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeCommandEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_COMMAND;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterCommandEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_COMMAND;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeTestEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_TEST;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterTestEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_TEST;
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeAddModiEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterAddModiEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeRemoveModiEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterRemoveModiEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforePropChangeEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterPropChangeEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeCommandEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterCommandEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeTestEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterTestEvent");
            _eventTypeInfoList.Find(e => e.eventName == "BeforeDiscoverEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_DISCOVER;
            _eventTypeInfoList.Find(e => e.eventName == "AfterDiscoverEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_DISCOVER;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeSelectEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_SELECT;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterSelectEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_SELECT;
            _eventTypeInfoList.Find(e => e.eventName == "BeforeEquipItemEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_EQUIP;
            _eventTypeInfoList.Find(e => e.eventName == "AfterEquipItemEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_EQUIP;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeElementEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_ELEMENT;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterElementEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_ELEMENT;
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeElementMixEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterElementMixEvent");
            _eventTypeInfoList.Sort((a, b) => a.eventName.CompareTo(b.eventName));
            //以后可能有自定义的事件类型，不过先不管
            return _eventTypeInfoList.ToArray();
        }
        public static bool isValidEventTypeName(string eventTypeName)
        {
            return FindEventTypeInfo(eventTypeName) != null;
        }
        public static EventDefine FindEventTypeInfo(string eventTypeName)
        {
            if (_eventTypeInfoList == null)
                getOrLoadEventTypes();
            return _eventTypeInfoList.Find(e => e.eventName == eventTypeName || (e.obsoleteNames != null && e.obsoleteNames.Contains(eventTypeName)));
        }
        #endregion
        static List<EventDefine> _eventTypeInfoList = null;
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
        public string[] getEventTypeNamesForEditor()
        {
            string[] eventNames = new string[Count];
            for (int i = 0; i < eventNames.Length; i++)
            {
                eventNames[i] = string.IsNullOrEmpty(this[i].editorName) ? this[i].eventName : this[i].editorName;
            }
            return eventNames;
        }
    }

}
