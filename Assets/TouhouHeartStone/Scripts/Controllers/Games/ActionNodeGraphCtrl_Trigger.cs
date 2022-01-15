using TouhouCardEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 公有方法
        /// <summary>
        /// 打开一个触发器的动作节点图
        /// </summary>
        /// <param name="trigger"></param>
        public void openTriggerGraph(int cardPoolId, CardDefine cardDefine, TriggerGraph trigger)
        {
            display(new DisplayInfo(cardPoolId, cardDefine, null, trigger, null));
        }
        #endregion
        #region 私有方法
        #region 事件回调
        private void onChangeTriggerTypeIndex(int typeIndex)
        {
            _trigger.eventName = getEventTypeNameByIndex(typeIndex);
            _entryNodeItem.setOutputs(getTriggerOutputNames());
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
            
            //转成index
            var triggerIndex = getEventTypeIndex(_trigger.eventName);

            //显示UI
            _ui.clearGraph();
            //创建入口节点
            _entryNodeItem = _ui.createTriggerNode(0, 0, getTranslatedTargetCheckerTypes(), triggerIndex, getTriggerTargetCheckerViewData());
            //初始化事件选项
            _entryNodeItem.SetEventOptions(_eventTypeInfoList.Select(i => i.eventName).ToList());
            if (trigger.action != null)
            {
                var actionNodeItem = await createGraph4ActionNode(trigger.action);
                _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0);
            }
            //创建离散节点
            for (int i = 0; i < graphInfo.freeActionNodeList.Count; i++)
            {
                await createGraph4ActionNode(graphInfo.freeActionNodeList[i]);
            }
        }
        private (int typeIndex, string name, string errorTip)[] getTriggerTargetCheckerViewData()
        {
            var targetCheckers = new (int typeIndex, string name, string errorTip)[_trigger.targetCheckerList.Count];
            for (int i = 0; i < targetCheckers.Length; i++)
            {
                targetCheckers[i].typeIndex = Array.IndexOf(_targetCheckerTypes, _trigger.targetCheckerList[i].targetType);
                targetCheckers[i].name = "目标" + (i + 1);
                targetCheckers[i].errorTip = _trigger.targetCheckerList[i].errorTip;
            }
            return targetCheckers;
        }
        /// <summary>
        /// 获取当前触发器的输出值的名称。
        /// </summary>
        /// <returns></returns>
        private string[] getTriggerOutputNames()
        {
            int typeIndex = getEventTypeIndex(_trigger.eventName);
            EventTypeInfo eventTypeInfo = _eventTypeInfoList[typeIndex];
            if (eventTypeInfo.variableInfos != null)
            {
                string[] outputNames = new string[eventTypeInfo.variableInfos.Length];
                for (int i = 0; i < outputNames.Length; i++)
                {
                    //爱，这个名字要弄成中文可麻烦了，我总不能把字段的名称都搞成中文吧？
                    //不过考虑到自定义的时候玩家也不可能都写英文，所以这倒也不是不合理。
                    outputNames[i] = game.langManager.getString(eventTypeInfo.variableInfos[i].name);
                }
                return outputNames;
            }
            else
                return null;
        }
        /// <summary>
        /// 加载所有事件类型
        /// </summary>
        private void loadEventTypes()
        {
            _eventTypeInfoList.Clear();
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
            //以后可能有自定义的事件类型，不过先不管
        }
        private int getEventTypeIndex(string eventName)
        {
            return _eventTypeInfoList.FindIndex(e => e.eventName == eventName);
        }
        private string getEventTypeNameByIndex(int index)
        {
            return _eventTypeInfoList[index].eventName;
        }
        #endregion
        #region 属性字段
        List<EventTypeInfo> _eventTypeInfoList = new List<EventTypeInfo>();
        #endregion
    }
    class EventTypeInfo
    {
        public string eventName;
        public EventVariableInfo[] variableInfos;
    }
}
