using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using BJSYGameCore.UI;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    partial class ActionNodeGraphCtrl : GameObjectCtrl<MainSceneCtrl>, IController<MainCtrl>, IUICtrl
    {
        #region 公有方法
        /// <summary>
        /// 退出节点图。
        /// </summary>
        public void quitGraph()
        {
        }
        public Task display(UIDisplayInfo uiDisplayInfo)
        {
            if (uiDisplayInfo is DisplayInfo info)
            {
                quitGraph();

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
                _ui.resetView();
                if (info.editObject is GeneratedActionDefine actionDefine)
                    return openGeneratedActionDefine(actionDefine);
                else if (info.editObject is GeneratedEffect effect)
                    return openActionGraph(effect);
                else
                    return Task.CompletedTask;
            }
            throw new NotImplementedException();
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            object editObject;
            if (_effect != null)
                editObject = _effect;
            else if (_generatedActionDefine != null)
                editObject = _generatedActionDefine;
            else
                editObject = null;
            return new DisplayInfo(_curCardPoolId, editObject, _editObjectStack.Count > 0 ? _editObjectStack.ToArray() : null, _graphInfo);
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
            _ui.onTriggerEventTypeChanged += onChangeTriggerTypeIndex;
            _ui.onClickAddTargetCheckerButton += onClickAddTargetCheckerButton;
            _ui.onTargetCheckerTypeValueChanged += onTargetCheckerTypeValueChanged;
            _ui.onClickRemoveTargetCheckerButton += onClickRemoveTargetCheckerButton;
            _ui.onTargetCheckerErrorTipValueChanged += onTargetCheckerErrorTipValueChanged;
            _ui.onClickReturnButton += onClickReturnButton;
            //增益
            _ui.onItemBuffChanged += onItemBuffChangedCallback;
            _ui.onClickItemEditBuffButton += onItemEditBuffCallback;

            _cardPoolViewPanelCtrl.enableCount(false);
            _cardPoolViewPanelCtrl.onClickCard += onClickCardSelectPanelCard;

            string[] languages = game.langManager.getLanguages();
            for (int i = 0; i < languages.Length; i++)
            {
                languages[i] = game.langManager.getString(game.ui.getTranslateNameForLanguage(languages[i]));
            }
            _ui.init(
                languages,
                CardEditorCtrl.getKeywords(),
                CardEditorCtrl.piles.Select(p => (p.Value, p.Key)).ToArray());
        }
        #endregion
        #region 事件回调
        /// <summary>
        /// 进行效果修改后将编辑器的未保存状态设置为true。
        /// </summary>
        private void OnEdited()
        {
            scene.cardEditor.MarkCardUnsaved();
        }
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
                    await scene.editor.openActionEditor(_curCardPoolId, generatedActionDefine);
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
                                string name = a.defineName;
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
                        await scene.editor.openActionEditor(_curCardPoolId, generatedActionDefine);
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
            OnEdited();
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
            Node outputActionNode = getActionNode(outputActionNodeItem);
            Node inputActionNode = getActionNode(inputActionNodeItem);
            IPort outputPort = outputActionNode.getOutputPortAt(outputIndex);
            IPort inputPort = inputActionNode.getInputPortAt(inputIndex);

            if (validatePortConnection(outputPort, inputPort, true))
            {
                if (outputPort is ValueOutput valueOutput && inputPort is ValueInput valueInput)
                {
                    connectValuePorts(valueOutput, valueInput);
                }
                else if (outputPort is ControlOutput controlOutput && inputPort is ControlInput controlInput)
                {
                    connectControlPorts(controlOutput, controlInput);
                }
            }
            OnEdited();
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
            Node outputActionNode = getActionNode(outputActionNodeItem);
            Node inputActionNode = getActionNode(inputActionNodeItem);
            IPort outputPort = outputActionNode.getOutputPortAt(outputIndex);
            IPort inputPort = inputActionNode.getInputPortAt(inputIndex);
            disconnectPorts(outputPort, inputPort);
            OnEdited();
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
            openMenuForNodeItem(actionNodeItem, pointerPosition);
        }
        /// <summary>
        /// 当用户长按节点，展开节点菜单。
        /// </summary>
        /// <param name="actionNodeItem"></param>
        private void onLongPressActionNode(ActionNodeItem actionNodeItem, Vector2 pointerPosition)
        {
            openMenuForNodeItem(actionNodeItem, pointerPosition);
        }
        private void onDragActionNode(ActionNodeItem actionNodeItem, Vector2 graphPosition)
        {
            Vector2 rootPosition = _ui.getNodesRootPosition();
            Node node = getActionNode(actionNodeItem);
            node.posX = graphPosition.x - rootPosition.x;
            node.posY = graphPosition.y - rootPosition.y;
            _ui.moveActionNodeTo(actionNodeItem, graphPosition);
            updateContentSize();
            OnEdited();
        }
        /// <summary>
        /// 当用户更改节点的常量值，设置对应的常量。
        /// </summary>
        /// <param name="actionNodeItem"></param>
        /// <param name="constIndex"></param>
        /// <param name="value"></param>
        private void onActionNodeValueChange(ActionNodeItem actionNodeItem, int constIndex, object value)
        {
            var actionNode = getActionNode(actionNodeItem);
            if (actionNode is ActionNode action)
            {
                var actionDefine = getActionDefine(action.defineName);
                var name = actionDefine.constDefines.ElementAt(constIndex).name;
                action.setConst(name, value);
                OnEdited();
            }
        }
        private async void onClickActionNodeConstValueEditButton(ActionNodeItem actionNodeItem, int index)
        {
            Node actionNode = getActionNode(actionNodeItem);
            if (!(actionNode is ActionNode action))
                return;
            ActionDefine actionDefine = getActionDefine(action.defineName);
            var constDefine = actionDefine.constDefines.ElementAtOrDefault(index);
            if (constDefine == null)
                return;
            if (constDefine.type == typeof(DefineReference))
            {
                _ui.displayCardSelectPanel(true);
                if (action.getConst(constDefine.name) is DefineReference value)
                {
                    //打开卡牌选择界面
                    await _cardPoolViewPanelCtrl.display(value.defineId, cardPoolsId: new long[] { value.cardPoolId }, cardFilter: c =>
                    {
                        //所有卡都可以选
                        return true;
                    });
                }
                else
                {
                    await _cardPoolViewPanelCtrl.display(cardPoolsId: new long[] { _curCardPoolId }, cardFilter: c =>
                    {
                        //所有卡都可以选
                        return true;
                    });
                }
                _curEditActionNode = action;
                _curEditActionNodeConstName = constDefine.name;
                _curEditActionNodeConstItemIndex = -1;
            }
        }
        private async void onClickActionNodeConstValueItemEditButton(ActionNodeItem actionNodeItem, int index, int itemIndex)
        {
            Node actionNode = getActionNode(actionNodeItem);
            if (!(actionNode is ActionNode action))
                return;
            ActionDefine actionDefine = getActionDefine(action.defineName);
            var constDefine = actionDefine.constDefines.ElementAtOrDefault(index);
            if (constDefine == null)
                return;
            if (constDefine.type == typeof(DefineReference[]))
            {
                _ui.displayCardSelectPanel(true);
                if (actionNode.getConst(constDefine.name) is DefineReference[] value)
                {
                    if (value[itemIndex] == null)
                    {
                        value[itemIndex] = new DefineReference();
                    }
                    //打开卡牌选择界面
                    await _cardPoolViewPanelCtrl.display(value[itemIndex].defineId, new long[] { value[itemIndex].cardPoolId }, cardFilter: c =>
                    {
                        //所有卡都可以选
                        return true;
                    });
                }
                else
                {
                    await _cardPoolViewPanelCtrl.display(cardPoolsId: new long[] { _curCardPoolId }, cardFilter: c =>
                    {
                        //所有卡都可以选
                        return true;
                    });
                }
                _curEditActionNode = action;
                _curEditActionNodeConstName = constDefine.name;
                _curEditActionNodeConstItemIndex = itemIndex;
            }
            OnEdited();
        }
        private async void onClickCardSelectPanelCard(CardDefine cardDefine)
        {
            if (_curEditActionNode == null)
            {
                return;
            }
            var consts = _curEditActionNode.consts;
            if (_curEditActionNodeConstItemIndex < 0)
            {
                consts[_curEditActionNodeConstName] = cardDefine.getDefineRef();
            }
            else
            {
                (consts[_curEditActionNodeConstName] as DefineReference[])[_curEditActionNodeConstItemIndex] = cardDefine.getDefineRef();
            }
            ActionNodeItem actionNodeItem = getActionNodeItem(_curEditActionNode);

            var constIndex = consts.Keys.ToList().IndexOf(_curEditActionNodeConstName);
            ValueInputField cardInputField = actionNodeItem.getConstInputField(constIndex);
            CardSkinData cardSkin = null;
            Sprite image = null;
            if (cardDefine != null)
            {
                cardSkin = game.card.getDefaultSkin(cardDefine);
                image = cardSkin.image ??
                    await game.card.loadImage(cardSkin.imagePath, game.card.getCardPoolInfoOfCard(cardDefine) is CardPoolInfo cardPoolInfo2 ? cardPoolInfo2.cardPoolId : 0);
            }
            if (_curEditActionNodeConstItemIndex >= 0)
            {
                cardInputField = cardInputField.getItemAt(_curEditActionNodeConstItemIndex);
            }
            cardInputField.setCard(cardDefine, cardSkin, image);
            OnEdited();
            //重置字段
            _curEditActionNode = null;
            _curEditActionNodeConstName = string.Empty;
            _curEditActionNodeConstItemIndex = -1;
            //隐藏UI
            _ui.displayCardSelectPanel(false);
        }
        private void onClickCancelSelectCardButton()
        {
            //重置字段
            _curEditActionNode = null;
            _curEditActionNodeConstName = string.Empty;
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
        #region 打开节点图界面
        /// <summary>
        /// 打开某个效果的生效或者失效动作图
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="entryType"></param>
        private async Task openActionGraph(GeneratedEffect effect)
        {
            //清理数据
            resetEditObjects();
            _effect = effect;

            await ApplyGraph(effect.graph);

            updateContentSize();
        }
        #endregion
        #region 节点UI创建/删除/获取
        /// <summary>
        /// 为动作节点以及其所有子节点创建动作节点图
        /// </summary>
        /// <param name="actionNode"></param>
        private async Task ApplyGraph(ActionGraph graph)
        {
            foreach (var node in graph.nodes)
            {
                if (node is ActionNode action)
                {
                    await createNodeUI4ActionNode(action);
                }
                else if (node is TriggerEntryNode trigger)
                {
                    string nodeName = game.lang._(StringTable.EDITOR_NODE_NAME_TRIGGER);
                    var targetTypes = getTranslatedTargetCheckerTypes();
                    var checkers = getTriggerTargetCheckerViewData(trigger);
                    var inputViewData = getTriggerInputViewData(trigger);
                    var outputViewData = getTriggerOutputViewData(trigger);

                    var actionNodeItem = _ui.createTriggerNode(trigger.posX, trigger.posY, nodeName, inputViewData, 
                        targetTypes, _eventTypeInfoList.getEventTypeIndex(trigger.eventName), checkers,
                        outputViewData);
                    actionNodeItem.displayEventType(!trigger.hideEvents);
                    _actionNodeItemList.Add(actionNodeItem);
                    _actionNodeList.Add(node);
                }
                else if (node is GeneratedActionEntryNode genEntry)
                {
                    string nodeName = game.lang._(StringTable.EDITOR_NODE_NAME_INPUT);
                    var inputViewData = new PortItemInfo[0];
                    ActionNodeValueInfo[] constsInfo = new ActionNodeValueInfo[0];
                    var outputViewData = getActionDefineInputViewData(_generatedActionDefine);

                    var actionNodeItem = _ui.createNode(genEntry.posX, genEntry.posY, nodeName, inputViewData, constsInfo, outputViewData);
                    _actionNodeItemList.Add(actionNodeItem);
                    _actionNodeList.Add(node);
                }
                else if (node is GeneratedActionReturnNode genReturn)
                {
                    string nodeName = game.lang._(StringTable.EDITOR_NODE_NAME_OUTPUT);
                    var inputViewData = getActionDefineOutputViewData(_generatedActionDefine);
                    ActionNodeValueInfo[] constsInfo = new ActionNodeValueInfo[0];
                    var outputViewData = new PortItemInfo[0];

                    var actionNodeItem = _ui.createNode(genReturn.posX, genReturn.posY, nodeName, inputViewData, constsInfo, outputViewData);
                    _actionNodeItemList.Add(actionNodeItem);
                    _actionNodeList.Add(node);
                }
            }

            foreach (var connection in graph.connections)
            {
                if (connection == null)
                    continue;
                var outputNode = connection.source?.node;
                var inputNode = connection.destination?.node;
                if (outputNode == null || inputNode == null)
                    continue;

                var outputNodeItem = getActionNodeItem(outputNode);
                var inputNodeItem = getActionNodeItem(inputNode);
                if (outputNodeItem == null || inputNodeItem == null)
                    continue;

                ActionNodeConnectionType type = ActionNodeConnectionType.value;
                if (connection.GetConnectionType() == PortType.Control)
                {
                    type = ActionNodeConnectionType.action;
                }

                _ui.connectNode(outputNodeItem, connection.source.GetPortIndex(), inputNodeItem, connection.destination.GetPortIndex(), type);
            }
        }
        /// <summary>
        /// 在指定位置上为动作节点创建节点图UI，不包括它的子节点。
        /// </summary>
        /// <param name="actionNode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns>动作节点对应的UI组件</returns>
        private async Task<ActionNodeItem> createNodeUI4ActionNode(ActionNode actionNode)
        {
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
            ActionNodeItem actionNodeItem;
            if (actionDefine is GetBuffDefineAction getBuffDefineAction)
            {
                //如果是获取增益定义节点，那么它和普通的节点不一样，会显示增益的编辑界面。
                //先获取节点的增益
                GeneratedBuffDefine generatedBuffDefine = null;
                BuffSkin buffSkin = null;
                string constName = GetBuffDefineAction.constName;
                if (actionNode.consts == null || actionNode.consts.Count < 1)
                {
                    //节点没有增益，那么就为它新建一个
                    generatedBuffDefine = game.card.createBuffDefine(_curCardPoolId);
                    actionNode.setConst(constName, new DefineReference(generatedBuffDefine.cardPoolId, generatedBuffDefine.id));
                    //同时还有增益的皮肤，也要新建一个
                    buffSkin = game.card.createBuffSkin(_curCardPoolId, generatedBuffDefine.id, game.langManager.getLanguage());
                }
                else if (!tryGetBuffDefine(actionNode.getConst(constName), out GeneratedBuffDefine buffDefine))
                {
                    //读到了增益id，但是查无此增益，那么重新创建一个
                    generatedBuffDefine = game.card.createBuffDefine(_curCardPoolId);
                    actionNode.setConst(constName, new DefineReference(_curCardPoolId, generatedBuffDefine.id));
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
                        buffSkin = game.card.createBuffSkin(
                            generatedBuffDefine.cardPoolId,
                            generatedBuffDefine.id,
                            game.langManager.getLanguage());
                    }
                }
                BuffDefine[] buffDefines = game.card.getBuffDefines(game.card.getLocalCardPoolReference(_curCardPoolId))
                    .OrderBy(p => p.Value.cardPoolId)
                    .ThenBy(p => p.Value.id)
                    .Select(p => p.Value)
                    .ToArray();


                var buffNames = new List<string>()
                {
                    game.lang._(ACTION_NODE_NEW_BUFF)
                };
                buffNames.AddRange(game.card.getBuffDefineNames(buffDefines));
                actionNodeItem = _ui.createNodeForBuff(actionNode.posX, actionNode.posY, new ActionNodeBuffViewData()
                {
                    buffNames = buffNames.ToArray(),
                    buffIndex = Array.IndexOf(buffDefines, generatedBuffDefine) + 1
                }, getActionNodeOutputNames(actionNode));
            }
            else if (actionDefine != null)
            {
                ActionNodeValueInfo[] constsInfo = getActionNodeConstValueInfo(actionNode);
                string nodeKey = actionDefine.hideDocument ? null : actionNode.defineName;
                actionNodeItem = _ui.createNode(
                    actionNode.posX,
                    actionNode.posY,
                    getActionNodeName(actionNode),
                    getActionNodeInputViewData(actionNode),
                    constsInfo,
                    getActionNodeOutputViewData(actionNode),
                    getActionNodeObsoleteMsg(actionNode),
                    nodeKey);
                for (int i = 0; i < constsInfo.Length; i++)
                {
                    var constInfo = constsInfo[i];
                    var name = constInfo.name;
                    if (constInfo.type == ActionNodeValueType.cardReference)
                    {
                        CardDefine cardDefine = null;
                        if (actionNode.getConst(name) is DefineReference cardReference)
                        {
                            long cardPoolId = cardReference.cardPoolId;
                            var ownedRef = game.card.getLocalCardPoolReference(cardPoolId);
                            // 如果卡池没有被加载，加载卡池。
                            if (!game.card.tryGetCardPool(ownedRef, out _))
                            {
                                var path = game.card.getCardPoolPath(ownedRef);
                                if (path != null)
                                {
                                    await game.card.loadCardPool(path);
                                }
                            }
                            cardDefine = game.card.getCardDefine(ownedRef, cardReference.defineId);
                        }
                        CardSkinData cardSkin = null;
                        Sprite sprite = null;
                        if (cardDefine != null)
                        {
                            cardSkin = game.card.getDefaultSkin(cardDefine);
                            sprite = cardSkin.image ??
                                await game.card.loadImage(cardSkin.imagePath, game.card.getCardPoolInfoOfCard(cardDefine) is CardPoolInfo cardPoolInfo ? cardPoolInfo.cardPoolId : 0);
                        }
                        actionNodeItem.getConstInputField(i).setCard(cardDefine, cardSkin, sprite);
                    }
                    else if (constInfo.type == ActionNodeValueType.cardReferenceCollection)
                    {
                        if (actionNode.getConst(name) is DefineReference[] cardRefArray)
                        {
                            for (int j = 0; j < cardRefArray.Length; j++)
                            {
                                CardDefine cardDefine = null;
                                if (cardRefArray[j] is DefineReference cardReference)
                                {
                                    var localRef = game.card.getLocalCardPoolReference(cardReference.cardPoolId);
                                    cardDefine = game.card.getCardDefine(localRef, cardReference.defineId);
                                }
                                CardSkinData cardSkin = null;
                                Sprite sprite = null;
                                if (cardDefine != null)
                                {
                                    cardSkin = game.card.getDefaultSkin(cardDefine);
                                    sprite = cardSkin.image ??
                                        await game.card.loadImage(cardSkin.imagePath, game.card.getCardPoolInfoOfCard(cardDefine) is CardPoolInfo cardPoolInfo ? cardPoolInfo.cardPoolId : 0);
                                }
                                actionNodeItem.getConstInputField(i).getItemAt(j).setCard(cardDefine, cardSkin, sprite);
                            }
                        }
                    }
                }
            }
            else
            {
                actionNodeItem = _ui.createNode(
                    actionNode.posX,
                    actionNode.posY,
                    game.langManager.getString(StringTable.EDITOR_NODE_NAME_UNKNOW));
                actionNodeItem.setInputs(new PortItemInfo[] { new PortItemInfo(string.Empty, game.editor.getColorByType(typeof(ActionNode)), true) });
                actionNodeItem.setOutputs(new PortItemInfo[] { new PortItemInfo(string.Empty, game.editor.getColorByType(typeof(ActionNode)), true) });
            }
            _actionNodeItemList.Add(actionNodeItem);
            if (!_actionNodeList.Contains(actionNode))
            {
                _actionNodeList.Add(actionNode);
            }
            return actionNodeItem;
        }
        private async Task<ActionNodeItem> createActionNodeAtCurPos(ActionDefine actionDefine)
        {
            var actionNode = graph.createActionNode(actionDefine, _curPosX, _curPosY);
            _actionNodeList.Add(actionNode);
            ActionNodeItem actionNodeItem = await createNodeUI4ActionNode(actionNode);
            updateContentSize();
            return actionNodeItem;
        }
        private void removeActionNode()
        {
            ActionNode actionNode = _curEditActionNode;
            int actionNodeIndex = _actionNodeList.IndexOf(_curEditActionNode);
            ActionNodeItem actionNodeItem = getActionNodeItem(actionNode);
            disconnectNodeAll(actionNode);
            //从列表中移除
            _actionNodeList.RemoveAt(actionNodeIndex);
            _actionNodeItemList.RemoveAt(actionNodeIndex);
            graph.removeNode(actionNode);
            //将节点从UI上移除
            _ui.removeNode(actionNodeItem);
            updateContentSize();
        }
        private ActionNodeItem getActionNodeItem(Node actionNode)
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
        #endregion
        #region 节点连接
        private void connectValuePorts(ValueOutput outputPort, ValueInput inputPort)
        {
            var outputNodeItem = getActionNodeItem(outputPort.node);
            var inputNodeItem = getActionNodeItem(inputPort.node);
            var outputIndex = outputPort.GetPortIndex();
            var inputIndex = inputPort.GetPortIndex();
            if (graph.isConnected(outputPort, inputPort))
            {
                //已经相连，什么也不做
                return;
            }
            else
            {
                var connectedPort = inputPort.getConnectedOutputPort();
                if (connectedPort != null)
                {
                    //相连的是其他输出节点，断开连接
                    graph.disconnect(inputPort, connectedPort);
                    var connectedNodeItem = getActionNodeItem(connectedPort.node);
                    var connectedNodeIndex = connectedPort.GetPortIndex();
                    _ui.unconnectNode(outputNodeItem, outputIndex, connectedNodeItem, connectedNodeIndex);
                }
            }

            //建立新的连接
            graph.connect(outputPort, inputPort);
            //更新ui
            _ui.connectNode(outputNodeItem, outputIndex, inputNodeItem, inputIndex, ActionNodeConnectionType.value);
            //更新变长参数。
            if (inputPort.node is ActionNode actionNode && actionNode.define?.isParams == true)
            {
                inputNodeItem.setInputs(getActionNodeInputViewData(actionNode));
            }
        }

        private void connectControlPorts(ControlOutput outputPort, ControlInput inputPort)
        {
            var outputNodeItem = getActionNodeItem(outputPort.node);
            var inputNodeItem = getActionNodeItem(inputPort.node);
            var outputIndex = outputPort.GetPortIndex();
            var inputIndex = inputPort.GetPortIndex();
            if (graph.isConnected(outputPort, inputPort))
            {
                //已经相连，什么也不做
                return;
            }
            else
            {
                var connectedPort = outputPort.getConnectedInputPort();
                if (connectedPort != null)
                {
                    //相连的是其他输入节点，断开连接
                    graph.disconnect(outputPort, connectedPort);
                    var connectedNodeItem = getActionNodeItem(connectedPort.node);
                    var connectedNodeIndex = connectedPort.GetPortIndex();
                    _ui.unconnectNode(outputNodeItem, outputIndex, connectedNodeItem, connectedNodeIndex);
                }
            }

            //建立新的连接
            graph.connect(outputPort, inputPort);
            //更新ui
            _ui.connectNode(outputNodeItem, outputIndex, inputNodeItem, inputIndex, ActionNodeConnectionType.action);
        }


        private void connectPorts(IPort outputPort, IPort inputPort)
        {
            if (outputPort is ControlOutput co && inputPort is ControlInput ci)
            {
                connectControlPorts(co, ci);
            }
            else if (outputPort is ValueOutput vo && inputPort is ValueInput vi)
            {
                connectValuePorts(vo, vi);
            }
        }
        #endregion
        #region 节点断开
        private void disconnectPorts(IPort outputPort, IPort inputPort)
        {
            //更新ui
            var outputNodeItem = getActionNodeItem(outputPort.node);
            var inputNodeItem = getActionNodeItem(inputPort.node);
            _ui.unconnectNode(outputNodeItem, outputPort.GetPortIndex(), inputNodeItem, inputPort.GetPortIndex());
            graph.disconnect(outputPort, inputPort);
            //更新变长参数。
            if (inputPort is ValueInput && inputPort.node is ActionNode actionNode && actionNode.define?.isParams == true)
            {
                inputNodeItem.setInputs(getActionNodeInputViewData(actionNode));
            }
        }
        /// 断开：动作节点----所有节点
        /// </summary>
        /// <param name="actionNode">动作节点</param>
        private void disconnectNodeAll(Node actionNode)
        {
            var connections = graph.getNodeConnections(actionNode);
            foreach (var connection in connections)
            {
                disconnectPorts(connection.source, connection.destination);
            }
        }
        #endregion
        #region 合法检测
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
        private bool validateConnectionType(Type outputType, Type inputType, bool showErrorTip = true)
        {
            if (!isValidTypeForConnection(outputType, inputType))
            {
                //相互连接的输入输出类型必须可以相互转化
                if (showErrorTip)
                    _ui.showErrorTip(game.lang._(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
                return false;
            }
            return true;
        }
        private void ShowValueConnectActionError()
        {
            _ui.showErrorTip(game.lang._(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
        }
        private bool validatePortConnection(IPort outputPort, IPort inputPort, bool showErrorTip = true)
        {
            if (outputPort.canConnectTo(inputPort))
            {
                try
                {
                    Type outputType = outputPort.define.type;
                    Type inputType = inputPort.define.type;
                    return validateConnectionType(outputType, inputType, showErrorTip);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"验证端点“{outputPort}”和“{inputPort}”的连接合法性时出现异常：{e}");
                    return false;
                }
            }
            else
            {
                //值输出只能与值输入相互连接
                if (showErrorTip)
                    ShowValueConnectActionError();
                return false;
            }
        }
        #endregion
        #region 节点操作
        private Node getActionNode(ActionNodeItem actionNodeItem)
        {
            int index = _actionNodeItemList.IndexOf(actionNodeItem);
            return _actionNodeList[index];
        }
        private Node getActionNode(int actionNodeId)
        {
            return _actionNodeList.Find(a => a != null && a.id == actionNodeId);
        }
        private string getActionNodeName(ActionNode actionNode)
        {
            if (_nameTextDict.TryGetValue(actionNode.defineName, out string text))
            {
                return game.lang._(text);
            }
            else
            {
                Debug.LogWarning("没有找到动作" + actionNode.defineName + "对应的名称");
                return game.lang._(actionNode.defineName);
            }
        }
        private string tryTranslateWithDict(string input)
        {
            if (_nameTextDict.TryGetValue(input, out string text))
            {
                return game.i18n._(text);
            }
            return game.i18n._(input);
        }

        private string getActionNodeObsoleteMsg(ActionNode actionNode)
        {
            ActionDefine actionDefine = getActionDefine(actionNode.defineName);
            if (actionDefine.obsoleteMsg == null) return null;
            if (actionDefine.obsoleteMsg == "") return "";
            return game.lang._(actionDefine.obsoleteMsg);
        }
        #endregion
        /// <summary>
        /// 为动作节点分配id，
        /// </summary>
        /// <returns></returns>
        #region 节点信息
        private ActionNodeValueInfo[] getActionNodeConstValueInfo(ActionNode actionNode)
        {
            var actionDefine = getActionDefine(actionNode.defineName);
            var count = actionDefine.constDefines.Count();
            var infoArray = new ActionNodeValueInfo[count];
            for (int i = 0; i < count; i++)
            {
                var constDefine = actionDefine.constDefines.ElementAt(i);
                // 翻译名称
                var name = tryTranslateWithDict(constDefine.getDisplayName());
                Type type = constDefine.type;
                object value = actionNode.getConst(constDefine.name);
                if (type == typeof(int))
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.integer, value);
                else if (type == typeof(float))
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.real, value);
                else if (type == typeof(bool))
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.boolean, value);
                else if (type == typeof(string))
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.text, value);
                else if (type.IsEnum)
                {
                    var optionalValues = Enum.GetValues(type).Cast<object>().ToArray();
                    var optionalNames = new string[optionalValues.Length];
                    for (int j = 0; j < optionalNames.Length; j++)
                    {
                        var optName = Enum.GetName(type, optionalValues[j]);
                        var indexName = type.FullName + "." + optName;
                        if (_nameTextDict.TryGetValue(indexName, out string nameText))
                        {
                            optionalNames[j] = game.i18n._(nameText);
                        }
                        else
                        {
                            // todo: Get attribute and translate
                            optionalNames[j] = optName;
                        }
                    }
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.enumValue, value, optionalNames, optionalValues);
                }
                else if (type == typeof(ActionValueRef))
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.actionValueRef, value);
                else if (type == typeof(DefineReference))
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.cardReference, value);
                else if (type == typeof(DefineReference[]))
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.cardReferenceCollection, value);
                else if (type == typeof(PileNameCollection))
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.pileNameCollection, value);
                else if (type == typeof(KeywordCollection))
                    infoArray[i] = new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.keywords, value);
                else
                    throw new NotImplementedException("没有" + type + "对应的常数类型");
            }
            return infoArray;
        }
        private string[] getActionNodeInputNames(ActionNode actionNode)
        {
            string[] inputNames = new string[actionNode.inputPorts.Count()];
            for (int i = 0; i < inputNames.Length; i++)
            {
                // 翻译输出节点
                var outputName = actionNode.inputPorts.ElementAt(i).define.getDisplayName();
                inputNames[i] = tryTranslateWithDict(outputName);
            }
            return inputNames;
        }
        private PortItemInfo[] getActionNodeInputViewData(ActionNode actionNode)
        {
            List<PortItemInfo> viewDataList = new List<PortItemInfo>();
            //值输入
            foreach (var inputPort in actionNode.inputPorts)
            {
                // 翻译输出节点
                var inputName = tryTranslateWithDict(inputPort.define.getDisplayName());
                viewDataList.Add(new PortItemInfo(inputName, game.editor.getColorByType(inputPort.define.type), inputPort.GetPortType() == PortType.Control));
            }
            return viewDataList.ToArray();
        }
        private string[] getActionNodeOutputNames(ActionNode actionNode)
        {
            string[] outputNames = new string[actionNode.outputPorts.Count()];
            for (int i = 0; i < outputNames.Length; i++)
            {
                // 翻译输出节点
                var outputName = actionNode.outputPorts.ElementAt(i).define.getDisplayName();
                outputNames[i] = tryTranslateWithDict(outputName);
            }
            return outputNames;

        }
        private PortItemInfo[] getActionNodeOutputViewData(ActionNode actionNode)
        {
            List<PortItemInfo> outputList = new List<PortItemInfo>();
            foreach (var output in actionNode.outputPorts)
            {
                // 翻译输出节点
                var outputName = tryTranslateWithDict(output.define.getDisplayName());
                outputList.Add(new PortItemInfo(outputName, game.editor.getColorByType(output.define.type), output.GetPortType() == PortType.Control));
            }
            return outputList.ToArray();
        }
        #endregion
        #region 节点菜单
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
            var options = getActionDefineGroups().Select(g => game.i18n._(g.Key) + (g.Count() > 1 ? " >" : "")).ToList();
            options.Add(game.i18n._(StringTable.EDITOR_ACTIONGRAPH_MENU_CUSTOM_ACTION));
            _curEditActionNode = null;
            openMenu(posX, posY, options.ToArray());
        }
        private void updateMenuForCategory()
        {
            var options = getActionDefineGroups().Select(g => game.i18n._(g.Key) + (g.Count() > 1 ? " >" : "")).ToList();
            options.Add(game.i18n._(StringTable.EDITOR_ACTIONGRAPH_MENU_CUSTOM_ACTION));
            _ui.updateMenu(options.ToArray());
        }
        private void openMenuForNodeItem(ActionNodeItem actionNodeItem, Vector2 pointerPosition)
        {
            var node = getActionNode(actionNodeItem);
            //不能通过长按编辑
            if (!(node is ActionNode action))
                return;
            var define = getActionDefine(action.defineName);
            if (define != null && define.disableMenu)
                return;
            _curEditActionNode = action;
            Vector2 position = _ui.worldPositionToGraphPosition(pointerPosition);
            List<string> optionList = new List<string>()
            {
                game.langManager.getString(StringTable.DELETE)//删除
            };
            if (getActionDefine(action.defineName) is GeneratedActionDefine)
            {
                optionList.Add(game.langManager.getString(StringTable.EDIT));//编辑
            }
            openMenu(position, optionList.ToArray());
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
        #endregion
        /// <summary>
        /// 重置所有对于编辑物体的引用。
        /// </summary>
        private void resetEditObjects()
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _ui.clearGraph();
            _effect = null;
            _generatedActionDefine = null;
        }
        /// <summary>
        /// 将当前正在编辑的对象入栈
        /// </summary>
        private void pushCurEditObjectToStack()
        {
            if (_generatedActionDefine != null)
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _generatedActionDefine, null, _graphInfo));
            else
                _editObjectStack.Push(new DisplayInfo(_curCardPoolId, _effect, null, _graphInfo));
            _ui.setReturnButtonActive(true);
        }
        private void popPrevEditObjectFromStackAndOpen()
        {
            display(_editObjectStack.Pop());
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

        }
        #endregion
        #region 属性字段
        [TranslateMsg("编辑器-参数名称-作用条件（自定义光环-作用条件-输出值）")]
        public const string EDITOR_PARAM_NAME_AFFECT_CONDITION = "作用条件";
        [TranslateMsg("编辑器-参数名称-目标增益（自定义光环-更新动作-输出值）")]
        public const string EDITOR_PARAM_NAME_TARGETS_BUFF = "目标增益";

        [Obsolete]
        public IAppManager app { get; }
        [Obsolete]
        public MainCtrl main { get; }
        [SerializeField]
        ActionNodeGraphPanel _ui;
        [SerializeField]
        EffectEditorCtrl _effectEditor = null;
        [SerializeField]
        CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        string _curCategroy = null;
        float _curPosX;
        float _curPosY;
        readonly List<Node> _actionNodeList = new List<Node>();
        readonly List<ActionNodeItem> _actionNodeItemList = new List<ActionNodeItem>();
        ActionGraph graph
        {
            get
            {
                if (_generatedActionDefine != null)
                    return _generatedActionDefine.graph;
                if (_effect != null)
                    return _effect.graph;
                return null;
            }
        }
        ActionNodeGraphInfo _graphInfo;
        long _curCardPoolId = 0;
        GeneratedEffect _effect;
        /// <summary>
        /// 当前正在编辑的动作节点
        /// </summary>
        ActionNode _curEditActionNode = null;
        /// <summary>
        /// 当前正在编辑的动作节点常量名
        /// </summary>
        string _curEditActionNodeConstName = string.Empty;
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

        [TranslateMsg("内置节点名称，用于通用效果设置属性")]
        const string EDITOR_NODE_NAME_GENERATED_EFFECT_DATA = "通用效果属性";
        [TranslateMsg("内置节点名称，用于光环效果设置属性")]
        const string EDITOR_NODE_NAME_AURA_DATA = "光环效果属性";

        [TranslateMsg("内置节点参数，通常用于比较和操作，表示操作符，如加、减、或、异或等")]
        const string EDITOR_PARAM_OPERATOR = "运算符";
        [TranslateMsg("内置节点参数，通常用于表示计算结果")]
        const string EDITOR_PARAM_RESULT = "结果";
        [TranslateMsg("节点参数 PropertyName")]
        const string EDITOR_PARAM_PROPERTY_NAME = "属性名";
        [TranslateMsg("节点参数 RemoveModifierEvent")]
        const string EDITOR_PARAM_REMOVE_MODIFIER_EVENT = "移除修改器事件";
        [TranslateMsg("节点参数 isSet")]
        const string EDITOR_PARAM_IS_SET = "设置";
        [TranslateMsg("节点参数 VariableName")]
        const string EDITOR_PARAM_VARIABLE_NAME = "变量名";
        [TranslateMsg("节点参数 PileName")]
        const string EDITOR_PARAM_PILE_NAME = "牌堆名";
        [TranslateMsg("节点参数 IsChanged")]
        const string EDITOR_PARAM_IS_CHANGED = "是否已改变";
        [TranslateMsg("节点参数 DamageEvent")]
        const string EDITOR_PARAM_DAMAGE_EVENT = "伤害事件";
        [TranslateMsg("节点参数 DamageSource")]
        const string EDITOR_PARAM_DAMAGE_SOURCE = "伤害来源";
        [TranslateMsg("节点参数 Amount")]
        const string EDITOR_PARAM_DAMAGE_AMOUNT = "伤害量";
        [TranslateMsg("节点参数 Target")]
        const string EDITOR_PARAM_TARGET = "目标";
        [TranslateMsg("节点参数 TargetName")]
        const string EDITOR_PARAM_TARGET_NAME = "目标名";
        [TranslateMsg("节点参数 TargetBuff")]
        const string EDITOR_PARAM_TARGET_BUFF = "目标增益";
        

        [TranslateMsg("整数运算符，加法 +")]
        const string EDITOR_ENUM_INT_OPERATOR_ADD = "加";
        [TranslateMsg("整数运算符，减法 -")]
        const string EDITOR_ENUM_INT_OPERATOR_SUB = "减";
        [TranslateMsg("整数运算符，乘法 *")]
        const string EDITOR_ENUM_INT_OPERATOR_MUL = "乘";
        [TranslateMsg("整数运算符，除法 /")]
        const string EDITOR_ENUM_INT_OPERATOR_DIV = "除";
        [TranslateMsg("整数运算符，取模 %")]
        const string EDITOR_ENUM_INT_OPERATOR_MOD = "取模";
        [TranslateMsg("逻辑运算符，取反 !")]
        const string EDITOR_ENUM_LOGIC_OPERATOR_NOT = "非";
        [TranslateMsg("逻辑运算符，逻辑与 &&")]
        const string EDITOR_ENUM_LOGIC_OPERATOR_AND = "与";
        [TranslateMsg("逻辑运算符，逻辑或 ||")]
        const string EDITOR_ENUM_LOGIC_OPERATOR_OR = "或";

        [TranslateMsg("比较运算符，等于 ==")]
        const string EDITOR_ENUM_CMP_OPERATOR_EQUAL = "等于";
        [TranslateMsg("比较运算符，不等于 !=")]
        const string EDITOR_ENUM_CMP_OPERATOR_UNEQUAL = "不等于";
        [TranslateMsg("比较运算符，大于 >")]
        const string EDITOR_ENUM_CMP_OPERATOR_GREATER = "大于";
        [TranslateMsg("比较运算符，大于等于 >=")]
        const string EDITOR_ENUM_CMP_OPERATOR_GREATER_EQUAL = "大于等于";
        [TranslateMsg("比较运算符，小于 <")]
        const string EDITOR_ENUM_CMP_OPERATOR_LESS = "小于";
        [TranslateMsg("比较运算符，小于等于 <=")]
        const string EDITOR_ENUM_CMP_OPERATOR_LESS_EQUAL = "小于等于";

        [TranslateMsg("卡牌类型，法术")]
        const string EDITOR_ENUM_CARD_TYPE_SPELL = "法术";
        [TranslateMsg("卡牌类型，仆从")]
        const string EDITOR_ENUM_CARD_TYPE_SERVANT = "仆从";
        [TranslateMsg("卡牌类型，英雄")]
        const string EDITOR_ENUM_CARD_TYPE_MASTER = "英雄";
        [TranslateMsg("卡牌类型，技能")]
        const string EDITOR_ENUM_CARD_TYPE_SKILL = "技能";
        [TranslateMsg("卡牌类型，道具")]
        const string EDITOR_ENUM_CARD_TYPE_ITEM = "道具";

        readonly Dictionary<string, string> _nameTextDict = new Dictionary<string, string>()
        {
            { DEFINE.TARGET_TYPE_CHARACTER, StringTable.GAME_TARGET_TYPE_CHARACTER },
            { DEFINE.TARGET_TYPE_SERVANT, StringTable.GAME_TARGET_TYPE_SERVANT },
            
            //节点分类
            { "Game", StringTable.EDITOR_NODE_CATEGORY_GAME },
            { "Player", StringTable.EDITOR_NODE_CATEGORY_PLAYER },
            { "Pile", StringTable.EDITOR_NODE_CATEGORY_PILE },
            { "Card", StringTable.EDITOR_NODE_CATEGORY_CARD },
            { "Const", StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST },
            { "Modifier", StringTable.EDITOR_NODE_CATEGORY_MODIFIER },
            { "Event", StringTable.EDITOR_NODE_CATEGORY_EVENT },
            { "CollectionOperation", StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION },
            //节点名称
            { "IntegerOperation", StringTable.EDITOR_NODE_NAME_INTEGER_OPERATION },
            { "LogicOperation", StringTable.EDIOTR_NODE_NAME_LOGIC_OPERATION },
            { "Compare", StringTable.EDIOTR_NODE_NAME_COMPARE },
            { "IntegerConst", StringTable.EDIOTR_NODE_NAME_INTEGER_CONST },
            { "StringConst", StringTable.EDIOTR_NODE_NAME_STRING_CONST },
            { "BooleanConst", StringTable.EDIOTR_NODE_NAME_BOOLEAN_CONST },
            { "ThisCard", StringTable.EDITOR_NODE_NAME_THIS_CARD },
            { "GetPile", StringTable.EDITOR_NODE_NAME_GET_PILE },
            { "GetSpellDamage", StringTable.EDITOR_NODE_NAME_GET_SPELL_DAMAGE },
            { "GetTarget", StringTable.EDITOR_NODE_NAME_GET_TARGET },
            { "IsPlayerPileChanged", StringTable.EDITOR_NODE_NAME_IS_PLAYER_PILE_CHANGED },
            { "GetOwner", StringTable.EDITOR_NODE_NAME_GET_OWNER },
            { "AddIntModifier", StringTable.EDITOR_NODE_NAME_ADD_INT_MODIFIER },
            { "RemoveModifier", StringTable.EDITOR_NODE_NAME_REMOVE_MODIFIER },
            { "GetProperty", StringTable.EDITOR_NODE_NAME_GET_PROPERTY },
            { "SetProperty", StringTable.EDITOR_NODE_NAME_SET_PROPERTY },
            { "GetUniquePropertyName", StringTable.EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME },
            { "Damage", StringTable.EDITOR_NODE_NAME_DAMAGE },
            { "SetIntPropModifierValue", StringTable.EDITOR_NODE_NAME_SET_INT_PROP_MODIFIER_VALUE },
            { "GetVariable", StringTable.EDITOR_NODE_NAME_GET_VARIABLE },
            { "SetVariable", StringTable.EDITOR_NODE_NAME_SET_VARIABLE },
            { "GetCount", StringTable.EDITOR_NODE_NAME_GET_COUNT },
            { "GeneratedEffectData", EDITOR_NODE_NAME_GENERATED_EFFECT_DATA },
            { "AuraData", EDITOR_NODE_NAME_AURA_DATA },
            // 内置参数
            { "value", StringTable.EDITOR_PARAM_NAME_VALUE },
            { "Value", StringTable.EDITOR_PARAM_NAME_VALUE },
            { "operator", EDITOR_PARAM_OPERATOR },
            { "result", EDITOR_PARAM_RESULT },
            { "PropertyName", EDITOR_PARAM_PROPERTY_NAME },
            { "RemoveModifierEvent", EDITOR_PARAM_PROPERTY_NAME },
            { "VariableName", EDITOR_PARAM_VARIABLE_NAME },
            { "isSet", EDITOR_PARAM_IS_SET },
            { "Collection", StringTable.EDITOR_PARAM_NAME_COLLECTION },
            { "PileName", EDITOR_PARAM_PILE_NAME },
            { "IsChanged", EDITOR_PARAM_IS_CHANGED },
            { "DamageEvent", EDITOR_PARAM_DAMAGE_EVENT },
            { "DamageSource", EDITOR_PARAM_DAMAGE_SOURCE },
            { "Amount", EDITOR_PARAM_DAMAGE_AMOUNT },
            { "Target", EDITOR_PARAM_TARGET },
            { "TargetName", EDITOR_PARAM_TARGET_NAME },
            { "EnableAction", StringTable.EDITOR_EFFECT_PROP_NAME_ON_ENABLE_ACTION },
            { "DisableAction", StringTable.EDITOR_EFFECT_PROP_NAME_ON_DISABLE_ACTION },
            { "Buff", StringTable.EDITOR_PARAM_NAME_BUFF },
            { "BuffDefine", StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE },
            { "PileList", StringTable.EDITOR_EFFECT_PROP_NAME_PILE_LIST },
            { "RangeList", StringTable.EDITOR_EFFECT_PROP_NAME_RANGE_LIST },
            { "AffectCondition", StringTable.EDITOR_EFFECT_PROP_NAME_AFFECT_CONDITION },
            { "TargetCard", StringTable.EDITOR_PARAM_NAME_TARGET_CARD },
            { "TargetBuff", EDITOR_PARAM_TARGET_BUFF },
            
            // Enum
            { "TouhouCardEngine.IntegerOperator.add", EDITOR_ENUM_INT_OPERATOR_ADD },
            { "TouhouCardEngine.IntegerOperator.sub", EDITOR_ENUM_INT_OPERATOR_SUB },
            { "TouhouCardEngine.IntegerOperator.mul", EDITOR_ENUM_INT_OPERATOR_MUL },
            { "TouhouCardEngine.IntegerOperator.div", EDITOR_ENUM_INT_OPERATOR_DIV },
            { "TouhouCardEngine.IntegerOperator.mod", EDITOR_ENUM_INT_OPERATOR_MOD },

            { "TouhouCardEngine.LogicOperator.not", EDITOR_ENUM_LOGIC_OPERATOR_NOT },
            { "TouhouCardEngine.LogicOperator.and", EDITOR_ENUM_LOGIC_OPERATOR_AND },
            { "TouhouCardEngine.LogicOperator.or", EDITOR_ENUM_LOGIC_OPERATOR_OR },

            { "TouhouCardEngine.CompareOperator.equals", EDITOR_ENUM_CMP_OPERATOR_EQUAL },
            { "TouhouCardEngine.CompareOperator.unequals", EDITOR_ENUM_CMP_OPERATOR_UNEQUAL },
            { "TouhouCardEngine.CompareOperator.greater", EDITOR_ENUM_CMP_OPERATOR_GREATER },
            { "TouhouCardEngine.CompareOperator.greaterEquals", EDITOR_ENUM_CMP_OPERATOR_GREATER_EQUAL },
            { "TouhouCardEngine.CompareOperator.less", EDITOR_ENUM_CMP_OPERATOR_LESS },
            { "TouhouCardEngine.CompareOperator.lessEquals", EDITOR_ENUM_CMP_OPERATOR_LESS_EQUAL },


            { "Game.CardDefineSelect.Spell", EDITOR_ENUM_CARD_TYPE_SPELL },
            { "Game.CardDefineSelect.Servant", EDITOR_ENUM_CARD_TYPE_SERVANT },
            { "Game.CardDefineSelect.Skill", EDITOR_ENUM_CARD_TYPE_SKILL },
            { "Game.CardDefineSelect.Master", EDITOR_ENUM_CARD_TYPE_MASTER },
            { "Game.CardDefineSelect.Item", EDITOR_ENUM_CARD_TYPE_ITEM },
        };
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(long cardPoolId, object editObject, DisplayInfo[] editObjectStack, ActionNodeGraphInfo graphInfo)
            {
                this.cardPoolId = cardPoolId;
                this.editObject = editObject;
                this.editObjectStack = editObjectStack;
                this.graphInfo = graphInfo;
            }
            public long cardPoolId;
            public object editObject;
            public DisplayInfo[] editObjectStack;
            public ActionNodeGraphInfo graphInfo;
        }
        #endregion
    }
}
