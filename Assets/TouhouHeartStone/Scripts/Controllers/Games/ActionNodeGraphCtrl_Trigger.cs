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
        /// <summary>
        /// 打开一个触发器的动作节点图
        /// </summary>
        /// <param name="trigger"></param>
        public void openTriggerGraph(int cardPoolId, TriggerGraph trigger, ActionNodeGraphInfo graphInfo)
        {
            display(new DisplayInfo(cardPoolId, null, trigger, null, graphInfo));
        }
        #endregion
        #region 私有方法
        #region 事件回调
        private void onChangeTriggerTypeIndex(int typeIndex)
        {
            _trigger.eventName = getEventTypeNameByIndex(typeIndex);
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
        private async Task openTriggerGraph(TriggerGraph trigger)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _entryType = null;
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
            _entryNodeItem = _ui.createTriggerNode(0, 0, game.langManager.getString(StringTable.EDITOR_NODE_NAME_TRIGGER), getTranslatedTargetCheckerTypes(), triggerIndex, getTriggerTargetCheckerViewData());
            _entryNodeItem.setOutputs(getTriggerOutputNames(), _ui.GetTypeListColor());
            if (trigger.action != null)
            {
                var actionNodeItem = await createGraph4ActionNode(trigger.action);
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
