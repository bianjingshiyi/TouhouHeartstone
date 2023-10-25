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
                addTargetChecker(activeEntry);
                addRecordBreakpoint();
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
                removeTargetChecker(activeEntry, index);
                addRecordBreakpoint();
            }
        }
        private void onTargetCheckerTypeValueChanged(ActionNodeItem item, int index, int value)
        {
            var actionNode = getActionNode(item);
            if (actionNode is ActiveEntryNode activeEntry)
            {
                changeTargetCheckerType(activeEntry, index, value);
                addRecordBreakpoint();
            }
        }
        private void onTargetCheckerErrorTipValueChanged(ActionNodeItem item, int index, string value)
        {
            var actionNode = getActionNode(item);
            if (actionNode is ActiveEntryNode activeEntry)
            {
                changeTargetCheckerErrorTip(activeEntry, index, value);
                addRecordBreakpoint();
            }
        }
        #endregion
        #endregion
        private void addTargetChecker(ActiveEntryNode entry)
        {
            entry.AddTargetChecker(new TargetChecker(_targetCheckerTypes.FirstOrDefault() ?? string.Empty, string.Empty));
            addOperationRecord(new AddTargetCheckerOperation(this, entry));
            updateActiveEntryNode(getActionNodeItem(entry), entry);
        }
        private void removeTargetChecker(ActiveEntryNode entry, int index)
        {

            // 重新连接端口。
            List<(ValueOutput, int)> delayedConnections = new List<(ValueOutput, int)>();

            var targetPorts = entry.getTargetConditionPorts();
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
            var removedTargetChecker = entry.targetCheckerList[index];
            entry.RemoveTargetChecker(index);
            addOperationRecord(new RemoveTargetCheckerOperation(this, entry, index, removedTargetChecker));
            updateActiveEntryNode(getActionNodeItem(entry), entry);

            targetPorts = entry.getTargetConditionPorts();
            foreach (var (output, inputindex) in delayedConnections)
            {
                var newInput = targetPorts[inputindex];
                if (newInput == null)
                    continue;
                connectPorts(output, newInput);
            }
        }
        private void changeTargetCheckerType(ActiveEntryNode entry, int index, int value)
        {
            var targetChecker = entry.targetCheckerList[index];
            var beforeValue = targetChecker.targetType;

            targetChecker.targetType = _targetCheckerTypes[value];
            addOperationRecord(new ChangeTargetCheckerTypeOperation(this, entry, index, beforeValue, targetChecker.targetType));
            updateActiveEntryNode(getActionNodeItem(entry), entry);
        }
        private void changeTargetCheckerErrorTip(ActiveEntryNode entry, int index, string value)
        {
            var targetChecker = entry.targetCheckerList[index];
            var beforeValue = targetChecker.errorTip;

            targetChecker.errorTip = value;
            addOperationRecord(new ChangeTargetCheckerErrorTipOperation(this, entry, index, beforeValue, targetChecker.errorTip));
            updateActiveEntryNode(getActionNodeItem(entry), entry);
        }
        private void updateActiveEntryNode(ActionNodeItem item, ActiveEntryNode activeEntry)
        {
            item.updateTargetCheckerList(getTranslatedTargetCheckerTypes(), getActiveEntryTargetCheckerViewData(activeEntry));
            item.setInputs(getActiveEntryInputViewData(activeEntry));
            item.setOutputs(getNodeOutputViewData(activeEntry));
            updateNodePortModes(activeEntry, _connectingPort);
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
        #region 内嵌类
        class AddTargetCheckerOperation : GraphOperation
        {
            public AddTargetCheckerOperation(ActionNodeGraphCtrl graphCtrl, ActiveEntryNode node) : base(graphCtrl)
            {
                _nodeId = node.id;
            }
            public override void undo()
            {
                var node = _graphCtrl.getActionNode(_nodeId);
                if (node is ActiveEntryNode activeEntry)
                {
                    _graphCtrl.removeTargetChecker(activeEntry, activeEntry.targetCheckerList.Count - 1);
                }
            }
            public override void redo()
            {
                var node = _graphCtrl.getActionNode(_nodeId);
                if (node is ActiveEntryNode activeEntry)
                {
                    _graphCtrl.addTargetChecker(activeEntry);
                }
            }
            private int _nodeId;
        }
        class RemoveTargetCheckerOperation : GraphOperation
        {
            public RemoveTargetCheckerOperation(ActionNodeGraphCtrl graphCtrl, ActiveEntryNode node, int index, TargetChecker targetChecker) : base(graphCtrl)
            {
                _nodeId = node.id;
                _removeIndex = index;
                _removedTargetChecker = new SerializableTargetChecker(targetChecker);
            }
            public override void undo()
            {
                var node = _graphCtrl.getActionNode(_nodeId);
                if (node is ActiveEntryNode activeEntry)
                {
                    var checker = _removedTargetChecker.toTargetChecker();
                    activeEntry.InsertTargetChecker(_removeIndex, checker);
                    var item = _graphCtrl.getActionNodeItem(node);
                    _graphCtrl.updateActiveEntryNode(item, activeEntry);
                }
            }
            public override void redo()
            {
                var node = _graphCtrl.getActionNode(_nodeId);
                if (node is ActiveEntryNode activeEntry)
                {
                    _graphCtrl.removeTargetChecker(activeEntry, _removeIndex);
                }
            }
            private int _nodeId;
            private int _removeIndex;
            private SerializableTargetChecker _removedTargetChecker;
        }
        class ChangeTargetCheckerTypeOperation : GraphOperation
        {
            public ChangeTargetCheckerTypeOperation(ActionNodeGraphCtrl graphCtrl, ActiveEntryNode node, int index, string beforeValue, string afterValue) : base(graphCtrl)
            {
                _nodeId = node.id;
                _index = index;
                _beforeValue = beforeValue;
                _afterValue = afterValue;
            }
            public override void undo()
            {
                var node = _graphCtrl.getActionNode(_nodeId);
                if (node is ActiveEntryNode activeEntry)
                {
                    var checker = activeEntry.targetCheckerList[_index];
                    checker.targetType = _beforeValue;
                    var item = _graphCtrl.getActionNodeItem(node);
                    _graphCtrl.updateActiveEntryNode(item, activeEntry);
                }
            }
            public override void redo()
            {
                var node = _graphCtrl.getActionNode(_nodeId);
                if (node is ActiveEntryNode activeEntry)
                {
                    var checker = activeEntry.targetCheckerList[_index];
                    checker.targetType = _afterValue;
                    var item = _graphCtrl.getActionNodeItem(node);
                    _graphCtrl.updateActiveEntryNode(item, activeEntry);
                }
            }
            private int _nodeId;
            private int _index;
            private string _beforeValue;
            private string _afterValue;
        }
        class ChangeTargetCheckerErrorTipOperation : GraphOperation
        {
            public ChangeTargetCheckerErrorTipOperation(ActionNodeGraphCtrl graphCtrl, ActiveEntryNode node, int index, string beforeValue, string afterValue) : base(graphCtrl)
            {
                _nodeId = node.id;
                _index = index;
                _beforeValue = beforeValue;
                _afterValue = afterValue;
            }
            public override void undo()
            {
                var node = _graphCtrl.getActionNode(_nodeId);
                if (node is ActiveEntryNode activeEntry)
                {
                    var checker = activeEntry.targetCheckerList[_index];
                    checker.errorTip = _beforeValue;
                    var item = _graphCtrl.getActionNodeItem(node);
                    _graphCtrl.updateActiveEntryNode(item, activeEntry);
                }
            }
            public override void redo()
            {
                var node = _graphCtrl.getActionNode(_nodeId);
                if (node is ActiveEntryNode activeEntry)
                {
                    var checker = activeEntry.targetCheckerList[_index];
                    checker.errorTip = _afterValue;
                    var item = _graphCtrl.getActionNodeItem(node);
                    _graphCtrl.updateActiveEntryNode(item, activeEntry);
                }
            }
            private int _nodeId;
            private int _index;
            private string _beforeValue;
            private string _afterValue;
        }
        #endregion
    }
}
