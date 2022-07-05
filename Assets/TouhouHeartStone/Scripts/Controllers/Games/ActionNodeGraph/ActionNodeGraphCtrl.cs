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
        /// 打开一个效果的某个动作图
        /// </summary>
        /// <param name="cardPoolId"></param>
        /// <param name="effect"></param>
        /// <param name="entryType"></param>
        /// <param name="graphInfo"></param>
        public void openActionGraph(int cardPoolId, GeneratedEffect effect, string entryType, ActionNodeGraphInfo graphInfo)
        {
            display(new DisplayInfo(cardPoolId, entryType, effect, null, graphInfo));
        }
        public Task openActionValueRefGraph(int cardPoolId, ActionValueRef valueRef, ActionNodeGraphInfo graphInfo)
        {
            return display(new DisplayInfo(cardPoolId, null, valueRef, null, graphInfo));
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
                _graphInfo = info.graphInfo;
                //更新UI
                _ui.gameObject.SetActive(true);
                _ui.displayCardSelectPanel(false);
                if (info.editObject is GeneratedActionDefine actionDefine)
                    return openGeneratedActionDefine(actionDefine);
                else if (info.editObject is TriggerGraph trigger)
                    return openTriggerGraph(trigger, info.entryType);
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
            return new DisplayInfo(_curCardPoolId, _entryType, editObject, _editObjectStack.Count > 0 ? _editObjectStack.ToArray() : null, _graphInfo);
        }
        public Task hide()
        {
            gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        public bool isActionDefineNameExist(string name)
        {
            return getActionDefine(name) != null;
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
            onAwake_ActionDefine();
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
            _ui.onClickActionNodeConstValueItemEditButton += onClickActionNodeConstValueItemEditButton;
            _ui.onClickCancelSelectButton += onClickCancelSelectCardButton;
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

            _cardPoolViewPanelCtrl.enableCount(false);
            _cardPoolViewPanelCtrl.onClickCard += onClickCardSelectPanelCard;

            string[] languages = game.langManager.getLanguages();
            for (int i = 0; i < languages.Length; i++)
            {
                languages[i] = game.langManager.getString(game.ui.getTranslateNameForLanguage(languages[i]));
            }
            _ui.init(languages, getEventTypeNamesForEditor());
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
            openMenuForCategory(posX, posY);
        }
        /// <summary>
        /// 当用户左击背景，隐藏菜单。
        /// </summary>
        /// <param name="posX">x坐标</param>
        /// <param name="posY">y坐标</param>
        private void onLeftClickBackground(float posX, float posY)
        {
            _curEditActionNode = null;
            closeMenu();
        }
        /// <summary>
        /// 当用户右击背景，打开菜单。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        private void onRightClickBackground(float posX, float posY)
        {
            openMenuForCategory(posX, posY);
        }
        /// <summary>
        /// 当用户点击菜单项，视具体点击的项目决定干什么
        /// </summary>
        /// <param name="index"></param>
        private async void onClickMenuItem(int index)
        {
            if (_curEditActionNode != null)
            {
                //节点菜单
                if (index == 0)//删除
                {
                    removeActionNode();
                }
                else if (index == 1)//编辑
                {
                    GeneratedActionDefine generatedActionDefine = getActionDefine(_curEditActionNode.defineName) as GeneratedActionDefine;
                    await game.ui.displayUIWithReturn(scene.effectEditor, new EffectEditorCtrl.DisplayInfo(
                        _curCardPoolId,
                        generatedActionDefine,
                        new DisplayInfo(
                            _curCardPoolId,
                            null,
                            generatedActionDefine,
                            null, game.card.getOrCreateActionDefineActionNodeGraphInfo(_curCardPoolId, generatedActionDefine.id))));
                }
                closeMenu();
            }
            else
            {
                //背景菜单
                if (_curCategroy == null)
                {
                    var categroyGroups = getActionDefineGroups();
                    if (index < categroyGroups.Length)
                    {
                        ActionDefine[] actions = categroyGroups[index].ToArray();
                        if (actions.Length == 1)
                        {
                            //直接创建节点
                            closeMenu();
                            await createActionNodeAtCurPos(actions[0]);
                        }
                        else
                        {
                            //打开子菜单
                            _curCategroy = categroyGroups[index].Key;
                            string[] options = new string[actions.Length + 1];
                            options[0] = game.langManager.getString(StringTable.EDITOR_ACTIONGRAPH_MENU_RETURN) + " <";
                            Array.Copy(actions.Select(a =>
                            {
                                string name = a.defineName.EndsWith(StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST) ?
                                    a.defineName.Substring(0, a.defineName.Length - 5) : a.defineName;
                                if (_nameTextDict.ContainsKey(name))
                                    name = _nameTextDict[name];
                                return game.langManager.getString(name);
                            }).ToArray(), 0, options, 1, actions.Length);
                            _ui.updateMenu(options);
                        }
                    }
                    else
                    {
                        closeMenu();
                        //新建自定义动作
                        GeneratedActionDefine generatedActionDefine = game.card.createActionDefine(
                            _curCardPoolId,
                            game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_NAME_DEFAULT));
                        await scene.effectEditor.display(_curCardPoolId, generatedActionDefine);
                    }
                }
                else
                {
                    if (index == 0)
                    {
                        //返回上一层
                        _curCategroy = null;
                        updateMenuForCategory();
                    }
                    else
                    {
                        //创建节点
                        var categroyGroups = getActionDefineGroups();
                        ActionDefine[] actions = Array.Find(categroyGroups, g => g.Key == _curCategroy).ToArray();
                        closeMenu();
                        await createActionNodeAtCurPos(actions[index - 1]);
                    }
                }
            }
            _curEditActionNode = null;
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
            //入口节点输出
            if (outputActionNodeItem == _entryNodeItem)
            {
                if (inputActionNodeItem == _returnNodeItem)
                {
                    //自定义动作参数返回连接
                    //检查合法性
                    if (outputIndex < 1)
                    {
                        //值输出只能与值输入相互连接
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                        return;
                    }
                    Type outputType = getActionDefineInputTypeAt(outputIndex - 1);
                    Type inputType = _generatedActionDefine.outputList[inputIndex].type;
                    if (!isValidTypeForConnection(outputType, inputType))
                    {
                        //相互连接的输入输出类型必须可以相互转化
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                        return;
                    }
                    connectNode4ActionDefineArgToReturn(outputIndex - 1, inputIndex);
                }
                else if (getActionNode(inputActionNodeItem) is ActionNode inputActionNode)
                {
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
                        if (_trigger != null)
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
                            Type inputType = getActionDefine(inputActionNode.defineName).inputs[inputIndex - inputActionNode.branches.Length].type;
                            if (!isValidTypeForConnection(outputType, inputType))
                            {
                                //相互连接的输入输出类型必须可以相互转化
                                _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                                return;
                            }
                            connectNode4TriggerEventVarRef(outputIndex - 1, inputActionNode, inputIndex - inputActionNode.branches.Length);
                        }
                        else if (_generatedActionDefine != null)
                        {
                            //自定义动作参数连接
                            //检查合法性
                            if (inputIndex < inputActionNode.branches.Length)
                            {
                                //值输出只能与值输入相互连接
                                _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                                return;
                            }
                            Type outputType = getActionDefineInputTypeAt(outputIndex - 1);
                            Type inputType = getActionDefine(inputActionNode.defineName).inputs[inputIndex - inputActionNode.branches.Length].type;
                            if (!isValidTypeForConnection(outputType, inputType))
                            {
                                //相互连接的输入输出类型必须可以相互转化
                                _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                                return;
                            }
                            connectNode4ActionDefineArg(outputIndex - 1, inputActionNode, inputIndex - inputActionNode.branches.Length);
                        }
                    }
                }
            }
            else if (inputActionNodeItem == _entryNodeItem)
            {
                //与触发器的条件节点相连
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                //检查连接合法性
                if (outputIndex < outputActionNode.branches.Length)
                {
                    //值输出只能与值输入相互连接
                    _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                    return;
                }
                connectNode4TriggerCondition(outputActionNode, outputIndex - outputActionNode.branches.Length, inputIndex);
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
            }
            else
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                ActionDefine outputActionDefine = getActionDefine(outputActionNode.defineName);
                int outputBranchesCount = getBranchesCount(outputActionDefine);
                ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                if (inputIndex == 0)
                {
                    //动作连接
                    if (outputActionNode.branches == null)
                    {
                        outputActionNode.branches = new ActionNode[outputBranchesCount];
                    }
                    //检查合法性
                    if (outputIndex >= outputBranchesCount)
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
                        outputActionNode.branches = new ActionNode[outputBranchesCount];
                    }
                    //检查合法性
                    if (outputIndex < outputBranchesCount)
                    {
                        //值输出只能与值输入相互连接
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
                        return;
                    }
                    //检查类型合法性，获取输入输出类型
                    Type outputType = outputActionDefine.getValueOutputAt(outputIndex - outputBranchesCount).type;
                    ActionDefine inputActionDefine = getActionDefine(inputActionNode.defineName);
                    Type inputType;
                    if (inputActionDefine.inputs.Last().isParams && inputIndex >= inputActionDefine.inputs.Length)
                    {
                        //多参数，获取最后一个参数类型
                        inputType = inputActionDefine.inputs.Last().type;
                    }
                    else
                    {
                        inputType = inputActionDefine.inputs[inputIndex - 1].type;
                    }
                    if (!isValidTypeForConnection(outputType, inputType))
                    {
                        //相互连接的输入输出类型必须可以相互转化
                        _ui.showErrorTip(game.langManager.getString(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                        return;
                    }
                    connectNode4ValueRef(outputActionNode, outputIndex - outputBranchesCount, inputActionNode, inputIndex - 1);
                }
            }
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
            if (outputActionNodeItem == _entryNodeItem)
            {
                if (inputActionNodeItem == _returnNodeItem)
                {
                    //自定义动作参数返回连接
                    unconnectNode4ActionDefineArgToReturn(outputIndex - 1, inputIndex);
                }
                else if (getActionNode(inputActionNodeItem) is ActionNode inputActionNode)
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
                }
            }
            else if (inputActionNodeItem == _entryNodeItem)
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                unconnectNode4TriggerCondition(outputActionNode, outputIndex - outputActionNode.branches.Length, inputIndex);
            }
            else if (inputActionNodeItem == _returnNodeItem && valueRef != null)
            {
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
                unconnectNode4ReturnValueRef(outputActionNode, valueRef.index, 0);
            }
            else
            {
                ActionNode inputActionNode = getActionNode(inputActionNodeItem);
                ActionNode outputActionNode = getActionNode(outputActionNodeItem);
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
            }
        }
        /// <summary>
        /// 当左击动作节点
        /// </summary>
        /// <param name="actionNodeItem"></param>
        private void onLeftClickActionNode(ActionNodeItem actionNodeItem)
        {
            _curEditActionNode = null;
            closeMenu();
        }
        /// <summary>
        /// 当右击动作节点
        /// </summary>
        /// <param name="actionNodeItem"></param>
        private void onRightClickActionNode(ActionNodeItem actionNodeItem, Vector2 pointerPosition)
        {
            //入口节点不能通过长按编辑
            if (actionNodeItem == _entryNodeItem)
                return;
            //出口节点也不行
            if (actionNodeItem == _returnNodeItem)
                return;
            _curEditActionNode = getActionNode(actionNodeItem);
            Vector2 position = _ui.worldPositionToGraphPosition(pointerPosition);
            List<string> optionList = new List<string>()
            {
                game.langManager.getString(StringTable.DELETE)//删除
            };
            if (getActionDefine(_curEditActionNode.defineName) is GeneratedActionDefine)
            {
                optionList.Add(game.langManager.getString(StringTable.EDIT));//编辑
            }
            openMenu(position, optionList.ToArray());
        }
        /// <summary>
        /// 当用户长按节点，展开节点菜单。
        /// </summary>
        /// <param name="actionNodeItem"></param>
        private void onLongPressActionNode(ActionNodeItem actionNodeItem, Vector2 pointerPosition)
        {
            //入口节点不能通过长按编辑
            if (actionNodeItem == _entryNodeItem)
                return;
            //出口节点也不行
            if (actionNodeItem == _returnNodeItem)
                return;
            _curEditActionNode = getActionNode(actionNodeItem);
            Vector2 position = _ui.worldPositionToGraphPosition(pointerPosition);
            openMenu(position, new string[]
            {
                game.langManager.getString(StringTable.DELETE),//删除
            });
        }
        private void onDragActionNode(ActionNodeItem actionNodeItem, Vector2 graphPosition)
        {
            if (actionNodeItem == _entryNodeItem || actionNodeItem == _returnNodeItem)
            {
                //入口节点因为不是动作的一部分所以是不可以移动的。
                return;
            }
            ActionNodeGraphNodeInfo nodeInfo = getNodeInfo(getActionNode(actionNodeItem));
            ActionNodeItem rootActionNodeItem = _entryNodeItem != null ? _entryNodeItem : _returnNodeItem;
            nodeInfo.posX = graphPosition.x - (rootActionNodeItem.transform as RectTransform).anchoredPosition.x;
            nodeInfo.posY = graphPosition.y - (rootActionNodeItem.transform as RectTransform).anchoredPosition.y;
            _ui.moveActionNodeTo(actionNodeItem, graphPosition);
            updateContentSize();
        }
        /// <summary>
        /// 当用户更改节点的常量值，设置对应的常量。
        /// </summary>
        /// <param name="actionNodeItem"></param>
        /// <param name="constIndex"></param>
        /// <param name="value"></param>
        private void onActionNodeValueChange(ActionNodeItem actionNodeItem, int constIndex, object value)
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
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
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
            else if (actionDefine.consts[index].type == typeof(CardReference))
            {
                _ui.displayCardSelectPanel(true);
                if (actionNode.consts[index] is CardReference value)
                {
                    //打开卡牌选择界面
                    await _cardPoolViewPanelCtrl.display(value.cardId, cardPoolsId: new int[] { value.cardPoolId }, cardFilter: c =>
                    {
                        //所有卡都可以选
                        return true;
                    });
                }
                else
                {
                    await _cardPoolViewPanelCtrl.display(cardPoolsId: new int[] { _curCardPoolId }, cardFilter: c =>
                    {
                        //所有卡都可以选
                        return true;
                    });
                }
                _curEditActionNode = actionNode;
                _curEditActionNodeConstIndex = index;
                _curEditActionNodeConstItemIndex = -1;
            }
        }
        private async void onClickActionNodeConstValueItemEditButton(ActionNodeItem actionNodeItem, int index, int itemIndex)
        {
            ActionNode actionNode = getActionNode(actionNodeItem);
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
            if (actionDefine.consts[index].type == typeof(CardReference[]))
            {
                _ui.displayCardSelectPanel(true);
                if (actionNode.consts[index] is CardReference[] value)
                {
                    if (value[itemIndex] == null)
                    {
                        value[itemIndex] = new CardReference();
                    }
                    //打开卡牌选择界面
                    await _cardPoolViewPanelCtrl.display(value[itemIndex].cardId, new int[] { value[itemIndex].cardPoolId }, cardFilter: c =>
                    {
                        //所有卡都可以选
                        return true;
                    });
                }
                else
                {
                    await _cardPoolViewPanelCtrl.display(cardPoolsId: new int[] { _curCardPoolId }, cardFilter: c =>
                    {
                        //所有卡都可以选
                        return true;
                    });
                }
                _curEditActionNode = actionNode;
                _curEditActionNodeConstIndex = index;
                _curEditActionNodeConstItemIndex = itemIndex;
            }
        }
        private async void onClickCardSelectPanelCard(CardDefine cardDefine)
        {
            if (_curEditActionNode == null)
            {
                return;
            }
            if (_curEditActionNodeConstItemIndex < 0)
            {
                _curEditActionNode.consts[_curEditActionNodeConstIndex] = new CardReference()
                {
                    cardPoolId = game.card.getCardPoolOfCard(cardDefine) is CardPoolInfo cardPoolInfo1 ? cardPoolInfo1.cardPoolId : 0,
                    cardId = cardDefine.id
                };
            }
            else
            {
                (_curEditActionNode.consts[_curEditActionNodeConstIndex] as CardReference[])[_curEditActionNodeConstItemIndex] = new CardReference()
                {
                    cardPoolId = game.card.getCardPoolOfCard(cardDefine) is CardPoolInfo cardPoolInfo1 ? cardPoolInfo1.cardPoolId : 0,
                    cardId = cardDefine.id
                };
            }
            ActionNodeItem actionNodeItem = getActionNodeItem(_curEditActionNode);
            if (cardDefine != null)
            {
                CardSkinData skin = game.card.getDefaultSkin(cardDefine);
                Sprite sprite = skin.image != null ?
                    skin.image :
                    await game.card.loadImage(skin.imagePath, game.card.getCardPoolOfCard(cardDefine) is CardPoolInfo cardPoolInfo2 ? cardPoolInfo2.cardPoolId : 0);
                if (_curEditActionNodeConstItemIndex < 0)
                {
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).card.isFaceup = true;
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).card.setCard(cardDefine, skin);
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).card.setImage(sprite);
                }
                else
                {
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).getItemAt(_curEditActionNodeConstItemIndex).card.isFaceup = true;
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).getItemAt(_curEditActionNodeConstItemIndex).card.setCard(cardDefine, skin);
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).getItemAt(_curEditActionNodeConstItemIndex).card.setImage(sprite);
                }
            }
            else
            {
                if (_curEditActionNodeConstItemIndex < 0)
                {
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).card.isFaceup = false;
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).card.setCard(null, null);
                }
                else
                {
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).getItemAt(_curEditActionNodeConstItemIndex).card.isFaceup = false;
                    actionNodeItem.getConstInputField(_curEditActionNodeConstIndex).getItemAt(_curEditActionNodeConstItemIndex).card.setCard(null, null);
                }
            }
            //重置字段
            _curEditActionNode = null;
            _curEditActionNodeConstIndex = -1;
            _curEditActionNodeConstItemIndex = -1;
            //隐藏UI
            _ui.displayCardSelectPanel(false);
        }
        private void onClickCancelSelectCardButton()
        {
            //重置字段
            _curEditActionNode = null;
            _curEditActionNodeConstIndex = -1;
            _curEditActionNodeConstItemIndex = -1;
            //隐藏UI
            _ui.displayCardSelectPanel(false);
        }
        /// <summary>
        /// 当用户点击返回按钮，出栈之前的显示信息并重新打开之前的编辑界面
        /// </summary>
        private void onClickReturnButton()
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
                        new (string name, Color color)[0],
                        new ActionNodeValueInfo[0],
                        new (string name, Color color)[]
                        {
                            (string.Empty,game.editor.getColorByType(typeof(ActionNode)))
                        });
                    var actionNodeItem = await createGraph4ActionNode(_effect.onEnableAction);
                    _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0, ActionNodeConnectionType.action);
                }
                else
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONENABLE),
                        new (string name, Color color)[0],
                        new ActionNodeValueInfo[0],
                        new (string name, Color color)[]
                        {
                            (string.Empty, game.editor.getColorByType(typeof(ActionNode)))
                        });
                }
            }
            else if (entryType == nameof(GeneratedEffect.onDisableAction))
            {
                if (_effect.onDisableAction != null)
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONDISABLE),
                        new (string name, Color color)[0],
                        new ActionNodeValueInfo[0],
                        new (string name, Color color)[]
                        {
                            (string.Empty, game.editor.getColorByType(typeof(ActionNode)))
                        });
                    var actionNodeItem = await createGraph4ActionNode(_effect.onDisableAction);
                    _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0, ActionNodeConnectionType.action);
                }
                else
                {
                    _entryNodeItem = _ui.createNode(0, 0,
                        game.langManager.getString(StringTable.EDITOR_NODE_NAME_ONDISABLE),
                        new (string name, Color color)[0],
                        new ActionNodeValueInfo[0],
                        new (string name, Color color)[]
                        {
                            (string.Empty, game.editor.getColorByType(typeof(ActionNode)))
                        });
                }
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
                new (string name, Color color)[]
                {
                    (game.langManager.getString(StringTable.EDITOR_PARAM_NAME_RETURN), game.editor.getColorByType(typeof(object)))
                },
                new ActionNodeValueInfo[0],
                new (string name, Color color)[0]);
            if (valueRef.action != null)
            {
                ActionNodeItem actionNodeItem = await createGraph4ActionNode(valueRef.action);
                _ui.connectNode(actionNodeItem, valueRef.index + 1, _returnNodeItem, 0, ActionNodeConnectionType.action);
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
            _returnNodeItem = _ui.createNode(0, 0,
                game.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT),
                new (string name, Color color)[]
                {
                    (game.langManager.getString(StringTable.EDITOR_PARAM_NAME_RETURN), game.editor.getColorByType(typeof(bool)))
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
                new (string name, Color color)[0]);
            if (targetChecker.condition != null && targetChecker.condition.action != null)
            {
                ActionNodeItem actionNodeItem = await createGraph4ActionNode(targetChecker.condition.action);
                _ui.connectNode(actionNodeItem, valueRef.index + 1, _returnNodeItem, 0, ActionNodeConnectionType.value);
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
            ActionNodeItem rootActionNodeItem = _entryNodeItem != null ? _entryNodeItem : _returnNodeItem;
            Vector2 pos;
            if (nodeInfo != null)
            {
                //有图信息，从图中取位置
                pos = new Vector2((rootActionNodeItem.transform as RectTransform).anchoredPosition.x + nodeInfo.posX,
                    (rootActionNodeItem.transform as RectTransform).anchoredPosition.y + nodeInfo.posY);
            }
            else if (prevNodeItem != null)
            {
                //没有图信息，自动计算位置。位置和上一个节点以及其他子节点的位置有关，同时还要计算所有子节点在这个排布规则下的尺寸。
                //暂时先不做那么复杂的自动布局计算……分支节点基本上都是下一个动作，直接接在当前动作屁股后面就行。中间间隔参数的嵌套层数的宽度
                //输入参数的高度直接往下排就完事了。
                if (sizeDict == null)
                    sizeDict = new Dictionary<ActionNode, Vector2>();
                Vector2 size = calcActionNodeGraphSize(actionNode, sizeDict);
                pos = new Vector2((rootActionNodeItem.transform as RectTransform).anchoredPosition.x + prevNodeItem.posX + size.x,
                   (rootActionNodeItem.transform as RectTransform).anchoredPosition.y + startPosOffset);
                nodeInfo = new ActionNodeGraphNodeInfo(pos.x, pos.y);
                setNodeInfo(actionNode, nodeInfo);
            }
            else if (nextNodeItem != null)
            {
                if (sizeDict == null)
                    sizeDict = new Dictionary<ActionNode, Vector2>();
                Vector2 size = calcActionNodeGraphSize(actionNode, sizeDict);
                pos = new Vector2((rootActionNodeItem.transform as RectTransform).anchoredPosition.x + nextNodeItem.posX - size.x,
                     (rootActionNodeItem.transform as RectTransform).anchoredPosition.y + startPosOffset);
                nodeInfo = new ActionNodeGraphNodeInfo(pos.x, pos.y);
                setNodeInfo(actionNode, nodeInfo);
            }
            else
            {
                //没有信息也没有前置节点，那我也不知道该放在哪里了。
                pos = new Vector2((rootActionNodeItem.transform as RectTransform).anchoredPosition.x, (rootActionNodeItem.transform as RectTransform).anchoredPosition.y);
                nodeInfo = new ActionNodeGraphNodeInfo(pos.x, pos.y);
                setNodeInfo(actionNode, nodeInfo);
            }
            actionNodeItem = await createNodeUI4ActionNodeAt(actionNode, pos.x, pos.y);
            //为输入节点创建节点图
            float lastNodeOffset = 0;
            if (actionNode.inputs != null)
            {
                for (int i = 0; i < actionNode.inputs.Length; i++)
                {
                    ActionValueRef valueRef = actionNode.inputs[i];
                    if (valueRef == null)
                        continue;
                    if (sizeDict == null)
                        sizeDict = new Dictionary<ActionNode, Vector2>();
                    if (!string.IsNullOrEmpty(valueRef.eventVarName))
                    {
                        //与触发节点连接
                        _ui.connectNode(_entryNodeItem, 1 + getEventVarNameIndex(valueRef.eventVarName), actionNodeItem, 1 + i, ActionNodeConnectionType.value);
                    }
                    else
                    {
                        ActionNode inputActionNode = valueRef.action != null ? valueRef.action : getActionNode(valueRef.actionNodeId);
                        if (inputActionNode != null)
                        {
                            //与普通节点连接
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
                        else
                        {
                            _ui.connectNode(_entryNodeItem, 1 + valueRef.argIndex, actionNodeItem, 1 + i, ActionNodeConnectionType.value);
                        }
                    }
                }
            }
            //为动作分支创建节点图
            lastNodeOffset = 0;
            if (actionNode.branches != null)
            {
                for (int i = 0; i < actionNode.branches.Length; i++)
                {
                    if (actionNode.branches[i] == null)
                        continue;
                    ActionNode nextActionNode = actionNode.branches[i];
                    if (sizeDict == null)
                        sizeDict = new Dictionary<ActionNode, Vector2>();
                    ActionNodeItem nextActionNodeItem = await createGraph4ActionNode(nextActionNode,
                        prevNodeItem: actionNodeItem,
                        startPosOffset: lastNodeOffset,
                        sizeDict: sizeDict);
                    Vector2 size = calcActionNodeGraphSize(nextActionNode, sizeDict);
                    lastNodeOffset -= size.y;
                    _ui.connectNode(actionNodeItem, i, nextActionNodeItem, 0, ActionNodeConnectionType.action);
                }
            }
            return actionNodeItem;
        }
        private Vector2 calcActionNodeGraphSize(ActionNode actionNode, Dictionary<ActionNode, Vector2> sizeDict)
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
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
            ActionNodeItem actionNodeItem;
            if (actionDefine is MethodActionDefine methodActionDefine && methodActionDefine.methodName == StringTable.EDITOR_NODE_NAME_GET_BUFF_DEFINE)
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
                    generatedBuffDefine = buffDefine;
                    //获取增益对应的皮肤
                    buffSkin = game.card.getBuffSkin(generatedBuffDefine);
                    if (buffSkin == null)
                    {
                        //如果不存在皮肤，那就创建一个
                        buffSkin = game.card.createBuffSkin(_curCardPoolId, generatedBuffDefine.id, game.langManager.getLanguage());
                    }
                }
                BuffDefine[] buffDefines = game.card.getBuffDefines(_curCardPoolId);
                actionNodeItem = _ui.createNodeForBuff(posX, posY, new ActionNodeBuffViewData()
                {
                    buffNames = buffDefines.Select(b => b.ToString()).ToArray(),
                    buffIndex = Array.IndexOf(buffDefines, generatedBuffDefine),
                    buffLanguageIndex = Array.IndexOf(game.langManager.getLanguages(), buffSkin.language),
                    buffName = buffSkin.name,
                    buffDesc = buffSkin.desc,
                    buffImage = await game.card.loadImage(buffSkin.imagePath, _curCardPoolId),
                    propertyTypes = _propertyTypes.Select(s => game.langManager.getString(s)).ToArray(),
                    modifyTypes = _modifyTypes.Select(s => game.langManager.getString(s)).ToArray(),
                    propModifiersViewData = getBuffDefinePropModifiersViewData(generatedBuffDefine),
                    effectTypeNames = game.card.getEffectNames(),
                    effectNames = game.card.getEffectNames(generatedBuffDefine.effectList.ToArray())
                }, getActionNodeOutputNames(actionNode));
            }
            else if (actionDefine != null)
            {
                ActionNodeValueInfo[] constsInfo = getActionNodeConstValueInfo(actionNode);
                actionNodeItem = _ui.createNode(
                    posX,
                    posY,
                    getActionNodeName(actionNode),
                    getActionNodeInputViewData(actionNode),
                    constsInfo,
                    getActionNodeOutputViewData(actionNode));
                for (int i = 0; i < constsInfo.Length; i++)
                {
                    if (constsInfo[i].type == ActionNodeValueType.cardReference)
                    {
                        CardDefine cardDefine = null;
                        if (actionNode.consts[i] is CardReference cardReference)
                        {
                            cardDefine = game.card.getCardDefine(cardReference.cardPoolId, cardReference.cardId);
                        }
                        if (cardDefine != null)
                        {
                            actionNodeItem.getConstInputField(i).card.isFaceup = true;
                            CardSkinData cardSkin = game.card.getDefaultSkin(cardDefine);
                            actionNodeItem.getConstInputField(i).card.setCard(cardDefine, cardSkin);
                            Sprite sprite = cardSkin.image != null ?
                                cardSkin.image :
                                await game.card.loadImage(cardSkin.imagePath, game.card.getCardPoolOfCard(cardDefine) is CardPoolInfo cardPoolInfo ? cardPoolInfo.cardPoolId : 0);
                            actionNodeItem.getConstInputField(i).card.setImage(sprite);
                        }
                        else
                        {
                            actionNodeItem.getConstInputField(i).card.isFaceup = false;
                        }
                    }
                    else if (constsInfo[i].type == ActionNodeValueType.cardReferenceCollection)
                    {
                        if (actionNode.consts[i] is CardReference[] cardRefArray)
                        {
                            for (int j = 0; j < cardRefArray.Length; j++)
                            {
                                CardDefine cardDefine = null;
                                if (cardRefArray[j] is CardReference cardReference)
                                {
                                    cardDefine = game.card.getCardDefine(cardReference.cardPoolId, cardReference.cardId);
                                }
                                if (cardDefine != null)
                                {
                                    actionNodeItem.getConstInputField(i).getItemAt(j).card.isFaceup = true;
                                    CardSkinData cardSkin = game.card.getDefaultSkin(cardDefine);
                                    actionNodeItem.getConstInputField(i).getItemAt(j).card.setCard(cardDefine, cardSkin);
                                    Sprite sprite = cardSkin.image != null ?
                                        cardSkin.image :
                                        await game.card.loadImage(cardSkin.imagePath, game.card.getCardPoolOfCard(cardDefine) is CardPoolInfo cardPoolInfo ? cardPoolInfo.cardPoolId : 0);
                                    actionNodeItem.getConstInputField(i).getItemAt(j).card.setImage(sprite);
                                }
                                else
                                {
                                    actionNodeItem.getConstInputField(i).getItemAt(j).card.isFaceup = false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                actionNodeItem = _ui.createNode(
                    posX,
                    posY,
                    game.langManager.getString(StringTable.EDITOR_NODE_NAME_UNKNOW));
                actionNodeItem.setInputs(new (string name, Color color)[] { (string.Empty, game.editor.getColorByType(typeof(ActionNode))) });
                actionNodeItem.setOutputs(new (string name, Color color)[] { (string.Empty, game.editor.getColorByType(typeof(ActionNode))) });
            }
            _actionNodeItemList.Add(actionNodeItem);
            if (!_actionNodeList.Contains(actionNode))
            {
                _actionNodeList.Add(actionNode);
                actionNode.id = allocActionNodeId();
                _graphInfo.freeActionNodeList.Add(actionNode);
                setNodeInfo(actionNode, new ActionNodeGraphNodeInfo(posX, posY));
            }
            return actionNodeItem;
        }
        private void removeActionNode()
        {
            ActionNode actionNode = _curEditActionNode;
            int actionNodeIndex = _actionNodeList.IndexOf(_curEditActionNode);
            ActionNodeItem actionNodeItem = getActionNodeItem(actionNode);
            unconnectNodeAll(actionNode);
            //从列表中移除
            _actionNodeList.RemoveAt(actionNodeIndex);
            _actionNodeItemList.RemoveAt(actionNodeIndex);
            //从编辑器信息中删除
            removeNodeInfo(actionNode);
            _graphInfo.freeActionNodeList.Remove(actionNode);
            //将节点从UI上移除
            _ui.removeNode(actionNodeItem);
            updateContentSize();
        }
        private ActionNodeItem getActionNodeItem(ActionNode actionNode)
        {
            int index = _actionNodeList.IndexOf(actionNode);
            return _actionNodeItemList[index];
        }
        private bool tryGetActionNodeItem(ActionNode actionNode, out ActionNodeItem actionNodeItem)
        {
            int index = _actionNodeList.FindIndex(a => a.id == actionNode.id);
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
            _graphInfo.freeActionNodeList.Remove(inputActionNode);
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
            if (_generatedActionDefine != null)
            {
                if (_generatedActionDefine.returnValueRefList == null)
                    _generatedActionDefine.returnValueRefList = new List<ReturnValueRef>();
                //判断是否已经相连
                ReturnValueRef returnValueRef = _generatedActionDefine.returnValueRefList.Find(r => r != null && r.valueRef != null && r.returnIndex == inputIndex);
                if (returnValueRef != null)
                {
                    if (returnValueRef.valueRef.action == outputActionNode && returnValueRef.valueRef.index == outputIndex)
                    {
                        //已经相连，什么也不做
                        return;
                    }
                    else
                    {
                        //如果输入已经和其他输出连接，则断开。
                        unconnectNode4AnyType(outputActionNode, outputIndex, null, inputIndex);
                    }
                }
                //建立新的连接
                _generatedActionDefine.returnValueRefList.Add(new ReturnValueRef(outputActionNode, outputIndex, inputIndex));
                //从离散节点列表中去掉被连接的节点
                _graphInfo.freeActionNodeList.Remove(outputActionNode);
                //更新ui
                _ui.connectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, _returnNodeItem, 0, ActionNodeConnectionType.value);
            }
            else if (_valueRef != null)
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
                _graphInfo.freeActionNodeList.Remove(outputActionNode);
                //更新ui
                _ui.connectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, _returnNodeItem, 0, ActionNodeConnectionType.value);
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
            _graphInfo.freeActionNodeList.Remove(inputActionNode);
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
                    else
                    {
                        //输入与其他输出相连，断开
                        unconnectNode4AnyType(outputActionNode, outputIndex, inputActionNode, inputIndex);
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
            ActionDefine inputActionDefine = getActionDefine(inputActionNode.defineName);
            ActionDefine outputActionDefine = getActionDefine(outputActionNode.defineName);
            //因为动作的序列化类型和运行时类型彻底分开，所以不再需要维持动作节点在树状结构中的唯一实例存在
            //if (!isActionNodeValueRefedInPrevNodes(outputActionNode, inputActionNode, inputIndex))
            //{
            //    inputActionNode.inputs[inputIndex] = new ActionValueRef(outputActionNode, outputIndex);
            //    //遍历子节点，使其中所有对相同值输出的引用从直接引用变为id引用
            //    traverseActionNode(inputActionNode, false, a =>
            //    {
            //        if (a == inputActionNode)
            //            return;
            //        if (a.inputs != null && a.inputs.Length > 0)
            //        {
            //            for (int i = 0; i < a.inputs.Length; i++)
            //            {
            //                if (a.inputs[i] == null)
            //                    continue;
            //                if (a.inputs[i].action == outputActionNode)
            //                {
            //                    a.inputs[i].action = null;
            //                    a.inputs[i].actionNodeId = outputActionNode.id;
            //                }
            //            }
            //        }
            //    });
            //}
            //else
            //{
            //    inputActionNode.inputs[inputIndex] = new ActionValueRef(outputActionNode.id, outputIndex);
            //}
            inputActionNode.inputs[inputIndex] = new ActionValueRef(outputActionNode, outputIndex);
            if (inputActionDefine.inputs.Last().isParams && inputIndex >= inputActionDefine.inputs.Length - 1)
            {
                //是多参数
                ActionValueRef lastValueRef = inputActionNode.inputs[inputActionNode.inputs.Length - 1];
                if (lastValueRef.action != null || lastValueRef.actionNodeId != 0 || !string.IsNullOrEmpty(lastValueRef.eventVarName))
                {
                    //最后一个参数不为空，需要一个额外的空位
                    ActionValueRef[] newInputs = new ActionValueRef[inputActionNode.inputs.Length + 1];
                    Array.Copy(inputActionNode.inputs, 0, newInputs, 0, inputActionNode.inputs.Length);
                    inputActionNode.inputs = newInputs;
                    getActionNodeItem(inputActionNode).setInputs(getActionNodeInputViewData(inputActionNode));
                }
            }
            //更新输出节点的输出变量状态
            updateRegVar(outputActionNode);
            //从离散节点列表中去掉被连接的节点
            _graphInfo.freeActionNodeList.Remove(outputActionNode);
            //更新ui
            _ui.connectNode(getActionNodeItem(outputActionNode), getBranchesCount(outputActionDefine) + outputIndex, getActionNodeItem(inputActionNode), 1 + inputIndex, ActionNodeConnectionType.action);
        }
        private void unconnectNode4RootAction(ActionNode inputActionNode)
        {
            if (inputActionNode == rootActionNode)
            {
                //断开与入口动作节点的连接
                rootActionNode = null;
            }
            //输入节点一定会变得离散
            if (!_graphInfo.freeActionNodeList.Contains(inputActionNode))
                _graphInfo.freeActionNodeList.Add(inputActionNode);
            _ui.unconnectNode(_entryNodeItem, 0, getActionNodeItem(inputActionNode), 0);
        }
        private void unconnectNode4ReturnValueRef(ActionNode outputActionNode, int outputIndex, int inputIndex)
        {
            if (_generatedActionDefine != null)
            {
                ReturnValueRef returnValueRef = _generatedActionDefine.returnValueRefList.Find(r =>
                    r != null &&
                    r.valueRef != null &&
                    r.valueRef.action == outputActionNode &&
                    r.valueRef.index == outputIndex &&
                    r.returnIndex == inputIndex);
                if (returnValueRef != null)
                {
                    //断开与返回值节点的连接
                    _generatedActionDefine.returnValueRefList.Remove(returnValueRef);
                }
                //输出节点一定会变得离散
                if (!_graphInfo.freeActionNodeList.Contains(outputActionNode))
                    _graphInfo.freeActionNodeList.Add(outputActionNode);
                _ui.unconnectNode(getActionNodeItem(outputActionNode), valueRef.index + 1, _returnNodeItem, 0);
            }
            else if (_valueRef != null)
            {
                if (rootActionNode == outputActionNode)
                {
                    //断开与返回值节点的连接
                    rootActionNode = null;
                }
                //输出节点一定会变得离散
                if (!_graphInfo.freeActionNodeList.Contains(outputActionNode))
                    _graphInfo.freeActionNodeList.Add(outputActionNode);
                _ui.unconnectNode(getActionNodeItem(outputActionNode), valueRef.index + 1, _returnNodeItem, 0);
            }
            else
                throw new NotImplementedException();
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
            if (!_graphInfo.freeActionNodeList.Contains(inputActionNode))
                _graphInfo.freeActionNodeList.Add(inputActionNode);
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
            ActionValueRef inputActionValueRef = inputActionNode.inputs[inputIndex];
            //断开之前的节点连接
            ActionDefine inputActionDefine = getActionDefine(inputActionNode.defineName);
            if (inputActionDefine != null &&
                inputActionDefine.inputs.Last().isParams &&
                inputIndex >= inputActionDefine.inputs.Length - 1)
            {
                //如果断开的是多参数节点
                List<ActionValueRef> inputList = new List<ActionValueRef>(inputActionNode.inputs);
                inputList.RemoveAt(inputIndex);
                inputActionNode.inputs = inputList.ToArray();
                //更新ui
                _ui.unconnectNodeAllInput(inputActionNodeItem);
                for (int i = 0; i < inputActionNode.branches.Length; i++)
                {
                    if (inputActionNode.branches[i] == null)
                    {
                        continue;
                    }
                    _ui.connectNode(getActionNodeItem(inputActionNode.branches[i]), 0, inputActionNodeItem, i, ActionNodeConnectionType.value);
                }
                for (int i = inputActionNode.branches.Length; i < inputList.Count; i++)
                {
                    if (inputList[i] == null)
                    {
                        continue;
                    }
                    if (inputList[i].action != null)
                    {
                        _ui.connectNode(getActionNodeItem(inputList[i].action), inputList[i].index + 1, inputActionNodeItem, i, ActionNodeConnectionType.value);
                    }
                    else if (inputList[i].actionNodeId != 0)
                    {
                        _ui.connectNode(getActionNodeItem(getActionNode(inputList[i].actionNodeId)), inputList[i].index + 1, inputActionNodeItem, i, ActionNodeConnectionType.value);
                    }
                    else if (!string.IsNullOrEmpty(inputList[i].eventVarName))
                    {
                        _ui.connectNode(_entryNodeItem, getEventVarNameIndex(inputList[i].eventVarName) + 1, inputActionNodeItem, i, ActionNodeConnectionType.value);
                    }
                }
                inputActionNodeItem.setInputs(getActionNodeInputViewData(inputActionNode));
            }
            else
            {
                //不是端参数，直接断开
                inputActionNode.inputs[inputIndex] = null;
                //更新ui
                if (tryGetActionNodeItem(outputActionNode, out ActionNodeItem outputActionNodeItem))
                {
                    _ui.unconnectNode(outputActionNodeItem, outputActionNode.branches.Length + outputIndex, inputActionNodeItem, inputActionNode.branches.Length + inputIndex);
                }
            }
            //因为动作的序列化类型和运行时类型彻底分开，所以动作节点不需要维持树状结构，进而不需要维持每个节点在树中的唯一直接引用
            //if (inputActionValueRef.action != null)
            //{
            //    //寻找子节点中的第一个值引用，将其改为引用对象本身
            //    traverseActionNode(inputActionNode, false, a =>
            //    {
            //        if (a.inputs == null)
            //            return false;
            //        for (int i = 0; i < a.inputs.Length; i++)
            //        {
            //            if (a.inputs[i] != null && a.inputs[i].actionNodeId == inputActionValueRef.action.id)
            //            {
            //                a.inputs[i].action = inputActionValueRef.action;
            //                a.inputs[i].actionNodeId = 0;
            //                return true;
            //            }
            //        }
            //        return false;
            //    });
            //}
            //更新离散和输出状态
            if (isFreeActionNode(outputActionNode))
            {
                outputActionNode.regVar = null;
                //如果输出节点变为离散节点，将其添加进入列表
                if (!_graphInfo.freeActionNodeList.Contains(outputActionNode))
                    _graphInfo.freeActionNodeList.Add(outputActionNode);
            }
            else
            {
                //更新输出变量状态
                updateRegVar(outputActionNode);
            }
        }
        /// <summary>
        /// 断开输入节点指定输入的任意类型输出连接
        /// </summary>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNode"></param>
        /// <param name="inputIndex"></param>
        private void unconnectNode4AnyType(ActionNode outputActionNode, int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            if (inputActionNode == null)
            {
                if (outputActionNode == null)
                    unconnectNode4ActionDefineArgToReturn(outputIndex, inputIndex);
                else
                    unconnectNode4ReturnValueRef(outputActionNode, outputIndex, inputIndex);
            }
            else if (inputActionNode.inputs[inputIndex].action != null)
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
                //与事件变量相连
                unconnectNode4TriggerEventVarRef(getEventVarNameIndex(inputActionNode.inputs[inputIndex].eventVarName), inputActionNode, inputIndex);
            }
            else
            {
                //与自定义动作参数相连
                unconnectNode4ActionDefineArg(outputIndex, inputActionNode, inputIndex);
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
                    if (actionNode.inputs[i] == null)
                    {
                        continue;
                    }
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
        private bool isValidTypeForConnection(Type outputType, Type inputType)
        {
            //如果输入类型是ActionValueRef，那么无论输出类型是什么都可以，至少暂时还不方便做ActionValueRef的类型检查
            if (inputType == typeof(ActionValueRef))
            {
                return true;
            }
            //类型之间可以相互转化
            if (outputType.IsAssignableFrom(inputType) || inputType.IsAssignableFrom(outputType))
            {
                return true;
            }
            //在包装成数组之后可以相互转化
            if (inputType.IsArray && (outputType.IsAssignableFrom(inputType.GetElementType()) || inputType.GetElementType().IsAssignableFrom(outputType)))
            {
                return true;
            }
            //在拆包成对象之后可以相互转化
            if (outputType.IsArray && (outputType.GetElementType().IsAssignableFrom(inputType) || inputType.IsAssignableFrom(outputType.GetElementType())))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 判断一个节点是否是离散的节点，实际上不影响游戏但是可能对没有完成的编辑工作有用所以保存在编辑器里
        /// </summary>
        /// <param name="actionNode"></param>
        /// <returns></returns>
        private bool isFreeActionNode(ActionNode actionNode)
        {
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                if (_actionNodeList[i] == null)
                    continue;
                if (_actionNodeList[i].branches != null &&
                    _actionNodeList[i].branches.Length > 0 &&
                    Array.IndexOf(_actionNodeList[i].branches, actionNode) >= 0)
                {
                    //被其他节点作为后续节点引用，不是离散节点
                    return false;
                }
                if (_actionNodeList[i].inputs != null &&
                    _actionNodeList[i].inputs.Length > 0 &&
                    Array.Exists(_actionNodeList[i].inputs, v => v != null && (v.action == actionNode || v.actionNodeId == actionNode.id)))
                {
                    //被其他节点引用输入值，不是离散节点
                    return false;
                }
            }
            //没有被任何节点引用，是离散的
            return true;
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
        private int getActionNodeRefCount(ActionNode actionNode, int outputIndex)
        {
            int count = 0;
            //节点是根节点
            if (rootActionNode == actionNode)
            {
                count++;
            }
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                if (_actionNodeList[i].branches != null && _actionNodeList[i].branches.Length > 0)
                {
                    for (int j = 0; j < _actionNodeList[i].branches.Length; j++)
                    {
                        if (_actionNodeList[i].branches[j] == null)
                            continue;
                        //节点本身作为动作分支被引用
                        if (_actionNodeList[i].branches[j] == actionNode)
                        {
                            count++;
                        }
                    }
                }
                if (_actionNodeList[i].inputs != null && _actionNodeList[i].inputs.Length > 0)
                {
                    for (int j = 0; j < _actionNodeList[i].inputs.Length; j++)
                    {
                        if (_actionNodeList[i].inputs[j] == null)
                            continue;
                        //节点的输出值被引用
                        if ((_actionNodeList[i].inputs[j].action == actionNode || _actionNodeList[i].inputs[j].actionNodeId == actionNode.id) &&
                            _actionNodeList[i].inputs[j].index == outputIndex)
                        {
                            count++;
                        }
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
            //没有父节点，则不可能已经被引用
            if (!tryGetPrevActionNode(curActionNode, out curActionNode, out _))
            {
                return false;
            }
            //判断父节点
            while (curActionNode != null)
            {
                //判断是否在父节点中被引用
                if (isActionNodeValueRefedInNode(valueActionNode, curActionNode))
                {
                    return true;
                }
                //尝试获取下一个父节点
                if (!tryGetPrevActionNode(curActionNode, out curActionNode, out _))
                {
                    return false;
                }
            }
            return false;
        }
        private bool isActionNodeValueRefedInNode(ActionNode valueActionNode, ActionNode actionNode, int curIndex = -1)
        {
            //检查该节点是否是目标节点
            if (valueActionNode == actionNode)
                return true;
            if (actionNode == null || actionNode.inputs == null)
                return false;
            for (int i = 0; i < (curIndex < 0 || curIndex >= actionNode.inputs.Length ? actionNode.inputs.Length : curIndex); i++)
            {
                if (actionNode.inputs[i] == null)
                    continue;
                ActionNode childActionNode;
                if (actionNode.inputs[i].action != null)
                    childActionNode = actionNode.inputs[i].action;
                else
                    childActionNode = getActionNode(actionNode.inputs[i].actionNodeId);
                //递归检查输入节点是否是目标节点
                if (isActionNodeValueRefedInNode(valueActionNode, childActionNode))
                {
                    return true;
                }
            }
            return false;
        }
        private int getOutputCount(ActionNode actionNode)
        {
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
            return actionDefine.outputs.Length;
        }
        private int getBranchesCount(ActionDefine actionDefine)
        {
            int count = 1;//每个动作至少具有一个动作输出
            for (int i = 0; i < actionDefine.outputs.Length; i++)
            {
                if (actionDefine.outputs[i].type == typeof(ActionNode))
                {
                    count++;
                }
            }
            return count;
        }
        /// <summary>
        /// 更新动作节点的变量注册状态
        /// </summary>
        /// <param name="actionNode"></param>
        private void updateRegVar(ActionNode actionNode)
        {
            int outputCount = getOutputCount(actionNode);
            if (actionNode.regVar == null || actionNode.regVar.Length != outputCount)
            {
                actionNode.regVar = new bool[outputCount];
            }
            for (int i = 0; i < actionNode.regVar.Length; i++)
            {
                //如果节点本身或者节点的输出值被重复引用了，那必然要注册变量。
                actionNode.regVar[i] = getActionNodeRefCount(actionNode, i) > 1;
            }
        }
        /// <summary>
        /// 将当前正在编辑的对象入栈
        /// </summary>
        private void pushCurEditObjectToStack()
        {
            if (_trigger != null)
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _entryType, _trigger, null, _graphInfo));
            else if (_targetChecker != null)
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _entryType, _targetChecker, null, _graphInfo));
            else if (_valueRef != null)
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _entryType, _valueRef, null, _graphInfo));
            else
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _entryType, _effect, null, _graphInfo));
            _ui.setReturnButtonActive(true);
        }
        private void popPrevEditObjectFromStackAndOpen()
        {
            display(_editObjectStack.Pop());
        }
        private IGrouping<string, ActionDefine>[] getActionDefineGroups()
        {
            return getAllActionDefines().GroupBy(actionDefine =>
            {
                string name;
                if (actionDefine is MethodActionDefine methodActionDefine)
                {
                    if (string.IsNullOrEmpty(methodActionDefine.category))
                        name = methodActionDefine.defineName;
                    else
                        name = methodActionDefine.category;
                }
                else if (actionDefine is GeneratedActionDefine generatedActionDefine)
                {
                    if (string.IsNullOrEmpty(generatedActionDefine.category))
                        name = generatedActionDefine.defineName;
                    else
                        name = generatedActionDefine.category;
                }
                else if (actionDefine.defineName.EndsWith(StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST))
                {
                    //以Const结尾的类型是常量
                    name = StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST;
                }
                else
                {
                    name = actionDefine.defineName;
                }
                if (_nameTextDict.ContainsKey(name))
                    name = _nameTextDict[name];
                return game.langManager.getString(name);
            }, a => a).OrderBy(g => g.Count() > 1).ThenBy(g => g.Key).ToArray();
        }
        private string getActionNodeName(ActionNode actionNode)
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
        private (string name, Color color)[] getActionNodeInputViewData(ActionNode actionNode)
        {
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
            List<(string name, Color color)> viewDataList = new List<(string name, Color color)>();
            //动作输入
            viewDataList.Add((string.Empty, game.editor.getColorByType(typeof(ActionNode))));
            //值输入
            for (int i = 0; i < actionDefine.inputs.Length; i++)
            {
                viewDataList.Add((actionDefine.inputs[i].name, game.editor.getColorByType(actionDefine.inputs[i].type)));
                if (i == actionDefine.inputs.Length - 1 && actionDefine.inputs[i].isParams)
                {
                    //变长参数，要多已有参数的连接点和一个空的连接点
                    if (actionNode.inputs != null && actionNode.inputs.Length > actionDefine.inputs.Length)
                    {
                        for (int j = actionDefine.inputs.Length; j < actionNode.inputs.Length; j++)
                        {
                            //空的直接去掉，跳过
                            if (j != actionNode.inputs.Length - 1 && actionNode.inputs[j] == null)
                            {
                                var list = new List<ActionValueRef>(actionNode.inputs);
                                list.RemoveAt(j);
                                actionNode.inputs = list.ToArray();
                                j--;
                                continue;
                            }
                            viewDataList.Add((actionDefine.inputs[i].name, game.editor.getColorByType(actionDefine.inputs[i].type)));
                        }
                    }
                    else if (i < actionNode.inputs.Length && actionNode.inputs[i] != null)
                        viewDataList.Add((actionDefine.inputs[i].name, game.editor.getColorByType(actionDefine.inputs[i].type)));
                }
            }
            return viewDataList.ToArray();
        }

        private string[] getActionNodeInputNames(ActionNode actionNode)
        {
            //TODO:这里应该考虑动作节点输入的翻译，但是暂时先不这么做。
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
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
        private ActionNodeValueInfo[] getActionNodeConstValueInfo(ActionNode actionNode)
        {
            var actionDefine = getActionDefine(actionNode.defineName);
            var infoArray = new ActionNodeValueInfo[actionDefine.consts.Length];
            for (int i = 0; i < infoArray.Length; i++)
            {
                //TODO:这里应该考虑动作节点常量值的翻译，但是暂时先不这么做。
                Type type = actionDefine.consts[i].type;
                if (type == typeof(int))
                    infoArray[i] = new ActionNodeValueInfo(actionDefine.consts[i].name, ActionNodeValueType.integer, actionNode.consts[i]);
                else if (type == typeof(float))
                    infoArray[i] = new ActionNodeValueInfo(actionDefine.consts[i].name, ActionNodeValueType.real, actionNode.consts[i]);
                else if (type == typeof(bool))
                    infoArray[i] = new ActionNodeValueInfo(actionDefine.consts[i].name, ActionNodeValueType.boolean, actionNode.consts[i]);
                else if (type == typeof(string))
                    infoArray[i] = new ActionNodeValueInfo(actionDefine.consts[i].name, ActionNodeValueType.text, actionNode.consts[i]);
                else if (type.IsEnum)
                {
                    infoArray[i] = new ActionNodeValueInfo(actionDefine.consts[i].name, ActionNodeValueType.enumValue, actionNode.consts[i], type);
                }
                else if (type == typeof(ActionValueRef))
                {
                    infoArray[i] = new ActionNodeValueInfo(actionDefine.consts[i].name, ActionNodeValueType.actionValueRef, actionNode.consts[i]);
                }
                else if (type == typeof(CardReference))
                {
                    infoArray[i] = new ActionNodeValueInfo(actionDefine.consts[i].name, ActionNodeValueType.cardReference, actionNode.consts[i]);
                }
                else if (type == typeof(CardReference[]))
                {
                    infoArray[i] = new ActionNodeValueInfo(actionDefine.consts[i].name, ActionNodeValueType.cardReferenceCollection, actionNode.consts[i]);
                }
                else
                    throw new NotImplementedException("没有" + type + "对应的常数类型");
            }
            return infoArray;
        }
        private string[] getActionNodeOutputNames(ActionNode actionNode)
        {
            //TODO:这里应该考虑动作节点输出的翻译，但是暂时先不这么做。
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
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
        private (string name, Color color)[] getActionNodeOutputViewData(ActionNode actionNode)
        {
            //TODO:这里应该考虑动作节点输出的翻译，但是暂时先不这么做。
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
            List<(string name, Color color)> outputList = new List<(string name, Color color)>()
            {
                (string.Empty, game.editor.getColorByType(typeof(ActionNode)))
            };
            for (int i = 0; i < actionDefine.outputs.Length; i++)
            {
                if (actionDefine.outputs[i].type == typeof(ActionNode))
                {
                    //动作分支节点
                    outputList.Add((actionDefine.outputs[i].name, game.editor.getColorByType(typeof(ActionNode))));
                }
                else
                {
                    //输出值节点
                    outputList.Add((actionDefine.outputs[i].name, game.editor.getColorByType(actionDefine.outputs[i].type)));
                }
            }
            return outputList.ToArray();
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
            return _graphInfo.actionInfoDict[node.id.ToString()];
        }
        void setNodeInfo(ActionNode node, ActionNodeGraphNodeInfo graphNodeInfo)
        {
            _graphInfo.actionInfoDict[node.id.ToString()] = graphNodeInfo;
        }
        bool hasNodeInfo(ActionNode node)
        {
            return _graphInfo.actionInfoDict.ContainsKey(node.id.ToString());
        }
        bool removeNodeInfo(ActionNode node)
        {
            return _graphInfo.actionInfoDict.Remove(node.id.ToString());
        }
        private async Task<ActionNodeItem> createActionNodeAtCurPos(ActionDefine actionDefine)
        {
            var actionNode = new ActionNode(0, actionDefine.defineName,
                new ActionValueRef[/*actionDefine.inputs.Length > 0 && actionDefine.inputs.Last().isParams ? actionDefine.inputs.Length - 1 :*/ actionDefine.inputs.Length],
                new object[actionDefine.consts.Length],
                null, new ActionNode[actionDefine.getActionOutputs().Length + 1]);
            _actionNodeList.Add(actionNode);
            actionNode.id = allocActionNodeId();
            _graphInfo.freeActionNodeList.Add(actionNode);
            setNodeInfo(actionNode, new ActionNodeGraphNodeInfo(_curPosX, _curPosY));
            ActionNodeItem actionNodeItem = await createNodeUI4ActionNodeAt(actionNode, _curPosX, _curPosY);
            updateContentSize();
            return actionNodeItem;
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
        private void openMenuForCategory(float posX, float posY)
        {
            string[] categoryNames = getActionDefineGroups().Select(g => g.Key + (g.Count() > 1 ? " >" : "")).ToArray();
            string[] options = new string[categoryNames.Length + 1];
            Array.Copy(categoryNames, 0, options, 0, categoryNames.Length);
            options[options.Length - 1] = StringTable.EDITOR_ACTIONGRAPH_MENU_CUSTOM_ACTION;
            openMenu(posX, posY, options);
        }
        private void updateMenuForCategory()
        {
            string[] categoryNames = getActionDefineGroups().Select(g => g.Key + (g.Count() > 1 ? " >" : "")).ToArray();
            string[] options = new string[categoryNames.Length + 1];
            Array.Copy(categoryNames, 0, options, 0, categoryNames.Length);
            options[options.Length - 1] = StringTable.EDITOR_ACTIONGRAPH_MENU_CUSTOM_ACTION;
            _ui.updateMenu(options);
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
            _curCategroy = null;
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
        private void updateContentSize()
        {
            //更新边界
            Vector2 size = new Vector2();
            for (int i = 0; i < _actionNodeItemList.Count; i++)
            {
                RectTransform childRectTransform = _actionNodeItemList[i].transform as RectTransform;
                Rect childRect = childRectTransform.rect;
                if (size.x < Mathf.Abs(childRectTransform.anchoredPosition.x + childRect.xMin))
                {
                    size.x = Mathf.Abs(childRectTransform.anchoredPosition.x + childRect.xMin);
                }
                if (size.x < Mathf.Abs(childRectTransform.anchoredPosition.x + childRect.xMax))
                {
                    size.x = Mathf.Abs(childRectTransform.anchoredPosition.x + childRect.xMax);
                }
                if (size.y < Mathf.Abs(childRectTransform.anchoredPosition.y + childRect.yMin))
                {
                    size.y = Mathf.Abs(childRectTransform.anchoredPosition.y + childRect.yMin);
                }
                if (size.y < Mathf.Abs(childRectTransform.anchoredPosition.y + childRect.yMax))
                {
                    size.y = Mathf.Abs(childRectTransform.anchoredPosition.y + childRect.yMax);
                }
            }
            _ui.setScrollViewContentSize(size * 2);
        }
        #endregion
        #region 属性字段
        [Obsolete]
        public IAppManager app { get; }
        [Obsolete]
        public MainCtrl main { get; }
        [SerializeField]
        ActionNodeGraphPanel _ui;
        [SerializeField]
        CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        string _curCategroy = null;
        float _curPosX;
        float _curPosY;
        readonly List<ActionNode> _actionNodeList = new List<ActionNode>();
        readonly List<ActionNodeItem> _actionNodeItemList = new List<ActionNodeItem>();
        ActionNodeGraphInfo _graphInfo;
        ActionNodeItem _entryNodeItem;
        ActionNodeItem _returnNodeItem;
        ActionNode rootActionNode
        {
            get
            {
                if (_generatedActionDefine != null)
                    return _generatedActionDefine.action;
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
                if (_generatedActionDefine != null)
                    _generatedActionDefine.action = value;
                else if (_effect != null)
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
        /// <summary>
        /// 当前正在编辑的动作节点
        /// </summary>
        ActionNode _curEditActionNode = null;
        /// <summary>
        /// 当前正在编辑的动作节点常量索引
        /// </summary>
        int _curEditActionNodeConstIndex = -1;
        /// <summary>
        /// 当前正在编辑的动作节点常量子项索引
        /// </summary>
        int _curEditActionNodeConstItemIndex = -1;
        readonly Stack<DisplayInfo> _editObjectStack = new Stack<DisplayInfo>();
        string[] _targetCheckerTypes = new string[]
        {
            DEFINE.TARGET_TYPE_CHARACTER,
            DEFINE.TARGET_TYPE_SERVANT
        };
        string[] _propertyTypes = new string[]
        {
            StringTable.GAME_CARD_PROP_NAME_COST,
            StringTable.GAME_CARD_PROP_NAME_ATTACK,
            StringTable.GAME_CARD_PROP_NAME_LIFE,
            StringTable.GAME_CARD_PROP_NAME_DAMAGE_REDUCE
        };
        string[] _modifyTypes = new string[]
        {
            StringTable.GAME_CARD_PROP_MODIFY_ADD,
            StringTable.GAME_CARD_PROP_MODIFY_SETTO,
            StringTable.GAME_CARD_PROP_MODIFY_ADD_PROP,
            StringTable.GAME_CARD_PROP_MODIFY_SETTO_PROP
        };
        readonly Dictionary<string, string> _nameTextDict = new Dictionary<string, string>()
        {
            { DEFINE.TARGET_TYPE_CHARACTER, StringTable.GAME_TARGET_TYPE_CHARACTER },
            { DEFINE.TARGET_TYPE_SERVANT, StringTable.GAME_TARGET_TYPE_SERVANT },
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
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(int cardPoolId, string entryType, object editObject, DisplayInfo[] editObjectStack, ActionNodeGraphInfo graphInfo)
            {
                this.cardPoolId = cardPoolId;
                this.entryType = entryType;
                this.editObject = editObject;
                this.editObjectStack = editObjectStack;
                this.graphInfo = graphInfo;
            }
            public int cardPoolId;
            public string entryType;
            public object editObject;
            public DisplayInfo[] editObjectStack;
            public ActionNodeGraphInfo graphInfo;
        }
        #endregion
    }
}
