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
using BJSYGameCore.UI;
using System.Threading.Tasks;

namespace Controllers
{
    partial class ActionNodeGraphCtrl : GameObjectCtrl<MainSceneCtrl>, IController<MainCtrl>, IUICtrl
    {
        #region 公有方法
        /// <summary>
        /// 打开一个效果的生效效果动作图。
        /// </summary>
        /// <param name="effect"></param>
        /// 
        public void openOnEnableActionGraph(int cardPoolId, CardDefine cardDefine, GeneratedEffect effect)
        {
            display(new DisplayInfo(cardPoolId, cardDefine, nameof(GeneratedEffect.onEnableAction), effect, null));
        }
        /// <summary>
        /// 打开一个效果的失效效果动作图。
        /// </summary>
        /// <param name="effect"></param>
        /// 
        public void openOnDisableActionGraph(int cardPoolId, CardDefine cardDefine, GeneratedEffect effect)
        {
            display(new DisplayInfo(cardPoolId, cardDefine, nameof(GeneratedEffect.onDisableAction), effect, null));
        }
        public void openActionGraph(int cardPoolId, CardDefine cardDefine, GeneratedEffect effect, string entryType)
        {
            display(new DisplayInfo(cardPoolId, cardDefine, entryType, effect, null));
        }
        public Task openActionValueRefGraph(int cardPoolId, CardDefine cardDefine, ActionValueRef valueRef)
        {
            return display(new DisplayInfo(cardPoolId, cardDefine, null, valueRef, null));
        }
        public Task display(UIDisplayInfo uiDisplayInfo)
        {
            if (uiDisplayInfo is DisplayInfo info)
            {
                _editObjectStack.Clear();
                if (info.editObjectStack != null)
                {
                    for (int i = 0; i < info.editObjectStack.Length; i++)
                    {
                        _editObjectStack.Push(info.editObjectStack[i]);
                    }
                }
                if (_editObjectStack.Count < 1)
                    _ui.setReturnButtonActive(false);
                _curCardPoolId = info.cardPoolId;
                _cardDefine = info.cardDefine;
                _ui.gameObject.SetActive(true);
                if (info.editObject is TriggerGraph trigger)
                    return openTriggerGraph(trigger);
                else if (info.editObject is TargetChecker targetChecker)
                    return openTargetCheckerGraph(targetChecker);
                else if (info.editObject is ActionValueRef valueRef)
                    return openValueRefGraph(valueRef);
                else if (info.editObject is GeneratedEffect effect)
                    return openActionGraph(effect, info.entryType);
                else
                    return Task.CompletedTask;
            }
            throw new NotImplementedException();
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            object editObject;
            if (_trigger != null)
                editObject = _trigger;
            else if (_targetChecker != null)
                editObject = _targetChecker;
            else if (_valueRef != null)
                editObject = _valueRef;
            else if (_effect != null)
                editObject = _effect;
            else
                return null;
            return new DisplayInfo(_curCardPoolId, _cardDefine, _entryType, editObject, _editObjectStack.Count > 0 ? _editObjectStack.ToArray() : null);
        }
        public Task hide()
        {
            gameObject.SetActive(false);
            return Task.CompletedTask;
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
            loadEventTypes();
            _actionDefineDict = ActionDefine.loadDefinesFromAssemblies(new Assembly[]
            {
                typeof(CardEngine).Assembly,
                typeof(HearthStoneRule).Assembly,
                typeof(EffectEditorLibrary).Assembly
            });
            //生成节点菜单
            _backgroundMenuOptionList = new List<BackgroundMenuItem>();
            foreach (var pair in _actionDefineDict)
            {
                string defineName = pair.Key;
                ActionDefine actionDefine = pair.Value;
                BackgroundMenuItem menuItem;
                BackgroundMenuItem menuCategory;
                if (actionDefine is MethodActionDefine method)
                {
                    //反射方法的选项要带上它的分类
                    if (string.IsNullOrEmpty(method.category))
                    {
                        menuItem = new BackgroundMenuItem(_nameTextDict.TryGetValue(method.methodName, out string text) ?
                            game.langManager.getString(text) :
                            method.methodName);
                        menuItem.action = onMenuClick_CreateNode;
                        _backgroundMenuOptionList.Add(menuItem);
                    }
                    else
                    {
                        menuCategory = _backgroundMenuOptionList.FirstOrDefault(e => e.title ==
                           (_nameTextDict.TryGetValue(method.category, out string categoryText) ?
                                game.langManager.getString(categoryText) :
                                method.category));
                        if (menuCategory == null)
                        {
                            menuCategory = new BackgroundMenuItem(_nameTextDict.TryGetValue(method.category, out string categoryText) ?
                            game.langManager.getString(categoryText) :
                            method.category);
                            menuCategory.action = onMenuClick_EnterSubMenu;
                            _backgroundMenuOptionList.Add(menuCategory);
                        }
                        if (menuCategory.childList == null)
                            menuCategory.childList = new List<BackgroundMenuItem>();
                        menuItem = new BackgroundMenuItem(_nameTextDict.TryGetValue(method.methodName, out string text) ?
                            game.langManager.getString(text) :
                            method.methodName);
                        menuItem.action = onMenuClick_CreateNode;
                        menuCategory.childList.Add(menuItem);
                    }
                }
                else
                {
                    if (defineName.EndsWith(StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST))
                    {
                        //常量
                        menuCategory = _backgroundMenuOptionList.FirstOrDefault(e => e.title == game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST));
                        if (menuCategory == null)
                        {
                            menuCategory = new BackgroundMenuItem(game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST));
                            menuCategory.action = onMenuClick_EnterSubMenu;
                            _backgroundMenuOptionList.Add(menuCategory);
                        }
                        if (menuCategory.childList == null)
                            menuCategory.childList = new List<BackgroundMenuItem>();
                        menuItem = new BackgroundMenuItem(_nameTextDict.TryGetValue(defineName.Substring(0, defineName.Length - 5), out string text) ?
                            game.langManager.getString(text) :
                            defineName.Substring(0, defineName.Length - 5));
                        menuItem.action = onMenuClick_CreateNode;
                        menuCategory.childList.Add(menuItem);
                    }
                    else
                    {
                        menuItem = new BackgroundMenuItem(_nameTextDict.TryGetValue(defineName, out string text) ?
                            game.langManager.getString(text) :
                            defineName);
                        menuItem.action = onMenuClick_CreateNode;
                        _backgroundMenuOptionList.Add(menuItem);
                    }
                }
                async void onMenuClick_CreateNode()
                {
                    closeMenu();
                    await createActionNodeAtCurPos(defineName, actionDefine);
                }
                void onMenuClick_EnterSubMenu()
                {
                    enterSubMenu(new int[] { _backgroundMenuOptionList.IndexOf(menuCategory) });
                }
            }
            _ui.onLeftClickBackground += onLeftClickBackground;
            _ui.onRightClickBackground += onRightClickBackground;
            _ui.onLongPressBackground += onLongPressBackground;
            _ui.onClickMenuItem += onClickMenuItem;
            _ui.onConnectNodes += onConnectNodes;
            _ui.onUnconnectNodes += onUnconnectNodes;
            _ui.onLeftClickActionNode += onLeftClickActionNode;
            _ui.onRightClickActionNode += onRightClickActionNode;
            _ui.onLongPressActionNode += onLongPressActionNode;
            _ui.onDragActionNode += onDragActionNode;
            _ui.onActionNodeValueChange += onActionNodeValueChange;
            _ui.onClickActionNodeConstValueEditButton += onClickActionNodeConstValueEditButton;
            //触发
            _ui.onChangeTriggerEventName += onChangeTriggerEventName;
            _ui.onTriggerEventTypeChanged += onChangeTriggerTypeIndex;
            _ui.onClickTriggerEditConditionButton += onClickTriggerEditConditionButton;
            _ui.onClickAddTargetCheckerButton += onClickAddTargetCheckerButton;
            _ui.onTargetCheckerTypeValueChanged += onTargetCheckerTypeValueChanged;
            _ui.onClickEditTargetCheckerButton += onClickEditTargetCheckerButton;
            _ui.onClickRemoveTargetCheckerButton += onClickRemoveTargetCheckerButton;
            _ui.onTargetCheckerErrorTipValueChanged += onTargetCheckerErrorTipValueChanged;
            _ui.onClickReturnButton += onClickReturnButton;
            //增益
            _ui.onItemBuffChanged += onItemBuffChangedCallback;
            _ui.onClickItemNewBuffButton += onClickItemNewBuffButtonCallback;
            _ui.onClickItemDeleteBuffButton += onClickItemDeleteBuffButtonCallback;
            _ui.onClickItemAddPropModifierButton += onClickItemAddPropModifierButtonCallback;
            _ui.onClickItemRemovePropModifierButton += onClickItemRemovePropModifierButtonCallback;
            _ui.onItemPropModifierPropertyTypeChanged += onItemPropModifierPropertyTypeChangedCallback;
            _ui.onItemPropModifierModifyTypeChanged += onItemPropModifierModifyTypeChangedCallback;
            _ui.onItemPropModifierValueChanged += onItemPropModifierValueChangedCallback;
            _ui.onItemPropModifierAddEffect += onItemPropModifierAddEffectCallback;
            _ui.onItemPropModifierRemoveEffect += onItemPropModifierRemoveEffectCallback;
            _ui.onItemPropModifierEditEffect += onItemPropModifierEditEffectCallback;
            _ui.onItemBuffNameChanged += onItemBuffNameChangedCallback;
            _ui.onItemBuffDescChanged += onItemBuffDescChangedCallback;
            _ui.onItemBuffLanguageChanged += onItemBuffLanguageChangedCallback;
            _ui.onItemNeedSelectBuffImage += onItemNeedSelectBuffImage;

