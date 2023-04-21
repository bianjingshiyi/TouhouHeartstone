using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 私有方法
        #region 事件回调
        #region 目标检查器
        /// <summary>
        /// 当用户点击新增目标按钮，为当前触发器新建目标检查器，然后在UI列表中新增项目
        /// </summary>
        private void onClickAddTargetCheckerButton(ActionNodeItem item)
        {
            var actionNode = getActionNode(item);
            if (actionNode is ActiveEntryNode activeEntry)
            {
                activeEntry.AddTargetChecker(new TargetChecker(string.Empty, string.Empty));
                updateActiveEntryNode(item, activeEntry);
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
            if (actionNode is ActiveEntryNode activeEntry)
            {
                // 重新连接端口。
                List<(ValueOutput, int)> delayedConnections = new List<(ValueOutput, int)>();

                var targetPorts = activeEntry.getTargetConditionPorts();
                for (int i = index; i < targetPorts.Length; i++)
                {
                    var input = targetPorts[i];
                    if (input == null)
                        continue;
                    var connectedOutput = input.getConnectedOutputPort();
                    if (connectedOutput == null)
                        continue;

                    disconnectPorts(connectedOutput, input);

                    if (i > index)
                    {
                        delayedConnections.Add((connectedOutput, i - 1));
                    }
                }
                activeEntry.RemoveTargetChecker(index);
                updateActiveEntryNode(item,activeEntry);

                targetPorts = activeEntry.getTargetConditionPorts();
                foreach (var (output, inputindex) in delayedConnections)
                {
                    var newInput = targetPorts[inputindex];
                    if (newInput == null)
                        continue;
                    connectPorts(output, newInput);
                }
                OnEdited();
            }
        }
        private void onTargetCheckerTypeValueChanged(ActionNodeItem item, int index, int value)
        {
            var actionNode = getActionNode(item);
            if (actionNode is ActiveEntryNode activeEntry)
            {
                activeEntry.targetCheckerList[index].targetType = _targetCheckerTypes[value];
                OnEdited();
            }
        }
        private void onTargetCheckerErrorTipValueChanged(ActionNodeItem item, int index, string value)
        {
            var actionNode = getActionNode(item);
            if (actionNode is ActiveEntryNode activeEntry)
            {
                activeEntry.targetCheckerList[index].errorTip = value;
                OnEdited();
            }
        }
        #endregion
        #endregion
        private void updateActiveEntryNode(ActionNodeItem item, ActiveEntryNode activeEntry)
        {
            item.updateTargetCheckerList(getTranslatedTargetCheckerTypes(), getActiveEntryTargetCheckerViewData(activeEntry));
            item.setInputs(getActiveEntryInputViewData(activeEntry));
            item.setOutputs(getNodeOutputViewData(activeEntry));
        }
        private PortItemInfo[] getActiveEntryInputViewData(ActiveEntryNode node)
        {
            List<PortItemInfo> viewDataList = new List<PortItemInfo>();
            //值输入
            var conditionPorts = node.getInputPorts<ValueInput>();
            foreach (var port in conditionPorts)
            {
                // 翻译输出节点
                var inputName = game.lang._(port.define.getDisplayName(), port.paramIndex + 1);
                viewDataList.Add(new PortItemInfo(inputName, game.editor.getColorByType(port.define.type), port.GetPortType() == PortType.Control));
            }
            return viewDataList.ToArray();
        }
		private (int typeIndex, string name, string errorTip)[] getActiveEntryTargetCheckerViewData(ActiveEntryNode activeEntry)
        {
            var targetList = activeEntry.targetCheckerList;
            var viewDatas = new (int typeIndex, string name, string errorTip)[targetList.Count];
            for (int i = 0; i < viewDatas.Length; i++)
            {
                var targetChecker = targetList[i];
                var viewData = viewDatas[i];
                viewData.typeIndex = Array.IndexOf(_targetCheckerTypes, targetChecker.targetType);
                if (viewData.typeIndex < 0)
                {
                    targetChecker.targetType = DEFINE.TARGET_TYPE_SERVANT;
                    viewData.typeIndex = Array.IndexOf(_targetCheckerTypes, DEFINE.TARGET_TYPE_SERVANT);
                }
                viewData.name = "目标" + (i + 1);
                viewData.errorTip = targetList[i].errorTip;
                viewDatas[i] = viewData;
            }
            return viewDatas;
        }
        #endregion
    }
}
