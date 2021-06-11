using System;
using System.Linq;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using BJSYGameCore;
using System.Reflection;
using MukioI18n;
using Game;

namespace Controllers
{
    partial class ActionNodeGraphCtrl : IController<MainCtrl>
    {
        #region 公有方法
        public ActionNodeGraphCtrl(IAppManager app, MainCtrl main, ActionNodeGraphPanel compo)
        {
            this.app = app;
            this.main = main;
            _compo = compo;
            _actionDefineDict = ActionDefine.loadDefinesFromAssemblies(new Assembly[] { typeof(CardEngine).Assembly, typeof(HearthStoneRule).Assembly });
            _backgroundMenuOptions = _actionDefineDict.Keys.ToArray();
            _compo.onClickBackground += onClickBackground;
            _compo.onLongPressBackground += onLongPressBackground;
            _compo.onClickMenuItem += onClickMenuItem;
            _compo.onConnectNodes += onConnectNodes;
            _compo.onLongPressActionNode += onLongPressActionNode;
            _compo.onActionNodeValueChange += onActionNodeValueChange;
            _compo.onChangeTriggerEventName += onChangeTriggerEventName;
            _compo.onClickTriggerEditConditionButton += onClickTriggerEditConditionButton;
            _compo.onClickAddTargetCheckerButton += onClickAddTargetCheckerButton;
            _compo.onClickEditTargetCheckerButton += onClickEditTargetCheckerButton;
            _compo.onClickRemoveTargetCheckerButton += onClickRemoveTargetCheckerButton;
            _compo.onClickReturnButton += onClickReturnButton;
        }
        /// <summary>
        /// 打开一个没有事件参数且没有输出的动作节点
        /// </summary>
        /// <param name="entryName"></param>
        /// <param name="actionNode"></param>
        /// <param name="graphInfo"></param>
        public void openActionGraph(string entryName, ActionNode actionNode, ActionNodeGraphInfo[] graphInfos)
        {
            _editObjectStack.Clear();
            _graphInfos = graphInfos;
            openActionGraph(entryName, actionNode);
        }
        /// <summary>
        /// 打开一个触发器的动作节点图
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="graphInfos"></param>
        public void openTriggerGraph(TriggerGraph trigger, ActionNodeGraphInfo[] graphInfos)
        {
            _editObjectStack.Clear();
            _graphInfos = graphInfos;
            openTriggerGraph(trigger, graphInfos);
        }
        #endregion
        #region 私有方法
        void openActionGraph(string entryName, ActionNode actionNode)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _compo.clearGraph();
            ActionNodeGraphInfo graphInfo = _graphInfos.First(g => g.rootActionNodeId == actionNode.id);
            _entryName = entryName;
            _entryNodeItem = _compo.createNode(graphInfo.entryInfo.posX, graphInfo.entryInfo.posY, _entryName,
                new string[0],
                new ActionNodeValueInfo[0],
                new string[] { string.Empty });
            _rootActionNode = actionNode;
            _valueRef = null;
            _targetChecker = null;
            _trigger = null;
            var actionNodeItem = createGraph4ActionNode(actionNode, graphInfo);
            _compo.connectNode(_entryNodeItem, 0, actionNodeItem, 0);
        }
        /// <summary>
        /// 打开一个有输出的值引用动作节点
        /// </summary>
        /// <param name="entryName"></param>
        /// <param name="valueRef"></param>
        /// <param name="graphInfo"></param>
        void openValueRefGraph(ActionValueRef valueRef)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _compo.clearGraph();
            _entryName = null;
            _rootActionNode = null;
            _valueRef = valueRef;
            _targetChecker = null;
            _trigger = null;
            ActionNodeGraphInfo graphInfo = _graphInfos.First(g => g.rootActionNodeId == valueRef.action.id);
            var actionNodeItem = createGraph4ActionNode(valueRef.action, graphInfo);
            _outputNodeItem = _compo.createNode(graphInfo.outputInfo.posX, graphInfo.outputInfo.posY, app.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT),
                new string[] { string.Empty },
                new ActionNodeValueInfo[0],
                new string[0]);
            _compo.connectNode(actionNodeItem, valueRef.index + 1, _outputNodeItem, 0);
        }
        void openTargetCheckerGraph(TargetChecker targetChecker)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _compo.clearGraph();
            _entryName = null;
            _rootActionNode = null;
            _valueRef = null;
            _targetChecker = targetChecker;
            _trigger = null;
            ActionNodeGraphInfo graphInfo = _graphInfos.First(g => g.rootActionNodeId == targetChecker.condition.action.id);
            var actionNodeItem = createGraph4ActionNode(targetChecker.condition.action, graphInfo);
            _outputNodeItem = _compo.createNode(graphInfo.outputInfo.posX, graphInfo.outputInfo.posY, app.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT),
                new string[] { string.Empty },
                new ActionNodeValueInfo[]
                {
                    new ActionNodeValueInfo()
                    {
                        name = app.langManager.getString(StringTable.EDITOR_VALUE_NAME_TARGET_TYPE),
                        type = ActionNodeValueType.text,
                        value = _targetChecker.targetType
                    },
                    new ActionNodeValueInfo()
                    {
                        name = app.langManager.getString(StringTable.EDITOR_VALUE_NAME_INVALID_MSG),
                        type = ActionNodeValueType.text,
                        value = _targetChecker.invalidMsg
                    }
                },
                new string[0]);
            _compo.connectNode(actionNodeItem, valueRef.index + 1, _outputNodeItem, 0);
        }
        void openTriggerGraph(TriggerGraph trigger)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _compo.clearGraph();
            _entryName = null;
            _rootActionNode = null;
            _valueRef = null;
            _targetChecker = null;
            _trigger = trigger;
            ActionNodeGraphInfo graphInfo = _graphInfos.First(g => g.rootActionNodeId == trigger.action.id);
            var actionNodeItem = createGraph4ActionNode(trigger.action, graphInfo);
            _entryNodeItem = _compo.createTriggerNode(graphInfo.entryInfo.posX, graphInfo.entryInfo.posY, _trigger.eventName, _trigger.targetCheckerList.Count);
            _compo.connectNode(_entryNodeItem, 0, actionNodeItem, 0);
        }
        /// <summary>
        /// 为动作节点以及其所有子节点创建动作节点图
        /// </summary>
        /// <param name="actionNode"></param>
        /// <param name="graphInfo"></param>
        ActionNodeItem createGraph4ActionNode(ActionNode actionNode, ActionNodeGraphInfo graphInfo)
        {
            _actionNodeList.Add(actionNode);
            var actionDefine = _actionDefineDict[actionNode.defineName];
            var actionNodeItem = _compo.createNode(
                graphInfo.actionInfoDict[actionNode.id].posX,
                graphInfo.actionInfoDict[actionNode.id].posY,
                getActionNodeName(actionNode),
                getActionNodeInputNames(actionDefine),
                getActionNodeConstValueInfo(actionNode),
                getActionDefineOutputNames(actionDefine));
            _actionNodeItemList.Add(actionNodeItem);
            //为动作分支创建节点图
            for (int i = 0; i < actionNode.branches.Length; i++)
            {
                var nextActionNode = actionNode.branches[i];
                if (nextActionNode == null)
                    continue;
                var nextActionNodeItem = createGraph4ActionNode(nextActionNode, graphInfo);
                _compo.connectNode(actionNodeItem, i, nextActionNodeItem, 0);
            }
            //为输入节点创建节点图
            for (int i = 0; i < actionNode.inputs.Length; i++)
            {
                var valueRef = actionNode.inputs[i];
                if (valueRef == null)
                    continue;
                var inputActionNodeItem = createGraph4ActionNode(valueRef.action, graphInfo);
                //输出索引要被动作分支占用若干个空位
                //输入索引要被前置动作占用一个空位
                _compo.connectNode(inputActionNodeItem, valueRef.action.branches.Length + valueRef.index, actionNodeItem, 1 + i);
            }
            return actionNodeItem;
        }
        /// <summary>
        /// 在指定位置上为动作节点创建节点图，不包括它的子节点。
        /// </summary>
        /// <param name="actionNode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        ActionNodeItem createGraph4ActionNodeAt(ActionNode actionNode, float posX, float posY)
        {
            _actionNodeList.Add(actionNode);
            var actionDefine = _actionDefineDict[actionNode.defineName];
            var actionNodeItem = _compo.createNode(
                posX,
                posY,
                getActionNodeName(actionNode),
                getActionNodeInputNames(actionDefine),
                getActionNodeConstValueInfo(actionNode),
                getActionDefineOutputNames(actionDefine));
            _actionNodeItemList.Add(actionNodeItem);
            return actionNodeItem;
        }
        ActionNodeItem getActionNodeItem(ActionNode actionNode)
        {
            int index = _actionNodeList.IndexOf(actionNode);
            return _actionNodeItemList[index];
        }
        ActionNode getActionNode(ActionNodeItem actionNodeItem)
        {
            int index = _actionNodeItemList.IndexOf(actionNodeItem);
            return _actionNodeList[index];
        }
        /// <summary>
        /// 将当前正在编辑的对象入栈
        /// </summary>
        void pushCurEditObjectToStack()
        {
            if (_trigger != null)
                _editObjectStack.Push(new DisplayInfo() { entryName = null, editObject = _trigger });
            else if (_targetChecker != null)
                _editObjectStack.Push(new DisplayInfo() { entryName = null, editObject = _targetChecker });
            else if (_valueRef != null)
                _editObjectStack.Push(new DisplayInfo() { entryName = null, editObject = _valueRef });
            else
                _editObjectStack.Push(new DisplayInfo() { entryName = _entryName, editObject = _rootActionNode });
            _compo.setReturnButtonActive(true);
        }
        void popPrevEditObjectFromStackAndOpen()
        {
            DisplayInfo info = _editObjectStack.Pop();
            if (_editObjectStack.Count < 1)
                _compo.setReturnButtonActive(false);
            if (info.editObject is TriggerGraph trigger)
                openTriggerGraph(trigger);
            else if (info.editObject is TargetChecker targetChecker)
                openTargetCheckerGraph(targetChecker);
            else if (info.editObject is ActionValueRef valueRef)
                openValueRefGraph(valueRef);
            else if (info.editObject is ActionNode actionNode)
                openActionGraph(info.entryName, actionNode);
        }
        string getActionNodeName(ActionNode actionNode)
        {
            //TODO:这里应该考虑动作节点名称的翻译，但是暂时先不这么做。
            return actionNode.defineName;
        }
        string[] getActionNodeInputNames(ActionDefine actionDefine)
        {
            //TODO:这里应该考虑动作节点输入的翻译，但是暂时先不这么做。
            string[] inputNames = new string[actionDefine.inputs.Length + 1];
            inputNames[0] = string.Empty;
            for (int i = 0; i < actionDefine.inputs.Length; i++)
            {
                inputNames[i + 1] = actionDefine.inputs[i].name;
            }
            return inputNames;
        }
        ActionNodeValueInfo[] getActionNodeConstValueInfo(ActionNode actionNode)
        {
            var actionDefine = _actionDefineDict[actionNode.defineName];
            var infoArray = new ActionNodeValueInfo[actionDefine.consts.Length];
            for (int i = 0; i < infoArray.Length; i++)
            {
                infoArray[i] = new ActionNodeValueInfo()
                {
                    //TODO:这里应该考虑动作节点常量值的翻译，但是暂时先不这么做。
                    name = actionDefine.consts[i].name,
                    value = actionNode.consts[i]
                };
                Type type = actionDefine.consts[i].type;
                if (type == typeof(int))
                    infoArray[i].type = ActionNodeValueType.integer;
                else if (type == typeof(float))
                    infoArray[i].type = ActionNodeValueType.real;
                else if (type == typeof(bool))
                    infoArray[i].type = ActionNodeValueType.boolean;
                else if (type == typeof(string))
                    infoArray[i].type = ActionNodeValueType.text;
            }
            return infoArray;
        }
        string[] getActionDefineOutputNames(ActionDefine actionDefine)
        {
            //TODO:这里应该考虑动作节点输出的翻译，但是暂时先不这么做。
            return actionDefine.outputs.Select(o => o.name).ToArray();
        }
        #region UI回调
        /// <summary>
        /// 当用户长按背景，展开菜单。目前点菜单内容只有创建节点。
        /// </summary>
        void onLongPressBackground(float posX, float posY)
        {
            _curPosX = posX;
            _curPosY = posY;
            _compo.openMenu(posX, posY, _backgroundMenuOptions);
        }
        /// <summary>
        /// 当用户点击背景，隐藏菜单。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        void onClickBackground(float posX, float posY)
        {
            _compo.closeMenu();
        }
        /// <summary>
        /// 当用户点击菜单项，视具体点击的项目决定干什么
        /// </summary>
        /// <param name="index"></param>
        void onClickMenuItem(int index)
        {
            if (_curActionNode != null)
            {
                //节点菜单
                if (index == 0)//删除
                {
                    int actionNodeIndex = _actionNodeList.IndexOf(_curActionNode);
                    var actionNodeItem = _actionNodeItemList[actionNodeIndex];
                    var actionNode = _curActionNode;
                    //断开节点和所有其他节点的连接
                    for (int i = 0; i < _actionNodeList.Count; i++)
                    {
                        //断开前一个动作
                        int branchNodeIndex = Array.IndexOf(_actionNodeList[i].branches, actionNode);
                        if (branchNodeIndex >= 0)
                        {
                            _actionNodeList[i].branches[branchNodeIndex] = null;
                            _compo.unconnectNode(_actionNodeItemList[i], branchNodeIndex, actionNodeItem, 0);
                        }
                        //断开输入
                        int inputValueRefIndex = Array.FindIndex(actionNode.inputs, v => v.action == _actionNodeList[i]);
                        if (inputValueRefIndex >= 0)
                        {
                            var inputActionNode = _actionNodeList[i];
                            var inputValueRef = actionNode.inputs[inputValueRefIndex];
                            actionNode.inputs[inputValueRefIndex] = null;
                            _compo.unconnectNode(_actionNodeItemList[i], inputActionNode.branches.Length + inputValueRef.index, actionNodeItem, 1 + inputValueRefIndex);
                        }
                        //断开后一个动作
                        branchNodeIndex = Array.IndexOf(actionNode.branches, _actionNodeList[i]);
                        if (branchNodeIndex >= 0)
                        {
                            actionNode.branches[branchNodeIndex] = null;
                            _compo.unconnectNode(actionNodeItem, branchNodeIndex, _actionNodeItemList[i], 0);
                        }
                        //断开输出
                        inputValueRefIndex = Array.FindIndex(_actionNodeList[i].inputs, v => v.action == actionNode);
                        if (inputValueRefIndex >= 0)
                        {
                            var inputValueRef = _actionNodeList[i].inputs[inputValueRefIndex];
                            _actionNodeList[i].inputs[inputValueRefIndex] = null;
                            _compo.unconnectNode(actionNodeItem, actionNode.branches.Length + inputValueRef.index, _actionNodeItemList[i], 1 + inputValueRefIndex);
                        }
                    }
                    //从列表中移除
                    _actionNodeList.RemoveAt(index);
                    _actionNodeItemList.RemoveAt(index);
                }
            }
            else
            {
                //背景菜单
                string option = _backgroundMenuOptions[index];
                //目前只能创建节点
                var actionDefine = _actionDefineDict[option];
                var actionNode = new ActionNode(option, new ActionValueRef[actionDefine.inputs.Length], new object[actionDefine.consts.Length], new ActionNode[1]);
                createGraph4ActionNodeAt(actionNode, _curPosX, _curPosY);
            }
        }
        /// <summary>
        /// 当用户连接节点，检查一下连接的合法性，如果合法那么连接，否则提示。
        /// </summary>
        /// <param name="outputActionNodeItem"></param>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNodeItem"></param>
        /// <param name="inputIndex"></param>
        void onConnectNodes(ActionNodeItem outputActionNodeItem, int outputIndex, ActionNodeItem inputActionNodeItem, int inputIndex)
        {
            if (outputActionNodeItem == _entryNodeItem)
            {
                ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                //入口节点更加特殊，它只有一个分支需要检查
                if (rootActionNode != null)
                {
                    rootActionNode = null;
                    _compo.unconnectNode(_entryNodeItem, 0, getActionNodeItem(rootActionNode), 0);
                }
                //如果输入已经和其他节点输出连接了的话，先断开连接
                ActionValueRef outputValueRef = inputActionNode.inputs[inputIndex - inputActionNode.branches.Length];
                if (outputValueRef != null)
                {
                    var actionNode = outputValueRef.action;
                    actionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                    var actionNodeItem = getActionNodeItem(actionNode);
                    _compo.unconnectNode(actionNodeItem, actionNode.branches.Length + outputValueRef.index, inputActionNodeItem, inputIndex);
                }
                //检查合法性
                //入口节点只有一个输出节点
                if (outputIndex != 0 || inputIndex != 0)
                {
                    //值输出只能与值输入相互连接
                    _compo.showErrorTip(app.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                    return;
                }
                rootActionNode = inputActionNode;
            }
            else if (inputActionNodeItem == _outputNodeItem)
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                //出口节点也只有一个参数
                if (rootActionNode != null)
                {
                    rootActionNode = null;
                    _compo.unconnectNode(getActionNodeItem(rootActionNode), valueRef.index + 1, _outputNodeItem, 0);
                }
                //检查合法性
                //输出值节点只有一个输出节点
                if (outputIndex == 0)
                {
                    //值输出只能与值输入相互连接
                    _compo.showErrorTip(app.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                    return;
                }
                rootActionNode = outputActionNode;
                valueRef.index = outputIndex - 1;
            }
            else
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                //为了避免逻辑上的问题，目前节点图规定一个节点只能有一个输出。
                //当你尝试将输出已经连接的节点连接到另外一个节点的时候，自动断开当前连接再连接到新的节点上。
                for (int i = 0; i < _actionNodeList.Count; i++)
                {
                    var actionNode = _actionNodeList[i];
                    //断开和分支动作的连接
                    int branchNodeIndex = Array.IndexOf(outputActionNode.branches, actionNode);
                    if (branchNodeIndex >= 0)
                    {
                        var actionNodeItem = _actionNodeItemList[i];
                        outputActionNode.branches[branchNodeIndex] = null;
                        _compo.unconnectNode(outputActionNodeItem, branchNodeIndex, actionNodeItem, 0);
                    }
                    //断开输出值引用的连接
                    int valueRefIndex = Array.FindIndex(actionNode.inputs, v => v.action == outputActionNode);
                    if (valueRefIndex >= 0)
                    {
                        var actionNodeItem = _actionNodeItemList[i];
                        actionNode.inputs[valueRefIndex] = null;
                        _compo.unconnectNode(outputActionNodeItem, actionNode.inputs[valueRefIndex].index, actionNodeItem, actionNode.branches.Length + valueRefIndex);
                    }
                }
                //如果输入已经和其他节点输出连接了的话，先断开连接
                ActionValueRef outputValueRef = inputActionNode.inputs[inputIndex - inputActionNode.branches.Length];
                if (outputValueRef != null)
                {
                    var actionNode = outputValueRef.action;
                    actionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                    var actionNodeItem = getActionNodeItem(actionNode);
                    _compo.unconnectNode(actionNodeItem, actionNode.branches.Length + outputValueRef.index, inputActionNodeItem, inputIndex);
                }
                //检查合法性
                var inputActionDefine = _actionDefineDict[inputActionNode.defineName];
                var inputValueDefine = inputActionDefine.inputs[inputIndex - 1];
                var outputActionDefine = _actionDefineDict[outputActionNode.defineName];
                var outputValueDefine = outputActionDefine.outputs[outputIndex - outputActionNode.branches.Length];
                if ((outputIndex < outputActionNode.branches.Length) != (inputIndex == 0))
                {
                    //值输出只能与值输入相互连接
                    _compo.showErrorTip(app.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                    return;
                }
                if (!inputValueDefine.type.IsAssignableFrom(outputValueDefine.type) && !outputValueDefine.type.IsAssignableFrom(inputValueDefine.type))
                {
                    //相互连接的输入输出类型必须可以相互转化
                    _compo.showErrorTip(app.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                    return;
                }
                inputActionNode.inputs[inputIndex - 1] = new ActionValueRef(outputActionNode, outputIndex - outputActionNode.branches.Length);
            }
            _compo.connectNode(outputActionNodeItem, outputIndex, inputActionNodeItem, inputIndex);
        }
        /// <summary>
        /// 当用户长按节点，展开节点菜单。
        /// </summary>
        /// <param name="actionNodeItem"></param>
        void onLongPressActionNode(ActionNodeItem actionNodeItem)
        {
            //入口节点不能通过长按编辑
            if (actionNodeItem == _entryNodeItem)
                return;
            //出口节点也不行
            if (actionNodeItem == _outputNodeItem)
                return;
            _curActionNode = getActionNode(actionNodeItem);
            actionNodeItem.getPos(out float posX, out float posY);
            _compo.openMenu(posX, posY, new string[]
            {
                app.langManager.getString(StringTable.DELETE),//删除
            });
        }
        /// <summary>
        /// 当用户更改节点的常量值，设置对应的常量。
        /// </summary>
        /// <param name="actionNodeItem"></param>
        /// <param name="constIndex"></param>
        /// <param name="value"></param>
        void onActionNodeValueChange(ActionNodeItem actionNodeItem, int constIndex, object value)
        {
            if (_targetChecker != null && actionNodeItem == _outputNodeItem)
            {
                if (constIndex == 0)
                    _targetChecker.targetType = (string)value;
                else if (constIndex == 1)
                    _targetChecker.invalidMsg = (string)value;
            }
            else
            {
                var actionNode = getActionNode(actionNodeItem);
                actionNode.consts[constIndex] = value;
            }
        }
        /// <summary>
        /// 当用户更改触发器的事件名称，将事件名称改为更改值
        /// </summary>
        /// <param name="eventName"></param>
        void onChangeTriggerEventName(string eventName)
        {
            _trigger.eventName = eventName;
        }
        /// <summary>
        /// 当用户点击编辑条件按钮，将当前的编辑信息入栈，打开条件编辑
        /// </summary>
        void onClickTriggerEditConditionButton()
        {
            pushCurEditObjectToStack();
            openValueRefGraph(_trigger.condition);
        }
        /// <summary>
        /// 当用户点击新增目标按钮，为当前触发器新建目标检查器，然后在UI列表中新增项目
        /// </summary>
        void onClickAddTargetCheckerButton()
        {
            _trigger.targetCheckerList.Add(new TargetChecker(string.Empty, new ActionValueRef(null, 0), string.Empty));
            _entryNodeItem.addTargetCheckerItem();
        }
        /// <summary>
        /// 当用户点击编辑目标按钮，将当前的编辑信息入栈，打开目标编辑
        /// </summary>
        /// <param name="index"></param>
        void onClickEditTargetCheckerButton(int index)
        {
            pushCurEditObjectToStack();
            openTargetCheckerGraph(_trigger.targetCheckerList[index]);
        }
        /// <summary>
        /// 当用户点击删除目标按钮，为当前触发器删除目标检查器，然后在UI列表中也删除对应索引项目
        /// </summary>
        /// <param name="index"></param>
        void onClickRemoveTargetCheckerButton(int index)
        {
            _trigger.targetCheckerList.RemoveAt(index);
            _entryNodeItem.removeTargetCheckerItemAt(index);
        }
        /// <summary>
        /// 当用户点击返回按钮，出栈之前的显示信息并重新打开之前的编辑界面
        /// </summary>
        void onClickReturnButton()
        {
            popPrevEditObjectFromStackAndOpen();
        }
        #endregion
        #endregion
        public IAppManager app { get; }
        public MainCtrl main { get; }
        ActionNodeGraphPanel _compo;
        Dictionary<string, ActionDefine> _actionDefineDict;
        string[] _backgroundMenuOptions;
        float _curPosX;
        float _curPosY;
        readonly List<ActionNode> _actionNodeList = new List<ActionNode>();
        readonly List<ActionNodeItem> _actionNodeItemList = new List<ActionNodeItem>();
        ActionNodeGraphInfo[] _graphInfos;
        string _entryName;
        ActionNodeItem _entryNodeItem;
        ActionNodeItem _outputNodeItem;
        ActionNode rootActionNode
        {
            get
            {
                if (_trigger != null)
                    return _trigger.action;
                if (valueRef != null)
                    return valueRef.action;
                return _rootActionNode;
            }
            set
            {
                if (_trigger != null)
                    _trigger.action = value;
                else if (valueRef != null)
                    valueRef.action = value;
                else
                    _rootActionNode = value;
            }
        }
        ActionNode _rootActionNode;
        ActionValueRef valueRef
        {
            get
            {
                if (_targetChecker != null)
                    return _targetChecker.condition;
                return _valueRef;
            }
        }
        ActionValueRef _valueRef;
        TargetChecker _targetChecker;
        TriggerGraph _trigger;
        ActionNode _curActionNode;
        readonly Stack<DisplayInfo> _editObjectStack = new Stack<DisplayInfo>();
        class DisplayInfo
        {
            public string entryName;
            public object editObject;
        }
    }
    [Serializable]
    class ActionNodeGraphInfo
    {
        public int rootActionNodeId;
        public ActionNodeGraphNodeInfo entryInfo;
        public ActionNodeGraphNodeInfo outputInfo;
        public Dictionary<int, ActionNodeGraphNodeInfo> actionInfoDict;
    }
    [Serializable]
    class ActionNodeGraphNodeInfo
    {
        public float posX;
        public float posY;
    }
}
