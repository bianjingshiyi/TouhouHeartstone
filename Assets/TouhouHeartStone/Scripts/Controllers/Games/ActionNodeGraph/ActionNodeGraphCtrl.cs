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
    partial class ActionNodeGraphCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 公有方法
        /// <summary>
        /// 退出节点图。
        /// </summary>
        public void quitGraph()
        {
            _recordList.clear();
        }
        public Task display(UIDisplayInfo uiDisplayInfo)
        {
            if (uiDisplayInfo is DisplayInfo info)
            {
                quitGraph();
                _curCardPoolId = info.cardPoolId;
                // 获取事件定义列表
                var poolRef = game.card.getLocalCardPoolReference(_curCardPoolId);
                var mainEventDefines = game.card.getAllEventDefines(includeHidden: false, poolRef);
                _eventTriggerTimes = mainEventDefines.getEventTriggerTimes();
                _eventReferences = mainEventDefines.Select(d => d.getReference()).ToArray();
                var eventEditorNames = mainEventDefines.Select(d => game.lang._(d.editorName)).ToArray();
                //更新UI
                _ui.gameObject.SetActive(true);
                _ui.initEventReferences(eventEditorNames, _eventReferences);
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
            return new DisplayInfo(_curCardPoolId, editObject);
        }
        public Task hide()
        {
            gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        public bool isActionDefineNameExist(string name)
        {
            foreach (var pair in game.card.builtinActionDefines)
            {
                var actionDefine = pair.Value;
                if (actionDefine.editorName == name)
                {
                    return true;
                }
            }
            foreach (var actionDefine in game.card.getLocalActionDefines(_curCardPoolId))
            {
                if (actionDefine.editorName == name)
                {
                    return true;
                }
            }
            return false;
        }
        public string tryTranslateWithDict(string input)
        {
            if (_nameTextDict.TryGetValue(input, out string text))
            {
                return game.i18n._(text);
            }
            return game.i18n._(input);
        }
        public Vector2 getViewportCenter()
        {
            return _ui.getViewCenterPosition();
        }
        /// <summary>
        /// 进行效果修改后将编辑器的未保存状态设置为true。
        /// </summary>
        public void callOnEdited()
        {
            OnGraphEdited?.Invoke();
        }

        public bool isFocused()
        {
            return _ui.isFocused();
        }
        public static SerializableActionNodeGraph copyNodes(IEnumerable<Node> nodes, out ActionGraph buffer)
        {
            buffer = new ActionGraph();
            foreach (var node in nodes)
            {
                buffer.addNode(node);
                var connections = buffer.getNodeConnections(node).Where(c => c != null && nodes.Contains(c.source?.node) && nodes.Contains(c.destination?.node));
                buffer.AddConnections(connections);
            }
            return copyNodeGraph(buffer);
        }
        public static SerializableActionNodeGraph copyNodeGraph(ActionGraph buffer)
        {
            return new SerializableActionNodeGraph(buffer);
        }
        public static ActionGraph pasteNodes(SerializableActionNodeGraph seri, ActionGraph graph, Vector2 pos, INodeDefiner definer, out Dictionary<int, int> idMap)
        {
            var clipGraph = seri.toActionGraph(definer);
            Vector2 centerPos = new Vector2(clipGraph.centerX, clipGraph.centerY);
            idMap = new Dictionary<int, int>();
            foreach (var node in clipGraph.nodes)
            {
                var oldId = node.id;
                node.id = graph.getUniqueNodeId();
                node.posX += pos.x - centerPos.x;
                node.posY += pos.y - centerPos.y;
                node.graph = graph;
                graph.addNode(node);
                idMap.Add(oldId, node.id);
            }
            graph.AddConnections(clipGraph.connections);
            return clipGraph;
        }
        /// <summary>
        /// 防止增益被跨卡池引用。
        /// </summary>
        /// <param name="node"></param>
        public void fixTransCardPoolBuffRef(Node node)
        {
            // 防止增益被跨卡池引用
            // 虽然这以后肯定是要实现的，但是不是现在要做的事情
            if (node is ActionNode action && action.defineRef.isBuiltinReference(BuiltinActionDefineID.GetBuffDefine))
            {
                var pairs = node.consts.Where(p => p.Value is DefineReference defRef && defRef.cardPoolId != _curCardPoolId).ToArray();
                foreach (var pair in pairs)
                {
                    node.setConst(pair.Key, new DefineReference(_curCardPoolId, 0));
                }
            }
        }


        #region 节点选择
        public string copySelection()
        {
            var selectedNodes = _ui.getSelectedNodes();
            if (selectedNodes.Count() <= 0)
                return null;
            var seri = copyNodes(selectedNodes.Select(i => getActionNode(i)).Where(n => CanNodeBeCloned(n)), out _);
            return seri.toJson();
        }
        public Task pasteClipboard(string clipboard)
        {
            var pos = _ui.getViewCenterPosition();
            return pasteClipboard(clipboard, pos);
        }
        public async Task pasteClipboard(string clipboard, Vector2 pos)
        {
            if (string.IsNullOrEmpty(clipboard))
                return;
            var seri = JsonHelper.fromJson<SerializableActionNodeGraph>(clipboard);
            var definer = game.card.getLocalNodeDefiner();
            var clipGraph = pasteNodes(seri, graph, pos, definer, out _);

            foreach (var node in clipGraph.nodes)
            {
                fixTransCardPoolBuffRef(node);
                await createNodeUI4Node(node);
                addOperationRecord(new CreateNodeOperation(this, node));
            }
            foreach (var connection in clipGraph.connections)
            {
                connectPorts4UI(connection.source, connection.destination);
                addOperationRecord(new ConnectOperation(this, connection));
            }
            _ui.clearSelection();
        }
        public void deleteSelection()
        {
            var selectedNodes = _ui.getSelectedNodes().Select(n => getActionNode(n)).Where(n => CanNodeBeDeleted(n));
            if (selectedNodes.Count() <= 0)
                return;
            foreach (var node in selectedNodes.ToArray())
            {
                removeActionNode(node);
            }
            _curEditNode = null;
        }
        public Node[] getSelectedNodes()
        {
            return _ui.getSelectedNodes().Select(n => getActionNode(n)).ToArray();
        }

        public static bool CanNodeBeCloned(Node node)
        {
            if (node == null)
                return false;
            if (!(node is ActionNode))
                return false;
            return true;
        }
        public static bool CanNodeBeDeleted(Node node)
        {
            if (node is EventEntryNode) // 事件入口节点可以删除
                return true;
            if (!(node is ActionNode action))
                return false;
            return true;
        }
        public bool CanNodeBeEdited(Node node)
        {
            return node is ActionNode action && getActionDefine(action.defineRef) is GeneratedActionDefine;
        }
        public static bool CanActionDefineBeCreated(ActionDefine define)
        {
            if (define == null)
                return false;
            return true;
        }
        public static bool CanNodeOpenDocument(Node node)
        {
            if (!(node is ActionNode actionNode)) // 不是动作节点
                return false;
            if (actionNode.defineRef.cardPoolId != 0) // 不是内置节点
                return false;
            return true;
        }
        public static bool CanNodeDefineOpenDocument(NodeDefine node)
        {
            if (node.actionDefine == null) // 不是动作节点
                return false;
            if (node.actionDefine.cardPoolId != 0) // 不是内置节点
                return false;
            return true;
        }
        #endregion

        #region 节点创建
        public async Task<ActionNodeItem> createActionNode(ActionDefine actionDefine, float x, float y)
        {
            var actionNode = graph.createActionNode(actionDefine, x, y);
            _actionNodeList.Add(actionNode);
            addOperationRecord(new CreateNodeOperation(this, actionNode));
            ActionNodeItem actionNodeItem = await createNodeUI4ActionNode(actionNode);
            return actionNodeItem;
        }
        public Task<ActionNodeItem> createActionNode(ActionDefine actionDefine, Vector2 pos)
        {
            return createActionNode(actionDefine, pos.x, pos.y);
        }
        public ActionNodeItem createEventEntryNode(EventTriggerTime eventTriggerTime, float x, float y, bool showErrorTip = true)
        {
            // 尝试为非事件效果创建事件入口节点。
            if (!(_effect is GeneratedEventEffect eventEffect))
            {
                if (showErrorTip)
                    showCreateEventEntryForNonEventEffectError();
                return null;
            }
            // 已经有事件入口节点。
            if (eventEffect.entryNode != null)
            {
                if (showErrorTip)
                    showEventEntryAlreadyExistsError();
                return null;
            }
            var actionNode = graph.createEventEntryNode(eventTriggerTime, x, y);
            addOperationRecord(new CreateNodeOperation(this, actionNode));
            ActionNodeItem actionNodeItem = createNodeUI4PassiveEntryNode(actionNode);
            updateEffectProperties();
            return actionNodeItem;
        }
        public ActionNodeItem createEventEntryNode(EventTriggerTime eventTriggerTime, Vector2 pos)
        {
            return createEventEntryNode(eventTriggerTime, pos.x, pos.y);
        }
        public async Task<Node> createNode(NodeDefine define, Vector2 pos)
        {
            ActionNodeItem item = null;
            if (define.actionDefine != null)
            {
                item = await createActionNode(define.actionDefine, pos);
            }
            else if (define.eventTriggerTime != null)
            {
                item = createEventEntryNode(define.eventTriggerTime, pos);
            }
            if (item == null)
                return null;
            return getActionNode(item);
        }
        public void moveNodeToFreePosition(Node node)
        {
            var item = getActionNodeItem(node);
            _ui.updateGraphFreePlaceCheckRegion(item.getNodeItemRect().size * 2, item);
            var pos = _ui.findFreeNodeCreatePosition(item);
            moveNode(node.id, pos);
        }
        public void moveNode(int nodeID, Vector2 pos)
        {
            var node = graph.nodes.FirstOrDefault(n => n.id == nodeID);
            if (node == null)
                return;
            var item = getActionNodeItem(node);
            node.posX = pos.x;
            node.posY = pos.y;
            _ui.moveActionNodeTo(item, pos);
            callOnEdited();
        }
        #endregion

        public void setSelectedCardFromPanel(CardDefine cardDefine)
        {
            if (!string.IsNullOrEmpty(_curCardSeekingEffectPropName))
            {
                selectCardForEffectProperty(cardDefine);
            }
            else
            {
                selectCardForNodeConst(cardDefine);
            }
            //重置字段
            resetCardSelectInfo();
        }
        public void resetCardSelectInfo()
        {
            //重置字段
            _curEditNode = null;
            _curEditActionNodeConstName = string.Empty;
            _curEditActionNodeConstItemIndex = -1;
            _curCardSeekingEffectPropName = string.Empty;
            _curCardSeekingEffectPropIndex = -1;
        }

        /// <summary>
        /// 设置一个节点的一个常量的值，并将这次修改加入到撤销列表记录中。
        /// </summary>
        /// <param name="node">节点。</param>
        /// <param name="name">常量名。</param>
        /// <param name="value">常量值。</param>
        public void setNodeConst(Node node, string name, object value)
        {
            var beforeValue = node.getConst(name);
            if (value is ICloneable clonable)
            {
                value = clonable.Clone();
            }
            if (beforeValue is ICloneable beforeClonable)
            {
                beforeValue = beforeClonable.Clone();
            }
            node.setConst(name, value);
            addOperationRecord(new ChangeNodeConstOperation(this, node, name, beforeValue, value));
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

            _ui.onConnectNodes += onConnectNodes;
            _ui.onUnconnectNodes += onUnconnectNodes;
            _ui.onClickInputPort += onClickInputPort;
            _ui.onClickOutputPort += onClickOutputPort;
            _ui.onMoveActionNode += onMoveActionNode;
            _ui.onEndDragActionNode += onEndMoveActionNode;
            _ui.onStartConnection += onStartConnection;
            _ui.onEndConnection += onEndConnection;
            _ui.onActionNodeValueChange += onActionNodeValueChange;
            _ui.onClickActionNodeConstValueButton += onClickActionNodeConstValueButton;
            _ui.onClickActionNodeConstValueCardButton += onClickActionNodeConstValueCardButton;
            _ui.onClickActionNodeConstValueCardEditButton += onClickActionNodeConstValueCardEditButton;
            _ui.onClickActionNodeConstValueItemButton += onClickActionNodeConstValueItemButton;
            _ui.onClickActionNodeConstValueItemCardButton += onClickActionNodeConstValueItemCardButton;
            _ui.onClickActionNodeConstValueItemCardEditButton += onClickActionNodeConstValueItemCardEditButton;
            _ui.onNodeSelected += onNodeSelected;
            _ui.onNodeDeselected += onNodeDeselected;
            _ui.onNodeVisualEffectButtonClicked += onNodeVisualEffectButtonClickedCallback;
            _ui.onClickActionNodeObsoleteButton += onClickActionNodeObsoleteButtonCallback;
            //触发
            _ui.onClickAddTargetCheckerButton += onClickAddTargetCheckerButton;
            _ui.onTargetCheckerTypeValueChanged += onTargetCheckerTypeValueChanged;
            _ui.onClickRemoveTargetCheckerButton += onClickRemoveTargetCheckerButton;
            _ui.onTargetCheckerErrorTipValueChanged += onTargetCheckerErrorTipValueChanged;
            //增益
            _ui.onItemBuffChanged += onItemBuffChangedCallback;
            _ui.onClickItemEditBuffButton += onItemEditBuffCallback;

            // 效果属性
            _ui.onClickAddEffectPropertyButton += onClickAddEffectPropertyButtonCallback;
            _ui.onClickEffectPropertyItemButton += onClickEffectPropertyItemButtonCallback;
            _ui.onClickEffectPropertyItemCardButton += onClickEffectPropertyItemCardButtonCallback;
            _ui.onClickEffectPropertyItemCardEditButton += onClickEffectPropertyItemCardEditButtonCallback;
            _ui.onEffectPropertyItemValueChanged += onEffectPropertyItemValueChangedCallback;
            _ui.onClickEffectPropertyButton += onClickEffectPropertyButtonCallback;
            _ui.onClickEffectPropertyCardButton += onClickEffectPropertyCardButtonCallback;
            _ui.onClickEffectPropertyCardEditButton += onClickEffectPropertyCardEditButtonCallback;
            _ui.onEffectPropertyValueChanged += onEffectPropertyValueChangedCallback;
            _ui.onDeleteEffectProperty += onDeleteEffectPropertyCallback;
            _ui.onClickEffectPropertyAddItemButton += onClickEffectPropertyAddItemButtonCallback;
            _ui.onClickEffectPropertyRemoveItemButton += onClickEffectPropertyRemoveItemButtonCallback;

            _ui.initPiles(getAllPiles());
            _ui.initCardProperties(CardEditorCtrl.cardProps.ToArray());
            var keywords = CardEditorCtrl.getKeywords();
            _ui.initKeywords(keywords.Select(k => k.name).ToArray(), keywords.Select(k => k.value).ToArray());
            var effectTagNames = _activeEffectTagNames.Union(_passiveEffectTagNames).ToArray();
            var effectTagValues = getAllEffectTagValues();
            _ui.initEffectTags(effectTagNames, effectTagValues);
        }
        #endregion
        #region 事件回调
        /// <summary>
        /// 当用户连接节点，检查一下连接的合法性，如果合法那么连接，否则提示。
        /// </summary>
        /// <param name="outputActionNodeItem"></param>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNodeItem"></param>
        /// <param name="inputIndex"></param>
        private void onConnectNodes(ActionNodeItem outputActionNodeItem, int outputIndex, ActionNodeItem inputActionNodeItem, int inputIndex)
        {
            if (isReadonly)
                return;
            Node outputActionNode = getActionNode(outputActionNodeItem);
            Node inputActionNode = getActionNode(inputActionNodeItem);
            IPort outputPort = outputActionNode.getOutputPortAt(outputIndex);
            IPort inputPort = inputActionNode.getInputPortAt(inputIndex);

            if (validatePortConnection(outputPort, inputPort, true))
            {
                connectPorts(outputPort, inputPort);
            }
            addRecordBreakpoint();
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
            if (isReadonly)
                return;
            Node outputActionNode = getActionNode(outputActionNodeItem);
            Node inputActionNode = getActionNode(inputActionNodeItem);
            IPort outputPort = outputActionNode.getOutputPortAt(outputIndex);
            IPort inputPort = inputActionNode.getInputPortAt(inputIndex);
            disconnectPorts(outputPort, inputPort);
            addRecordBreakpoint();
        }
        private void onClickInputPort(ActionNodeItem item, int inputIndex)
        {
            if (isReadonly)
                return;
            var node = getActionNode(item);
            var inputPort = node.getInputPortAt(inputIndex);
            if (inputPort == null)
                return;
            if (_connectingPort == inputPort)
            {
                // 点击的输入点是本身。
                // 取消选择。
                _ui.selectPort(null);
                resetConnectingPort();
                return;
            }
            else if (_connectingPort != null && _connectingPort.isOutput)
            {
                // 选择的输出点存在。
                var outputPort = _connectingPort;
                bool connected = node.graph.isConnected(inputPort, _connectingPort);

                if (connected)
                {
                    // 已经连接，断开连接。
                    disconnectPorts(outputPort, inputPort);
                }
                else
                {
                    // 连接输入点与输出点。
                    if (validatePortConnection(outputPort, inputPort, true))
                    {
                        connectPorts(outputPort, inputPort);
                    }
                }
                _ui.selectPort(null);
                resetConnectingPort();
                addRecordBreakpoint();
                return;
            }
            else
            {
                // 点击的输入点不是本身，并且当前选择的输出点不存在。
                // 选择新的输入点。
                _ui.selectPort(item.getInput(inputIndex));
                _connectingPort = inputPort;
                updatePortModes(inputPort);
            }
        }
        private void onClickOutputPort(ActionNodeItem item, int outputIndex)
        {
            if (isReadonly)
                return;
            var node = getActionNode(item);
            var outputPort = node.getOutputPortAt(outputIndex);
            if (outputPort == null)
                return;
            if (_connectingPort == outputPort)
            {
                // 点击的输入点是本身。
                // 取消选择。
                _ui.selectPort(null);
                resetConnectingPort();
                return;
            }
            else if (_connectingPort != null && !_connectingPort.isOutput)
            {
                // 选择的输出点存在。
                var inputPort = _connectingPort;
                bool connected = node.graph.isConnected(outputPort, _connectingPort);

                if (connected)
                {
                    // 已经连接，断开连接。
                    disconnectPorts(outputPort, inputPort);
                }
                else
                {
                    // 连接输入点与输出点。
                    if (validatePortConnection(outputPort, inputPort, true))
                    {
                        connectPorts(outputPort, inputPort);
                    }
                }
                _ui.selectPort(null);
                resetConnectingPort();
                addRecordBreakpoint();
                return;
            }
            else
            {
                // 点击的输入点不是本身，并且当前选择的输出点不存在。
                // 选择新的输入点。
                _ui.selectPort(item.getOutput(outputIndex));
                _connectingPort = outputPort;
                updatePortModes(outputPort);
            }
        }
        private void onMoveActionNode(ActionNodeItem actionNodeItem, Vector2 graphPosition)
        {
            if (isReadonly)
                return;
            // 同时拖动所有被选中的节点
            Vector2 rootPosition = _ui.getNodesRootPosition();
            float movedX = graphPosition.x - rootPosition.x - actionNodeItem.posX;
            float movedY = graphPosition.y - rootPosition.y - actionNodeItem.posY;
            var selectedNodes = _ui.getSelectedNodes();
            foreach (ActionNodeItem item in selectedNodes)
            {
                Node node = getActionNode(item);
                if (!_movingNodeDict.ContainsKey(item))
                {
                    _movingNodeDict.Add(item, new Vector2(node.posX, node.posY));
                }
                node.posX += movedX;
                node.posY += movedY;
                _ui.moveActionNodeTo(item, new Vector2(item.posX + movedX, item.posY + movedY));
            }
            if (selectedNodes.Count() == 1)
            {
                moveExistingNodeItem(selectedNodes.FirstOrDefault());
            }
            callOnEdited();
        }
        private void onEndMoveActionNode(ActionNodeItem actionNodeItem)
        {
            if (isReadonly)
                return;
            foreach (var pair in _movingNodeDict)
            {
                var node = getActionNode(pair.Key);
                addOperationRecord(new MoveNodeOperation(this, node.id, pair.Value, new Vector2(node.posX, node.posY)));
            }
            if (_movingNodeDict.Count == 1 && isNodeSnapped())
            {
                var nodeItem = _movingNodeDict.FirstOrDefault().Key;
                var node = getActionNode(nodeItem);
                var pos = new Vector2(node.posX, node.posY);
                pos = getNodeSnapPreviewPosition(pos);
                moveNode(node.id, pos);
                connectSnapConnections(node);
                moveNodeToFreePosition(node);
            }
            hidePreviewNode();
            _movingNodeDict.Clear();
            addRecordBreakpoint();
        }
        private void onStartConnection(ActionNodeItem actionNodeItem, int index, bool isOutput)
        {
            if (isReadonly)
                return;
            var node = getActionNode(actionNodeItem);
            var port = isOutput ? node.getOutputPortAt(index) : node.getInputPortAt(index);
            _connectingPort = port;
            updatePortModes(port);
        }
        private void onEndConnection(ActionNodeItem actionNodeItem, int index, bool isOutput)
        {
            if (isReadonly)
                return;
            resetConnectingPort();
        }
        /// <summary>
        /// 当用户更改节点的常量值，设置对应的常量。
        /// </summary>
        /// <param name="actionNodeItem"></param>
        /// <param name="constIndex"></param>
        /// <param name="value"></param>
        private void onActionNodeValueChange(ActionNodeItem actionNodeItem, int constIndex, object value)
        {
            if (isReadonly)
                return;
            var node = getActionNode(actionNodeItem);
            if (node is ActionNode action)
            {
                var actionDefine = getActionDefine(action.defineRef);
                var name = actionDefine.constDefines.ElementAt(constIndex).name;
                setNodeConst(node, name, value);
                updateNodeItemConsts(node);
                addRecordBreakpoint();
            }
            else
            {
                string name = getNodeConstNameByIndex(node, constIndex);
                if (!string.IsNullOrEmpty(name))
                {
                    setNodeConst(node, name, value);
                    updateNodeItemConsts(node);
                    addRecordBreakpoint();
                }
                else
                {
                    Debug.LogError($"节点{node}不包含常量{name}。");
                }
            }
        }
        private void onClickActionNodeConstValueButton(ActionNodeItem actionNodeItem, int index)
        {
        }
        private async void onClickActionNodeConstValueCardButton(ActionNodeItem actionNodeItem, int index)
        {
            if (isReadonly)
                return;
            Node actionNode = getActionNode(actionNodeItem);
            if (!(actionNode is ActionNode action))
                return;
            ActionDefine actionDefine = getActionDefine(action.defineRef);
            var constDefine = actionDefine.constDefines.ElementAtOrDefault(index);
            if (constDefine == null)
                return;
            if (constDefine.type == typeof(DefineReference))
            {
                long cardPoolId = _curCardPoolId;
                if (action.getConst(constDefine.name) is DefineReference value)
                {
                    //打开卡牌选择界面
                    cardPoolId = value.cardPoolId;
                }
                await displayCardSelectPanel(cardPoolId);
                _curEditNode = action;
                _curEditActionNodeConstName = constDefine.name;
                _curEditActionNodeConstItemIndex = -1;
            }
        }
        private void onClickActionNodeConstValueCardEditButton(ActionNodeItem actionNodeItem, int index)
        {
            Node node = getActionNode(actionNodeItem);
            if (!(node is ActionNode action))
                return;
            ActionDefine actionDefine = getActionDefine(action.defineRef);
            var constDefine = actionDefine.constDefines.ElementAtOrDefault(index);
            if (constDefine == null)
                return;
            onOpenActionNodeConstCardDefine?.Invoke(action, constDefine.name);
        }
        private void onClickActionNodeConstValueItemButton(ActionNodeItem actionNodeItem, int index, int itemIndex)
        {
        }
        private async void onClickActionNodeConstValueItemCardButton(ActionNodeItem actionNodeItem, int index, int itemIndex)
        {
            if (isReadonly)
                return;
            Node actionNode = getActionNode(actionNodeItem);
            if (!(actionNode is ActionNode action))
                return;
            ActionDefine actionDefine = getActionDefine(action.defineRef);
            var constDefine = actionDefine.constDefines.ElementAtOrDefault(index);
            if (constDefine == null)
                return;
            if (constDefine.type == typeof(CardReferenceCollection) || constDefine.type == typeof(DefineReference[]))
            {
                long cardPoolId = _curCardPoolId;
                if (actionNode.getConst(constDefine.name) is CardReferenceCollection defRefCol)
                {
                    if (defRefCol[itemIndex] == null)
                    {
                        defRefCol[itemIndex] = new DefineReference();
                    }
                    cardPoolId = defRefCol[itemIndex].cardPoolId;
                }
                else if (actionNode.getConst(constDefine.name) is DefineReference[] defRefs)
                {
                    if (defRefs[itemIndex] == null)
                    {
                        defRefs[itemIndex] = new DefineReference();
                    }
                    cardPoolId = defRefs[itemIndex].cardPoolId;
                }
                //打开卡牌选择界面
                await displayCardSelectPanel(cardPoolId);
                _curEditNode = action;
                _curEditActionNodeConstName = constDefine.name;
                _curEditActionNodeConstItemIndex = itemIndex;
            }
        }
        private void onClickActionNodeConstValueItemCardEditButton(ActionNodeItem actionNodeItem, int index, int itemIndex)
        {
            Node node = getActionNode(actionNodeItem);
            if (!(node is ActionNode action))
                return;
            ActionDefine actionDefine = getActionDefine(action.defineRef);
            var constDefine = actionDefine.constDefines.ElementAtOrDefault(index);
            if (constDefine == null)
                return;
            onOpenActionNodeConstItemCardDefine?.Invoke(action, constDefine.name, itemIndex);
        }
        private void onNodeSelected(ActionNodeItem actionNodeItem)
        {
            OnNodeSelected?.Invoke(getActionNode(actionNodeItem));
        }
        private void onNodeDeselected(ActionNodeItem actionNodeItem)
        {
            OnNodeDeselected?.Invoke(getActionNode(actionNodeItem));
        }
        private void onNodeVisualEffectButtonClickedCallback(ActionNodeItem actionNodeItem)
        {
            OnNodeVisualEffectButtonClicked?.Invoke(getActionNode(actionNodeItem));
        }
        private void onClickActionNodeObsoleteButtonCallback(string msg)
        {
            game.ui.dialog?.ShowConfirm(game.lang._(TITLE_NODE_OBSOLETE), game.lang._(MESSAGE_NODE_OBSOLETE, msg));
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

            _ui.clearGraph();
            await ApplyGraph(effect.graph);

            updateEffectProperties();
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
                // 先更新自定义动作节点和事件入口节点，因为自定义节点和自定义事件的输出输入值可能会变动。
                if (node is ActionNode action)
                {
                    var define = getActionDefine(action.defineRef);
                    if (define is GeneratedActionDefine gen)
                    {
                        action.define = gen;
                        action.Define();
                    }
                }
                else if (node is EventEntryNode eventEntryNode)
                {
                    var poolRef = game.card.getLocalCardPoolReference(eventEntryNode.eventCardPoolId);
                    var triggerTime = game.card.getEventTriggerTime(poolRef, eventEntryNode.eventName, true);
                    eventEntryNode.define = triggerTime;
                    eventEntryNode.Define();
                }
                await createNodeUI4Node(node);
            }

            disconnectAllInvalidConnections(graph);
            foreach (var connection in graph.connections)
            {
                if (connection == null) continue;
                connectPorts4UI(connection.source, connection.destination);
            }
            updatePortModes(null);

            updateReadonly();
            // 更新事件变量节点的事件变量名。
            foreach (var node in graph.nodes)
            {
                var actionNodeItem = getActionNodeItem(node);
                checkEventVariableNodeItemVarNames(actionNodeItem, node);
            }
        }

        private async Task<ActionNodeItem> createNodeUI4Node(Node node)
        {
            if (node is ActionNode action)
            {
                return await createNodeUI4ActionNode(action);
            }
            else if (node is ActiveEntryNode activeEntry)
            {
                string nodeName = game.lang._(EDITOR_NODE_NAME_ACTIVE_ENTRY_NODE);
                var targetTypes = getTranslatedTargetCheckerTypes();
                var checkers = getActiveEntryTargetCheckerViewData(activeEntry);
                var inputViewData = getActiveEntryInputViewData(activeEntry);
                var outputViewData = getNodeOutputViewData(activeEntry);

                var actionNodeItem = _ui.createActiveEntryNode(activeEntry.posX, activeEntry.posY, nodeName,
                    inputViewData,
                    targetTypes, checkers,
                    outputViewData);
                actionNodeItem.initEffectTags(_activeEffectTagNames, _activeEffectTags);

                _actionNodeItemList.Add(actionNodeItem);
                _actionNodeList.Add(node);
                updateNodeItemConsts(node);
                updateNodePortModes(node, _connectingPort);
                return actionNodeItem;
            }
            else if (node is PassiveEntryNode passiveEntry)
            {
                return createNodeUI4PassiveEntryNode(passiveEntry);
            }
            else if (node is GeneratedActionEntryNode genEntry)
            {
                string nodeName = game.lang._(StringTable.EDITOR_NODE_NAME_INPUT);
                var inputViewData = new PortItemInfo[0];
                ActionNodeValueInfo[] constsInfo = new ActionNodeValueInfo[0];
                var outputViewData = getActionDefineEntryNodeOutputViewData(genEntry);

                var actionNodeItem = _ui.createNode(genEntry.posX, genEntry.posY, nodeName, inputViewData, constsInfo, outputViewData);
                _actionNodeItemList.Add(actionNodeItem);
                _actionNodeList.Add(node);
                updateNodePortModes(node, _connectingPort);
                return actionNodeItem;
            }
            else if (node is GeneratedActionReturnNode genReturn)
            {
                string nodeName = game.lang._(StringTable.EDITOR_NODE_NAME_OUTPUT);
                var inputViewData = getActionDefineReturnNodeInputViewData(genReturn);
                ActionNodeValueInfo[] constsInfo = new ActionNodeValueInfo[0];
                var outputViewData = new PortItemInfo[0];

                var actionNodeItem = _ui.createNode(genReturn.posX, genReturn.posY, nodeName, inputViewData, constsInfo, outputViewData);
                _actionNodeItemList.Add(actionNodeItem);
                _actionNodeList.Add(node);
                updateNodePortModes(node, _connectingPort);
                return actionNodeItem;
            }
            else if (node is AuraEntryNode auraEntry)
            {
                return createNodeUI4AuraEntryNode(auraEntry);
            }
            return null;
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
            ActionDefine actionDefine = getActionDefine(actionNode.defineRef);
            ActionNodeItem actionNodeItem;
            if (actionDefine is GetBuffDefineAction getBuffDefineAction)
            {
                //如果是获取增益定义节点，那么它和普通的节点不一样，会显示增益的编辑界面。
                //先获取节点的增益
                GeneratedBuffDefine generatedBuffDefine = null;
                BuffSkin buffSkin = null;
                string constName = GetBuffDefineAction.constName;
                if (actionNode.consts == null || actionNode.consts.Count < 1 || !tryGetBuffDefine(actionNode.getConst(constName), out GeneratedBuffDefine buffDefine))
                {
                    // 节点没有增益引用，或者找不到对应的增益
                    var localRef = game.card.getLocalCardPoolReference(_curCardPoolId);
                    var poolBuffs = game.card.getBuffDefines(localRef);
                    if (poolBuffs.Count <= 0) //卡池内没有任何增益
                    {
                        // 新建一个。
                        generatedBuffDefine = game.card.createBuffDefine(_curCardPoolId);
                        game.card.addBuffDefine(_curCardPoolId, generatedBuffDefine);
                    }
                    else
                    {
                        // 设置为卡池内的第一个增益。
                        generatedBuffDefine = poolBuffs.Values.OfType<GeneratedBuffDefine>().FirstOrDefault(b => b != null);
                    }
                    actionNode.setConst(constName, new DefineReference(generatedBuffDefine.cardPoolId, generatedBuffDefine.id));
                }
                else
                {
                    //获取到了增益id，并且确实存在对应的增益
                    generatedBuffDefine = buffDefine;
                }
                //获取增益对应的皮肤
                buffSkin = game.card.getBuffSkin(generatedBuffDefine);
                if (buffSkin == null)
                {
                    //如果不存在皮肤，那就创建一个
                    buffSkin = game.card.createBuffSkin(
                        generatedBuffDefine.cardPoolId,
                        generatedBuffDefine.id,
                        game.langManager.getLanguage());
                    game.card.addBuffSkin(_curCardPoolId, buffSkin);
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
                }, getNodeOutputNames(actionNode));
            }
            else if (actionDefine != null)
            {
                ActionNodeValueInfo[] constsInfo = getNodeConstValueInfos(actionNode);
                int nodeKey = CanNodeOpenDocument(actionNode) ? actionNode.defineRef.defineId : -1;
                bool visualEffect = game.anims.canActionAddEffect(actionNode.defineRef);
                actionNodeItem = _ui.createNode(
                    actionNode.posX,
                    actionNode.posY,
                    getActionNodeName(actionNode),
                    getNodeInputViewData(actionNode),
                    constsInfo,
                    getNodeOutputViewData(actionNode),
                    getActionNodeObsoleteMsg(actionNode),
                    nodeKey,
                    visualEffect);
                for (int i = 0; i < constsInfo.Length; i++)
                {
                    var constInfo = constsInfo[i];
                    var name = constInfo.name;
                    if (actionNode.getConst(name) == null)
                    {
                        await initNodeConstDefaultValue(actionNodeItem, actionNode, i, constInfo);
                    }
                    if (constInfo.type == ActionNodeValueType.cardReference || constInfo.type == ActionNodeValueType.cardReferenceCollection)
                    {
                        await updateCardDefineConst(actionNodeItem, actionNode, i, actionNode.getConst(name));
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
                updateNodePortModes(actionNode, _connectingPort);
            }
            _actionNodeItemList.Add(actionNodeItem);
            if (!_actionNodeList.Contains(actionNode))
            {
                _actionNodeList.Add(actionNode);
            }
            updateNodePortModes(actionNode, _connectingPort);
            return actionNodeItem;
        }
        private ActionNodeItem createNodeUI4PassiveEntryNode(PassiveEntryNode passiveEntry)
        {
            string nodeName = null;
            var inputViewData = getNodeInputViewData(passiveEntry);
            var outputViewData = getNodeOutputViewData(passiveEntry);
            if (passiveEntry is EventEntryNode eventEntry)
            {
                var typeIndex = _eventTriggerTimes.getEventTriggerTimeIndex(eventEntry.eventName);
                var editorNames = _eventTriggerTimes.getEventTriggerTimeNamesForEditor(game);
                if (typeIndex >= 0)
                {
                    nodeName = editorNames[typeIndex];
                }
            }
            else
            {
                nodeName = EDITOR_NODE_NAME_PASSIVE_ENTRY_NODE;
            }
            nodeName = game.lang._(nodeName != null ? nodeName : EDITOR_NODE_NAME_UNKNOWN_EVENT);

            var actionNodeItem = _ui.createNode(passiveEntry.posX, passiveEntry.posY,
                nodeName,
                inputViewData,
                Array.Empty<ActionNodeValueInfo>(), // 清空常量，不然在下面初始化牌堆的时候，因为已经创建了，所以对模板的常量项的修改不会应用到新的上面。
                outputViewData,
                visualEffect: true);
            actionNodeItem.initEffectTags(_passiveEffectTagNames, _passiveEffectTags);
            actionNodeItem.switchToEventEntryNode();

            _actionNodeItemList.Add(actionNodeItem);
            _actionNodeList.Add(passiveEntry);
            updateNodeItemConsts(passiveEntry);
            updateNodePortModes(passiveEntry, _connectingPort);
            return actionNodeItem;
        }
        private ActionNodeItem createNodeUI4AuraEntryNode(AuraEntryNode auraEntry)
        {
            string nodeName = EDITOR_NODE_NAME_AURA_ENTRY_NODE;
            nodeName = game.lang._(nodeName);
            var inputViewData = getNodeInputViewData(auraEntry);
            var outputViewData = getNodeOutputViewData(auraEntry);

            var actionNodeItem = _ui.createNode(auraEntry.posX, auraEntry.posY,
                nodeName,
                inputViewData,
                Array.Empty<ActionNodeValueInfo>(), // 清空常量，不然在下面初始化牌堆的时候，因为已经创建了，所以对模板的常量项的修改不会应用到新的上面。
                outputViewData, 
                visualEffect: false);
            actionNodeItem.switchToEventEntryNode();

            _actionNodeItemList.Add(actionNodeItem);
            _actionNodeList.Add(auraEntry);
            updateNodeItemConsts(auraEntry);
            updateNodePortModes(auraEntry, _connectingPort);
            return actionNodeItem;
        }
        private void removeActionNode(Node node)
        {
            int actionNodeIndex = _actionNodeList.IndexOf(node);
            ActionNodeItem actionNodeItem = getActionNodeItem(node);
            disconnectNodeAll(node);
            _ui.deselectNode(actionNodeItem);
            //从列表中移除
            _actionNodeList.RemoveAt(actionNodeIndex);
            _actionNodeItemList.RemoveAt(actionNodeIndex);
            graph.removeNode(node);
            addOperationRecord(new DeleteNodeOperation(this, node));
            //将节点从UI上移除
            _ui.removeNode(actionNodeItem);
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
                    _ui.unconnectNode(connectedNodeItem, connectedNodeIndex, inputNodeItem, inputIndex);
                }
            }

            //建立新的连接
            graph.connect(outputPort, inputPort);
            addOperationRecord(new ConnectOperation(this, outputPort.node, outputPort.name, inputPort.node, inputPort.name, inputPort.paramIndex));
            //更新ui
            _ui.connectNode(outputNodeItem, outputIndex, inputNodeItem, inputIndex, ActionNodeConnectionType.value);
            updatePortMode(_connectingPort, outputPort);
            updatePortMode(_connectingPort, inputPort);
            //更新变长参数。
            updateInputParamsPort(inputPort);
            // 更新事件变量节点的事件变量名。
            checkEventVariableNodeItemVarNames(inputNodeItem, inputPort.node);
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
            addOperationRecord(new ConnectOperation(this, outputPort.node, outputPort.name, inputPort.node, inputPort.name, -1));
            //更新ui
            _ui.connectNode(outputNodeItem, outputIndex, inputNodeItem, inputIndex, ActionNodeConnectionType.action);
            updatePortMode(_connectingPort, outputPort);
            updatePortMode(_connectingPort, inputPort);
        }

        private void connectPorts4UI(IPort outputPort, IPort inputPort)
        {
            if (IsInvalidConnection(outputPort, inputPort))
            {
                graph.disconnect(outputPort, inputPort);
                return;
            }
            var outputNode = outputPort?.node;
            var inputNode = inputPort?.node;
            if (outputNode == null || inputNode == null)
                return;

            var outputNodeItem = getActionNodeItem(outputNode);
            var inputNodeItem = getActionNodeItem(inputNode);
            if (outputNodeItem == null || inputNodeItem == null)
                return;

            ActionNodeConnectionType type = ActionNodeConnectionType.value;
            if (outputPort.GetPortType() == PortType.Control)
            {
                type = ActionNodeConnectionType.action;
            }

            _ui.connectNode(outputNodeItem, outputPort.GetPortIndex(), inputNodeItem, inputPort.GetPortIndex(), type);
            updatePortMode(_connectingPort, outputPort);
            updatePortMode(_connectingPort, inputPort);
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
        /// <summary>
        /// <para>当端口被增加或删除时，移动所有端口的连接。</para>
        /// <para><b>请在增加端口之后，或删除端口之前调用该方法。</b></para>
        /// </summary>
        /// <param name="port">发生变化的端口。</param>
        /// <param name="direction">向下移动为正，向上移动为负。</param>
        private void movePortConnections(IPort port, int direction)
        {
            if (direction == 0)
                return;

            var node = port.node;
            int portIndex = port.GetPortIndex();
            List<NodeConnection> movedConnections = new List<NodeConnection>();
            var ports = (port.isOutput ? node.outputPorts : node.inputPorts).ToArray();
            for (int i = portIndex + 1; i < ports.Length; i++)
            {
                var movedPort = ports[i];
                if (movedPort == null)
                    return;
                movedConnections.AddRange(movedPort.connections);
            }

            foreach (var connection in movedConnections)
            {
                if (connection == null)
                    continue;
                var outputPort = connection.source;
                var inputPort = connection.destination;
                var inputNodeItem = getActionNodeItem(inputPort.node);
                var outputNodeItem = getActionNodeItem(outputPort.node);
                if (outputPort == null || inputPort == null || inputNodeItem == null || outputNodeItem == null)
                    continue;

                int outputIndex = outputPort.GetPortIndex();
                int inputIndex = inputPort.GetPortIndex();
                int oldOutputIndex = outputIndex;
                int oldInputIndex = inputIndex;
                // 向下移动。
                if (direction > 0)
                {
                    if (port.isOutput)
                    {
                        oldOutputIndex = outputIndex - direction;
                    }
                    else
                    {
                        oldInputIndex = inputIndex - direction;
                    }
                }
                else //向上移动。
                {
                    if (port.isOutput)
                    {
                        outputIndex += direction;
                    }
                    else
                    {
                        inputIndex += direction;
                    }
                }

                _ui.unconnectNode(outputNodeItem, oldOutputIndex, inputNodeItem, oldInputIndex);
                _ui.connectNode(outputNodeItem, outputIndex, inputNodeItem, inputIndex, port is IValuePort ? ActionNodeConnectionType.value : ActionNodeConnectionType.action);
                updatePortMode(_connectingPort, outputPort);
                updatePortMode(_connectingPort, inputPort);
            }
        }

        private void updatePortModes(IPort connecting)
        {
            foreach (var node in _actionNodeList)
            {
                updateNodePortModes(node, connecting);
            }
        }
        private void updateNodePortModes(Node node, IPort connecting)
        {
            foreach (var input in node.inputPorts)
            {
                updatePortMode(connecting, input);
            }
            foreach (var output in node.outputPorts)
            {
                updatePortMode(connecting, output);
            }
        }
        private void updatePortMode(IPort connecting, IPort port)
        {
            var node = port.node;
            if (!_actionNodeList.Contains(node))
            {
                return;
            }
            var item = getActionNodeItem(node);
            var index = port.GetPortIndex();
            ActionNodeItemKnob portItem;
            if (port.isOutput)
            {
                portItem = item.getOutput(index);
            }
            else
            {
                portItem = item.getInput(index);
            }
            if (!portItem)
                return;

            NodePortMode mode;
            if (port.connections.Count() > 0 || connecting == port)
            {
                mode = NodePortMode.Connected;
            }
            else if (connecting != null && connecting.canConnectTo(port))
            {
                mode = NodePortMode.Highlighted;
            }
            else
            {
                mode = NodePortMode.NotConnected;
            }
            portItem.setMode(mode);
        }
        private void resetConnectingPort()
        {
            _connectingPort = null;
            updatePortModes(_connectingPort);
        }
        #endregion
        #region 节点断开
        /// <summary>
        /// 连接是否不合法。
        /// </summary>
        private bool IsInvalidConnection(NodeConnection connection)
        {
            if (connection is InvalidNodeConnection)
                return true;
            var src = connection.source;
            var dest = connection.destination;
            return IsInvalidConnection(src, dest);
        }
        /// <summary>
        /// 连接是否不合法。
        /// </summary>
        private bool IsInvalidConnection(IPort outputPort, IPort inputPort)
        {
            var src = outputPort;
            var dest = inputPort;
            if (src == null || dest == null) // 有一个端口不存在
                return true;
            if (src.node == null || dest.node == null) // 有一个端口的节点不存在
                return true;
            if (!src.node.outputPorts.Contains(src) || !dest.node.inputPorts.Contains(dest)) // 有一个端口的节点不包括该端口
                return true;
            if (!isValidTypeForConnection(src.define.type, dest.define.type)) // 端口类型不正确
                return true;
            return false;
        }
        /// <summary>
        /// 断开所有不合法的连接。
        /// </summary>
        /// <param name="port"></param>
        private void disconnectAllInvalidConnections(ActionGraph graph)
        {
            //更新ui
            var connections = graph.connections.Where(c => IsInvalidConnection(c));

            foreach (var connection in connections.ToArray())
            {
                if (connection == null)
                    continue;
                disconnect(connection);
            }
        }
        /// <summary>
        /// 断开连接到该端口上的所有连接。
        /// </summary>
        /// <param name="port"></param>
        private void disconnectPortAll(IPort port)
        {
            //更新ui
            foreach (var connection in port.connections.ToArray())
            {
                if (connection == null)
                    continue;
                disconnect(connection);
            }
        }
        private void disconnectPorts(IPort outputPort, IPort inputPort)
        {
            var connection = graph.getConnection(outputPort, inputPort);
            if (connection == null)
                return;
            disconnect(connection);
        }
        private void disconnect(NodeConnection connection)
        {
            //更新ui
            var inputPort = connection.destination;
            var outputPort = connection.source;
            var inputIndex = inputPort?.GetPortIndex() ?? -1;
            var outputIndex = outputPort?.GetPortIndex() ?? -1;
            graph.disconnect(connection);
            ActionNodeItem outputNodeItem = null;
            ActionNodeItem inputNodeItem = null;

            if (outputPort != null)
            {
                outputNodeItem = getActionNodeItem(outputPort.node);
                updatePortMode(_connectingPort, outputPort);
            }

            if (inputPort != null)
            {
                inputNodeItem = getActionNodeItem(inputPort.node);
                updatePortMode(_connectingPort, inputPort);
                //更新变长参数。
                updateInputParamsPort(inputPort);
                // 更新事件变量节点的事件变量名。
                checkEventVariableNodeItemVarNames(inputNodeItem, inputPort.node);
            }

            if (outputPort != null && inputPort != null)
            {
                _ui.unconnectNode(outputNodeItem, outputIndex, inputNodeItem, inputIndex);
                int paramIndex = inputPort is ValueInput vi ? vi.paramIndex : -1;
                addOperationRecord(new DisconnectOperation(this, outputPort.node, outputPort.name, inputPort.node, inputPort.name, paramIndex));
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
                disconnect(connection);
            }
        }
        #endregion
        #region 合法检测
        private bool isValidTypeForConnection(Type outputType, Type inputType)
        {
            return PortDefine.CanTypeConvert(outputType, inputType);
        }
        private bool validateConnectionType(Type outputType, Type inputType, bool showErrorTip = true)
        {
            if (!isValidTypeForConnection(outputType, inputType))
            {
                //相互连接的输入输出类型必须可以相互转化
                if (showErrorTip)
                {
                    showTypeConnectError();
                }
                return false;
            }
            return true;
        }
        private void showErrorTip(string msg)
        {
            onShowErrorTip?.Invoke(msg);
        }
        private void showOutputConnectError()
        {
            showErrorTip(game.lang._(StringTable.EDITOR_ERROR_OUTPUT_CONNECT_INPUT));
        }
        private void showTypeConnectError()
        {
            showErrorTip(game.lang._(StringTable.EDITOR_ERROR_INVALID_TYPE_CONNECT));
        }
        private void showCreateEventEntryForNonEventEffectError()
        {
            showErrorTip(game.lang._(EDITOR_ERROR_CREATE_ENTRY_FOR_NON_EVENT_EFFECT));
        }
        private void showEventEntryAlreadyExistsError()
        {
            showErrorTip(game.lang._(EDITOR_ERROR_ALREADY_HAS_EVENT_ENTRY));
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
                if (showErrorTip)
                {
                    if (!PortDefine.CanTypeConvert(outputPort.define?.type, inputPort.define?.type))
                    {
                        //类型必须可以转化
                        showTypeConnectError();
                    }
                    else if (outputPort.isOutput == inputPort.isOutput)
                    {
                        //值输出只能与值输入相互连接
                        showOutputConnectError();
                    }
                }
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
            var define = getActionDefine(actionNode.defineRef);
            if (define == null)
            {
                Debug.LogWarning($"没有找到动作{actionNode.defineRef}对应的名称");
                return tryTranslateWithDict(StringTable.EDITOR_NODE_NAME_UNKNOW);
            }
            else
            {
                return tryTranslateWithDict(define.editorName);
            }
        }

        private string getActionNodeObsoleteMsg(ActionNode actionNode)
        {
            ActionDefine actionDefine = getActionDefine(actionNode.defineRef);
            if (actionDefine.obsoleteMsg == null) return null;
            if (actionDefine.obsoleteMsg == "") return "";
            return game.lang._(actionDefine.obsoleteMsg);
        }
        private void selectCardForNodeConst(CardDefine cardDefine)
        {
            if (_curEditNode == null)
            {
                return;
            }
            var defineRef = cardDefine.getDefineRef();
            if (_curEditActionNodeConstItemIndex < 0)
            {
                setNodeConst(_curEditNode, _curEditActionNodeConstName, defineRef);
            }
            else
            {
                IList<DefineReference> collection = _curEditNode.getConst(_curEditActionNodeConstName) as IList<DefineReference>;
                collection[_curEditActionNodeConstItemIndex] = defineRef;
                setNodeConst(_curEditNode, _curEditActionNodeConstName, collection);
            }
            // 更新UI.
            updateNodeItemConsts(_curEditNode);
            addRecordBreakpoint();
        }
        #endregion
        #region 节点信息
        private PortDefine[] getNodeConstDefines(Node node)
        {
            if (node is ActiveEntryNode)
            {
                return new PortDefine[]
                {
                    PortDefine.Const(typeof(EffectTagCollection), ActiveEntryNode.tagsConstName, StringTable.EDITOR_PARAM_NAME_TAG)
                };
            }
            else if (node is AuraEntryNode)
            {
                return new PortDefine[]
                {
                    PortDefine.Const(typeof(PileNameCollection), AuraEntryNode.pileListName, StringTable.EDITOR_EFFECT_PROP_NAME_PILE_LIST),
                    PortDefine.Const(typeof(PileNameCollection), AuraEntryNode.rangeListName, StringTable.EDITOR_EFFECT_PROP_NAME_RANGE_LIST)
                };
            }
            else if (node is PassiveEntryNode)
            {
                return new PortDefine[]
                {
                    PortDefine.Const(typeof(EffectTagCollection), PassiveEntryNode.tagsConstName, StringTable.EDITOR_PARAM_NAME_TAG),
                    PortDefine.Const(typeof(PileNameCollection), PassiveEntryNode.pileListName, StringTable.EDITOR_EFFECT_PROP_NAME_PILE_LIST),
                };
            }
            else if (node is ActionNode actionNode)
            {
                var define = getActionDefine(actionNode.defineRef);
                return define.constDefines.ToArray();
            }
            else
            {
                return Array.Empty<PortDefine>();
            }
        }
        private ActionNodeValueInfo[] getNodeConstValueInfos(Node node)
        {
            var constDefines = getNodeConstDefines(node);
            var count = constDefines.Length;
            var infoArray = new ActionNodeValueInfo[count];
            for (int i = 0; i < count; i++)
            {
                var constDefine = constDefines[i];
                // 翻译名称
                infoArray[i] = getConstValueInfoFromDefine(constDefine, node.getConst(constDefine.name));
            }
            return infoArray;
        }
        private ActionNodeValueInfo getConstValueInfoFromDefine(PortDefine constDefine, object value)
        {
            var name = tryTranslateWithDict(constDefine.getDisplayName());
            Type type = constDefine.type;
            if (type == typeof(int))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.integer, value);
            else if (type == typeof(float))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.real, value);
            else if (type == typeof(bool))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.boolean, value);
            else if (type == typeof(string))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.text, value);
            else if (type.IsEnum)
            {
                var optionalValues = Enum.GetValues(type).Cast<object>().ToArray();
                var optionalNames = new string[optionalValues.Length];
                for (int j = 0; j < optionalNames.Length; j++)
                {
                    var optName = Enum.GetName(type, optionalValues[j]);
                    var indexName = $"{type.FullName}.{optName}";
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
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.enumValue, value, optionalNames, optionalValues);
            }
            else if (type == typeof(EffectTag))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.effectTag, value);
            else if (type == typeof(ActionValueRef))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.actionValueRef, value);
            else if (type == typeof(DefineReference))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.cardReference, value);
            else if (type == typeof(DefineReference[]))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.cardReferenceCollection, value);
            else if (type == typeof(PileNameCollection))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.pileNameCollection, value);
            else if (type == typeof(KeywordCollection))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.keywords, value);
            else if (type == typeof(EventArgReference) || type == typeof(EventReferenceProxy))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.eventReference, value);
            else if (type == typeof(EffectTagCollection))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.effectTagCollection, value);
            else if (type == typeof(CardPropertySetterName))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.cardPropertyNameSet, value);
            else if (type == typeof(CardPropertyGetterName))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.cardPropertyNameGet, value);
            else if (type == typeof(CardDefinePropertyGetterName))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.cardDefinePropertyNameGet, value);
            else if (type == typeof(PileName))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.pileName, value);
            else if (type == typeof(EventVarName))
                return new ActionNodeValueInfo(name, constDefine.name, ActionNodeValueType.eventVarName, value);
            else
                throw new NotImplementedException($"没有{type}对应的常数类型");
        }
        private string[] getNodeInputNames(Node node)
        {
            string[] inputNames = new string[node.inputPorts.Count()];
            for (int i = 0; i < inputNames.Length; i++)
            {
                // 翻译输出节点
                var outputName = node.inputPorts.ElementAt(i).define.getDisplayName();
                inputNames[i] = tryTranslateWithDict(outputName);
            }
            return inputNames;
        }
        private PortItemInfo[] getNodeInputViewData(Node node)
        {
            if (node is ActiveEntryNode activeEntry)
            {
                return getActiveEntryInputViewData(activeEntry);
            }
            List<PortItemInfo> viewDataList = new List<PortItemInfo>();
            //值输入
            foreach (var inputPort in node.inputPorts)
            {
                // 翻译输出节点
                var inputName = tryTranslateWithDict(inputPort.define.getDisplayName());
                viewDataList.Add(new PortItemInfo(inputName, game.editor.getColorByType(inputPort.define.type), inputPort.GetPortType() == PortType.Control));
            }
            return viewDataList.ToArray();
        }
        private string[] getNodeOutputNames(Node node)
        {
            string[] outputNames = new string[node.outputPorts.Count()];
            for (int i = 0; i < outputNames.Length; i++)
            {
                // 翻译输出节点
                var outputName = node.outputPorts.ElementAt(i).define.getDisplayName();
                outputNames[i] = tryTranslateWithDict(outputName);
            }
            return outputNames;

        }
        private PortItemInfo[] getNodeOutputViewData(Node node)
        {
            List<PortItemInfo> outputList = new List<PortItemInfo>();
            foreach (var output in node.outputPorts)
            {
                // 翻译输出节点
                var outputName = tryTranslateWithDict(output.define.getDisplayName());
                outputList.Add(new PortItemInfo(outputName, game.editor.getColorByType(output.define.type), output.GetPortType() == PortType.Control));
            }
            return outputList.ToArray();
        }
        #endregion
        /// <summary>
        /// 重置所有对于编辑物体的引用。
        /// </summary>
        private void resetEditObjects()
        {
            _ui.clearSelection();
            _actionNodeList.Clear();
            _connectingPort = null;
            _actionNodeItemList.Clear();
            _ui.clearGraph();
            _effect = null;
            _generatedActionDefine = null;
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
        #region 常量
        public string getNodeConstNameByIndex(Node node, int index)
        {
            if (_nodeConstNameDict.TryGetValue(node.GetType(), out var constNames))
            {
                if (index >= 0 && index < constNames.Length)
                {
                    return constNames[index];
                }
            }
            return null;
        }
        private async void updateNodeItemConsts(Node node)
        {
            var item = getActionNodeItem(node);
            if (node is ActionNode actionNode)
            {
                if (actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.GetBuffDefine))
                {
                    if (tryGetBuffDefine(actionNode.getConst(GetBuffDefineAction.constName), out GeneratedBuffDefine buffDefine))
                    {
                        updateBuffActionNode(item, buffDefine);
                        return;
                    }
                }
            } 

            ActionNodeValueInfo[] constViewData = getNodeConstValueInfos(node);
            if (constViewData == null)
                return;
            item.setConsts(constViewData);

            // 更新卡牌引用。
            foreach (var constPair in node.consts)
            {
                await updateCardDefineConst(item, node, constPair.Key, constPair.Value);
            }
            // 更新“转换事件类型”连接的输出端口。
            checkCastEventTypeNodeOutputs(node);
        }
        private Task updateCardDefineConst(ActionNodeItem item, Node node, string name, object value)
        {
            var consts = getNodeConstDefines(node);
            var constDefine = consts.FirstOrDefault(c => c.name == name);
            if (constDefine == null)
                return Task.CompletedTask;
            var constIndex = Array.IndexOf(consts, constDefine);
            return updateCardDefineConst(item, node, constIndex, value);
        }
        private async Task updateCardDefineConst(ActionNodeItem item, Node node, int constIndex, object value)
        {
            ValueInputField cardInputField = item.getConstInputField(constIndex);
            if (value is DefineReference cardReference)
            {
                await updateCardDefineInputField(node, cardInputField, cardReference);
            }
            else if (value is IList<DefineReference> cardReferenceList)
            {
                for (int i = 0; i < cardReferenceList.Count; i++)
                {
                    var childInputField = cardInputField.getItemAt(i);
                    var defRef = cardReferenceList[i];
                    await updateCardDefineInputField(node, childInputField, defRef);
                }
            }
        }
        /// <summary>
        /// 在“转换事件类型”节点的常量更改后，更新该节点的输出端口连接的节点的事件变量名列表。
        /// </summary>
        /// <param name="node">节点。</param>
        private void checkCastEventTypeNodeOutputs(Node node)
        {
            if (node is ActionNode actionNode && 
                (actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.CastEventType) ||
                actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.CastEventRecordType)))
            {
                var output = actionNode.getOutputPort<ValueOutput>(EffectEditorLibrary.EVENT_CAST_EVENT_TYPE_NODE_CAST_EVENT_PARAM_KEY);
                foreach (var connection in output.connections)
                {
                    var destNode = connection?.destination?.node;
                    if (destNode == null)
                        continue;
                    checkEventVariableNodeItemVarNames(getActionNodeItem(destNode), destNode);
                }
            }
        }
        /// <summary>
        /// 根据设置/获取事件变量节点的连接端口，更新该节点的事件变量名列表。
        /// </summary>
        /// <param name="item">节点UI。</param>
        /// <param name="node">节点。</param>
        private void checkEventVariableNodeItemVarNames(ActionNodeItem item, Node node)
        {
            if (node is not ActionNode action)
                return;
            if (!action.defineRef.isBuiltinReference(BuiltinActionDefineID.SetEventVariable) &&
                !action.defineRef.isBuiltinReference(BuiltinActionDefineID.GetEventVariable) &&
                !action.defineRef.isBuiltinReference(BuiltinActionDefineID.GetEventRecordVariable))
                return;

            var inputPort = action.defineRef.isBuiltinReference(BuiltinActionDefineID.GetEventRecordVariable) ?
                node.getInputPort<ValueInput>(EffectEditorLibrary.EVENT_RECORD_VARIABLE_NODE_EVENT_PARAM_KEY) :
                node.getInputPort<ValueInput>(EffectEditorLibrary.EVENT_VARIABLE_NODE_EVENT_PARAM_KEY);
            var eventRef = getConnectedEventReference(inputPort);
            var eventDefine = game.card.getLocalEventDefine(eventRef);

            var varInfos = eventDefine?.afterVariableInfos;

            string[] eventVarNames = varInfos != null ? eventVarNames = varInfos.Select(v => v.name).ToArray() :  Array.Empty<string>();
            var eventVarNamesEditor = eventVarNames.Select(n => game.lang._(n)).ToArray();

            item.setEventVarNames(eventVarNamesEditor, eventVarNames);
            var consts = getNodeConstValueInfos(node);
            item.setConsts(consts);
        }
        /// <summary>
        /// 获取连接一个值输入端口的事件类型引用。
        /// </summary>
        /// <param name="inputPort">值输入端口。</param>
        /// <returns></returns>
        private EventReference getConnectedEventReference(ValueInput inputPort)
        {
            var connectedOutput = inputPort?.getConnectedOutputPort();
            if (connectedOutput != null)
            {
                // 转换事件类型节点。
                var connectedNode = connectedOutput.node;
                if (connectedNode is ActionNode conAction && 
                    (conAction.defineRef.isBuiltinReference(BuiltinActionDefineID.CastEventType) ||
                    conAction.defineRef.isBuiltinReference(BuiltinActionDefineID.CastEventRecordType)))
                {
                    return conAction.getConst<EventReference>(EffectEditorLibrary.EVENT_CAST_EVENT_TYPE_NODE_TYPE_REF_PARAM_KEY);
                }
                // 其他事件输出。
                var eventType = connectedOutput.define?.type;
                if (eventType != null)
                {
                    if (eventType.IsArray)
                        eventType = eventType.GetElementType();
                    if (typeof(IEventArg).IsAssignableFrom(eventType))
                        return new EventReference(0, EventHelper.getName(eventType));
                }
            }
            return null;
        }
        private async Task initNodeConstDefaultValue(ActionNodeItem item, Node node, int constIndex, ActionNodeValueInfo constInfo)
        {
            if (constInfo.type == ActionNodeValueType.cardReference || constInfo.type == ActionNodeValueType.cardReferenceCollection)
            {
                ValueInputField field = item.getConstInputField(constIndex);
                await updateCardDefineInputField(node, field, null);
            }
            else
            {
                // 这里不用setNodeConst，因为setNodeConst会调用addOperationRecord，将节点图变为未保存状态
                var value = getNodeConstDefaultValue(constInfo.type);
                node.setConst(constInfo.name, value);
            }
        }
        private object getNodeConstDefaultValue(ActionNodeValueType valueType)
        {
            switch (valueType)
            {
                case ActionNodeValueType.cardDefinePropertyNameGet:
                    return CardEditorCtrl.cardProps.FirstOrDefault(p => p.canDefineGet).value;
                case ActionNodeValueType.cardPropertyNameGet:
                    return CardEditorCtrl.cardProps.FirstOrDefault(p => p.canGet).value;
                case ActionNodeValueType.cardPropertyNameSet:
                    return CardEditorCtrl.cardProps.FirstOrDefault(p => p.canSet).value;
                case ActionNodeValueType.effectTag:
                    return getAllEffectTagValues()[0];
                case ActionNodeValueType.eventReference:
                    return _eventReferences[0];
                case ActionNodeValueType.pileName:
                    return getAllPiles()[0].value;
                case ActionNodeValueType.eventVarName:
                    return null;
            }
            return null;
        }
        private async Task updateCardDefineInputField(Node node, ValueInputField cardInputField, DefineReference cardReference)
        {
            if (!cardInputField)
                return;
            CardDefine cardDefine = null;
            if (cardReference != null)
            {
                var localRef = game.card.getLocalCardPoolReference(cardReference.cardPoolId);
                // 如果卡池没有被加载，加载卡池。
                await game.card.getOrLoadCardPool(localRef, true);
                cardDefine = game.card.getCardDefine(localRef, cardReference.defineId);
            }
            CardSkinData cardSkin = null;
            Sprite image = null;
            if (cardDefine != null)
            {
                cardSkin = game.card.getDefaultSkin(cardDefine);
                image = await game.card.loadCardImage(cardDefine);
            }
            cardInputField.setCard(cardDefine, cardSkin, image);
        }
        #endregion
        private void updateInputParamsPort(IPort inputPort)
        {
            if (inputPort.node is ActionNode actionNode)
            {
                var inputNodeItem = getActionNodeItem(actionNode);
                var lastInputDefine = actionNode.define?.inputDefines.LastOrDefault();
                if (lastInputDefine == inputPort.define && lastInputDefine?.isParams == true)
                {
                    if (_connectingPort != null && _connectingPort.define == lastInputDefine)
                    {
                        if (!actionNode.inputPorts.Contains(_connectingPort))
                        {
                            resetConnectingPort();
                        }
                    }
                    inputNodeItem.setInputs(getNodeInputViewData(actionNode));
                    updateNodePortModes(actionNode, _connectingPort);
                }
            }
        }
        private (string name, string value)[] getAllPiles()
        {
            return CardEditorCtrl.piles.Select(p => (p.Value, p.Key)).ToArray();
        }
        private string[] getAllEffectTagValues()
        {
            return _activeEffectTags.Union(_passiveEffectTags).ToArray();
        }
        private Task displayCardSelectPanel(long cardPoolId)
        {
            if (isReadonly)
                return Task.CompletedTask;
            if (onStartSelectCard != null)
            {
                return onStartSelectCard.Invoke(cardPoolId);
            }
            return Task.CompletedTask;
        }
        private void updateReadonly()
        {
            _ui.setReadonly(_isReadonly);
        }
        #endregion
        #region 事件
        public event Action OnGraphEdited;
        public event Action<Node> OnNodeSelected;
        public event Action<Node> OnNodeDeselected;
        public event Action<Node> OnNodeVisualEffectButtonClicked;
        public event Func<long, Task> onStartSelectCard;
        public event Action<string> onShowErrorTip;
        public event Action<ActionNode, string> onOpenActionNodeConstCardDefine;
        public event Action<ActionNode, string, int> onOpenActionNodeConstItemCardDefine;
        #endregion
        #region 属性字段
        [TranslateMsg("编辑器-在分类中返回上一层的按钮({0}为按钮文本)")]
        public const string EDITOR_CATEGORY_BACK_TEMPLATE = "< {0}";
        [TranslateMsg("编辑器-分类模板({0}为分类名称)")]
        public const string EDITOR_CATEGORY_TEMPLATE = "{0} >";
        [TranslateMsg("编辑器-分类名称-事件入口节点")]
        public const string EDITOR_CATEGORY_EVENT_ENTRY_NODE = "事件入口";
        [TranslateMsg("编辑器-参数名称-作用条件（自定义光环-作用条件-输出值）")]
        public const string EDITOR_PARAM_NAME_AFFECT_CONDITION = "作用条件";
        [TranslateMsg("编辑器-参数名称-目标增益（自定义光环-更新动作-输出值）")]
        public const string EDITOR_PARAM_NAME_TARGETS_BUFF = "目标增益";
        [TranslateMsg("编辑器-节点名称-主动效果入口节点")]
        public const string EDITOR_NODE_NAME_ACTIVE_ENTRY_NODE = "主动效果入口";
        [TranslateMsg("编辑器-节点名称-被动效果入口节点")]
        public const string EDITOR_NODE_NAME_PASSIVE_ENTRY_NODE = "被动效果入口";
        [TranslateMsg("编辑器-节点名称-光环效果入口节点")]
        public const string EDITOR_NODE_NAME_AURA_ENTRY_NODE = "光环效果入口";
        [TranslateMsg("编辑器-节点名称-未知事件入口节点")]
        public const string EDITOR_NODE_NAME_UNKNOWN_EVENT = "未知事件入口节点";

        [TranslateMsg("编辑器-错误提示")]
        public const string EDITOR_ERROR_CREATE_ENTRY_FOR_NON_EVENT_EFFECT = "非事件效果不能添加事件入口节点。";
        [TranslateMsg("编辑器-错误提示")]
        public const string EDITOR_ERROR_ALREADY_HAS_EVENT_ENTRY = "该事件效果已包含入口节点，请将其删除。";
        public bool isReadonly => _isReadonly;
        [SerializeField]
        ActionNodeGraphPanel _ui;
        [SerializeField]
        bool _isReadonly;

        ConfigManager _config;
        ConfigManager config 
        {
            get
            {
                if (!_config)
                {
                    _config = this.findInstance<ConfigManager>();
                }
                return _config;
            }
        }

        readonly List<Node> _actionNodeList = new List<Node>();
        readonly List<ActionNodeItem> _actionNodeItemList = new List<ActionNodeItem>();
        private Dictionary<ActionNodeItem, Vector2> _movingNodeDict = new Dictionary<ActionNodeItem, Vector2>();
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
        long _curCardPoolId = 0;
        GeneratedEffect _effect;
        /// <summary>
        /// 当前正在进行连接的端口。
        /// </summary>
        IPort _connectingPort = null;
        /// <summary>
        /// 当前正在编辑的动作节点
        /// </summary>
        Node _curEditNode = null;
        /// <summary>
        /// 当前正在编辑的动作节点常量名
        /// </summary>
        string _curEditActionNodeConstName = string.Empty;
        /// <summary>
        /// 当前正在编辑的动作节点常量子项索引
        /// </summary>
        int _curEditActionNodeConstItemIndex = -1;
        string[] _targetCheckerTypes = new string[]
        {
            DEFINE.TARGET_TYPE_CHARACTER,
            DEFINE.TARGET_TYPE_SERVANT
        };
        EventTriggerTime[] _eventTriggerTimes;
        EventReference[] _eventReferences;

        [TranslateMsg("内置节点名称，用于通用效果设置属性")]
        const string EDITOR_NODE_NAME_GENERATED_EFFECT_DATA = "通用效果属性";
        [TranslateMsg("内置节点名称，用于事件效果设置属性")]
        const string EDITOR_NODE_NAME_EVENT_EFFECT_DATA = "事件效果属性";
        [TranslateMsg("内置节点名称，用于光环效果设置属性")]
        const string EDITOR_NODE_NAME_AURA_DATA = "光环效果属性";

        [TranslateMsg("内置节点参数，通常用于比较和操作，表示操作符，如加、减、或、异或等")]
        const string EDITOR_PARAM_OPERATOR = "运算符";
        [TranslateMsg("内置节点参数，通常用于表示计算结果")]
        const string EDITOR_PARAM_RESULT = "结果";
        [TranslateMsg("节点参数 PropertyName")]
        const string EDITOR_PARAM_PROPERTY_NAME = "属性名";
        [TranslateMsg("节点参数 DamageEvent")]
        const string EDITOR_PARAM_DAMAGE_EVENT = "伤害事件";
        [TranslateMsg("节点参数 DamageSource")]
        const string EDITOR_PARAM_DAMAGE_SOURCE = "伤害来源";
        [TranslateMsg("节点参数 Amount")]
        const string EDITOR_PARAM_DAMAGE_AMOUNT = "伤害量";
        [TranslateMsg("节点参数 Target")]
        const string EDITOR_PARAM_TARGET = "目标";
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

        [TranslateMsg]
        const string TITLE_NODE_OBSOLETE = "过时的节点";
        [TranslateMsg("参数0为节点自带的消息")]
        const string MESSAGE_NODE_OBSOLETE = "此节点已被弃用，很可能在之后的版本中删除。\n{0}";

        public static readonly Dictionary<string, string> _nameTextDict = new Dictionary<string, string>()
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
            { "GetSpellDamage", StringTable.EDITOR_NODE_NAME_GET_SPELL_DAMAGE },
            { "GetOwner", StringTable.EDITOR_NODE_NAME_GET_OWNER },
            { "GetProperty", StringTable.EDITOR_NODE_NAME_GET_PROPERTY },
            { "SetProperty", StringTable.EDITOR_NODE_NAME_SET_PROPERTY },
            { "GetUniquePropertyName", StringTable.EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME },
            { "Damage", StringTable.EDITOR_NODE_NAME_DAMAGE },
            { "SetIntPropModifierValue", StringTable.EDITOR_NODE_NAME_SET_INT_PROP_MODIFIER_VALUE },
            { "GetCount", StringTable.EDITOR_NODE_NAME_GET_COUNT },
            { "GeneratedEffectData", EDITOR_NODE_NAME_GENERATED_EFFECT_DATA },
            { "EventEffectData", EDITOR_NODE_NAME_EVENT_EFFECT_DATA },
            { "AuraData", EDITOR_NODE_NAME_AURA_DATA },
            // 内置参数
            { "value", StringTable.EDITOR_PARAM_NAME_VALUE },
            { "Value", StringTable.EDITOR_PARAM_NAME_VALUE },
            { "operator", EDITOR_PARAM_OPERATOR },
            { "result", EDITOR_PARAM_RESULT },
            { "PropertyName", EDITOR_PARAM_PROPERTY_NAME },
            { "RemoveModifierEvent", EDITOR_PARAM_PROPERTY_NAME },
            { "Collection", StringTable.EDITOR_PARAM_NAME_COLLECTION },
            { "DamageEvent", EDITOR_PARAM_DAMAGE_EVENT },
            { "DamageSource", EDITOR_PARAM_DAMAGE_SOURCE },
            { "Amount", EDITOR_PARAM_DAMAGE_AMOUNT },
            { "Target", EDITOR_PARAM_TARGET },
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

        readonly Dictionary<Type, string[]> _nodeConstNameDict = new Dictionary<Type, string[]>()
        {
            //节点分类
            { typeof(ActiveEntryNode), new string[] { 
                ActiveEntryNode.tagsConstName 
            } },
            { typeof(PassiveEntryNode), new string[] { 
                PassiveEntryNode.tagsConstName,
                PassiveEntryNode.pileListName
            } },
            { typeof(EventEntryNode), new string[] { 
                PassiveEntryNode.tagsConstName,
                PassiveEntryNode.pileListName
            } },
            { typeof(AuraEntryNode), new string[] {
                AuraEntryNode.pileListName,
                AuraEntryNode.rangeListName
            } },
        };
        static readonly string[] _activeEffectTags = new string[]
        {
            EFFECTTAG.ACTIVE_PLAYED
        };
        static readonly string[] _activeEffectTagNames = new string[]
        {
            StringTable.GAME_KEYWORD_BATTLECRY
        };
        static readonly string[] _passiveEffectTags = new string[]
        {
            EFFECTTAG.PASSIVE_LASTWORD,
            EFFECTTAG.EVENT_TRAP,
        };
        static readonly string[] _passiveEffectTagNames = new string[]
        {
            StringTable.GAME_KEYWORD_DEATHRATTLE,
            StringTable.GAME_KEYWORD_SECRET,
        };
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(long cardPoolId, object editObject)
            {
                this.cardPoolId = cardPoolId;
                this.editObject = editObject;
            }
            public long cardPoolId;
            public object editObject;
            public ActionNodeGraphInfo graphInfo;
        }
        #endregion
    }
}
