using TouhouCardEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UI;
using Card = TouhouCardEngine.Card;
using Game;
using TouhouHeartstone;

namespace Controllers
{
    public class IOConnectInfo
    {
        public string type = string.Empty;
        public string label = string.Empty;
    }
    partial class ActionNodeGraphCtrl
    {
        #region 公有方法

        #endregion
        #region 私有方法
        //private void openGeneratedActionDefine(GeneratedActionDefine generatedActionDefine)
        //{
        //    _actionNodeList.Clear();
        //    _actionNodeItemList.Clear();
        //    _entryType = null;
        //    _effect = null;
        //    _valueRef = null;
        //    _targetChecker = null;
        //    _trigger = null;
        //    _generatedActionDefine = generatedActionDefine;
        //    //显示UI
        //    _ui.clearGraph();
        //    //创建入口节点
        //    _entryNodeItem = _ui.createTriggerNode(0, 0,
        //        game.langManager.getString(StringTable.EDITOR_NODE_NAME_TRIGGER),
        //        getTranslatedTargetCheckerTypes(),
        //        triggerIndex,
        //        getTriggerTargetCheckerViewData());
        //    if (_entryType == nameof(GeneratedBattlecry.activeTrigger))
        //    {
        //        //战吼类型节点图隐藏事件类型
        //        _entryNodeItem.displayEventType(false);
        //    }
        //    else
        //    {
        //        _entryNodeItem.displayEventType(true);
        //    }
        //    _entryNodeItem.setInputs(getTriggerInputNames(), _ui.GetTypeListColor());
        //    _entryNodeItem.setOutputs(getTriggerOutputNames(), _ui.GetTypeListColor());
        //    //创建触发条件节点
        //    if (_trigger.condition != null && _trigger.condition.action != null)
        //    {
        //        ActionNodeItem actionNodeItem = await createGraph4ActionNode(_trigger.condition.action);
        //        _ui.connectNode(actionNodeItem, _trigger.condition.action.branches.Length + _trigger.condition.index, _entryNodeItem, 0, ActionNodeConnectionType.value);
        //    }
        //    //创建目标条件节点
        //    if (_trigger.targetCheckerList != null && _trigger.targetCheckerList.Count > 0)
        //    {
        //        for (int i = 0; i < _trigger.targetCheckerList.Count; i++)
        //        {
        //            if (_trigger.targetCheckerList[i] == null ||
        //                _trigger.targetCheckerList[i].condition == null ||
        //                _trigger.targetCheckerList[i].condition.action == null)
        //                continue;
        //            ActionNodeItem actionNodeItem = await createGraph4ActionNode(_trigger.targetCheckerList[i].condition.action);
        //            _ui.connectNode(
        //                actionNodeItem,
        //                _trigger.targetCheckerList[i].condition.action.branches.Length + _trigger.targetCheckerList[i].condition.index,
        //                _entryNodeItem,
        //                1 + i,
        //                ActionNodeConnectionType.value);
        //        }
        //    }
        //    //创建动作节点
        //    if (_trigger.action != null)
        //    {
        //        ActionNodeItem actionNodeItem = await createGraph4ActionNode(_trigger.action);
        //        _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0, ActionNodeConnectionType.action);
        //    }
        //    //创建离散节点
        //    for (int i = 0; i < _graphInfo.freeActionNodeList.Count; i++)
        //    {
        //        if (_graphInfo.freeActionNodeList[i] == null)
        //            continue;
        //        if (getActionNode(_graphInfo.freeActionNodeList[i].id) != null)
        //        {
        //            //防止因为离散节点表中错误的记录了已经存在的节点而重复进行创建
        //            _graphInfo.freeActionNodeList.RemoveAt(i);
        //            i--;
        //            continue;
        //        }
        //        await createGraph4ActionNode(_graphInfo.freeActionNodeList[i]);
        //    }
        //    updateContentSize();
        //}
        #endregion
        #region 属性字段
        private GeneratedActionDefine _generatedActionDefine;
        #endregion
    }
    partial class ActionNodeGraphCtrl
    {
        #region 公有方法
        /// <summary>
        /// 打开一个触发器的动作节点图
        /// </summary>
        /// <param name="trigger"></param>
        public void openTriggerGraph(int cardPoolId, string entryType, TriggerGraph trigger, ActionNodeGraphInfo graphInfo)
        {
            display(new DisplayInfo(cardPoolId, entryType, trigger, null, graphInfo));
        }
        #endregion
        #region 私有方法
        #region 事件回调
        private void onChangeTriggerTypeIndex(int typeIndex)
        {
            _trigger.eventName = getEventTypeNameByIndex(typeIndex);
            //TODO:切换事件要断开所有事件变量引用连接
            _entryNodeItem.setOutputs(getTriggerOutputNames(), _ui.GetTypeListColor());
        }
        /// <summary>
        /// 当用户更改触发器的事件名称，将事件名称改为更改值
        /// </summary>
        /// <param name="eventName"></param>
        private void onChangeTriggerEventName(string eventName)
        {
            _trigger.eventName = eventName;
        }
        /// <summary>
        /// 当用户点击编辑条件按钮，将当前的编辑信息入栈，打开条件编辑
        /// </summary>
        private async void onClickTriggerEditConditionButton()
        {
            pushCurEditObjectToStack();
            if (_trigger.condition == null)
                _trigger.condition = new ActionValueRef();
            await openValueRefGraph(_trigger.condition);
        }
        /// <summary>
        /// 当用户点击新增目标按钮，为当前触发器新建目标检查器，然后在UI列表中新增项目
        /// </summary>
        private void onClickAddTargetCheckerButton()
        {
            _trigger.targetCheckerList.Add(new TargetChecker(string.Empty, new ActionValueRef(null, 0), string.Empty));
            _entryNodeItem.updateTargetCheckerList(getTranslatedTargetCheckerTypes(), getTriggerTargetCheckerViewData());
        }
        private void onTargetCheckerTypeValueChanged(int index, int value)
        {
            _trigger.targetCheckerList[index].targetType = _targetCheckerTypes[value];
        }
        /// <summary>
        /// 当用户点击编辑目标按钮，将当前的编辑信息入栈，打开目标编辑
        /// </summary>
        /// <param name="index"></param>
        private async void onClickEditTargetCheckerButton(int index)
        {
            pushCurEditObjectToStack();
            await openTargetCheckerGraph(_trigger.targetCheckerList[index]);
        }
        /// <summary>
        /// 当用户点击删除目标按钮，为当前触发器删除目标检查器，然后在UI列表中也删除对应索引项目
        /// </summary>
        /// <param name="index"></param>
        private void onClickRemoveTargetCheckerButton(int index)
        {
            _trigger.targetCheckerList.RemoveAt(index);
            _entryNodeItem.updateTargetCheckerList(getTranslatedTargetCheckerTypes(), getTriggerTargetCheckerViewData());
        }
        private void onTargetCheckerErrorTipValueChanged(int index, string value)
        {
            _trigger.targetCheckerList[index].errorTip = value;
        }
        #endregion
        /// <summary>
        /// 加载所有事件类型
        /// </summary>
        private void loadEventTypes()
        {
            _eventTypeInfoList.Clear();
            _eventTypeInfoList.AddRange(EventTypeInfo.getOrLoadEventTypes());
        }
        /// <summary>
        /// 打开一个触发器的动作节点图
        /// </summary>
        /// <param name="trigger"></param>
        private async Task openTriggerGraph(TriggerGraph trigger, string entryType)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _entryType = entryType;
            _effect = null;
            _valueRef = null;
            _targetChecker = null;
            _trigger = trigger;
            //显示UI
            _ui.clearGraph();
            //转成index
            int triggerIndex = getEventTypeIndex(_trigger.eventName);
            if (triggerIndex < 0)
            {
                //没有类型
                string[] eventNames = getEventTypeNames();
                triggerIndex = eventNames.Length - 1;
                _trigger.eventName = eventNames[triggerIndex];
            }
            //创建入口节点
            _entryNodeItem = _ui.createTriggerNode(0, 0,
                game.langManager.getString(StringTable.EDITOR_NODE_NAME_TRIGGER),
                getTranslatedTargetCheckerTypes(),
                triggerIndex,
                getTriggerTargetCheckerViewData());
            if (_entryType == nameof(GeneratedBattlecry.activeTrigger))
            {
                //战吼类型节点图隐藏事件类型
                _entryNodeItem.displayEventType(false);
            }
            else
            {
                _entryNodeItem.displayEventType(true);
            }
            _entryNodeItem.setInputs(getTriggerInputNames(), _ui.GetTypeListColor());
            _entryNodeItem.setOutputs(getTriggerOutputNames(), _ui.GetTypeListColor());
            //创建触发条件节点
            if (_trigger.condition != null && _trigger.condition.action != null)
            {
                ActionNodeItem actionNodeItem = await createGraph4ActionNode(_trigger.condition.action);
                _ui.connectNode(actionNodeItem, _trigger.condition.action.branches.Length + _trigger.condition.index, _entryNodeItem, 0, ActionNodeConnectionType.value);
            }
            //创建目标条件节点
            if (_trigger.targetCheckerList != null && _trigger.targetCheckerList.Count > 0)
            {
                for (int i = 0; i < _trigger.targetCheckerList.Count; i++)
                {
                    if (_trigger.targetCheckerList[i] == null ||
                        _trigger.targetCheckerList[i].condition == null ||
                        _trigger.targetCheckerList[i].condition.action == null)
                        continue;
                    ActionNodeItem actionNodeItem = await createGraph4ActionNode(_trigger.targetCheckerList[i].condition.action);
                    _ui.connectNode(
                        actionNodeItem,
                        _trigger.targetCheckerList[i].condition.action.branches.Length + _trigger.targetCheckerList[i].condition.index,
                        _entryNodeItem,
                        1 + i,
                        ActionNodeConnectionType.value);
                }
            }
            //创建动作节点
            if (_trigger.action != null)
            {
                ActionNodeItem actionNodeItem = await createGraph4ActionNode(_trigger.action);
                _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0, ActionNodeConnectionType.action);
            }
            //创建离散节点
            for (int i = 0; i < _graphInfo.freeActionNodeList.Count; i++)
            {
                if (_graphInfo.freeActionNodeList[i] == null)
                    continue;
                if (getActionNode(_graphInfo.freeActionNodeList[i].id) != null)
                {
                    //防止因为离散节点表中错误的记录了已经存在的节点而重复进行创建
                    _graphInfo.freeActionNodeList.RemoveAt(i);
                    i--;
                    continue;
                }
                await createGraph4ActionNode(_graphInfo.freeActionNodeList[i]);
            }
            updateContentSize();
        }
        private (int typeIndex, string name, string errorTip)[] getTriggerTargetCheckerViewData()
        {
            var targetCheckers = new (int typeIndex, string name, string errorTip)[_trigger.targetCheckerList.Count];
            for (int i = 0; i < targetCheckers.Length; i++)
            {
                targetCheckers[i].typeIndex = Array.IndexOf(_targetCheckerTypes, _trigger.targetCheckerList[i].targetType);
                if (targetCheckers[i].typeIndex < 0)
                {
                    _trigger.targetCheckerList[i].targetType = DEFINE.TARGET_TYPE_SERVANT;
                    targetCheckers[i].typeIndex = Array.IndexOf(_targetCheckerTypes, DEFINE.TARGET_TYPE_SERVANT);
                }
                targetCheckers[i].name = "目标" + (i + 1);
                targetCheckers[i].errorTip = _trigger.targetCheckerList[i].errorTip;
            }
            return targetCheckers;
        }
        private IOConnectInfo[] getTriggerInputNames()
        {
            IOConnectInfo[] inputs = new IOConnectInfo[1 + _trigger.targetCheckerList.Count];
            inputs[0] = new IOConnectInfo()
            {
                type = "Func<bool>",
                label = game.langManager.getString(StringTable.EDITOR_PARAM_NAME_TRIGGER_CONDITION)
            };
            for (int i = 0; i < _trigger.targetCheckerList.Count; i++)
            {
                inputs[1 + i] = new IOConnectInfo()
                {
                    type = "Func<Card, bool>",
                    label = string.Format(game.langManager.getString(StringTable.EDITOR_PARAM_NAME_TARGET_CONDITION), 1 + i)
                };
            }
            return inputs;
        }
        /// <summary>
        /// 获取当前触发器的输出值的名称。
        /// </summary>
        /// <returns></returns>
        private IOConnectInfo[] getTriggerOutputNames()
        {
            int typeIndex = getEventTypeIndex(_trigger.eventName);
            EventTypeInfo eventTypeInfo = _eventTypeInfoList[typeIndex];
            if (eventTypeInfo.variableInfos != null)
            {
                IOConnectInfo[] outputNames = new IOConnectInfo[eventTypeInfo.variableInfos.Length + 1];
                //动作输出
                outputNames[0] = new IOConnectInfo()
                {
                    type = string.Empty,
                    label = string.Empty
                };
                for (int i = 0; i < eventTypeInfo.variableInfos.Length; i++)
                {
                    //这里加入了类型，这块有些凌乱，我暂时就用string来处理type
                    outputNames[i + 1] = new IOConnectInfo()
                    {
                        type = eventTypeInfo.variableInfos[i].name,
                        //爱，这个名字要弄成中文可麻烦了，我总不能把字段的名称都搞成中文吧？
                        //不过考虑到自定义的时候玩家也不可能都写英文，所以这倒也不是不合理。
                        label = game.langManager.getString(eventTypeInfo.variableInfos[i].name)
                    };
                }
                return outputNames;
            }
            else
            {
                //只有一个动作输出
                return new IOConnectInfo[]
                {
                    new IOConnectInfo()
                    {
                        type = string.Empty,
                        label =  string.Empty
                    }
                }; ;
            }
        }
        /// <summary>
        /// 将事件节点的输出值连接到输入上
        /// </summary>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNode"></param>
        /// <param name="inputIndex"></param>
        private void connectNode4TriggerEventVarRef(int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            string[] curEventVarNames = getEventVarNames();
            //判断是否已经相连
            if (inputActionNode.inputs != null &&
                inputActionNode.inputs.Length > inputIndex &&
                inputActionNode.inputs[inputIndex] != null)
            {
                if (inputActionNode.inputs[inputIndex].eventVarName == curEventVarNames[outputIndex])
                {
                    //已经相连，什么也不做
                    return;
                }
                else
                {
                    //相连的是其他输入节点，断开连接
                    if (inputActionNode.inputs[inputIndex].action != null)
                    {
                        //直接与节点值连接
                        unconnectNode4ValueRef(inputActionNode.inputs[inputIndex].action, inputActionNode.inputs[inputIndex].index, inputActionNode, inputIndex);
                    }
                    else if (inputActionNode.inputs[inputIndex].actionNodeId != 0)
                    {
                        //间接与节点值连接
                        unconnectNode4ValueRef(getActionNode(inputActionNode.inputs[inputIndex].actionNodeId), inputActionNode.inputs[inputIndex].index, inputActionNode, inputIndex);
                    }
                    else if (!string.IsNullOrEmpty(inputActionNode.inputs[inputIndex].eventVarName))
                    {
                        unconnectNode4TriggerEventVarRef(getEventVarNameIndex(inputActionNode.inputs[inputIndex].eventVarName), inputActionNode, inputIndex);
                    }
                }
            }
            //建立新的连接
            inputActionNode.inputs[inputIndex] = new ActionValueRef(curEventVarNames[outputIndex]);
            //更新ui
            _ui.connectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), inputActionNode.branches.Length + inputIndex, ActionNodeConnectionType.value);
        }
        private void connectNode4TriggerCondition(ActionNode outputActionNode, int outputIndex, int inputIndex)
        {
            if (inputIndex == 0)
            {
                //触发条件
                //判断是否已经相连
                if (_trigger.condition != null &&
                    _trigger.condition.action != null)
                {
                    if (_trigger.condition.action == outputActionNode &&
                        _trigger.condition.index == outputIndex)
                    {
                        //已经相连，什么也不做
                        return;
                    }
                    else
                    {
                        //与其他节点相连，先断开连接
                        ActionNode prevActionNode = _trigger.condition.action;
                        unconnectNode4TriggerCondition(prevActionNode, prevActionNode.branches.Length + _trigger.condition.index, 0);
                    }
                }
                //建立新的连接
                _trigger.condition = new ActionValueRef(outputActionNode, outputIndex);
                //更新UI
                _ui.connectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, _entryNodeItem, 0, ActionNodeConnectionType.value);
            }
            else if (inputIndex > 0)
            {
                //目标条件
                int targetCheckerIndex = inputIndex - 1;
                //判断是否已经相连
                if (_trigger.targetCheckerList != null &&
                    targetCheckerIndex < _trigger.targetCheckerList.Count &&
                    _trigger.targetCheckerList[targetCheckerIndex] != null &&
                    _trigger.targetCheckerList[targetCheckerIndex].condition != null &&
                    _trigger.targetCheckerList[targetCheckerIndex].condition.action != null)
                {
                    if (_trigger.targetCheckerList[targetCheckerIndex].condition.action == outputActionNode &&
                        _trigger.targetCheckerList[targetCheckerIndex].condition.index == outputIndex)
                    {
                        //已经连接，什么都不做
                        return;
                    }
                    else
                    {
                        //与其他节点连接，断开连接
                        unconnectNode4TriggerCondition(outputActionNode, outputIndex, inputIndex);
                    }
                }
                //建立新的连接
                _trigger.targetCheckerList[targetCheckerIndex].condition = new ActionValueRef(outputActionNode, outputIndex);
                //更新UI
                _ui.connectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, _entryNodeItem, inputIndex, ActionNodeConnectionType.value);
            }
        }
        private void unconnectNode4TriggerCondition(ActionNode outputActionNode, int outputIndex, int inputIndex)
        {
            if (inputIndex == 0)
            {
                //触发条件
                //判断是否已经相连
                if (_trigger.condition != null &&
                    _trigger.condition.action == outputActionNode &&
                    _trigger.condition.index == inputIndex)
                {
                    //相连，断开连接
                    _trigger.condition.action = null;
                    _trigger.condition.index = 0;
                    //更新ui
                    _ui.unconnectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, _entryNodeItem, inputIndex);
                }
            }
            else if (inputIndex > 0)
            {
                //目标条件
                //判断是否已经相连
                int targetCheckerIndex = inputIndex - 1;
                if (_trigger.targetCheckerList != null &&
                    targetCheckerIndex < _trigger.targetCheckerList.Count &&
                    _trigger.targetCheckerList[targetCheckerIndex] != null &&
                    _trigger.targetCheckerList[targetCheckerIndex].condition != null &&
                    _trigger.targetCheckerList[targetCheckerIndex].condition.action == outputActionNode &&
                    _trigger.targetCheckerList[targetCheckerIndex].condition.index == outputIndex)
                {
                    //相连，断开连接
                    _trigger.targetCheckerList[targetCheckerIndex].condition.action = null;
                    _trigger.targetCheckerList[targetCheckerIndex].condition.index = 0;
                    //更新UI
                    _ui.unconnectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, _entryNodeItem, inputIndex);
                }
            }
        }
        private void unconnectNode4TriggerEventVarRef(int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            string[] curEventVarNames = getEventVarNames();
            //判断是否已经相连
            if (inputActionNode.inputs != null &&
                inputActionNode.inputs.Length > inputIndex &&
                inputActionNode.inputs[inputIndex] != null)
            {
                if (inputActionNode.inputs[inputIndex].eventVarName == curEventVarNames[outputIndex])
                {
                    //相连，断开连接
                    inputActionNode.inputs[inputIndex] = null;
                    //更新ui
                    _ui.unconnectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), inputActionNode.branches.Length + inputIndex);
                }
            }
        }
        private int getEventTypeIndex(string eventName)
        {
            return _eventTypeInfoList.FindIndex(e => e.eventName == eventName);
        }
        private string getEventTypeNameByIndex(int index)
        {
            return _eventTypeInfoList[index].eventName;
        }
        private string[] getEventTypeNames()
        {
            string[] eventNames = new string[_eventTypeInfoList.Count];
            for (int i = 0; i < eventNames.Length; i++)
            {
                eventNames[i] = _eventTypeInfoList[i].eventName;
            }
            return eventNames;
        }
        private string[] getEventTypeNamesForEditor()
        {
            string[] eventNames = new string[_eventTypeInfoList.Count];
            for (int i = 0; i < eventNames.Length; i++)
            {
                eventNames[i] = string.IsNullOrEmpty(_eventTypeInfoList[i].editorName) ? _eventTypeInfoList[i].eventName : _eventTypeInfoList[i].editorName;
            }
            return eventNames;
        }
        private string[] getEventVarNames()
        {
            EventTypeInfo eventTypeInfo = _eventTypeInfoList.Find(e => e.eventName == _trigger.eventName);
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
        private int getEventVarNameIndex(string eventVarName)
        {
            EventTypeInfo eventTypeInfo = _eventTypeInfoList.Find(e => e.eventName == _trigger.eventName);
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
        private EventVariableInfo getEventVarInfo(int index)
        {
            EventTypeInfo eventTypeInfo = _eventTypeInfoList.Find(e => e.eventName == _trigger.eventName);
            if (eventTypeInfo.variableInfos != null && index < eventTypeInfo.variableInfos.Length)
            {
                return eventTypeInfo.variableInfos[index];
            }
            return null;
        }
        #endregion
        #region 属性字段
        List<EventTypeInfo> _eventTypeInfoList = new List<EventTypeInfo>();
        #endregion
    }
    class EventTypeInfo
    {
        #region 公有方法
        /// <summary>
        /// 获取所有事件类型，如果未加载则进行加载
        /// </summary>
        public static EventTypeInfo[] getOrLoadEventTypes()
        {
            if (_eventTypeInfoList != null)
                return _eventTypeInfoList.ToArray();
            _eventTypeInfoList = new List<EventTypeInfo>();
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
                        _eventTypeInfoList.Add(new EventTypeInfo()
                        {
                            eventName = EventHelper.getNameBefore(eventArg),
                            variableInfos = eventArg.getBeforeEventVarInfos()
                        });
                        //发生后事件
                        _eventTypeInfoList.Add(new EventTypeInfo()
                        {
                            eventName = EventHelper.getNameAfter(eventArg),
                            variableInfos = eventArg.getAfterEventVarInfos()
                        });
                    }
                }
            }
            //一些无法通过反射加载的事件类型
            _eventTypeInfoList.Add(new EventTypeInfo()
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
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeDamageEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterDamageEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeHealEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterHealEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforeDeathEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterDeathEvent");
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeDamageEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_DAMAGE;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterDamageEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_DAMAGE;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeHealEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_HEAL;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterHealEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_HEAL;
            //_eventTypeInfoList.Find(e => e.eventName == "BeforeDeathEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_DEATH;
            //_eventTypeInfoList.Find(e => e.eventName == "AfterDeathEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_DEATH;
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
            _eventTypeInfoList.RemoveAll(e => e.eventName == "BeforePreHeatEvent");
            _eventTypeInfoList.RemoveAll(e => e.eventName == "AfterPreHeatEvent");
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
            _eventTypeInfoList.Find(e => e.eventName == "BeforeSelectEvent").editorName = StringTable.EDITOR_EVENT_NAME_BEFORE_SELECT;
            _eventTypeInfoList.Find(e => e.eventName == "AfterSelectEvent").editorName = StringTable.EDITOR_EVENT_NAME_AFTER_SELECT;
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
            if (_eventTypeInfoList == null)
                getOrLoadEventTypes();
            return _eventTypeInfoList.Find(e => e.eventName == eventTypeName) != null;
        }
        public static EventTypeInfo findEventTypeInfoForObsolateName(string eventTypeName)
        {
            if (_eventTypeInfoList == null)
                getOrLoadEventTypes();
            return _eventTypeInfoList.Find(e => e.obsoleteNames != null && e.obsoleteNames.Contains(eventTypeName));
        }
        #endregion
        #region 属性字段
        public string eventName;
        public string editorName;
        public EventVariableInfo[] variableInfos;
        public string[] obsoleteNames;
        static List<EventTypeInfo> _eventTypeInfoList = null;
        #endregion
    }
}