            _ui.init(game.langManager.getLanguages(), getEventTypeNames());
        }
        #endregion
        #region 事件回调
        /// <summary>
        /// 当用户长按背景，展开菜单。目前点菜单内容只有创建节点。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        private void onLongPressBackground(float posX, float posY)
        {
            openMenu(posX, posY, _backgroundMenuOptionList.Select(menuItem => menuItem.title + (menuItem.childList != null ? " >" : "")).ToArray());
        }
        /// <summary>
        /// 当用户左击背景，隐藏菜单。
        /// </summary>
        /// <param name="posX">x坐标</param>
        /// <param name="posY">y坐标</param>
        private void onLeftClickBackground(float posX, float posY)
        {
            _curActionNode = null;
            closeMenu();
        }
        /// <summary>
        /// 当用户右击背景，打开菜单。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        private void onRightClickBackground(float posX, float posY)
        {
            openMenu(posX, posY, _backgroundMenuOptionList.Select(menuItem => menuItem.title + (menuItem.childList != null ? " >" : "")).ToArray());
        }
        /// <summary>
        /// 当用户点击菜单项，视具体点击的项目决定干什么
        /// </summary>
        /// <param name="index"></param>
        private void onClickMenuItem(int index)
        {
            if (_curActionNode != null)
            {
                //节点菜单
                if (index == 0)//删除
                {
                    int actionNodeIndex = _actionNodeList.IndexOf(_curActionNode);
                    ActionNodeItem actionNodeItem = _actionNodeItemList[actionNodeIndex];
                    ActionNode actionNode = _curActionNode;
                    //断开节点和所有其他节点的连接
                    //for (int i = 0; i < _actionNodeList.Count; i++)
                    //{
                    //    //断开前一个动作
                    //    int branchNodeIndex = Array.IndexOf(_actionNodeList[i].branches, actionNode);
                    //    if (branchNodeIndex >= 0)
                    //    {
                    //        _actionNodeList[i].branches[branchNodeIndex] = null;
                    //        _ui.unconnectNode(_actionNodeItemList[i], branchNodeIndex, actionNodeItem, 0);
                    //    }
                    //    //断开输入
                    //    int inputValueRefIndex = Array.FindIndex(actionNode.inputs, v => v != null && (v.action == _actionNodeList[i] || v.actionNodeId == _actionNodeList[i].id));
                    //    if (inputValueRefIndex >= 0)
                    //    {
                    //        var inputActionNode = _actionNodeList[i];
                    //        var inputValueRef = actionNode.inputs[inputValueRefIndex];
                    //        actionNode.inputs[inputValueRefIndex] = null;
                    //        _ui.unconnectNode(_actionNodeItemList[i], inputActionNode.branches.Length + inputValueRef.index, actionNodeItem, 1 + inputValueRefIndex);
                    //    }
                    //    //断开后一个动作
                    //    branchNodeIndex = Array.IndexOf(actionNode.branches, _actionNodeList[i]);
                    //    if (branchNodeIndex >= 0)
                    //    {
                    //        actionNode.branches[branchNodeIndex] = null;
                    //        _ui.unconnectNode(actionNodeItem, branchNodeIndex, _actionNodeItemList[i], 0);
                    //    }
                    //    //断开输出
                    //    inputValueRefIndex = Array.FindIndex(_actionNodeList[i].inputs, v => v != null && (v.action == actionNode || v.actionNodeId == actionNode.id));
                    //    if (inputValueRefIndex >= 0)
                    //    {
                    //        var inputValueRef = _actionNodeList[i].inputs[inputValueRefIndex];
                    //        _actionNodeList[i].inputs[inputValueRefIndex] = null;
                    //        _ui.unconnectNode(actionNodeItem, actionNode.branches.Length + inputValueRef.index, _actionNodeItemList[i], 1 + inputValueRefIndex);
                    //    }
                    //}
                    unconnectNodeAll(actionNode);
                    //从列表中移除
                    _actionNodeList.RemoveAt(actionNodeIndex);
                    _actionNodeItemList.RemoveAt(actionNodeIndex);
                    //从编辑器信息中删除
                    removeNodeInfo(actionNode);
                    graphInfo.freeActionNodeList.Remove(actionNode);
                    //将节点从UI上移除
                    _ui.removeNode(actionNodeItem);
                }
                closeMenu();
            }
            else
            {
                //背景菜单
                List<BackgroundMenuItem> optionList = _backgroundMenuOptionList;
                bool isSubMenu = false;
                if (_menuLayerStack != null)
                {
                    for (int i = 0; i < _menuLayerStack.Length; i++)
                    {
                        isSubMenu = true;
                        optionList = optionList[_menuLayerStack[i]].childList;
                    }
                }
                if (isSubMenu)
                {
                    if (index == 0)
                    {
                        //返回上一层
                        enterSubMenu(_menuLayerStack.Take(_menuLayerStack.Length - 1).ToArray());
                    }
                    else
                    {
                        optionList[index - 1].action?.Invoke();
                    }
                }
                else
                {
                    optionList[index].action?.Invoke();
                }
            }
            _curActionNode = null;
        }
        /// <summary>
        /// 当用户连接节点，检查一下连接的合法性，如果合法那么连接，否则提示。
        /// </summary>
        /// <param name="outputActionNodeItem"></param>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNodeItem"></param>
        /// <param name="inputIndex"></param>
        private void onConnectNodes(ActionNodeItem outputActionNodeItem, int outputIndex, ActionNodeItem inputActionNodeItem, int inputIndex)
        {
            if (outputActionNodeItem == _entryNodeItem)
            {
                ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                if (outputIndex < 1)
                {
                    //动作连接
                    //检查合法性
                    if (inputIndex > 0)
                    {
                        //值输出只能与值输入相互连接
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                        return;
                    }
                    //连接节点
                    connectNode4RootAction(inputActionNode);
                }
                else
                {
                    //事件变量值连接
                    //检查合法性
                    if (inputIndex < inputActionNode.branches.Length)
                    {
                        //值输出只能与值输入相互连接
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                        return;
                    }
                    Type outputType = getEventVarInfo(outputIndex - 1).type;
                    Type inputType = _actionDefineDict[inputActionNode.defineName].inputs[inputIndex - inputActionNode.branches.Length].type;
                    if (!outputType.IsAssignableFrom(inputType) && !inputType.IsAssignableFrom(outputType))
                    {
                        //相互连接的输入输出类型必须可以相互转化
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                        return;
                    }
                    connectNode4TriggerEventVarRef(outputIndex - 1, inputActionNode, inputIndex - inputActionNode.branches.Length);
                }
                //ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                ////入口节点更加特殊，它只有一个分支需要检查
                //if (rootActionNode != null)
                //{
                //    _ui.unconnectNode(_entryNodeItem, 0, getActionNodeItem(rootActionNode), 0);
                //    rootActionNode = null;
                //}
                ////如果输入已经和其他节点输出连接了的话，先断开连接
                //ActionValueRef outputValueRef = inputIndex < inputActionNode.branches.Length ? null :
                //    inputActionNode.inputs[inputIndex - inputActionNode.branches.Length];
                //if (outputValueRef != null)
                //{
                //    var actionNode = outputValueRef.action != null ? outputValueRef.action : getActionNode(outputValueRef.actionNodeId);
                //    actionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                //    var actionNodeItem = getActionNodeItem(actionNode);
                //    _ui.unconnectNode(actionNodeItem, actionNode.branches.Length + outputValueRef.index, inputActionNodeItem, inputIndex);
                //}
                ////检查合法性
                ////入口节点只有一个输出节点
                //if (outputIndex != 0 || inputIndex != 0)
                //{
                //    //值输出只能与值输入相互连接
                //    _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                //    return;
                //}
                //rootActionNode = inputActionNode;
                ////从离散节点列表中去掉被连接的节点
                //graphInfo.freeActionNodeList.Remove(inputActionNode);
            }
            else if (inputActionNodeItem == _returnNodeItem)
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                //检查连接合法性
                if (outputIndex < outputActionNode.branches.Length)
                {
                    //值输出只能与值输入相互连接
                    _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                    return;
                }
                connectNode4ReturnValueRef(outputActionNode, outputIndex - outputActionNode.branches.Length, 0);
                //出口节点也只有一个参数
                //if (rootActionNode != null)
                //{
                //    _ui.unconnectNode(getActionNodeItem(rootActionNode), valueRef.index + 1, _returnNodeItem, 0);
                //    rootActionNode = null;
                //}
                ////检查合法性
                ////输出值节点只有一个输出节点
                //if (outputIndex == 0)
                //{
                //    //值输出只能与值输入相互连接
                //    _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                //    return;
                //}
                //rootActionNode = outputActionNode;
                //valueRef.index = outputIndex - 1;
                ////从离散节点列表中去掉被连接的节点
                //graphInfo.freeActionNodeList.Remove(outputActionNode);
            }
            else
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                if (inputIndex == 0)
                {
                    //动作连接
                    if (outputActionNode.branches == null)
                    {
                        outputActionNode.branches = new ActionNode[1];
                    }
                    //检查合法性
                    if (outputIndex >= outputActionNode.branches.Length)
                    {
                        //值输出只能与值输入相互连接
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                        return;
                    }
                    connectNode4Action(outputActionNode, outputIndex, inputActionNode);
                }
                else
                {
                    //值连接
                    if (outputActionNode.branches == null)
                    {
                        outputActionNode.branches = new ActionNode[1];
                    }
                    //检查合法性
                    if (outputIndex < outputActionNode.branches.Length)
                    {
                        //值输出只能与值输入相互连接
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                        return;
                    }
                    //检查类型合法性
                    Type outputType = _actionDefineDict[outputActionNode.defineName].outputs[outputIndex - outputActionNode.branches.Length].type;
                    Type inputType = _actionDefineDict[inputActionNode.defineName].inputs[inputIndex - 1].type;
                    if (!outputType.IsAssignableFrom(inputType) && !inputType.IsAssignableFrom(outputType))
                    {
                        //相互连接的输入输出类型必须可以相互转化
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                        return;
                    }
                    connectNode4ValueRef(outputActionNode, outputIndex - outputActionNode.branches.Length, inputActionNode, inputIndex - 1);
                }
                ////一个节点的动作输出只可以连接一个动作输入，先自动断开和之前节点的连接
                //if (inputIndex == 0)
                //{
                //    ActionNode prevOutputActionNode = null;
                //    int prevOutputIndex = -1;
                //    for (int i = 0; i < _actionNodeList.Count; i++)
                //    {
                //        if (_actionNodeList[i].branches == null)
                //            continue;
                //        for (int j = 0; j < _actionNodeList[i].branches.Length; j++)
                //        {
                //            if (_actionNodeList[i].branches[j] == inputActionNode)
                //            {
                //                prevOutputActionNode = _actionNodeList[i];
                //                prevOutputIndex = j;
                //                break;
                //            }
                //        }
                //        if (prevOutputActionNode != null)
                //        {
                //            break;
                //        }
                //    }
                //    if (prevOutputActionNode != null)
                //    {
                //        prevOutputActionNode.branches[prevOutputIndex] = null;
                //        //将断开连接的节点加入离散列表
                //        if (!graphInfo.freeActionNodeList.Contains(inputActionNode))
                //            graphInfo.freeActionNodeList.Add(inputActionNode);
                //        _ui.unconnectNode(getActionNodeItem(prevOutputActionNode), prevOutputIndex, inputActionNodeItem, inputIndex);
                //    }
                //}
                //if (outputActionNode.branches != null && outputIndex < outputActionNode.branches.Length)
                //{
                //    ActionNode prevInputActionNode = outputActionNode.branches[outputIndex];
                //    if (prevInputActionNode != null)
                //    {
                //        outputActionNode.branches[outputIndex] = null;
                //        //将断开连接的节点加入离散列表
                //        if (!graphInfo.freeActionNodeList.Contains(prevInputActionNode))
                //            graphInfo.freeActionNodeList.Add(prevInputActionNode);
                //        _ui.unconnectNode(outputActionNodeItem, outputIndex, getActionNodeItem(prevInputActionNode), 0);
                //    }
                //}
                ////一个值输入只能接受一个节点的值输出，自动断开之前的输出连接
                //if (inputActionNode.inputs != null &&
                //    inputIndex > 0 &&
                //    inputIndex - 1 < inputActionNode.inputs.Length &&
                //    (inputActionNode.inputs[inputIndex - 1] == null ||
                //    (inputActionNode.inputs[inputIndex - 1].action != outputActionNode &&
                //    inputActionNode.inputs[inputIndex - 1].actionNodeId != outputActionNode.id)))
                //{
                //    ActionValueRef inputValueRef = inputActionNode.inputs[inputIndex - 1];
                //    //一个节点可能提供多个输出值，但是只有在顺序上最先引用它的节点才会保存对它的引用以便序列化，其余情况只保留id和index
                //    inputActionNode.inputs[inputIndex - 1] = null;
                //    if (inputValueRef != null)
                //    {
                //        ActionNode prevOutputActionNode = inputValueRef.action != null ? inputValueRef.action : getActionNode(inputValueRef.actionNodeId);
                //        _ui.unconnectNode(getActionNodeItem(prevOutputActionNode), prevOutputActionNode.branches.Length + inputValueRef.index, inputActionNodeItem, inputIndex);
                //        if (isFreeActionNode(prevOutputActionNode))
                //        {
                //            prevOutputActionNode.regVar = null;
                //            if (!graphInfo.freeActionNodeList.Contains(prevOutputActionNode))
                //                graphInfo.freeActionNodeList.Add(prevOutputActionNode);
                //        }
                //        else
                //        {
                //            //如果之前的输出节点没有变成自由节点，那要更新一下它的输出变量状态
                //            updateRegVar(prevOutputActionNode);
                //        }
                //    }
                //}
                //ActionValueRef outputValueRef =
                //    inputIndex < inputActionNode.branches.Length ||
                //    inputIndex - inputActionNode.branches.Length >= inputActionNode.inputs.Length ?
                //    null : inputActionNode.inputs[inputIndex - inputActionNode.branches.Length];
                //if (outputValueRef != null)
                //{
                //    var actionNode = outputValueRef.action;
                //    actionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                //    var actionNodeItem = getActionNodeItem(actionNode);
                //    _ui.unconnectNode(actionNodeItem, actionNode.branches.Length + outputValueRef.index, inputActionNodeItem, inputIndex);
                //}
                //检查合法性
                //if ((outputIndex < outputActionNode.branches.Length) != (inputIndex == 0))
                //{
                //    //值输出只能与值输入相互连接
                //    _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                //    return;
                //}
                //if (outputIndex >= outputActionNode.branches.Length && inputIndex > 0)
                //{
                //    //值的相互连接
                //    var outputActionDefine = _actionDefineDict[outputActionNode.defineName];
                //    var outputValueDefine = outputActionDefine.outputs[outputIndex - outputActionNode.branches.Length];
                //    var inputActionDefine = _actionDefineDict[inputActionNode.defineName];
                //    if (inputActionDefine.inputs.Last().isParams && inputIndex - 1 >= inputActionDefine.inputs.Length - 1)
                //    {
                //        //是允许输入多个值的参数
                //        var inputValueDefine = inputActionDefine.inputs.Last();
                //        if (!inputValueDefine.type.IsAssignableFrom(outputValueDefine.type) && !outputValueDefine.type.IsAssignableFrom(inputValueDefine.type))
                //        {
                //            //相互连接的输入输出类型必须可以相互转化
                //            _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                //            return;
                //        }
                //        if (inputIndex - 1 >= inputActionNode.inputs.Length)
                //        {
                //            //如果是赋值给最后一个空白参数，那么自动添加一个空的参数
                //            ActionValueRef[] inputs = new ActionValueRef[inputActionNode.inputs.Length + 1];
                //            inputActionNode.inputs.CopyTo(inputs, 0);
                //            inputActionNode.inputs = inputs;
                //        }
                //        inputActionNodeItem.setInputs(getActionNodeInputNames(inputActionNode));
                //    }
                //    else
                //    {
                //        var inputValueDefine = inputActionDefine.inputs[inputIndex - 1];
                //        if (!inputValueDefine.type.IsAssignableFrom(outputValueDefine.type) && !outputValueDefine.type.IsAssignableFrom(inputValueDefine.type))
                //        {
                //            //相互连接的输入输出类型必须可以相互转化
                //            _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                //            return;
                //        }
                //    }
                //    //一个输出值可以与多个输入相连，只有顺序上第一个输入或者动作输入才可以保持对节点本身的引用
                //    if (!isValueRefedInParentNodes(outputActionNode, inputActionNode))
                //    {
                //        inputActionNode.inputs[inputIndex - 1] = new ActionValueRef(outputActionNode, outputIndex - outputActionNode.branches.Length);
                //        //遍历子节点，使其中所有对相同值输出的引用从直接引用变为id引用
                //        traverseActionNode(inputActionNode, false, a =>
                //        {
                //            if (a == inputActionNode)
                //                return;
                //            if (a.inputs != null && a.inputs.Length > 0)
                //            {
                //                for (int i = 0; i < a.inputs.Length; i++)
                //                {
                //                    if (a.inputs[i] == null)
                //                        continue;
                //                    if (a.inputs[i].action == outputActionNode)
                //                    {
                //                        a.inputs[i].action = null;
                //                        a.inputs[i].actionNodeId = outputActionNode.id;
                //                    }
                //                }
                //            }
                //        });
                //    }
                //    else
                //    {
                //        inputActionNode.inputs[inputIndex - 1] = new ActionValueRef(outputActionNode.id, outputIndex - outputActionNode.branches.Length);
                //    }
                //    //更新输出节点的输出变量状态
                //    updateRegVar(outputActionNode);
                //    //输出值的节点一定不是离散节点
                //    graphInfo.freeActionNodeList.Remove(outputActionNode);
                //}
                //else
                //{
                //    //动作的相互连接
                //    outputActionNode.branches[outputIndex] = inputActionNode;
                //    //后一个动作节点一定不是离散节点
                //    graphInfo.freeActionNodeList.Remove(inputActionNode);
                //}
            }
            //_ui.connectNode(outputActionNodeItem, outputIndex, inputActionNodeItem, inputIndex, ActionNodeConnectionType.action);
        }
        /// <summary>
        /// 当用户断开节点连接
        /// </summary>
        /// <param name="outputActionNodeItem"></param>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNodeItem"></param>
        /// <param name="inputIndex"></param>
        private void onUnconnectNodes(ActionNodeItem outputActionNodeItem, int outputIndex, ActionNodeItem inputActionNodeItem, int inputIndex)
        {
            ActionNode outputActionNode = getActionNode(outputActionNodeItem);
            ActionNode inputActionNode = getActionNode(inputActionNodeItem);
            if (outputActionNodeItem == _entryNodeItem)
            {
                if (inputIndex == 0)
                {
                    //动作连接
                    unconnectNode4RootAction(inputActionNode);
                }
                else
                {
                    //值连接
                    unconnectNode4TriggerEventVarRef(outputIndex - 1, inputActionNode, inputIndex - 1);
                }
                //ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                //if (inputActionNode == rootActionNode)
                //{
                //    //断开与入口动作节点的连接
                //    _ui.unconnectNode(_entryNodeItem, 0, getActionNodeItem(rootActionNode), 0);
                //    rootActionNode = null;
                //}
                //else
                //{
                //    //断开与值节点的连接
                //    ActionValueRef inputValueRef = inputIndex < inputActionNode.branches.Length ? null :
                //        inputActionNode.inputs[inputIndex - inputActionNode.branches.Length];
                //    if (inputValueRef != null)
                //    {
                //        var actionNode = inputValueRef.action;
                //        actionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                //        var actionNodeItem = getActionNodeItem(actionNode);
                //        _ui.unconnectNode(actionNodeItem, actionNode.branches.Length + inputValueRef.index, inputActionNodeItem, inputIndex);
                //    }
                //}
                //输入节点一定会变得离散
                //if (!graphInfo.freeActionNodeList.Contains(inputActionNode))
                //    graphInfo.freeActionNodeList.Add(inputActionNode);
            }
            else if (inputActionNodeItem == _returnNodeItem && valueRef != null)
            {
                unconnectNode4ReturnValueRef(outputActionNode, valueRef.index, 0);
                //ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                //if (rootActionNode == outputActionNode)
                //{
                //    //断开与返回值节点的连接
                //    _ui.unconnectNode(outputActionNodeItem, valueRef.index + 1, _returnNodeItem, 0);
                //    rootActionNode = null;
                //}
                ////输出节点一定会变得离散
                //if (!graphInfo.freeActionNodeList.Contains(outputActionNode))
                //    graphInfo.freeActionNodeList.Add(outputActionNode);
            }
            else
            {
                if (inputIndex == 0)
                {
                    //动作连接
                    unconnectNode4Action(outputActionNode, outputIndex, inputActionNode);
                }
                else
                {
                    //值连接
                    unconnectNode4ValueRef(outputActionNode, outputIndex - outputActionNode.branches.Length, inputActionNode, inputIndex - 1);
                }
                //ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                //ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                //if (outputIndex < outputActionNode.branches.Length)
                //{
                //    outputActionNode.branches[outputIndex] = null;
                //    _ui.unconnectNode(outputActionNodeItem, outputIndex, inputActionNodeItem, inputIndex);
                //    //输入节点一定会变得离散
                //    if (!graphInfo.freeActionNodeList.Contains(inputActionNode))
                //        graphInfo.freeActionNodeList.Add(inputActionNode);
                //}
                //else
                //{
                //    ActionValueRef inputActionValueRef = inputActionNode.inputs[inputIndex - inputActionNode.branches.Length];
                //    ActionDefine inputActionDefine = _actionDefineDict[inputActionNode.defineName];
                //    if (inputActionDefine.inputs.Last().isParams && inputIndex - inputActionNode.branches.Length >= inputActionDefine.inputs.Length - 1)
                //    {
                //        //如果断开的是多参数节点
                //        List<ActionValueRef> inputList = new List<ActionValueRef>(inputActionNode.inputs);
                //        inputList.RemoveAt(inputIndex - inputActionNode.branches.Length);
                //        inputActionNode.inputs = inputList.ToArray();
                //        //断开所有连接再重新连接
                //        _ui.unconnectNodeAllInput(inputActionNodeItem);
                //        for (int i = 0; i < inputActionNode.branches.Length; i++)
                //        {
                //            _ui.connectNode(getActionNodeItem(inputActionNode.branches[i]), 0, inputActionNodeItem, i, ActionNodeConnectionType.value);
                //        }
                //        for (int i = inputActionNode.branches.Length; i < inputList.Count; i++)
                //        {
                //            _ui.connectNode(getActionNodeItem(inputList[i].action), inputList[i].index + 1, inputActionNodeItem, i, ActionNodeConnectionType.value);
                //        }
                //    }
                //    else
                //    {
                //        inputActionNode.inputs[inputIndex - inputActionNode.branches.Length] = null;
                //        _ui.unconnectNode(outputActionNodeItem, outputIndex, inputActionNodeItem, inputIndex);
                //    }
                //    if (inputActionValueRef.action != null)
                //    {
                //        //寻找子节点中的第一个值引用，将其改为引用对象本身
                //        traverseActionNode(inputActionNode, false, a =>
                //        {
                //            if (a.inputs == null)
                //                return false;
                //            for (int i = 0; i < a.inputs.Length; i++)
                //            {
                //                if (a.inputs[i] != null && a.inputs[i].actionNodeId == inputActionValueRef.action.id)
                //                {
                //                    a.inputs[i].action = inputActionValueRef.action;
                //                    a.inputs[i].actionNodeId = 0;
                //                    return true;
                //                }
                //            }
                //            return false;
                //        });
                //    }
                //    if (isFreeActionNode(outputActionNode))
                //    {
                //        outputActionNode.regVar = null;
                //        //如果输出节点变为离散节点，将其添加进入列表
                //        if (!graphInfo.freeActionNodeList.Contains(outputActionNode))
                //            graphInfo.freeActionNodeList.Add(outputActionNode);
                //    }
                //    else
                //    {
                //        //更新输出变量状态
                //        updateRegVar(outputActionNode);
                //    }
                //}
            }
        }
        /// <summary>
        /// 当左击动作节点
        /// </summary>
        /// <param name="actionNodeItem"></param>
        void onLeftClickActionNode(ActionNodeItem actionNodeItem)
        {
            _curActionNode = null;
            closeMenu();
        }
        /// <summary>
        /// 当右击动作节点
        /// </summary>
        /// <param name="actionNodeItem"></param>
        void onRightClickActionNode(ActionNodeItem actionNodeItem, Vector2 pointerPosition)
        {
            //入口节点不能通过长按编辑
            if (actionNodeItem == _entryNodeItem)
                return;
            //出口节点也不行
            if (actionNodeItem == _returnNodeItem)
                return;
            _curActionNode = getActionNode(actionNodeItem);
            Vector2 position = _ui.worldPositionToGraphPosition(pointerPosition);
            openMenu(position, new string[]
            {
                game.langManager.getString(StringTable.DELETE),//删除
            });
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
            openMenu(position, new string[]
            {
                game.langManager.getString(StringTable.DELETE),//删除
            });
        }
        void onDragActionNode(ActionNodeItem actionNodeItem, Vector2 graphPosition)
        {
            if (actionNodeItem == _entryNodeItem || actionNodeItem == _returnNodeItem)
            {
                //入口节点因为不是动作的一部分所以是不可以移动的。
                return;
            }
            ActionNodeGraphNodeInfo nodeInfo = getNodeInfo(getActionNode(actionNodeItem));
            nodeInfo.posX = graphPosition.x;
            nodeInfo.posY = graphPosition.y;
            _ui.moveActionNodeTo(actionNodeItem, graphPosition);
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
                    _targetChecker.errorTip = (string)value;
            }
            else
            {
                var actionNode = getActionNode(actionNodeItem);
                actionNode.consts[constIndex] = value;
            }
        }
        private async void onClickActionNodeConstValueEditButton(ActionNodeItem actionNodeItem, int index)
        {
            ActionNode actionNode = getActionNode(actionNodeItem);
            ActionDefine actionDefine = _actionDefineDict[actionNode.defineName];
            if (actionDefine.consts[index].type == typeof(ActionValueRef))
            {
                ActionValueRef value = actionNode.consts[index] as ActionValueRef;
                if (value == null)
                {
                    value = new ActionValueRef();
                    actionNode.consts[index] = value;
                }
                pushCurEditObjectToStack();
                await openValueRefGraph(value);
            }
        }
        /// <summary>
        /// 当用户点击返回按钮，出栈之前的显示信息并重新打开之前的编辑界面
        /// </summary>
        void onClickReturnButton()
        {
            popPrevEditObjectFromStackAndOpen();
        }
        #endregion
        /// <summary>
        /// 打开某个效果的生效或者失效动作图
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="entryType"></param>
        private async Task openActionGraph(GeneratedEffect effect, string entryType)
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
            //显示UI
            if (_entryType == nameof(GeneratedEffect.onEnableAction))
            {
                if (_effect.onEnableAction != null)
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONENABLE),
                        new IOConnectInfo[]
                        {
                            new IOConnectInfo()
                            {
                                type = String.Empty,
                                label = String.Empty
                            }
                        },
                        new ActionNodeValueInfo[0],
                        new IOConnectInfo[]
                        {
                            new IOConnectInfo()
                            {
                                type = String.Empty,
                                label = String.Empty
                            }
                        });
                    var actionNodeItem = await createGraph4ActionNode(_effect.onEnableAction);
                    _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0, ActionNodeConnectionType.action);
                }
                else
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONENABLE),
                        new IOConnectInfo[]
                        {
                            new IOConnectInfo()
                            {
                                type = String.Empty,
                                label = String.Empty
                            }
                        },
                        new ActionNodeValueInfo[0],
                        new IOConnectInfo[]
                        {
                            new IOConnectInfo()
                            {
                                type = String.Empty,
                                label = String.Empty
                            }
                        });
                }
            }
            else if (entryType == nameof(GeneratedEffect.onDisableAction))
            {
                if (_effect.onDisableAction != null)
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONDISABLE),
                        new IOConnectInfo[]
                        {
                            new IOConnectInfo()
                            {
                                type = String.Empty,
                                label = String.Empty
                            }
                        },
                        new ActionNodeValueInfo[0],
                        new IOConnectInfo[]
                        {
                            new IOConnectInfo()
                            {
                                type = String.Empty,
                                label = String.Empty
                            }
                        });
                    var actionNodeItem = await createGraph4ActionNode(_effect.onDisableAction);
                    _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0, ActionNodeConnectionType.action);
                }
                else
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONDISABLE),
                        new IOConnectInfo[]
                        {
                            new IOConnectInfo()
                            {
                                type = String.Empty,
                                label = String.Empty
                            }
                        },
                        new ActionNodeValueInfo[0],
                        new IOConnectInfo[]
                        {
                            new IOConnectInfo()
                            {
                                type = String.Empty,
                                label = String.Empty
                            }
                        });
                }
            }
        }
        /// <summary>
        /// 打开一个有输出的值引用动作节点
        /// </summary>
        /// <param name="entryName"></param>
        /// <param name="valueRef"></param>
        /// <param name="graphInfo"></param>
        private async Task openValueRefGraph(ActionValueRef valueRef)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _ui.clearGraph();
            _entryType = null;
            _effect = null;
            _valueRef = valueRef;
            _targetChecker = null;
            _trigger = null;
            _returnNodeItem = _ui.createNode(0, 0,
                game.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT),
                new IOConnectInfo[]
                {
                    new IOConnectInfo()
                    {
                        type = String.Empty,
                        label = String.Empty
                    }
                },
                new ActionNodeValueInfo[0],
                new IOConnectInfo[]
                {
                    new IOConnectInfo()
                    {
                        type = String.Empty,
                        label = String.Empty
                    }
                });
            if (valueRef.action != null)
            {
                ActionNodeItem actionNodeItem = await createGraph4ActionNode(valueRef.action);
                _ui.connectNode(actionNodeItem, valueRef.index + 1, _returnNodeItem, 0, ActionNodeConnectionType.action);
            }
        }
        private async Task openTargetCheckerGraph(TargetChecker targetChecker)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _ui.clearGraph();
            _entryType = null;
            _effect = null;
            _valueRef = null;
            _targetChecker = targetChecker;
            _trigger = null;
            var actionNodeItem = await createGraph4ActionNode(targetChecker.condition.action);
            _returnNodeItem = _ui.createNode(0, 0,
                game.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT),
                new IOConnectInfo[]
                {
                    new IOConnectInfo()
                    {
                        type = String.Empty,
                        label = String.Empty
                    }
                },
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
                        value = _targetChecker.errorTip
                    }
                },
                new IOConnectInfo[]
                {
                    new IOConnectInfo()
                    {
                        type = String.Empty,
                        label = String.Empty
                    }
                });
            _ui.connectNode(actionNodeItem, valueRef.index + 1, _returnNodeItem, 0, ActionNodeConnectionType.value);
        }
        /// <summary>
        /// 为动作节点以及其所有子节点创建动作节点图
        /// </summary>
        /// <param name="actionNode"></param>
        private async Task<ActionNodeItem> createGraph4ActionNode(ActionNode actionNode, ActionNodeItem prevNodeItem = null, ActionNodeItem nextNodeItem = null, float startPosOffset = 0, Dictionary<ActionNode, Vector2> sizeDict = null)
        {
            _actionNodeList.Add(actionNode);
            ActionNodeItem actionNodeItem;
            ActionNodeGraphNodeInfo nodeInfo =
                hasNodeInfo(actionNode) ?
                getNodeInfo(actionNode) : null;
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
                nodeInfo = new ActionNodeGraphNodeInfo(pos.x, pos.y);
                setNodeInfo(actionNode, nodeInfo);
            }
            else if (nextNodeItem != null)
            {
                if (sizeDict == null)
                    sizeDict = new Dictionary<ActionNode, Vector2>();
                Vector2 size = calcActionNodeGraphSize(actionNode, sizeDict);
                pos = new Vector2(nextNodeItem.posX - size.x, startPosOffset);
                nodeInfo = new ActionNodeGraphNodeInfo(pos.x, pos.y);
                setNodeInfo(actionNode, nodeInfo);
            }
            else
            {
                //没有信息也没有前置节点，那我也不知道该放在哪里了。
                pos = new Vector2(0, 0);
                nodeInfo = new ActionNodeGraphNodeInfo(pos.x, pos.y);
                setNodeInfo(actionNode, nodeInfo);
            }
            actionNodeItem = await createNodeUI4ActionNodeAt(actionNode, pos.x, pos.y);
            //为动作分支创建节点图
            float lastNodeOffset = 0;
            for (int i = 0; i < actionNode.branches.Length; i++)
            {
                var nextActionNode = actionNode.branches[i];
                if (nextActionNode == null)
                    continue;
                if (sizeDict == null)
                    sizeDict = new Dictionary<ActionNode, Vector2>();
                var nextActionNodeItem = await createGraph4ActionNode(nextActionNode,
                    prevNodeItem: actionNodeItem,
                    startPosOffset: lastNodeOffset,
                    sizeDict: sizeDict);
                Vector2 size = calcActionNodeGraphSize(nextActionNode, sizeDict);
                lastNodeOffset -= size.y;
                _ui.connectNode(actionNodeItem, i, nextActionNodeItem, 0, ActionNodeConnectionType.action);
            }
            //为输入节点创建节点图
            lastNodeOffset = 0;
            for (int i = 0; i < actionNode.inputs.Length; i++)
            {
                ActionValueRef valueRef = actionNode.inputs[i];
                if (valueRef == null)
                    continue;
                if (sizeDict == null)
                    sizeDict = new Dictionary<ActionNode, Vector2>();
                ActionNode inputActionNode = valueRef.action != null ? valueRef.action : getActionNode(valueRef.actionNodeId);
                try
                {
                    if (!tryGetActionNodeItem(inputActionNode, out ActionNodeItem inputActionNodeItem))
                    {
                        inputActionNodeItem = await createGraph4ActionNode(inputActionNode,
                            nextNodeItem: actionNodeItem,
                            startPosOffset: lastNodeOffset,
                            sizeDict: sizeDict);
                        Vector2 size = calcActionNodeGraphSize(inputActionNode, sizeDict);
                        lastNodeOffset -= size.y;
                    }
                    //输出索引要被动作分支占用若干个空位
                    //输入索引要被前置动作占用一个空位
                    _ui.connectNode(inputActionNodeItem, inputActionNode.branches.Length + valueRef.index, actionNodeItem, 1 + i, ActionNodeConnectionType.value);
                }
                catch (Exception e)
                {
                    Debug.LogError("创建子节点" + inputActionNode + "的图像时发生异常：" + e);
                    continue;
                }
            }
            return actionNodeItem;
        }
        Vector2 calcActionNodeGraphSize(ActionNode actionNode, Dictionary<ActionNode, Vector2> sizeDict)
        {
            if (actionNode == null)
                return Vector2.zero;
            if (sizeDict != null && sizeDict.ContainsKey(actionNode))
                return sizeDict[actionNode];
            Vector2 size = new Vector2();
            //计算输入的宽度
            float inputWidth = 0;
            for (int i = 0; i < actionNode.inputs.Length; i++)
            {
                ActionNode inputNode;
                if (actionNode.inputs[i] != null)
                {
                    if (actionNode.inputs[i].action != null)
                        inputNode = actionNode.inputs[i].action;
                    else
                        inputNode = getActionNode(actionNode.inputs[i].actionNodeId);
                }
                else
                    inputNode = null;
                float width = calcActionNodeGraphSize(inputNode, sizeDict).x;
                if (width > inputWidth)
                    inputWidth = width;
            }
            size.x = _ui.autoPosSpacingWidth + inputWidth;
            float inputHeight = 0;
            //计算输入的高度
            for (int i = 0; i < actionNode.inputs.Length; i++)
            {
                ActionNode inputNode;
                if (actionNode.inputs[i] != null)
                {
                    if (actionNode.inputs[i].action != null)
                        inputNode = actionNode.inputs[i].action;
                    else
                        inputNode = getActionNode(actionNode.inputs[i].actionNodeId);
                }
                else
                    inputNode = null;
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
        /// 在指定位置上为动作节点创建节点图UI，不包括它的子节点。
        /// </summary>
        /// <param name="actionNode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns>动作节点对应的UI组件</returns>
        private async Task<ActionNodeItem> createNodeUI4ActionNodeAt(ActionNode actionNode, float posX, float posY)
        {
            ActionDefine actionDefine = _actionDefineDict[actionNode.defineName];
            ActionNodeItem actionNodeItem;
            if (actionDefine is MethodActionDefine methodActionDefine && methodActionDefine.methodName == "GetBuffDefine")
            {
                //如果是获取增益定义节点，那么它和普通的节点不一样，会显示增益的编辑界面。
                //先获取节点的增益
                GeneratedBuffDefine generatedBuffDefine = null;
                BuffSkin buffSkin = null;
                if (actionNode.consts == null || actionNode.consts.Length < 1)
                {
                    //节点没有增益，那么就为它新建一个
                    generatedBuffDefine = game.card.createBuffDefine(_curCardPoolId);
                    actionNode.consts = new object[] { generatedBuffDefine.id };
                    //同时还有增益的皮肤，也要新建一个
                    buffSkin = game.card.createBuffSkin(_curCardPoolId, generatedBuffDefine.id, game.langManager.getLanguage());
                }
                else if (!(actionNode.consts[0] is int buffDefineId && game.card.tryGetBuffDefine(_curCardPoolId, buffDefineId, out GeneratedBuffDefine buffDefine)))
                {
                    //读到了增益id，但是查无此增益，那么重新创建一个
                    generatedBuffDefine = game.card.createBuffDefine(_curCardPoolId);
                    actionNode.consts[0] = generatedBuffDefine.id;
                    //皮肤也要新建一个
                    buffSkin = game.card.createBuffSkin(_curCardPoolId, generatedBuffDefine.id, game.langManager.getLanguage());
                }
                else
                {
                    //获取到了增益id，并且确实存在对应的增益
                    generatedBuffDefine = buffDefine as GeneratedBuffDefine;
                    //获取增益对应的皮肤
                    buffSkin = game.card.getBuffSkin(generatedBuffDefine);
                    if (buffSkin == null)
                    {
                        //如果不存在皮肤，那就创建一个
                        buffSkin = game.card.createBuffSkin(_curCardPoolId, generatedBuffDefine.id, game.langManager.getLanguage());
                    }
                }
                BuffDefine[] buffDefines = game.card.getBuffDefines(_curCardPoolId);
                ActionNodePropertyModifierViewData[] propModifiersViewData = new ActionNodePropertyModifierViewData[generatedBuffDefine.propModifierList.Count];
                for (int i = 0; i < generatedBuffDefine.propModifierList.Count; i++)
                {
                    PropModifier propModifier = generatedBuffDefine.propModifierList[i];
                    ActionNodePropertyModifierViewData propModifierViewData = new ActionNodePropertyModifierViewData();
                    propModifiersViewData[i] = propModifierViewData;
                    if (propModifier is CostModifier costModifier)
                    {
                        propModifierViewData.propertyTypeIndex = 0;
                        if (costModifier.isSet)
                            propModifierViewData.modifyTypeIndex = 1;
                        else
                            propModifierViewData.modifyTypeIndex = 0;
                        propModifierViewData.valueType = ActionNodeValueType.integer;
                        propModifierViewData.value = costModifier.value;
                    }
                    else if (propModifier is AttackModifier attackModifier)
                    {
                        propModifierViewData.propertyTypeIndex = 1;
                        if (attackModifier.isSet)
                            propModifierViewData.modifyTypeIndex = 1;
                        else
                            propModifierViewData.modifyTypeIndex = 0;
                        propModifierViewData.valueType = ActionNodeValueType.integer;
                        propModifierViewData.value = attackModifier.value;
                    }
                    else if (propModifier is LifeModifier lifeModifier)
                    {
                        propModifierViewData.propertyTypeIndex = 2;
                        if (lifeModifier.isSet)
                            propModifierViewData.modifyTypeIndex = 1;
                        else
                            propModifierViewData.modifyTypeIndex = 0;
                        propModifierViewData.valueType = ActionNodeValueType.integer;
                        propModifierViewData.value = lifeModifier.value;
                    }
                }
                actionNodeItem = _ui.createNodeForBuff(posX, posY, new ActionNodeBuffViewData()
                {
                    buffNames = buffDefines.Select(b => b.ToString()).ToArray(),
                    buffIndex = Array.IndexOf(buffDefines, generatedBuffDefine),
                    buffLanguageIndex = Array.IndexOf(game.langManager.getLanguages(), buffSkin.language),
                    buffName = buffSkin.name,
                    buffDesc = buffSkin.desc,
                    buffImage = await game.card.loadImage(buffSkin.imagePath, _curCardPoolId),
                    propertyTypes = _propertyTypes.Select(s => game.langManager.getString(s)).ToArray(),
                    modifyTypes = _modifyTypes,
                    propModifiersViewData = propModifiersViewData,
                    effectTypeNames = game.card.getEffectNames(),
                    effectNames = game.card.getEffectNames(generatedBuffDefine.effectList.ToArray())
                }, getActionNodeOutputNames(actionNode));
            }
            else
            {
                actionNodeItem = _ui.createNode(
                    posX,
                    posY,
                    getActionNodeName(actionNode),
                    getActionNodeInputInfos(actionNode),
                    getActionNodeConstValueInfo(actionNode),
                    getActionNodeOutputInfos(actionNode));
            }
            _actionNodeItemList.Add(actionNodeItem);
            if (!_actionNodeList.Contains(actionNode))
            {
                _actionNodeList.Add(actionNode);
                actionNode.id = allocActionNodeId();
                graphInfo.freeActionNodeList.Add(actionNode);
                setNodeInfo(actionNode, new ActionNodeGraphNodeInfo(posX, posY));
            }
            return actionNodeItem;
        }
        private ActionNodeItem getActionNodeItem(ActionNode actionNode)
        {
            int index = _actionNodeList.IndexOf(actionNode);
            return _actionNodeItemList[index];
        }
        private bool tryGetActionNodeItem(ActionNode actionNode, out ActionNodeItem actionNodeItem)
        {
            int index = _actionNodeList.IndexOf(actionNode);
            if (index < 0 || index >= _actionNodeItemList.Count)
            {
                actionNodeItem = null;
                return false;
            }
            else
            {
                actionNodeItem = _actionNodeItemList[index];
                return true;
            }
        }
        private ActionNodeItem getActionNodeItem(int actionNodeId)
        {
            return getActionNodeItem(getActionNode(actionNodeId));
        }
        private ActionNode getActionNode(ActionNodeItem actionNodeItem)
        {
            int index = _actionNodeItemList.IndexOf(actionNodeItem);
            return _actionNodeList[index];
        }
        private ActionNode getActionNode(int actionNodeId)
        {
            return _actionNodeList.Find(a => a != null && a.id == actionNodeId);
        }
        /// <summary>
        /// 将动作节点连接到入口节点的动作输出上
        /// </summary>
        /// <param name="inputActionNode"></param>
        private void connectNode4RootAction(ActionNode inputActionNode)
        {
            //判断判断输出是否与输入相连
            if (rootActionNode != null)
            {
                if (rootActionNode == inputActionNode)
                {
                    //输出已经与输入相连，什么都不做
                    return;
                }
                else
                {
                    //输出与其他输入相连，断开连接
                    unconnectNode4RootAction(rootActionNode);
                }
            }
            //判断输入是否与输出相连
            if (rootActionNode == inputActionNode)
            {
                //输入已经与输出相连，什么都不做
                return;
            }
            else if (tryGetPrevActionNode(inputActionNode, out ActionNode prevActionNode, out int prevOutputIndex))
            {
                //如果输入已经和其他输出连接，则断开。
                unconnectNode4Action(prevActionNode, prevOutputIndex, inputActionNode);
            }
            //建立新的连接
            rootActionNode = inputActionNode;
            //从离散节点列表中去掉被连接的节点
            graphInfo.freeActionNodeList.Remove(inputActionNode);
            //更新ui
            _ui.connectNode(_entryNodeItem, 0, getActionNodeItem(inputActionNode), 0, ActionNodeConnectionType.action);
        }
        /// <summary>
        /// 将节点的值输出连接到返回值节点的输入上
        /// </summary>
        /// <param name="outputActionNode"></param>
        /// <param name="outputIndex"></param>
        /// <param name="inputIndex"></param>
        private void connectNode4ReturnValueRef(ActionNode outputActionNode, int outputIndex, int inputIndex)
        {
            if (_valueRef != null)
            {
                //判断是否已经相连
                if (rootActionNode == outputActionNode)
                {
                    //已经相连，什么也不做
                    return;
                }
                else
                {
                    //如果输入已经和其他输出连接，则断开。
                    unconnectNode4ReturnValueRef(outputActionNode, outputIndex, inputIndex);
                }
                //建立新的连接
                rootActionNode = outputActionNode;
                //从离散节点列表中去掉被连接的节点
                graphInfo.freeActionNodeList.Remove(outputActionNode);
                //更新ui
                _ui.connectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, _entryNodeItem, 0, ActionNodeConnectionType.value);
            }
            else
            {
                //目前只支持对值引用的编辑
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// 连接节点的动作输入输出
        /// </summary>
        /// <param name="outputActionNode"></param>
        /// <param name="inputActionNode"></param>
        private void connectNode4Action(ActionNode outputActionNode, int outputIndex, ActionNode inputActionNode)
        {
            //判断输出是否已经和输入相连
            if (outputActionNode.branches != null &&
                outputActionNode.branches.Length > outputIndex)
            {
                if (outputActionNode.branches[outputIndex] == inputActionNode)
                {
                    //输出已经和输入相连，什么都不做
                    return;
                }
                else if (outputActionNode.branches[outputIndex] != null)
                {
                    //输出已经和其他节点输入相连，断开
                    unconnectNode4Action(outputActionNode, outputIndex, outputActionNode.branches[outputIndex]);
                }
            }
            else
            {
                //数组长度不够，扩大
                ActionNode[] branches = new ActionNode[outputIndex + 1];
                if (outputActionNode.branches != null)
                {
                    Array.Copy(outputActionNode.branches, 0, branches, 0, outputActionNode.branches.Length);
                }
                outputActionNode.branches = branches;
            }
            //判断输入是否已经和输出相连
            if (rootActionNode == inputActionNode)
            {
                //输入与入口动作输出相连，断开
                unconnectNode4RootAction(inputActionNode);
            }
            else if (tryGetPrevActionNode(inputActionNode, out ActionNode prevActionNode, out int prevOutputIndex))
            {
                if (prevActionNode == outputActionNode && prevOutputIndex == outputIndex)
                {
                    //输入已经和输出相连，什么也不做
                    return;
                }
                else
                {
                    //输入已经和其他输出连接，则断开。
                    unconnectNode4Action(prevActionNode, prevOutputIndex, inputActionNode);
                }
            }
            //建立新的连接
            outputActionNode.branches[outputIndex] = inputActionNode;
            //从离散节点列表中去掉被连接的节点
            graphInfo.freeActionNodeList.Remove(inputActionNode);
            //更新ui
            _ui.connectNode(getActionNodeItem(outputActionNode), outputIndex, getActionNodeItem(inputActionNode), 0, ActionNodeConnectionType.action);
        }
        /// <summary>
        /// 连接节点的值输入输出
        /// </summary>
        /// <param name="outputActionNode"></param>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNode"></param>
        /// <param name="inputIndex"></param>
        private void connectNode4ValueRef(ActionNode outputActionNode, int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            //值的输出可以与任意数量的输入相连，所以不做检查
            //判断输入是否已经和输出相连
            if (inputActionNode.inputs != null &&
                inputActionNode.inputs.Length > inputIndex)
            {
                if (inputActionNode.inputs[inputIndex] != null)
                {
                    if ((inputActionNode.inputs[inputIndex].action == outputActionNode || inputActionNode.inputs[inputIndex].actionNodeId == outputActionNode.id) &&
                        inputActionNode.inputs[inputIndex].index == outputIndex)
                    {
                        //输入已经与输出相连，什么都不做
                        return;
                    }
                    else if (inputActionNode.inputs[inputIndex].action != null)
                    {
                        //输入与其他输出相连，断开
                        unconnectNode4ValueRef(inputActionNode.inputs[inputIndex].action, inputActionNode.inputs[inputIndex].index, inputActionNode, inputIndex);
                    }
                    else if (inputActionNode.inputs[inputIndex].actionNodeId != 0)
                    {
                        //输入与其他输出相连，断开
                        unconnectNode4ValueRef(getActionNode(inputActionNode.inputs[inputIndex].actionNodeId), inputActionNode.inputs[inputIndex].index, inputActionNode, inputIndex);
                    }
                    else if (!string.IsNullOrEmpty(inputActionNode.inputs[inputIndex].eventVarName))
                    {
                        //输入与触发节点值输出相连，断开
                        unconnectNode4TriggerEventVarRef(getEventVarNameIndex(inputActionNode.inputs[inputIndex].eventVarName), inputActionNode, inputIndex);
                    }
                }
            }
            else
            {
                //数组长度不够，增长
                ActionValueRef[] inputs = new ActionValueRef[inputIndex + 1];
                if (inputActionNode.inputs != null)
                {
                    Array.Copy(inputActionNode.inputs, 0, inputs, 0, inputActionNode.inputs.Length);
                }
                inputActionNode.inputs = inputs;
            }
            //建立新的连接，一个输出值可以与多个输入相连
            //要从当前叶子节点出发，寻找父节点直到根
            //如果这个输出节点是第一次被引用，那么保存它的完整对象
            //否则保存它的id，然后从环境中获取它的输出值。
            //对于包含多个分支的动作，这里会有一个作用域的问题。
            //就像是在if的分支里面声明一个变量，在后续动作作用域中这个变量不一定都存在。
            //目前选择的解决方法是对于超出作用域的节点是无法通过id引用的。
            //这种解决方案会导致一个问题，那就是在ui上显示为一个的节点，实际上可能是多个不同的节点。
            if (!isActionNodeValueRefedInPrevNodes(outputActionNode, inputActionNode, inputIndex))
            {
                inputActionNode.inputs[inputIndex] = new ActionValueRef(outputActionNode, outputIndex);
                //遍历子节点，使其中所有对相同值输出的引用从直接引用变为id引用
                traverseActionNode(inputActionNode, false, a =>
                {
                    if (a == inputActionNode)
                        return;
                    if (a.inputs != null && a.inputs.Length > 0)
                    {
                        for (int i = 0; i < a.inputs.Length; i++)
                        {
                            if (a.inputs[i] == null)
                                continue;
                            if (a.inputs[i].action == outputActionNode)
                            {
                                a.inputs[i].action = null;
                                a.inputs[i].actionNodeId = outputActionNode.id;
                            }
                        }
                    }
                });
            }
            else
            {
                inputActionNode.inputs[inputIndex] = new ActionValueRef(outputActionNode.id, outputIndex);
            }
            //更新输出节点的输出变量状态
            updateRegVar(outputActionNode);
            //从离散节点列表中去掉被连接的节点
            graphInfo.freeActionNodeList.Remove(inputActionNode);
            //更新ui
            _ui.connectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, getActionNodeItem(inputActionNode), 0, ActionNodeConnectionType.action);
        }
        private void unconnectNode4RootAction(ActionNode inputActionNode)
        {
            if (inputActionNode == rootActionNode)
            {
                //断开与入口动作节点的连接
                rootActionNode = null;
            }
            //输入节点一定会变得离散
            if (!graphInfo.freeActionNodeList.Contains(inputActionNode))
                graphInfo.freeActionNodeList.Add(inputActionNode);
            _ui.unconnectNode(_entryNodeItem, 0, getActionNodeItem(inputActionNode), 0);
        }
        private void unconnectNode4ReturnValueRef(ActionNode outputActionNode, int outputIndex, int inputIndex)
        {
            if (rootActionNode == outputActionNode)
            {
                //断开与返回值节点的连接
                rootActionNode = null;
            }
            //输出节点一定会变得离散
            if (!graphInfo.freeActionNodeList.Contains(outputActionNode))
                graphInfo.freeActionNodeList.Add(outputActionNode);
            _ui.unconnectNode(getActionNodeItem(outputActionNode), valueRef.index + 1, _returnNodeItem, 0);
        }
        /// <summary>
        /// 断开节点的动作输入输出连接
        /// </summary>
        /// <param name="outputActionNode"></param>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNode"></param>
        private void unconnectNode4Action(ActionNode outputActionNode, int outputIndex, ActionNode inputActionNode)
        {
            //断开节点连接
            if (outputActionNode.branches != null &&
                outputActionNode.branches.Length > outputIndex)
            {
                outputActionNode.branches[outputIndex] = null;
            }
            //更新节点是否离散
            //输入节点一定会变得离散
            if (!graphInfo.freeActionNodeList.Contains(inputActionNode))
                graphInfo.freeActionNodeList.Add(inputActionNode);
            //更新ui
            _ui.unconnectNode(getActionNodeItem(outputActionNode), outputIndex, getActionNodeItem(inputActionNode), 0);
        }
        /// <summary>
        /// 断开节点的值输入输出连接
        /// </summary>
        /// <param name="outputActionNode"></param>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNode"></param>
        /// <param name="inputIndex"></param>
        private void unconnectNode4ValueRef(ActionNode outputActionNode, int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            ActionNodeItem inputActionNodeItem = getActionNodeItem(inputActionNode);
            ActionNodeItem outputActionNodeItem = getActionNodeItem(outputActionNode);
            ActionValueRef inputActionValueRef = inputActionNode.inputs[inputIndex];
            //断开之前的节点连接
            ActionDefine inputActionDefine = _actionDefineDict[inputActionNode.defineName];
            if (inputActionDefine.inputs.Last().isParams && inputIndex >= inputActionDefine.inputs.Length - 1)
            {
                //如果断开的是多参数节点
                List<ActionValueRef> inputList = new List<ActionValueRef>(inputActionNode.inputs);
                inputList.RemoveAt(inputIndex);
                inputActionNode.inputs = inputList.ToArray();
                //更新ui
                _ui.unconnectNodeAllInput(inputActionNodeItem);
                for (int i = 0; i < inputActionNode.branches.Length; i++)
                {
                    _ui.connectNode(getActionNodeItem(inputActionNode.branches[i]), 0, inputActionNodeItem, i, ActionNodeConnectionType.value);
                }
                for (int i = inputActionNode.branches.Length; i < inputList.Count; i++)
                {
                    _ui.connectNode(getActionNodeItem(inputList[i].action), inputList[i].index + 1, inputActionNodeItem, i, ActionNodeConnectionType.value);
                }
            }
            else
            {
                //不是端参数，直接断开
                inputActionNode.inputs[inputIndex] = null;
                //更新ui
                _ui.unconnectNode(outputActionNodeItem, outputActionNode.branches.Length + outputIndex, inputActionNodeItem, inputActionNode.branches.Length + inputIndex);
            }
            if (inputActionValueRef.action != null)
            {
                //寻找子节点中的第一个值引用，将其改为引用对象本身
                traverseActionNode(inputActionNode, false, a =>
                {
                    if (a.inputs == null)
                        return false;
                    for (int i = 0; i < a.inputs.Length; i++)
                    {
                        if (a.inputs[i] != null && a.inputs[i].actionNodeId == inputActionValueRef.action.id)
                        {
                            a.inputs[i].action = inputActionValueRef.action;
                            a.inputs[i].actionNodeId = 0;
                            return true;
                        }
                    }
                    return false;
                });
            }
            //更新离散和输出状态
            if (isFreeActionNode(outputActionNode))
            {
                outputActionNode.regVar = null;
                //如果输出节点变为离散节点，将其添加进入列表
                if (!graphInfo.freeActionNodeList.Contains(outputActionNode))
                    graphInfo.freeActionNodeList.Add(outputActionNode);
            }
            else
            {
                //更新输出变量状态
                updateRegVar(outputActionNode);
            }
        }
        /// <summary>
        /// 断开一个节点和其他节点的所有连接
        /// </summary>
        /// <param name="actionNode"></param>
        private void unconnectNodeAll(ActionNode actionNode)
        {
            //与入口节点断开连接
            if (rootActionNode == actionNode)
            {
                if (valueRef != null)
                    unconnectNode4ReturnValueRef(actionNode, valueRef.index, 0);
                else
                    unconnectNode4RootAction(actionNode);
            }
            if (actionNode.inputs != null)
            {
                for (int i = 0; i < actionNode.inputs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(actionNode.inputs[i].eventVarName))
                    {
                        unconnectNode4TriggerEventVarRef(getEventVarNameIndex(actionNode.inputs[i].eventVarName), actionNode, i);
                    }
                }
            }
            //与其他节点断开连接
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                if (_actionNodeList[i] == null)
                    continue;
                //与输出动作断开连接
                int index;
                if (actionNode.branches != null)
                {
                    index = Array.IndexOf(actionNode.branches, _actionNodeList[i]);
                    if (index >= 0)
                    {
                        unconnectNode4Action(actionNode, index, _actionNodeList[i]);
                    }
                }
                //与输出值断开连接
                if (_actionNodeList[i].inputs != null)
                {
                    index = Array.FindIndex(_actionNodeList[i].inputs, v => v != null && (v.action == actionNode || v.actionNodeId == actionNode.id));
                    if (index >= 0)
                    {
                        unconnectNode4ValueRef(actionNode, _actionNodeList[i].inputs[index].index, _actionNodeList[i], index);
                    }
                }
                //与输入动作断开连接
                if (_actionNodeList[i].branches != null)
                {
                    index = Array.IndexOf(_actionNodeList[i].branches, actionNode);
                    if (index >= 0)
                    {
                        unconnectNode4Action(_actionNodeList[i], index, actionNode);
                    }
                }
                //与输入值断开连接
                if (actionNode.inputs != null)
                {
                    index = Array.FindIndex(actionNode.inputs, v => v != null && (v.action == _actionNodeList[i] || v.actionNodeId == _actionNodeList[i].id));
                    if (index >= 0)
                    {
                        unconnectNode4ValueRef(_actionNodeList[i], actionNode.inputs[index].index, actionNode, index);
                    }
                }
            }
        }
        /// <summary>
        /// 判断一个节点是否是离散的节点，实际上不影响游戏但是可能对没有完成的编辑工作有用所以保存在编辑器里
        /// </summary>
        /// <param name="actionNode"></param>
        /// <returns></returns>
        private bool isFreeActionNode(ActionNode actionNode)
        {
            bool isFree = true;
            traverseActionNode(rootActionNode, true, a =>
            {
                if (a == actionNode)
                    isFree = false;
            });
            return isFree;
        }
        /// <summary>
        /// 获取当前节点的父节点。
        /// </summary>
        /// <param name="actionNode"></param>
        /// <returns></returns>
        private bool tryGetPrevActionNode(ActionNode actionNode, out ActionNode prevActionNode, out int outputIndex)
        {
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                if (_actionNodeList[i] == null)
                    continue;
                if (_actionNodeList[i].branches != null)
                {
                    for (int j = 0; j < _actionNodeList[i].branches.Length; j++)
                    {
                        if (_actionNodeList[i].branches[j] == actionNode)
                        {
                            prevActionNode = _actionNodeList[i];
                            outputIndex = j;
                            return true;
                        }
                    }
                }
            }
            prevActionNode = null;
            outputIndex = -1;
            return false;
        }
        private void traverseActionNode(ActionNode actionNode, bool traverseInputs, Action<ActionNode> action)
        {
            if (action == null)
                return;
            traverseActionNode(actionNode, traverseInputs, a =>
            {
                action.Invoke(a);
                return false;
            });
        }
        /// <summary>
        /// 遍历动作节点和它的子节点
        /// </summary>
        /// <param name="actionNode"></param>
        /// <param name="action">返回值表示是否终止遍历</param>
        private void traverseActionNode(ActionNode actionNode, bool traverseInputs, Func<ActionNode, bool> action)
        {
            if (actionNode == null)
                return;
            if (action == null)
                return;
            bool isBreak = action.Invoke(actionNode);
            if (isBreak)
                return;
            if (traverseInputs)
            {
                if (actionNode.inputs != null && actionNode.inputs.Length > 0)
                {
                    for (int i = 0; i < actionNode.inputs.Length; i++)
                    {
                        if (actionNode.inputs[i] == null)
                            continue;
                        if (actionNode.inputs[i].action != null)
                            traverseActionNode(actionNode.inputs[i].action, traverseInputs, action);
                        else
                            traverseActionNode(getActionNode(actionNode.inputs[i].actionNodeId), traverseInputs, action);
                    }
                }
            }
            if (actionNode.branches != null && actionNode.branches.Length > 0)
            {
                for (int i = 0; i < actionNode.branches.Length; i++)
                {
                    traverseActionNode(actionNode.branches[i], traverseInputs, action);
                }
            }
        }
        private int getValueRefCount(ActionNode actionNode, int outputIndex)
        {
            int count = 0;
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                if (_actionNodeList[i].inputs == null || _actionNodeList[i].inputs.Length < 1)
                    continue;
                for (int j = 0; j < _actionNodeList[i].inputs.Length; j++)
                {
                    if (_actionNodeList[i].inputs[j] == null)
                        continue;
                    if ((_actionNodeList[i].inputs[j].action == actionNode || _actionNodeList[i].inputs[j].actionNodeId == actionNode.id) &&
                        _actionNodeList[i].inputs[j].index == outputIndex)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        /// <summary>
        /// 获取引用了特定节点的输出的所有节点以及引用的输入索引
        /// </summary>
        /// <returns></returns>
        private (ActionNode actionNode, int index)[] getValueRefedActionNodes(ActionNode outputActionNode, int outputIndex)
        {
            List<(ActionNode actionNode, int index)> list = new List<(ActionNode actionNode, int index)>();
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                if (_actionNodeList[i].inputs == null || _actionNodeList[i].inputs.Length < 1)
                    continue;
                for (int j = 0; j < _actionNodeList[i].inputs.Length; j++)
                {
                    if (_actionNodeList[i].inputs[j] == null)
                        continue;
                    if ((_actionNodeList[i].inputs[j].action == outputActionNode || _actionNodeList[i].inputs[j].actionNodeId == outputActionNode.id) &&
                        _actionNodeList[i].inputs[j].index == outputIndex)
                    {
                        list.Add((_actionNodeList[i], j));
                    }
                }
            }
            return list.ToArray();
        }
        /// <summary>
        /// 获取一个动作节点的根节点，如果它已经是根节点那么就返回它。
        /// </summary>
        /// <param name="actionNode"></param>
        /// <returns></returns>
        private ActionNode getRootNode(ActionNode actionNode)
        {
            if (actionNode == null)
                return null;
            if (tryGetPrevActionNode(actionNode, out ActionNode prevActionNode, out _))
            {
                return getRootNode(prevActionNode);
            }
            else
                return actionNode;
        }
        /// <summary>
        /// 判断一个节点是否在当前节点之前的动作节点中被引用了。
        /// </summary>
        /// <param name="valueActionNode"></param>
        /// <param name="curActionNode"></param>
        /// <returns></returns>
        private bool isActionNodeValueRefedInPrevNodes(ActionNode valueActionNode, ActionNode curActionNode, int curIndex)
        {
            //判断当前节点是否已经引用了
            if (isActionNodeValueRefedInNode(valueActionNode, curActionNode, curIndex))
            {
                return true;
            }
            if (!tryGetPrevActionNode(curActionNode, out curActionNode, out _))
            {
                return false;
            }
            //判断父节点
            while (curActionNode != null)
            {
                if (isActionNodeValueRefedInNode(valueActionNode, curActionNode))
                {
                    return true;
                }
                if (!tryGetPrevActionNode(curActionNode, out curActionNode, out _))
                {
                    return false;
                }
            }
            return false;
        }
        private bool isActionNodeValueRefedInNode(ActionNode valueActionNode, ActionNode actionNode, int curIndex = -1)
        {
            if (valueActionNode == actionNode)
                return true;
            if (actionNode.inputs == null)
                return false;
            if (curIndex < 0 || curIndex >= actionNode.inputs.Length)
            {
                for (int i = 0; i < actionNode.inputs.Length; i++)
                {
                    ActionNode childActionNode;
                    if (actionNode.inputs[i].action != null)
                        childActionNode = actionNode.inputs[i].action;
                    else
                        childActionNode = getActionNode(actionNode.inputs[i].actionNodeId);
                    if (isActionNodeValueRefedInNode(valueActionNode, childActionNode))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                for (int i = 0; i < curIndex; i++)
                {
                    ActionNode childActionNode;
                    if (actionNode.inputs[i].action != null)
                        childActionNode = actionNode.inputs[i].action;
                    else
                        childActionNode = getActionNode(actionNode.inputs[i].actionNodeId);
                    if (isActionNodeValueRefedInNode(valueActionNode, childActionNode))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 判断一个节点是否在父节点和父节点之上的节点中被引用为值
        /// </summary>
        /// <param name="actionNode">要检查的节点</param>
        /// <param name="parentNode">父节点，检查的节点包括该节点</param>
        /// <returns></returns>
        private bool isValueRefedInParentNodes(ActionNode actionNode, ActionNode parentNode)
        {
            bool result = false;
            traverseActionNode(rootActionNode, true, a =>
            {
                if (a == actionNode)
                {
                    result = true;
                }
                if (a == parentNode)
                    return true;
                return false;
            });
            return result;
            //if (parentNode == null)
            //    return false;
            //if (parentNode == actionNode)
            //    return true;
            //if (parentNode.inputs != null)
            //{
            //    for (int i = 0; i < parentNode.inputs.Length; i++)
            //    {
            //        if (parentNode.inputs[i] == null)
            //            continue;
            //        if (prevNode != null && (parentNode.inputs[i].action == prevNode || parentNode.inputs[i].actionNodeId == prevNode.id))
            //            continue;
            //        if (isValueRefedInParentNodes(actionNode, parentNode.inputs[i].action != null ?
            //            parentNode.inputs[i].action :
            //            getActionNode(parentNode.inputs[i].actionNodeId)))
            //        {
            //            return true;
            //        }
            //    }
            //}
            //if (isValueRefedInParentNodes(actionNode, getPrevActionNode(parentNode), parentNode))
            //{
            //    return true;
            //}
            //return false;
        }
        private int getOutputCount(ActionNode actionNode)
        {
            ActionDefine actionDefine = _actionDefineDict[actionNode.defineName];
            return actionDefine.outputs.Length;
        }
        private void updateRegVar(ActionNode actionNode)
        {
            int outputCount = getOutputCount(actionNode);
            if (actionNode.regVar == null || actionNode.regVar.Length != outputCount)
            {
                actionNode.regVar = new bool[outputCount];
            }
            for (int i = 0; i < actionNode.regVar.Length; i++)
            {
                actionNode.regVar[i] = getValueRefCount(actionNode, i) > 1;
            }
        }
        /// <summary>
        /// 将当前正在编辑的对象入栈
        /// </summary>
        void pushCurEditObjectToStack()
        {
            if (_trigger != null)
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _cardDefine, null, _trigger, null));
            else if (_targetChecker != null)
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _cardDefine, null, _targetChecker, null));
            else if (_valueRef != null)
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _cardDefine, null, _valueRef, null));
            else
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _cardDefine, _entryType, _effect, null));
            _ui.setReturnButtonActive(true);
        }
        void popPrevEditObjectFromStackAndOpen()
        {
            display(_editObjectStack.Pop());
        }
        string getActionNodeName(ActionNode actionNode)
        {
            if (_nameTextDict.TryGetValue(actionNode.defineName, out string text))
            {
                return game.langManager.getString(text);
            }
            else
            {
                Debug.LogWarning("没有找到动作" + actionNode.defineName + "对应的名称");
                return actionNode.defineName;
            }
        }
        string[] getActionNodeInputNames(ActionNode actionNode)
        {
            //TODO:这里应该考虑动作节点输入的翻译，但是暂时先不这么做。
            ActionDefine actionDefine = _actionDefineDict[actionNode.defineName];
            List<string> inputNameList = new List<string>();
            //前一个动作节点
            inputNameList.Add(string.Empty);
            //输入
            string inputName;
            for (int i = 0; i < actionNode.inputs.Length; i++)
            {
                if (actionDefine.inputs.Length > 0 && actionDefine.inputs.Last().isParams && i >= actionDefine.inputs.Length - 1)
                {
                    inputName = actionDefine.inputs.Last().name;
                    if (_nameTextDict.TryGetValue(inputName, out inputName))
                    {
                        inputName = game.langManager.getString(inputName);
                    }
                    inputNameList.Add(inputName + (i - actionDefine.inputs.Length + 2));
                }
                else if (i < actionDefine.inputs.Length)
                {
                    //有可能由于ActionDefine的参数数量变化导致存在的输入参数比实际的多，忽略已有的参数
                    inputNameList.Add(actionDefine.inputs[i].name);
                }
            }
            if (actionDefine.inputs.Length > 0 && actionDefine.inputs.Last().isParams)
            {
                inputName = actionDefine.inputs.Last().name;
                if (_nameTextDict.TryGetValue(inputName, out inputName))
                {
                    inputName = game.langManager.getString(inputName);
                }
                inputNameList.Add(inputName + (actionNode.inputs.Length - actionDefine.inputs.Length + 2));
            }
            return inputNameList.ToArray();
        }
        
        
        IOConnectInfo[] getActionNodeInputInfos(ActionNode actionNode)
        {
            //TODO:这里应该考虑动作节点输入的翻译，但是暂时先不这么做。
            ActionDefine actionDefine = _actionDefineDict[actionNode.defineName];
            List<IOConnectInfo> inputNameList = new List<IOConnectInfo>();
            //前一个动作节点
            inputNameList.Add(new IOConnectInfo()
            {
                type = String.Empty,
                label = string.Empty
            });
            //输入
            string inputName;
            for (int i = 0; i < actionNode.inputs.Length; i++)
            {
                if (actionDefine.inputs.Length > 0 && actionDefine.inputs.Last().isParams && i >= actionDefine.inputs.Length - 1)
                {
                    inputName = actionDefine.inputs.Last().name;
                    if (_nameTextDict.TryGetValue(inputName, out inputName))
                    {
                        inputName = game.langManager.getString(inputName);
                    }
                    Debug.Log("inputs1：" + inputName);
                    inputNameList.Add(new IOConnectInfo()
                    {
                        type = inputName,
                        label = inputName + (i - actionDefine.inputs.Length + 2)
                    });
                }
                else if (i < actionDefine.inputs.Length)
                {
                    //有可能由于ActionDefine的参数数量变化导致存在的输入参数比实际的多，忽略已有的参数
                    Debug.Log("inputs2：" + actionDefine.inputs[i].name);
                    inputNameList.Add(new IOConnectInfo()
                    {
                        type = actionDefine.inputs[i].name,
                        label = actionDefine.inputs[i].name
                    });
                }
            }
            if (actionDefine.inputs.Length > 0 && actionDefine.inputs.Last().isParams)
            {
                inputName = actionDefine.inputs.Last().name;
                if (_nameTextDict.TryGetValue(inputName, out inputName))
                {
                    inputName = game.langManager.getString(inputName);
                }
                
                Debug.Log("inputs3：" + inputName);
                inputNameList.Add(new IOConnectInfo()
                {
                    type = inputName,
                    label = inputName + (actionNode.inputs.Length - actionDefine.inputs.Length + 2)
                });
            }
            return inputNameList.ToArray();
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
                else if (type.IsEnum)
                {
                    infoArray[i].type = ActionNodeValueType.enumValue;
                    infoArray[i].enumType = type;
                }
                else if (type == typeof(ActionValueRef))
                {
                    infoArray[i].type = ActionNodeValueType.actionValueRef;
                }
                else
                    throw new NotImplementedException("没有" + type + "对应的常数类型");
            }
            return infoArray;
        }
        string[] getActionNodeOutputNames(ActionNode actionNode)
        {
            //TODO:这里应该考虑动作节点输出的翻译，但是暂时先不这么做。
            ActionDefine actionDefine = _actionDefineDict[actionNode.defineName];
            string[] outputNames = new string[actionDefine.outputs.Length + actionNode.branches.Length];
            for (int i = 0; i < outputNames.Length; i++)
            {
                if (i < actionNode.branches.Length)
                {
                    //TODO:下一个动作节点不知道该叫啥，就先不给名字好了
                    outputNames[i] = string.Empty;
                }
                else
                {
                    outputNames[i] = actionDefine.outputs[i - actionNode.branches.Length].name;
                }
            }
            return outputNames;
        }
        
        IOConnectInfo[] getActionNodeOutputInfos(ActionNode actionNode)
        {
            //TODO:这里应该考虑动作节点输出的翻译，但是暂时先不这么做。
            ActionDefine actionDefine = _actionDefineDict[actionNode.defineName];
   
            IOConnectInfo[] outputNames = new IOConnectInfo[actionDefine.outputs.Length + actionNode.branches.Length];
            for (int i = 0; i < outputNames.Length; i++)
            {
                if (i < actionNode.branches.Length)
                {
                    //TODO:下一个动作节点不知道该叫啥，就先不给名字好了
                    outputNames[i] = new IOConnectInfo()
                    {
                        type = String.Empty,
                        label = String.Empty
                    };
                }
                else
                {
                    Debug.Log("output：" + actionDefine.outputs[i - actionNode.branches.Length].name);
                    outputNames[i] = new IOConnectInfo()
                    {
                        type = actionDefine.outputs[i - actionNode.branches.Length].name,
                        label = actionDefine.outputs[i - actionNode.branches.Length].name
                    };
                }
            }
            return outputNames;
        }
        /// <summary>
        /// 为动作节点分配id，
        /// </summary>
        /// <returns></returns>
        int allocActionNodeId()
        {
            int id = 1;
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                if (_actionNodeList[i] == null)
                {
                    _actionNodeList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_actionNodeList[i].id >= id)
                    id = _actionNodeList[i].id + 1;
            }
            return id;
        }
        ActionNodeGraphNodeInfo getNodeInfo(ActionNode node)
        {
            return graphInfo.actionInfoDict[node.id.ToString()];
        }
        void setNodeInfo(ActionNode node, ActionNodeGraphNodeInfo graphNodeInfo)
        {
            graphInfo.actionInfoDict[node.id.ToString()] = graphNodeInfo;
        }
        bool hasNodeInfo(ActionNode node)
        {
            return graphInfo.actionInfoDict.ContainsKey(node.id.ToString());
        }
        bool removeNodeInfo(ActionNode node)
        {
            return graphInfo.actionInfoDict.Remove(node.id.ToString());
        }
        private Task<ActionNodeItem> createActionNodeAtCurPos(string defineName, ActionDefine actionDefine)
        {
            var actionNode = new ActionNode(defineName,
                new ActionValueRef[actionDefine.inputs.Length > 0 && actionDefine.inputs.Last().isParams ? actionDefine.inputs.Length - 1 : actionDefine.inputs.Length],
                new object[actionDefine.consts.Length],
                new ActionNode[1]);
            return createNodeUI4ActionNodeAt(actionNode, _curPosX, _curPosY);
        }
        private void enterSubMenu(int[] index)
        {
            List<BackgroundMenuItem> optionList = _backgroundMenuOptionList;
            for (int i = 0; i < index.Length; i++)
            {
                optionList = optionList[index[i]].childList;
            }
            List<string> titleList = new List<string>();
            if (index.Length > 0)
            {
                titleList.Add(game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_MENU_RETURN + " <"));
            }
            for (int i = 0; i < optionList.Count; i++)
            {
                titleList.Add(optionList[i].childList != null ? optionList[i].title + " >" : optionList[i].title);
            }
            _menuLayerStack = index;
            _ui.updateMenu(titleList.ToArray());
        }
        /// <summary>
        /// 在对应位置打开菜单
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="options">菜单选项</param>
        private void openMenu(float posX, float posY, string[] options)
        {
            _curPosX = posX;
            _curPosY = posY;
            _ui.openMenu(posX, posY, options);
        }
        /// <summary>
        /// 在对应位置打开菜单
        /// </summary>
        /// <param name="position"></param>
        /// <param name="options">菜单选项</param>
        private void openMenu(Vector2 postion, string[] options)
        {
            openMenu(postion.x, postion.y, options);
        }
        private void closeMenu()
        {
            _menuLayerStack = null;
            _ui.closeMenu();
        }
        private string[] getTranslatedTargetCheckerTypes()
        {
            string[] targetCheckerTypes = new string[_targetCheckerTypes.Length];
            for (int i = 0; i < targetCheckerTypes.Length; i++)
            {
                targetCheckerTypes[i] = game.langManager.getString(_nameTextDict[_targetCheckerTypes[i]]);
            }
            return targetCheckerTypes;
        }
        #endregion
        [Obsolete]
        public IAppManager app { get; }
        [Obsolete]
        public MainCtrl main { get; }
        [SerializeField]
        ActionNodeGraphPanel _ui;
        Dictionary<string, ActionDefine> _actionDefineDict;
        int[] _menuLayerStack = new int[0];
        List<BackgroundMenuItem> _backgroundMenuOptionList = new List<BackgroundMenuItem>();
        float _curPosX;
        float _curPosY;
        readonly List<ActionNode> _actionNodeList = new List<ActionNode>();
        readonly List<ActionNodeItem> _actionNodeItemList = new List<ActionNodeItem>();
        ActionNodeGraphInfo graphInfo
        {
            get { return scene.effectEditor.graphInfo; }
        }
        ActionNodeItem _entryNodeItem;
        ActionNodeItem _returnNodeItem;
        ActionNode rootActionNode
        {
            get
            {
                if (_effect != null)
                {
                    if (_entryType == nameof(GeneratedEffect.onEnableAction))
                        return _effect.onEnableAction;
                    else if (_entryType == nameof(GeneratedEffect.onDisableAction))
                        return _effect.onDisableAction;
                    else
                        return _effect.getProp<ActionNode>(_entryType);
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
                    if (_entryType == nameof(GeneratedEffect.onEnableAction))
                        _effect.onEnableAction = value;
                    else if (_entryType == nameof(GeneratedEffect.onDisableAction))
                        _effect.onDisableAction = value;
                    else
                        _effect.setProp(_entryType, value);
                }
                else if (_trigger != null)
                    _trigger.action = value;
                else if (valueRef != null)
                    valueRef.action = value;
            }
        }
        string _entryType;
        int _curCardPoolId = 0;
        CardDefine _cardDefine;
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
        string[] _targetCheckerTypes = new string[]
        {
            TARGET_TYPE_CHARACTER,
            TARGET_TYPE_SERVANT
        };
        string[] _propertyTypes = new string[]
        {
            StringTable.GAME_CARD_PROP_NAME_COST,
            StringTable.GAME_CARD_PROP_NAME_ATTACK,
            StringTable.GAME_CARD_PROP_NAME_LIFE
        };
        string[] _modifyTypes = new string[]
        {
            StringTable.GAME_CARD_PROP_MODIFY_ADD,
            StringTable.GAME_CARD_PROP_MODIFY_SETTO
        };
        readonly Dictionary<string, string> _nameTextDict = new Dictionary<string, string>()
        {
            { TARGET_TYPE_CHARACTER, StringTable.GAME_TARGET_TYPE_CHARACTER },
            { TARGET_TYPE_SERVANT, StringTable.GAME_TARGET_TYPE_SERVANT },
            //节点名称
            { "IntegerOperation", StringTable.EDITOR_NODE_NAME_INTEGER_OPERATION },
            { "LogicOperation", StringTable.EDIOTR_NODE_NAME_LOGIC_OPERATION },
            { "Compare", StringTable.EDIOTR_NODE_NAME_COMPARE },
            { "IntegerConst", StringTable.EDIOTR_NODE_NAME_INTEGER_CONST },
            { "StringConst", StringTable.EDIOTR_NODE_NAME_STRING_CONST },
            { "BooleanConst", StringTable.EDIOTR_NODE_NAME_BOOLEAN_CONST },
            { "Game", StringTable.EDITOR_NODE_CATEGORY_GAME },
            { "ThisCard", StringTable.EDITOR_NODE_NAME_THIS_CARD },
            { "Player", StringTable.EDITOR_NODE_CATEGORY_PLAYER },
            { "GetPile", StringTable.EDITOR_NODE_NAME_GET_PILE },
            { "GetSpellDamage", StringTable.EDITOR_NODE_NAME_GET_SPELL_DAMAGE },
            { "GetTarget", StringTable.EDITOR_NODE_NAME_GET_TARGET },
            { "Pile", StringTable.EDITOR_NODE_CATEGORY_PILE },
            { "IsPlayerPileChanged", StringTable.EDITOR_NODE_NAME_IS_PLAYER_PILE_CHANGED },
            { "Card", StringTable.EDITOR_NODE_CATEGORY_CARD },
            { "GetOwner", StringTable.EDITOR_NODE_NAME_GET_OWNER },
            { "AddIntModifier", StringTable.EDITOR_NODE_NAME_ADD_INT_MODIFIER },
            { "RemoveModifier", StringTable.EDITOR_NODE_NAME_REMOVE_MODIFIER },
            { "GetProperty", StringTable.EDITOR_NODE_NAME_GET_PROPERTY },
            { "SetProperty", StringTable.EDITOR_NODE_NAME_SET_PROPERTY },
            { "GetUniquePropertyName", StringTable.EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME },
            { "Damage", StringTable.EDITOR_NODE_NAME_DAMAGE },
            { "Modifier", StringTable.EDITOR_NODE_CATEGORY_MODIFIER },
            { "SetIntPropModifierValue", StringTable.EDITOR_NODE_NAME_SET_INT_PROP_MODIFIER_VALUE },
            { "Event", StringTable.EDITOR_NODE_CATEGORY_EVENT },
            { "GetVariable", StringTable.EDITOR_NODE_NAME_GET_VARIABLE },
            { "SetVariable", StringTable.EDITOR_NODE_NAME_SET_VARIABLE },
            { "CollectionOperation", StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION },
            { "Except", StringTable.EDITOR_NODE_NAME_EXCEPT },
            { "GetCount", StringTable.EDITOR_NODE_NAME_GET_COUNT }
        };
        public const string TARGET_TYPE_CHARACTER = "Character";
        public const string TARGET_TYPE_SERVANT = "Servant";
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(int cardPoolId, CardDefine cardDefine, string entryType, object editObject, DisplayInfo[] editObjectStack)
            {
                this.cardPoolId = cardPoolId;
                this.cardDefine = cardDefine;
                this.entryType = entryType;
                this.editObject = editObject;
                this.editObjectStack = editObjectStack;
            }
            public int cardPoolId;
            public CardDefine cardDefine;
            public string entryType;
            public object editObject;
            public DisplayInfo[] editObjectStack;
        }
        class BackgroundMenuItem
        {
            public BackgroundMenuItem(string title)
            {
                this.title = title;
            }
            public string title;
            public Action action;
            public List<BackgroundMenuItem> childList;
        }
        #endregion
    }
    /// <summary>
    /// 动作节点图的编辑器信息，保存了每个效果的所有动作节点信息。
    /// </summary>
    [Serializable]
    public class ActionNodeGraphInfo
    {
        public ActionNodeGraphInfo(int cardId, int effectIndex)
        {
            this.cardId = cardId;
            this.effectIndex = effectIndex;
        }
        public int cardId;
        public int effectIndex;
        public Dictionary<string, ActionNodeGraphNodeInfo> actionInfoDict = new Dictionary<string, ActionNodeGraphNodeInfo>();
        /// <summary>
        /// 离散的，并未与根节点相连的动作节点。
        /// </summary>
        public List<ActionNode> freeActionNodeList = new List<ActionNode>();
    }
    [Serializable]
    public class ActionNodeGraphNodeInfo
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
