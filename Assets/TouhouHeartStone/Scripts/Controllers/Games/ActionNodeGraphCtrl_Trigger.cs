using TouhouCardEngine;
using System;

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
        private void onClickTriggerEditConditionButton()
        {
            pushCurEditObjectToStack();
            if (_trigger.condition == null)
                _trigger.condition = new ActionValueRef();
            openValueRefGraph(_trigger.condition);
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
        private void onClickEditTargetCheckerButton(int index)
        {
            pushCurEditObjectToStack();
            openTargetCheckerGraph(_trigger.targetCheckerList[index]);
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
        private void openTriggerGraph(TriggerGraph trigger)
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
            //创建入口节点
            _entryNodeItem = _ui.createTriggerNode(0, 0, getTranslatedTargetCheckerTypes(), _trigger.eventName, getTriggerTargetCheckerViewData());
            //创建根节点
            if (trigger.action != null)
            {
                var actionNodeItem = createGraph4ActionNode(trigger.action);
                _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0);
            }
            //创建离散节点
            for (int i = 0; i < graphInfo.freeActionNodeList.Count; i++)
            {
                createGraph4ActionNode(graphInfo.freeActionNodeList[i]);
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
        #endregion
    }
}
