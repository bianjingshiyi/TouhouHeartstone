using System;
using System.Linq;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using BJSYGameCore;
using System.Reflection;
using Game;
using UnityEngine;

namespace Controllers
{
    partial class ActionNodeGraphCtrl : GameObjectCtrl<MainSceneCtrl>, IController<MainCtrl>
    {
        #region 公有方法
        //public ActionNodeGraphCtrl(IAppManager app, MainCtrl main, ActionNodeGraphPanel compo)
        //{
        //    this.app = app;
        //    this.main = main;
        //    _ui = compo;
        //    _actionDefineDict = ActionDefine.loadDefinesFromAssemblies(new Assembly[] { typeof(CardEngine).Assembly, typeof(HearthStoneRule).Assembly });
        //    _backgroundMenuOptions = _actionDefineDict.Keys.ToArray();
        //    _ui.onClickBackground += onClickBackground;
        //    _ui.onLongPressBackground += onLongPressBackground;
        //    _ui.onClickMenuItem += onClickMenuItem;
        //    _ui.onConnectNodes += onConnectNodes;
        //    _ui.onLongPressActionNode += onLongPressActionNode;
        //    _ui.onActionNodeValueChange += onActionNodeValueChange;
        //    _ui.onChangeTriggerEventName += onChangeTriggerEventName;
        //    _ui.onClickTriggerEditConditionButton += onClickTriggerEditConditionButton;
        //    _ui.onClickAddTargetCheckerButton += onClickAddTargetCheckerButton;
        //    _ui.onClickEditTargetCheckerButton += onClickEditTargetCheckerButton;
        //    _ui.onClickRemoveTargetCheckerButton += onClickRemoveTargetCheckerButton;
        //    _ui.onClickReturnButton += onClickReturnButton;
        //}
        /// <summary>
        /// 打开一个效果的生效效果动作图。
        /// </summary>
        /// <param name="effect"></param>
        /// 
        public void openOnEnableActionGraph(GeneratedEffect effect)
        {
            _ui.gameObject.SetActive(true);
            _editObjectStack.Clear();
            openActionGraph(effect, EntryType.onEnable);
        }
        /// <summary>
        /// 打开一个效果的失效效果动作图。
        /// </summary>
        /// <param name="effect"></param>
        /// 
        public void openOnDisableActionGraph(GeneratedEffect effect)
        {
            _ui.gameObject.SetActive(true);
            _editObjectStack.Clear();
            openActionGraph(effect, EntryType.onDisable);
        }
        /// <summary>
        /// 打开一个触发器的动作节点图
        /// </summary>
        /// <param name="trigger"></param>
        public void openTriggerGraph(TriggerGraph trigger)
        {
            _ui.gameObject.SetActive(true);
            _editObjectStack.Clear();

            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _ui.clearGraph();
            _entryType = EntryType.none;
            _effect = null;
            _valueRef = null;
            _targetChecker = null;
            _trigger = trigger;
            //创建入口节点
            _entryNodeItem = _ui.createTriggerNode(0, 0, _trigger.eventName, _trigger.targetCheckerList.Count);
            if (_graphInfo != null)
            {
                //创建根节点
                if (trigger.action != null)
                {
                    var actionNodeItem = createGraph4ActionNode(trigger.action);
                    _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0);
                }
                //创建离散节点
            }
            if (trigger.action != null)
            {
                var actionNodeItem = createGraph4ActionNode(trigger.action);
                _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0);
            }
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponent<ActionNodeGraphPanel>();
            }
            _actionDefineDict = ActionDefine.loadDefinesFromAssemblies(new Assembly[] { typeof(CardEngine).Assembly, typeof(HearthStoneRule).Assembly });
            _backgroundMenuOptions = _actionDefineDict.Keys.ToArray();
            _ui.onClickBackground += onClickBackground;
            _ui.onLongPressBackground += onLongPressBackground;
            _ui.onClickMenuItem += onClickMenuItem;
            _ui.onConnectNodes += onConnectNodes;
            _ui.onUnconnectNodes += onUnconnectNodes;
            _ui.onClickActionNode += onClickActionNode;
            _ui.onLongPressActionNode += onLongPressActionNode;
            _ui.onActionNodeValueChange += onActionNodeValueChange;
            _ui.onChangeTriggerEventName += onChangeTriggerEventName;
            _ui.onClickTriggerEditConditionButton += onClickTriggerEditConditionButton;
            _ui.onClickAddTargetCheckerButton += onClickAddTargetCheckerButton;
            _ui.onClickEditTargetCheckerButton += onClickEditTargetCheckerButton;
            _ui.onClickRemoveTargetCheckerButton += onClickRemoveTargetCheckerButton;
            _ui.onClickReturnButton += onClickReturnButton;
            _ui.onActionNodeValueChange += onActionNodeValueChange;
        }
        #endregion
        /// <summary>
        /// 打开某个效果的生效或者失效动作图
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="entryType"></param>
        void openActionGraph(GeneratedEffect effect, EntryType entryType)
        {
            //清理数据
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _ui.clearGraph();
            //缓存数据
            _entryType = entryType;
            _effect = effect;
            _valueRef = null;
            _targetChecker = null;
            _trigger = null;
            if (_entryType == EntryType.onEnable)
            {
                if (_effect.onEnableAction != null)
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONENABLE),
                        new string[0],
                        new ActionNodeValueInfo[0],
                        new string[] { string.Empty });
                    var actionNodeItem = createGraph4ActionNode(_effect.onEnableAction);
                    _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0);
                }
                else
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONENABLE),
                        new string[0],
                        new ActionNodeValueInfo[0],
                        new string[] { string.Empty });
                }
            }
            else if (entryType == EntryType.onDisable)
            {
                if (_effect.onDisableAction != null)
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONDISABLE),
                        new string[0],
                        new ActionNodeValueInfo[0],
                        new string[] { string.Empty });
                    var actionNodeItem = createGraph4ActionNode(_effect.onDisableAction);
                    _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0);
                }
                else
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONDISABLE),
                        new string[0],
                        new ActionNodeValueInfo[0],
                        new string[] { string.Empty });
                }
            }
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
            _ui.clearGraph();
            _entryType = EntryType.none;
            _effect = null;
            _valueRef = valueRef;
            _targetChecker = null;
            _trigger = null;
            var actionNodeItem = createGraph4ActionNode(valueRef.action);
            _returnNodeItem = _ui.createNode(0, 0,
                game.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT),
                new string[] { string.Empty },
                new ActionNodeValueInfo[0],
                new string[0]);
            _ui.connectNode(actionNodeItem, valueRef.index + 1, _returnNodeItem, 0);
        }
        void openTargetCheckerGraph(TargetChecker targetChecker)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _ui.clearGraph();
            _entryType = EntryType.none;
            _effect = null;
            _valueRef = null;
            _targetChecker = targetChecker;
            _trigger = null;
            var actionNodeItem = createGraph4ActionNode(targetChecker.condition.action);
            _returnNodeItem = _ui.createNode(0, 0,
                game.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT),
                new string[] { string.Empty },
                new ActionNodeValueInfo[]
                {
                    new ActionNodeValueInfo()
                    {
                        name = game.langManager.getString(StringTable.EDITOR_VALUE_NAME_TARGET_TYPE),
                        type = ActionNodeValueType.text,
                        value = _targetChecker.targetType
                    },
                    new ActionNodeValueInfo()
                    {
                        name = game.langManager.getString(StringTable.EDITOR_VALUE_NAME_INVALID_MSG),
                        type = ActionNodeValueType.text,
                        value = _targetChecker.invalidMsg
                    }
                },
                new string[0]);
            _ui.connectNode(actionNodeItem, valueRef.index + 1, _returnNodeItem, 0);
        }
        /// <summary>
        /// 为动作节点以及其所有子节点创建动作节点图
        /// </summary>
        /// <param name="actionNode"></param>
        ActionNodeItem createGraph4ActionNode(ActionNode actionNode, ActionNodeItem prevNodeItem = null, ActionNodeItem nextNodeItem = null, float startPosOffset = 0, Dictionary<ActionNode, Vector2> sizeDict = null)
        {
            _actionNodeList.Add(actionNode);
            var actionDefine = _actionDefineDict[actionNode.defineName];
            ActionNodeItem actionNodeItem;
            ActionNodeGraphNodeInfo nodeInfo =
                _graphInfo != null && _graphInfo.actionInfoDict.ContainsKey(actionNode.id) ?
                _graphInfo.actionInfoDict[actionNode.id] : null;
            Vector2 pos;
            if (nodeInfo != null)
            {
                //有图信息，从图中取位置
                pos = new Vector2(nodeInfo.posX, nodeInfo.posY);
            }
            else if (prevNodeItem != null)
            {
                //没有图信息，自动计算位置。位置和上一个节点以及其他子节点的位置有关，同时还要计算所有子节点在这个排布规则下的尺寸。
                //暂时先不做那么复杂的自动布局计算……分支节点基本上都是下一个动作，直接接在当前动作屁股后面就行。中间间隔参数的嵌套层数的宽度
                //输入参数的高度直接往下排就完事了。
                if (sizeDict == null)
                    sizeDict = new Dictionary<ActionNode, Vector2>();
                Vector2 size = calcActionNodeGraphSize(actionNode, sizeDict);
                pos = new Vector2(prevNodeItem.posX + size.x, startPosOffset);
            }
            else if (nextNodeItem != null)
            {
                if (sizeDict == null)
                    sizeDict = new Dictionary<ActionNode, Vector2>();
                Vector2 size = calcActionNodeGraphSize(actionNode, sizeDict);
                pos = new Vector2(prevNodeItem.posX - _ui.autoPosSpacingWidth, startPosOffset);
            }
            else
            {
                //没有信息也没有前置节点，那我也不知道该放在哪里了。
                pos = new Vector2(0, 0);
            }
            actionNodeItem = _ui.createNode(pos.x, pos.y,
                getActionNodeName(actionNode),
                getActionNodeInputNames(actionDefine),
                getActionNodeConstValueInfo(actionNode),
                getActionDefineOutputNames(actionDefine));
            _actionNodeItemList.Add(actionNodeItem);
            //为动作分支创建节点图
            float lastNodeOffset = 0;
            for (int i = 0; i < actionNode.branches.Length; i++)
            {
                var nextActionNode = actionNode.branches[i];
                if (nextActionNode == null)
                    continue;
                if (sizeDict == null)
                    sizeDict = new Dictionary<ActionNode, Vector2>();
                var nextActionNodeItem = createGraph4ActionNode(nextActionNode,
                    prevNodeItem: actionNodeItem,
                    startPosOffset: lastNodeOffset,
                    sizeDict: sizeDict);
                Vector2 size = calcActionNodeGraphSize(nextActionNode, sizeDict);
                lastNodeOffset -= size.y;
                _ui.connectNode(actionNodeItem, i, nextActionNodeItem, 0);
            }
            //为输入节点创建节点图
            lastNodeOffset = 0;
            for (int i = 0; i < actionNode.inputs.Length; i++)
            {
                var valueRef = actionNode.inputs[i];
                if (valueRef == null)
                    continue;
                if (sizeDict == null)
                    sizeDict = new Dictionary<ActionNode, Vector2>();
                var inputActionNodeItem = createGraph4ActionNode(valueRef.action,
                    nextNodeItem: actionNodeItem,
                    startPosOffset: lastNodeOffset,
                    sizeDict: sizeDict);
                Vector2 size = calcActionNodeGraphSize(valueRef.action, sizeDict);
                lastNodeOffset -= size.y;
                //输出索引要被动作分支占用若干个空位
                //输入索引要被前置动作占用一个空位
                _ui.connectNode(inputActionNodeItem, valueRef.action.branches.Length + valueRef.index, actionNodeItem, 1 + i);
            }
            return actionNodeItem;
        }
        Vector2 calcActionNodeGraphSize(ActionNode actionNode, Dictionary<ActionNode, Vector2> sizeDict)
        {
            if (sizeDict != null && sizeDict.ContainsKey(actionNode))
                return sizeDict[actionNode];
            Vector2 size = new Vector2();
            //计算输入的宽度
            float inputWidth = 0;
            for (int i = 0; i < actionNode.inputs.Length; i++)
            {
                ActionNode inputNode = actionNode.inputs[i].action;
                float width = calcActionNodeGraphSize(inputNode, sizeDict).x;
                if (width > inputWidth)
                    inputWidth = width;
            }
            size.x = _ui.autoPosSpacingWidth + inputWidth;
            float inputHeight = 0;
            //计算输入的高度
            for (int i = 0; i < actionNode.inputs.Length; i++)
            {
                ActionNode inputNode = actionNode.inputs[i].action;
                inputHeight += calcActionNodeGraphSize(inputNode, sizeDict).y;
            }
            //float branchHeight = 0;
            ////计算分支高度
            //for (int i = 0; i < actionNode.branches.Length; i++)
            //{
            //    ActionNode branchNode = actionNode.branches[i];
            //    branchHeight += calcActionNodeGraphSize(branchNode, sizeDict).y;
            //}
            //size.y = Math.Max(inputHeight, Math.Max(branchHeight, _compo.autoPosSpacingHeight));
            size.y = Math.Max(inputHeight, _ui.autoPosSpacingHeight);
            sizeDict[actionNode] = size;
            return size;
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
            var actionNodeItem = _ui.createNode(
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
                _editObjectStack.Push(new DisplayInfo() { entryType = EntryType.none, editObject = _trigger });
            else if (_targetChecker != null)
                _editObjectStack.Push(new DisplayInfo() { entryType = EntryType.none, editObject = _targetChecker });
            else if (_valueRef != null)
                _editObjectStack.Push(new DisplayInfo() { entryType = EntryType.none, editObject = _valueRef });
            else
                _editObjectStack.Push(new DisplayInfo() { entryType = _entryType, editObject = _effect });
            _ui.setReturnButtonActive(true);
        }
        void popPrevEditObjectFromStackAndOpen()
        {
            DisplayInfo info = _editObjectStack.Pop();
            if (_editObjectStack.Count < 1)
                _ui.setReturnButtonActive(false);
            if (info.editObject is TriggerGraph trigger)
                openTriggerGraph(trigger);
            else if (info.editObject is TargetChecker targetChecker)
                openTargetCheckerGraph(targetChecker);
            else if (info.editObject is ActionValueRef valueRef)
                openValueRefGraph(valueRef);
            else if (info.editObject is GeneratedEffect effect)
                openActionGraph(effect, info.entryType);
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
            _ui.openMenu(posX, posY, _backgroundMenuOptions);
        }
        /// <summary>
        /// 当用户点击背景，隐藏菜单。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        void onClickBackground(float posX, float posY)
        {
            _ui.closeMenu();
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
                            _ui.unconnectNode(_actionNodeItemList[i], branchNodeIndex, actionNodeItem, 0);
                        }
                        //断开输入
                        int inputValueRefIndex = Array.FindIndex(actionNode.inputs, v => v.action == _actionNodeList[i]);
                        if (inputValueRefIndex >= 0)
                        {
                            var inputActionNode = _actionNodeList[i];
                            var inputValueRef = actionNode.inputs[inputValueRefIndex];
                            actionNode.inputs[inputValueRefIndex] = null;
                            _ui.unconnectNode(_actionNodeItemList[i], inputActionNode.branches.Length + inputValueRef.index, actionNodeItem, 1 + inputValueRefIndex);
                        }
                        //断开后一个动作
                        branchNodeIndex = Array.IndexOf(actionNode.branches, _actionNodeList[i]);
                        if (branchNodeIndex >= 0)
                        {
                            actionNode.branches[branchNodeIndex] = null;
                            _ui.unconnectNode(actionNodeItem, branchNodeIndex, _actionNodeItemList[i], 0);
                        }
                        //断开输出
                        inputValueRefIndex = Array.FindIndex(_actionNodeList[i].inputs, v => v.action == actionNode);
                        if (inputValueRefIndex >= 0)
                        {
                            var inputValueRef = _actionNodeList[i].inputs[inputValueRefIndex];
                            _actionNodeList[i].inputs[inputValueRefIndex] = null;
                            _ui.unconnectNode(actionNodeItem, actionNode.branches.Length + inputValueRef.index, _actionNodeItemList[i], 1 + inputValueRefIndex);
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
            _ui.closeMenu();
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
                    _ui.unconnectNode(_entryNodeItem, 0, getActionNodeItem(rootActionNode), 0);
                    rootActionNode = null;
                }
                //如果输入已经和其他节点输出连接了的话，先断开连接
                ActionValueRef outputValueRef = inputIndex < inputActionNode.branches.Length ? null :
                    inputActionNode.inputs[inputIndex - inputActionNode.branches.Length];
                if (outputValueRef != null)
                {
                    var actionNode = outputValueRef.action;
                    actionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                    var actionNodeItem = getActionNodeItem(actionNode);
                    _ui.unconnectNode(actionNodeItem, actionNode.branches.Length + outputValueRef.index, inputActionNodeItem, inputIndex);
                }
                //检查合法性
                //入口节点只有一个输出节点
                if (outputIndex != 0 || inputIndex != 0)
                {
                    //值输出只能与值输入相互连接
                    _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                    return;
                }
                rootActionNode = inputActionNode;
            }
            else if (inputActionNodeItem == _returnNodeItem)
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                //出口节点也只有一个参数
                if (rootActionNode != null)
                {
                    _ui.unconnectNode(getActionNodeItem(rootActionNode), valueRef.index + 1, _returnNodeItem, 0);
                    rootActionNode = null;
                }
                //检查合法性
                //输出值节点只有一个输出节点
                if (outputIndex == 0)
                {
                    //值输出只能与值输入相互连接
                    _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
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
                        _ui.unconnectNode(outputActionNodeItem, branchNodeIndex, actionNodeItem, 0);
                    }
                    //断开输出值引用的连接
                    int valueRefIndex = Array.FindIndex(actionNode.inputs, v => v != null && v.action == outputActionNode);
                    if (valueRefIndex >= 0)
                    {
                        var actionNodeItem = _actionNodeItemList[i];
                        actionNode.inputs[valueRefIndex] = null;
                        _ui.unconnectNode(outputActionNodeItem, actionNode.inputs[valueRefIndex].index, actionNodeItem, actionNode.branches.Length + valueRefIndex);
                    }
                }
                //如果输入已经和其他节点输出连接了的话，先断开连接
                ActionValueRef outputValueRef = inputIndex < inputActionNode.branches.Length ? null :
                    inputActionNode.inputs[inputIndex - inputActionNode.branches.Length];
                if (outputValueRef != null)
                {
                    var actionNode = outputValueRef.action;
                    actionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                    var actionNodeItem = getActionNodeItem(actionNode);
                    _ui.unconnectNode(actionNodeItem, actionNode.branches.Length + outputValueRef.index, inputActionNodeItem, inputIndex);
                }
                //检查合法性
                if ((outputIndex < outputActionNode.branches.Length) != (inputIndex == 0))
                {
                    //值输出只能与值输入相互连接
                    _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                    return;
                }
                if (outputIndex >= outputActionNode.branches.Length && inputIndex > 0)
                {
                    //值的相互连接
                    var inputActionDefine = _actionDefineDict[inputActionNode.defineName];
                    var inputValueDefine = inputActionDefine.inputs[inputIndex - 1];
                    var outputActionDefine = _actionDefineDict[outputActionNode.defineName];
                    var outputValueDefine = outputActionDefine.outputs[outputIndex - outputActionNode.branches.Length];
                    if (!inputValueDefine.type.IsAssignableFrom(outputValueDefine.type) && !outputValueDefine.type.IsAssignableFrom(inputValueDefine.type))
                    {
                        //相互连接的输入输出类型必须可以相互转化
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                        return;
                    }
                    inputActionNode.inputs[inputIndex - 1] = new ActionValueRef(outputActionNode, outputIndex - outputActionNode.branches.Length);
                }
                else
                {
                    //动作的相互连接
                    outputActionNode.branches[outputIndex] = inputActionNode;
                }
            }
            _ui.connectNode(outputActionNodeItem, outputIndex, inputActionNodeItem, inputIndex);
        }
        /// <summary>
        /// 当用户断开节点连接
        /// </summary>
        /// <param name="outputActionNodeItem"></param>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNodeItem"></param>
        /// <param name="inputIndex"></param>
        void onUnconnectNodes(ActionNodeItem outputActionNodeItem, int outputIndex, ActionNodeItem inputActionNodeItem, int inputIndex)
        {
            if (outputActionNodeItem == _entryNodeItem)
            {
                ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                if (inputActionNode == rootActionNode)
                {
                    //断开与入口动作节点的连接
                    _ui.unconnectNode(_entryNodeItem, 0, getActionNodeItem(rootActionNode), 0);
                    rootActionNode = null;
                }
                else
                {
                    //断开与值节点的连接
                    ActionValueRef outputValueRef = inputIndex < inputActionNode.branches.Length ? null :
                        inputActionNode.inputs[inputIndex - inputActionNode.branches.Length];
                    if (outputValueRef != null)
                    {
                        var actionNode = outputValueRef.action;
                        actionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                        var actionNodeItem = getActionNodeItem(actionNode);
                        _ui.unconnectNode(actionNodeItem, actionNode.branches.Length + outputValueRef.index, inputActionNodeItem, inputIndex);
                    }
                }
            }
            else if (inputActionNodeItem == _returnNodeItem)
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                if (rootActionNode == outputActionNode)
                {
                    //断开与返回值节点的连接
                    _ui.unconnectNode(outputActionNodeItem, valueRef.index + 1, _returnNodeItem, 0);
                    rootActionNode = null;
                }
            }
            else
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                if (outputIndex < outputActionNode.branches.Length)
                {
                    outputActionNode.branches[outputIndex] = null;
                }
                else
                {
                    inputActionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                }
                _ui.unconnectNode(outputActionNodeItem, outputIndex, inputActionNodeItem, inputIndex);
            }
        }
        /// <summary>
        /// 当点击动作节点
        /// </summary>
        /// <param name="actionNodeItem"></param>
        void onClickActionNode(ActionNodeItem actionNodeItem)
        {
            _ui.closeMenu();
        }
        /// <summary>
        /// 当用户长按节点，展开节点菜单。
        /// </summary>
        /// <param name="actionNodeItem"></param>
        void onLongPressActionNode(ActionNodeItem actionNodeItem, Vector2 pointerPosition)
        {
            //入口节点不能通过长按编辑
            if (actionNodeItem == _entryNodeItem)
                return;
            //出口节点也不行
            if (actionNodeItem == _returnNodeItem)
                return;
            _curActionNode = getActionNode(actionNodeItem);
            Vector2 position = _ui.worldPositionToGraphPosition(pointerPosition);
            _ui.openMenu(position.x, position.y, new string[]
            {
                game.langManager.getString(StringTable.DELETE),//删除
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
            if (_targetChecker != null && actionNodeItem == _returnNodeItem)
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
            if (_trigger.condition == null)
                _trigger.condition = new ActionValueRef();
            openValueRefGraph(_trigger.condition);
        }
        /// <summary>
        /// 当用户点击新增目标按钮，为当前触发器新建目标检查器，然后在UI列表中新增项目
        /// </summary>
        void onClickAddTargetCheckerButton()
        {
            _trigger.targetCheckerList.Add(new TargetChecker(string.Empty, new ActionValueRef(null, 0), string.Empty));
            string[] targetCheckerNames = new string[_trigger.targetCheckerList.Count];
            for (int i = 0; i < targetCheckerNames.Length; i++)
            {
                targetCheckerNames[i] = "目标" + (i + 1);
            }
            _entryNodeItem.updateTargetCheckerList(targetCheckerNames);
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
            string[] targetCheckerNames = new string[_trigger.targetCheckerList.Count];
            for (int i = 0; i < targetCheckerNames.Length; i++)
            {
                targetCheckerNames[i] = "目标" + (i + 1);
            }
            _entryNodeItem.updateTargetCheckerList(targetCheckerNames);
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
        [Obsolete]
        public IAppManager app { get; }
        [Obsolete]
        public MainCtrl main { get; }
        ActionNodeGraphPanel _ui;
        Dictionary<string, ActionDefine> _actionDefineDict;
        string[] _backgroundMenuOptions;
        float _curPosX;
        float _curPosY;
        readonly List<ActionNode> _actionNodeList = new List<ActionNode>();
        readonly List<ActionNodeItem> _actionNodeItemList = new List<ActionNodeItem>();
        /// <summary>
        /// 当前动作图的编辑器信息。它不一定存在，比如对于新建的图来讲它一定不存在，当退出图编辑的时候会自动保存。
        /// 在打开一张图的时候，不应该假设它一定存在。
        /// </summary>
        ActionNodeGraphInfo _graphInfo;
        ActionNodeItem _entryNodeItem;
        ActionNodeItem _returnNodeItem;
        ActionNode rootActionNode
        {
            get
            {
                if (_effect != null)
                {
                    if (_entryType == EntryType.onEnable)
                        return _effect.onEnableAction;
                    else if (_entryType == EntryType.onDisable)
                        return _effect.onDisableAction;
                }
                if (_trigger != null)
                    return _trigger.action;
                if (valueRef != null)
                    return valueRef.action;
                throw new NullReferenceException("没有引用动作的对象");
            }
            set
            {
                if (_effect != null)
                {
                    if (_entryType == EntryType.onEnable)
                        _effect.onEnableAction = value;
                    else if (_entryType == EntryType.onDisable)
                        _effect.onDisableAction = value;
                }
                else if (_trigger != null)
                    _trigger.action = value;
                else if (valueRef != null)
                    valueRef.action = value;
            }
        }
        EntryType _entryType = EntryType.none;
        GeneratedEffect _effect;
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
            public EntryType entryType;
            public object editObject;
        }
        enum EntryType
        {
            none,
            onEnable,
            onDisable
        }
    }
    /// <summary>
    /// 动作节点图的编辑器信息，保存了每个效果的所有动作节点信息。
    /// </summary>
    [Serializable]
    class ActionNodeGraphInfo
    {
        public ActionNodeGraphInfo(int cardId, int effectIndex)
        {
            this.cardId = cardId;
            this.effectIndex = effectIndex;
        }
        public int cardId;
        public int effectIndex;
        public Dictionary<int, ActionNodeGraphNodeInfo> actionInfoDict = new Dictionary<int, ActionNodeGraphNodeInfo>();
        /// <summary>
        /// 离散的，并未与根节点相连的动作节点。
        /// </summary>
        public List<ActionNode> freeActionNodeList = new List<ActionNode>();
    }
    [Serializable]
    class ActionNodeGraphNodeInfo
    {
        public ActionNodeGraphNodeInfo(float posX, float posY)
        {
            this.posX = posX;
            this.posY = posY;
        }
        public float posX;
        public float posY;
    }
}
