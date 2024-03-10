using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using BJSYGameCore.UI;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine.Shared;
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
        public Task display(object editObject)
        {
            quitGraph();

            // 初始化输入
            var typeInfos = GraphTypes.getAllPortTypeInfos();
            var keywords = EditorManager.editorKeywords;
            cardTypeValues = game.editor.getCardTypes();
            typeValues = typeInfos.Select(i => i.type.Name).ToArray();
            var fieldProps = new ValueInputFieldProperties()
            {
                graphTypeNames = game.editor.getEditorPortTypeNames(),
                graphTypeValues = typeValues,

                eventNames = eventEditorNames,
                eventValues = eventReferences,
                
                cardTypeNames = game.editor.getCardTypeNames(),
                cardTypeValues = cardTypeValues,

                buffNames = game.editor.getBuffNames(),
                buffValues = game.editor.getBuffReferences(),

                pileNames = game.editor.getAllPileNames(),
                pileValues = game.editor.getAllPileValues(),

                keywordNames = keywords.Select(k => game.card.getKeywordName(k)).ToArray(),
                keywordValues = keywords.ToArray(),

                effectTagNames = getAllEffectTagNames(),
                effectTagValues = getAllEffectTagValues(),

                cardProperties = game.editor.getTranslatedCardProperties().ToArray(),

                defaultValueTypeName = game.editor.getDefaultValueTypeName(),
                valueTypeNames = game.editor.getValueTypeNameMaps(),
            };
            _ui.initValueInputFieldProperties(fieldProps);

            //更新UI
            _ui.gameObject.SetActive(true);

            _ui.resetView();


            if (editObject is GeneratedActionDefine actionDefine)
                return openGeneratedActionDefine(actionDefine);
            if (editObject is GeneratedRuleLogic ruleLogic)
                return openGeneratedRuleLogic(ruleLogic);
            else if (editObject is GeneratedEffect effect)
                return openActionGraph(effect);
            else
                return Task.CompletedTask;
        }
        public Task display(UIDisplayInfo uiDisplayInfo)
        {
            if (uiDisplayInfo is DisplayInfo info)
            {
                return display(info.editObject);
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
            else if (_generatedRuleLogic != null)
                editObject = _generatedRuleLogic;
            else
                editObject = null;
            return new DisplayInfo(editObject);
        }
        public Task hide()
        {
            gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        public bool isActionDefineNameExist(string name)
        {
            foreach (var actionDefine in getActionDefines())
            {
                if (actionDefine.editorName == name)
                {
                    return true;
                }
            }
            return false;
        }
        #region 翻译文本
        public string tryTranslateWithDict(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            if (_nameTextDict.TryGetValue(input, out string text))
            {
                return game.i18n._(text);
            }
            return game.i18n._(input);
        }
        public string translateCategoryName(string category)
        {
            if (string.IsNullOrEmpty(category))
                return string.Empty;
            if (_nameTextDict.TryGetValue(category, out string text))
            {
                return game.lang._p(StringTable.CONTEXT_NODE_CATEGORY, text);
            }
            return game.lang._p(StringTable.CONTEXT_NODE_CATEGORY, category);
        }
        public string translateActionName(ActionDefine actionDefine)
        {
            return translateActionName(actionDefine?.editorName);
        }
        public string translateActionName(string actionName)
        {
            return tryTranslateWithDict(actionName);
        }
        public string translatePortName(IPort port)
        {
            return translatePortName(port?.define);
        }
        public string translatePortName(PortDefine portDefine)
        {
            return translatePortName(portDefine?.getDisplayName());
        }
        public string translatePortName(string portName)
        {
            if (string.IsNullOrEmpty(portName))
                return string.Empty;
            if (_nameTextDict.TryGetValue(portName, out string text))
            {
                return game.lang._p(StringTable.CONTEXT_NODE_PARAM_NAME, text);
            }
            return game.lang._p(StringTable.CONTEXT_NODE_PARAM_NAME, portName);
        }
        public string translateEventVarName(string eventVarName)
        {
            if (string.IsNullOrEmpty(eventVarName))
                return string.Empty;
            if (_nameTextDict.TryGetValue(eventVarName, out string text))
            {
                return game.lang._p(EVENTVAR.CONTEXT_EVENT_VAR, text);
            }
            return game.lang._p(EVENTVAR.CONTEXT_EVENT_VAR, eventVarName);
        }
        #endregion
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
            var definer = getNodeDefiner();
            var clipGraph = pasteNodes(seri, graph, pos, definer, out _);

            foreach (var node in clipGraph.nodes)
            {
                await applyNode(node);
                addOperationRecord(new CreateNodeOperation(this, node));
            }
            foreach (var connection in clipGraph.connections)
            {
                if (IsInvalidConnection(connection))
                    continue;
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
            var actionNodeItem = await applyNode(actionNode);
            addOperationRecord(new CreateNodeOperation(this, actionNode));
            return actionNodeItem;
        }
        public Task<ActionNodeItem> createActionNode(ActionDefine actionDefine, Vector2 pos)
        {
            return createActionNode(actionDefine, pos.x, pos.y);
        }
        public ActionNodeItem createEventEntryNode(EventDefine eventDefine, EventTriggerTimeType triggerType, float x, float y, bool showErrorTip = true)
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
            var actionNode = graph.createEventEntryNode(eventDefine, triggerType, x, y);
            registerNode(actionNode);
            ActionNodeItem actionNodeItem = createNodeUI4PassiveEntryNode(actionNode);
            addOperationRecord(new CreateNodeOperation(this, actionNode));
            updateEffectProperties();
            return actionNodeItem;
        }
        public ActionNodeItem createEventEntryNode(EventDefine eventDefine, EventTriggerTimeType triggerType, Vector2 pos)
        {
            return createEventEntryNode(eventDefine, triggerType, pos.x, pos.y);
        }
        public async Task<Node> createNode(NodeDefine define, Vector2 pos)
        {
            ActionNodeItem item = null;
            if (define.actionDefine != null)
            {
                item = await createActionNode(define.actionDefine, pos);
            }
            else if (define.eventDefine != null)
            {
                item = createEventEntryNode(define.eventDefine, define.eventTriggerType, pos);
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
                selectCardForNodeInputDefaultValue(cardDefine);
            }
            //重置字段
            resetCardSelectInfo();
        }
        public void resetCardSelectInfo()
        {
            //重置字段
            _curEditNode = null;
            _curEditNodeInputDefaultValueName = string.Empty;
            _curEditNodeInputDefaultValueParamIndex = -1;
            _curEditNodeInputDefaultValueItemIndexes = null;
            _curCardSeekingEffectPropName = string.Empty;
            _curCardSeekingEffectPropIndexes = null;
        }

        /// <summary>
        /// 设置一个节点的一个输入端口的默认值，并将这次修改加入到撤销列表记录中。
        /// </summary>
        /// <param name="node">节点。</param>
        /// <param name="name">常量名。</param>
        /// <param name="value">常量值。</param>
        public void setNodeDefaultValue(Node node, string name, int paramIndex, object value)
        {
            var beforeValue = node.getInputDefaultValue(name, paramIndex);
            if (value != null)
            {
                var inputPort = paramIndex < 0 ? node.getInputPort(name) : node.getParamInputPort(name, paramIndex);
                var valueType = inputPort.define.type.getNodeValueType();
                var forceHasValue = inputPort?.define?.isConst ?? false;
                value = validateInputFieldValue(valueType, value, forceHasValue);
            }
            value = value.cloneValue();
            beforeValue = beforeValue.cloneValue();
            node.setInputDefaultValue(name, paramIndex, value);
            addOperationRecord(new ChangeNodeDefaultValueOperation(this, node, name, paramIndex, beforeValue, value));
        }
        /// <summary>
        /// 验证一个节点的所有输入默认值是否合法。
        /// </summary>
        /// <param name="node"></param>
        private void validateNodeInputDefaultValues(Node node)
        {
            if (node == null)
                return;
            foreach (var inputPort in node.inputPorts.ToArray())
            {
                var name = inputPort.name;
                var paramIndex = inputPort is ValueInput vi ? vi.paramIndex : -1;
                var value = node.getInputDefaultValue(name, paramIndex);
                var valueType = inputPort.define?.type?.getNodeValueType() ?? default;
                var forceHasValue = inputPort.define?.isConst ?? false;
                var newValue = validateInputFieldValue(valueType, value, forceHasValue);
                if (value != newValue)
                {
                    node.setInputDefaultValue(name, paramIndex, newValue);
                }
            }
        }
        public string getNodeDefineName(NodeDefine define)
        {
            if (define.actionDefine != null)
            {
                return translateActionName(define.actionDefine);
            }
            if (define.isEventEntry())
            {
                return tryTranslateWithDict(game.eventManager.translateEventTriggerTimeNameForEditor(define.getEventTriggerTime()));
            }
            return game.lang._(StringTable.EDITOR_NODE_NAME_UNKNOWN);
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
            _ui.onNodeInputDefaultValueChanged += onNodeInputDefaultValueChangedCallback;
            _ui.onClickActionNodeDefaultValueButton += onClickActionNodeDefaultValueButton;
            _ui.onNodeSelected += onNodeSelected;
            _ui.onNodeDeselected += onNodeDeselected;
            _ui.onNodeVisualEffectButtonClicked += onNodeVisualEffectButtonClickedCallback;
            _ui.onClickActionNodeObsoleteButton += onClickActionNodeObsoleteButtonCallback;
            _ui.onClickActionNodeHelpButton += onClickActionNodeHelpButtonCallback;
            //触发
            _ui.onClickAddTargetCheckerButton += onClickAddTargetCheckerButton;
            _ui.onTargetCheckerTypeValueChanged += onTargetCheckerTypeValueChanged;
            _ui.onClickRemoveTargetCheckerButton += onClickRemoveTargetCheckerButton;
            _ui.onTargetCheckerErrorTipValueChanged += onTargetCheckerErrorTipValueChanged;

            // 效果属性
            _ui.onClickAddEffectPropertyButton += onClickAddEffectPropertyButtonCallback;
            _ui.onClickEffectPropertyButton += onClickEffectPropertyButtonCallback;
            _ui.onEffectPropertyValueChanged += onEffectPropertyValueChangedCallback;
            _ui.onDeleteEffectProperty += onDeleteEffectPropertyCallback;
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

                // 为该端口设置一个端口默认值，并使其显形。
                var name = inputPort.name;
                var paramIndex = inputPort is ValueInput vi ? vi.paramIndex : -1;
                if (!node.hasInputDefaultValue(name, paramIndex))
                {
                    Type type = inputPort.define.type;
                    var valueType = type.getNodeValueType();
                    if (!tryGetValueTypeDefaultValue(valueType.type, out var defaultValue))
                    {
                        defaultValue = valueType.getDefaultValue();
                    }
                    setNodeDefaultValue(node, name, paramIndex, defaultValue);
                    // 更新事件变量节点的事件变量名。
                    checkEventVariableNodeItemVarNames(item, node);
                    // 更新转换事件类型节点相连的节点的事件变量名。
                    checkCastEventTypeNodeOutputs(node);
                    updateUINodeItemInputPorts(node);
                    addRecordBreakpoint();
                }

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
                // 点击的输出点是本身。
                // 取消选择。
                _ui.selectPort(null);
                resetConnectingPort();
                return;
            }
            else if (_connectingPort != null && !_connectingPort.isOutput)
            {
                // 选择的输入点存在。
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
                // 点击的输入点不是本身，并且当前选择的输入点不存在。
                // 选择新的输出点。
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

            if (!isOutput)
            {
                var node = getActionNode(actionNodeItem);
                var port = node.getInputPortAt(index);
                // 移除端口的输入默认值。
                removeInputPortDefaultValue(port);
                // 更新转换事件类型节点相连的节点的事件变量名。
                checkCastEventTypeNodeOutputs(node);
                // 更新节点的输入端口列表
                updateUINodeItemInputPorts(node);
                addRecordBreakpoint();
            }
            resetConnectingPort();
        }
        /// <summary>
        /// 当用户更改节点的常量值，设置对应的常量。
        /// </summary>
        /// <param name="actionNodeItem"></param>
        /// <param name="inputIndex"></param>
        /// <param name="value"></param>
        private void onNodeInputDefaultValueChangedCallback(ActionNodeItem actionNodeItem, int inputIndex, object value)
        {
            if (isReadonly)
                return;
            var node = getActionNode(actionNodeItem);
            var inputPort = node.getInputPortAt(inputIndex);
            var name = inputPort.name;
            var paramIndex = inputPort is ValueInput vi ? vi.paramIndex : -1;
            setNodeDefaultValue(node, name, paramIndex, value);
            // 更新转换事件类型节点相连的节点的事件变量名。
            checkCastEventTypeNodeOutputs(node);
            updateUINodeItemInputPorts(node);
            addRecordBreakpoint();
        }
        private void onClickActionNodeDefaultValueButton(ActionNodeItem actionNodeItem, int inputIndex, int[] itemIndexes, ValueInputFieldButton type)
        {
            switch (type)
            {
                case ValueInputFieldButton.CardButton:
                    onClickNodeInputDefaultValueCardButton(actionNodeItem, inputIndex, itemIndexes);
                    break;
                case ValueInputFieldButton.CardEditButton:
                    onClickNodeDefaultValueCardEditButton(actionNodeItem, inputIndex, itemIndexes);
                    break;
                case ValueInputFieldButton.BuffEditButton:
                    onClickNodeDefaultValueBuffEditButton(actionNodeItem, inputIndex, itemIndexes);
                    break;
            }
        }
        private async void onClickNodeInputDefaultValueCardButton(ActionNodeItem actionNodeItem, int inputIndex, int[] itemIndexes)
        {
            if (isReadonly)
                return;
            Node actionNode = getActionNode(actionNodeItem);
            var inputPort = actionNode.getInputPortAt(inputIndex);
            if (inputPort == null)
                return;
            var inputDefine = inputPort.define;
            if (inputDefine == null)
                return;
            var paramIndex = inputPort is ValueInput vi ? vi.paramIndex : -1;
            long cardPoolId = curCardPoolId;
            var defaultValue = actionNode.getInputDefaultValue(inputDefine.name, paramIndex);
            if (defaultValue.tryGetListElementByIndexes(itemIndexes, out DefineReference defRef))
            {
                //打开卡牌选择界面
                cardPoolId = defRef.cardPoolId;
            }
            await displayCardSelectPanel(cardPoolId);
            _curEditNode = actionNode;
            _curEditNodeInputDefaultValueName = inputDefine.name;
            _curEditNodeInputDefaultValueParamIndex = paramIndex;
            _curEditNodeInputDefaultValueItemIndexes = itemIndexes;
        }
        private void onClickNodeDefaultValueCardEditButton(ActionNodeItem actionNodeItem, int index, int[] itemIndexes)
        {
            Node node = getActionNode(actionNodeItem);
            var inputPort = node.getInputPortAt(index);
            if (inputPort == null)
                return;
            var paramIndex = inputPort is ValueInput vi ? vi.paramIndex : -1;
            onOpenNodeInputDefaultValueCardDefine?.Invoke(node, inputPort.name, paramIndex, itemIndexes);
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
        private void onClickActionNodeHelpButtonCallback(int id)
        {
            var doc = game.editor.getNodeComment(id);
            if (doc == null)
                return;
                
            var lang = game.lang;
            var editor = game.editor;
            string str = "";
            // 标题
            var editorNameStr = lang._(doc.editorName);
            str += lang._(EDITOR_NODE_DOC_TITLE_TEMPLATE, editorNameStr); 
            // 分类
            if (!string.IsNullOrEmpty(doc.category))
            {
                var categoryStr = lang._p(StringTable.CONTEXT_NODE_CATEGORY, doc.category);
                str += lang._(EDITOR_NODE_DOC_CATEGORY_TEMPLATE, categoryStr); 
            }
            // 过期节点
            if (doc.obsoleteMsg != null)
            {
                var obsoleteMsgStr = lang._(doc.obsoleteMsg);
                str += lang._(EDITOR_NODE_DOC_OBSOLETE_TEMPLATE, obsoleteMsgStr); 
            }
            // 说明
            var summaryStr = editor.getNodeDocParticularString(StringTable.CONTEXT_NODE_DOC_SUMMARY, doc.summaryString);
            str += lang._p(StringTable.CONTEXT_NODE_DOC_SUMMARY, EDITOR_NODE_DOC_SUMMARY_TEMPLATE, summaryStr); 

            // 参数文档
            str += lang._(EDITOR_NODE_DOC_PARAMETER_TITLE);
            if (doc.inputs.Length > 0)
            {
                str += lang._(EDITOR_NODE_DOC_INPUT_PARAMETERS);
                str += GetParametersStr(doc.inputs);
            }
            if (doc.outputs.Length > 0)
            {
                str += lang._(EDITOR_NODE_DOC_OUTPUT_PARAMETERS);
                str += GetParametersStr(doc.outputs);
            }

            // 示例
            if (!string.IsNullOrEmpty(doc.exampleString))
            {
                str += lang._(EDITOR_NODE_DOC_EXAMPLE_TITLE);
                var exampleStr = editor.getNodeDocParticularString(StringTable.CONTEXT_NODE_DOC_EXAMPLE, doc.exampleString);
                str += lang._p(StringTable.CONTEXT_NODE_DOC_EXAMPLE, EDITOR_NODE_DOC_EXAMPLE_TEMPLATE, exampleStr);
            }
            _ui.showNodeDocument(str);

            

            string GetParameterStr(ActionParameterComment item)
            {
                var dotIndex = item.type.LastIndexOf(".");
                string type = dotIndex > 0 ? item.type.Substring(dotIndex + 1) : item.type;
                type = type.Replace("<", "&lt;").Replace(">", "&gt;");
                string displayName = lang._p(StringTable.CONTEXT_NODE_PARAM_NAME, item.displayName ?? item.name);
                string summary = editor.getNodeDocParticularString(StringTable.CONTEXT_NODE_DOC_PORT_SUMMARY, item.summaryString);
                return lang._(EDITOR_NODE_DOC_PARAMETER_TEMPLATE, displayName, type, item.isArray, item.isParams, summary);
            }
            string GetParametersStr(ActionParameterComment[] items)
            {
                string str = "";
                str += "<ul>";
                foreach (var item in items)
                {
                    str += GetParameterStr(item);
                }
                str += "</ul>";
                return str;
            }
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
            graph = effect.graph;

            _ui.clearGraph();
            await ApplyGraph(effect.graph);

            updateEffectProperties();
        }
        #endregion

        #region 更新节点图
        /// <summary>
        /// 为动作节点以及其所有子节点创建动作节点图
        /// </summary>
        /// <param name="actionNode"></param>
        private async Task ApplyGraph(ActionGraph graph)
        {
            foreach (var node in graph.nodes)
            {
                // 先更新端口可能会变动的节点。
                updateNodeWithVariablePorts(node);
                await applyNode(node);
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
        #endregion

        #region 创建节点UI
        private void registerNode(Node node)
        {
            validateNodeInputDefaultValues(node);
            _actionNodeList.Add(node);
        }
        private Task<ActionNodeItem> applyNode(Node node)
        {
            registerNode(node);
            return createNodeUI4Node(node);
        }
        private async Task<ActionNodeItem> createNodeUI4Node(Node node)
        {
            if (node is ActionNode action)
            {
                return await createNodeUI4ActionNode(action);
            }
            else if (node is ActiveEntryNode activeEntry)
            {
                return createNodeUI4ActiveEntryNode(activeEntry);
            }
            else if (node is PassiveEntryNode passiveEntry)
            {
                return createNodeUI4PassiveEntryNode(passiveEntry);
            }
            else if (node is GeneratedActionEntryNode genEntry)
            {
                return createNodeUI4GeneratedActionEntryNode(genEntry);
            }
            else if (node is GeneratedActionReturnNode genReturn)
            {
                return createNodeUI4GeneratedActionReturnNode(genReturn);
            }
            else if (node is AuraEntryNode auraEntry)
            {
                return createNodeUI4AuraEntryNode(auraEntry);
            }
            else if (node is EventRuleEntryNode eventRuleEntry)
            {
                return createNodeUI4EventRuleEntryNode(eventRuleEntry);
            }
            else if (node is GameRuleEntryNode gameRuleEntryNode)
            {
                return createNodeUI4GameRuleEntryNode(gameRuleEntryNode);
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
            if (actionDefine != null)
            {
                int nodeKey = CanNodeOpenDocument(actionNode) ? actionNode.defineRef.defineId : -1;
                bool visualEffect = game.anims.canActionAddEffect(actionNode.defineRef);
                var inputViewDatas = getNodeInputViewData(actionNode);
                var outputViewDatas = getNodeOutputViewData(actionNode);
                actionNodeItem = _ui.createActionNode(
                    actionNode.posX,
                    actionNode.posY,
                    getActionNodeName(actionNode),
                    inputViewDatas,
                    outputViewDatas,
                    getActionNodeObsoleteMsg(actionNode),
                    nodeKey,
                    visualEffect);
                var inputPorts = actionNode.inputPorts;
                var count = inputPorts.Count();

                // 更新卡牌定义图片。
                for (int i = 0; i < count; i++)
                {
                    var inputPort = actionNode.getInputPortAt(i);
                    var valueInfo = getDefaultValueInfoFromInput(inputPort);
                    if (valueInfo.type.type == ActionNodeValueType.cardReference)
                    {
                        var name = inputPort.name;
                        var paramIndex = inputPort is ValueInput vi ? vi.paramIndex : -1;
                        var defaultValue = actionNode.getInputDefaultValue(name, paramIndex);
                        await updateCardDefineDefaultValue(actionNodeItem, i, defaultValue);
                    }
                }
            }
            else
            {
                var actionPortInfo = new PortItemInfo(string.Empty, game.editor.getColorByType(typeof(ActionNode)), true);
                var inputViewDatas = new InputPortInfo[] { new InputPortInfo(actionPortInfo, default, true, false) };
                var outputViewDatas = new PortItemInfo[] { actionPortInfo };
                actionNodeItem = _ui.createActionNode(
                    actionNode.posX,
                    actionNode.posY,
                    game.langManager.getString(StringTable.EDITOR_NODE_NAME_UNKNOWN),
                    inputViewDatas,
                    outputViewDatas);
            }
            afterCreateNodeItem(actionNode, actionNodeItem);
            return actionNodeItem;
        }
        private ActionNodeItem createNodeUI4ActiveEntryNode(ActiveEntryNode activeEntry)
        {
            string nodeName = game.lang._(EDITOR_NODE_NAME_ACTIVE_ENTRY_NODE);

            var targetTypes = getTranslatedTargetCheckerTypes();
            var checkers = getActiveEntryTargetCheckerViewData(activeEntry);

            var inputViewData = getNodeInputViewData(activeEntry);
            var outputViewData = getNodeOutputViewData(activeEntry);
            var actionNodeItem = _ui.createActiveEntryNode(activeEntry.posX, activeEntry.posY, nodeName,
                inputViewData,
                targetTypes, checkers,
                outputViewData);
            var tagNames = _activeEffectTagNames.Select(n => game.lang._p(StringTable.CONTEXT_EFFECT_TAG, n)).ToArray();
            var properties = ValueInputFieldProperties.effectTag(tagNames, _activeEffectTags);
            actionNodeItem.initNodeInputFields(properties);

            afterCreateNodeItem(activeEntry, actionNodeItem);
            return actionNodeItem;
        }
        private ActionNodeItem createNodeUI4PassiveEntryNode(PassiveEntryNode passiveEntry)
        {
            string nodeName;
            if (passiveEntry is EventEntryNode eventEntry)
            {
                var triggerTime = eventEntry.triggerTime;
                nodeName = game.eventManager.translateEventTriggerTimeNameForEditor(triggerTime);
            }
            else
            {
                nodeName = game.lang._(EDITOR_NODE_NAME_PASSIVE_ENTRY_NODE);
            }

            var inputViewData = getNodeInputViewData(passiveEntry);
            var outputViewData = getNodeOutputViewData(passiveEntry);
            var actionNodeItem = _ui.createActionNode(passiveEntry.posX, passiveEntry.posY,
                nodeName,
                inputViewData,
                outputViewData,
                visualEffect: true);
            var tagNames = _passiveEffectTagNames.Select(n => game.lang._p(StringTable.CONTEXT_EFFECT_TAG, n)).ToArray();
            var properties = ValueInputFieldProperties.effectTag(tagNames, _passiveEffectTags);
            actionNodeItem.initNodeInputFields(properties);
            actionNodeItem.switchToEventEntryNode();

            afterCreateNodeItem(passiveEntry, actionNodeItem);
            return actionNodeItem;
        }
        private ActionNodeItem createNodeUI4GeneratedActionEntryNode(GeneratedActionEntryNode genEntry)
        {
            string nodeName = game.lang._(StringTable.EDITOR_NODE_NAME_INPUT);

            var inputViewData = getNodeInputViewData(genEntry);
            var outputViewData = getNodeOutputViewData(genEntry);
            var actionNodeItem = _ui.createActionNode(genEntry.posX, genEntry.posY, nodeName, inputViewData, outputViewData);
            afterCreateNodeItem(genEntry, actionNodeItem);
            return actionNodeItem;
        }
        private ActionNodeItem createNodeUI4GeneratedActionReturnNode(GeneratedActionReturnNode genReturn)
        {
            string nodeName = game.lang._(StringTable.EDITOR_NODE_NAME_OUTPUT);

            var inputViewData = getNodeInputViewData(genReturn);
            var outputViewData = getNodeOutputViewData(genReturn);
            var actionNodeItem = _ui.createActionNode(genReturn.posX, genReturn.posY, nodeName, inputViewData, outputViewData);
            afterCreateNodeItem(genReturn, actionNodeItem);
            return actionNodeItem;
        }
        private ActionNodeItem createNodeUI4AuraEntryNode(AuraEntryNode auraEntry)
        {
            string nodeName = EDITOR_NODE_NAME_AURA_ENTRY_NODE;
            nodeName = game.lang._(nodeName);

            var inputViewData = getNodeInputViewData(auraEntry);
            var outputViewData = getNodeOutputViewData(auraEntry);
            var actionNodeItem = _ui.createActionNode(auraEntry.posX, auraEntry.posY,
                nodeName,
                inputViewData,
                outputViewData, 
                visualEffect: false);
            actionNodeItem.switchToEventEntryNode();

            afterCreateNodeItem(auraEntry, actionNodeItem);
            return actionNodeItem;
        }
        private ActionNodeItem createNodeUI4EventRuleEntryNode(EventRuleEntryNode eventEntry)
        {
            var triggerTime = eventEntry.triggerTime;
            string nodeName = game.eventManager.translateEventTriggerTimeNameForEditor(triggerTime);

            var inputViewData = getNodeInputViewData(eventEntry);
            var outputViewData = getNodeOutputViewData(eventEntry);
            var actionNodeItem = _ui.createActionNode(eventEntry.posX, eventEntry.posY,
                nodeName,
                inputViewData,
                outputViewData,
                visualEffect: false);
            actionNodeItem.switchToActionNode();

            afterCreateNodeItem(eventEntry, actionNodeItem);
            return actionNodeItem;
        }
        private ActionNodeItem createNodeUI4GameRuleEntryNode(GameRuleEntryNode entry)
        {
            var logicType = entry.logicType;
            string nodeName = game.editor.getRuleLogicTypeEditorName(logicType);

            var inputViewData = getNodeInputViewData(entry);
            var outputViewData = getNodeOutputViewData(entry);
            var actionNodeItem = _ui.createActionNode(entry.posX, entry.posY,
                nodeName,
                inputViewData,
                outputViewData,
                visualEffect: false);
            actionNodeItem.switchToActionNode();

            afterCreateNodeItem(entry, actionNodeItem);
            return actionNodeItem;
        }
        private void afterCreateNodeItem(Node node, ActionNodeItem actionNodeItem)
        {
            _actionNodeItemList.Add(actionNodeItem);
            updateUINodeItemInputPorts(node);
            updateNodePortModes(node, _connectingPort);
        }
        #endregion

        #region 移除节点UI
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
        #endregion

        #region 获取节点UI
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

        #region 获取节点
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
                return translateActionName(StringTable.EDITOR_NODE_NAME_UNKNOWN);
            }
            else
            {
                return translateActionName(define);
            }
        }

        private string getActionNodeObsoleteMsg(ActionNode actionNode)
        {
            ActionDefine actionDefine = getActionDefine(actionNode.defineRef);
            if (actionDefine.obsoleteMsg == null) return null;
            if (actionDefine.obsoleteMsg == "") return "";
            return game.lang._(actionDefine.obsoleteMsg);
        }
        #endregion

        #region 节点操作
        private void selectCardForNodeInputDefaultValue(CardDefine cardDefine)
        {
            if (_curEditNode == null)
            {
                return;
            }
            var defineRef = cardDefine.getDefineRef();
            var name = _curEditNodeInputDefaultValueName;
            var paramIndex = _curEditNodeInputDefaultValueParamIndex;
            var itemIndexes = _curEditNodeInputDefaultValueItemIndexes;

            var defValue = _curEditNode.getInputDefaultValue(name, paramIndex);
            if (defValue.replaceListElementByIndexes(itemIndexes, defineRef, out var newDefValue))
            {
                setNodeDefaultValue(_curEditNode, name, paramIndex, newDefValue);
                // 更新UI.
                updateUINodeItemInputPorts(_curEditNode);
                addRecordBreakpoint();
            }
        }
        private void updateNodeWithVariablePorts(Node node)
        {
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
                var triggerTime = eventEntryNode.triggerTime;
                var eventDefineRef = triggerTime.defineRef;
                var eventDefine = getEventDefine(eventDefineRef);
                eventEntryNode.setEventDefine(eventDefine, triggerTime.type);
                eventEntryNode.Define();
            }
            else if (node is GameRuleEntryNode gameRuleEntryNode)
            {
                gameRuleEntryNode.Define();
            }
            else if (node is EventRuleEntryNode eventRuleEntryNode)
            {
                var triggerTime = eventRuleEntryNode.triggerTime;
                var eventDefineRef = triggerTime.defineRef;
                var eventDefine = getEventDefine(eventDefineRef);
                eventRuleEntryNode.setEventDefine(eventDefine, triggerTime.type);
                eventRuleEntryNode.Define();
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
            // 更新变长参数。
            updateInputParamsPort(inputPort);
            // 移除端口的输入默认值。
            removeInputPortDefaultValue(inputPort);
            // 更新事件变量节点的事件变量名。
            checkEventVariableNodeItemVarNames(inputNodeItem, inputPort.node);
            // 更新节点的输入端口列表
            updateUINodeItemInputPorts(inputPort.node);
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
                updateUINodeItemInputPorts(inputPort.node);
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


        #region 节点信息
        private ActionNodeValueInfo getDefaultValueInfoFromInput(IPort port)
        {
            var paramIndex = port is ValueInput vi ? vi.paramIndex : -1;
            var node = port.node;
            var defaultValue = node.getInputDefaultValue(port.name, paramIndex);
            return getValueInfoFromPortDefine(port.define, defaultValue);
        }
        private ActionNodeValueInfo getValueInfoFromPortDefine(PortDefine portDefine, object value)
        {
            var name = translatePortName(portDefine);
            Type type = portDefine.type;

            if (type.IsEnum)
            {
                var assignType = type.getNodeValueType();
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
                return new ActionNodeValueInfo(name, portDefine.name, assignType, value, optionalNames, optionalValues);
            }
            else
            {
                var assignType = type.getNodeValueType();
                var valueType = assignType.getAssignableValueTypeByValue(value);
                return new ActionNodeValueInfo(name, portDefine.name, valueType, value, assignType);
            }
        }
        private bool shouldInputPortShowPort(IPort inputPort)
        {
            return inputPort is not ValueInput valueInput || !valueInput.define.isConst;
        }
        private bool shouldInputPortShowDefaultValue(IPort inputPort)
        {
            if (inputPort is not ValueInput valueInput)
                return false;
            if (valueInput.getConnectedOutputPort() != null)
                return false;
            var defaultValue = inputPort.node.getInputDefaultValue(valueInput.name, valueInput.paramIndex);
            if (!inputPort.define.isConst && defaultValue == null)
                return false;
            return true;
        }
        private InputPortInfo[] getNodeInputViewData(Node node)
        {
            if (node is ActiveEntryNode activeEntry)
            {
                return getActiveEntryInputViewData(activeEntry);
            }
            else if (node is GeneratedActionEntryNode)
            {
                return Array.Empty<InputPortInfo>();
            }
            else if (node is GeneratedActionReturnNode genReturn)
            {
                return getActionDefineReturnNodeInputViewData(genReturn);
            }
            List<InputPortInfo> viewDataList = new List<InputPortInfo>();
            //值输入
            foreach (var inputPort in node.inputPorts)
            {
                // 翻译输入节点
                var inputName = translatePortName(inputPort);
                var portInfo = new PortItemInfo(inputName, game.editor.getColorByType(inputPort.define.type), inputPort.GetPortType() == PortType.Control);
                var defaultValue = getDefaultValueInfoFromInput(inputPort);
                var showPort = shouldInputPortShowPort(inputPort);
                var showDefaultValue = shouldInputPortShowDefaultValue(inputPort);
                viewDataList.Add(new InputPortInfo(portInfo, defaultValue, showPort, showDefaultValue));
            }
            return viewDataList.ToArray();
        }
        private PortItemInfo[] getNodeOutputViewData(Node node)
        {
            if (node is GeneratedActionEntryNode genEntry)
            {
                return getActionDefineEntryNodeOutputViewData(genEntry);
            }
            else if (node is GeneratedActionReturnNode)
            {
                return Array.Empty<PortItemInfo>();
            }
            else if (node is EventEntryNode eventEntry)
            {
                return getEventEntryNodeOutputViewData(eventEntry);
            }
            else if (node is EventRuleEntryNode eventRuleEntry)
            {
                return getEventEntryNodeOutputViewData(eventRuleEntry);
            }
            List<PortItemInfo> outputList = new List<PortItemInfo>();
            foreach (var output in node.outputPorts)
            {
                // 翻译输出节点
                var outputName = translatePortName(output);
                outputList.Add(new PortItemInfo(outputName, game.editor.getColorByType(output.define.type), output.GetPortType() == PortType.Control));
            }
            return outputList.ToArray();
        }
        private PortItemInfo[] getEventEntryNodeOutputViewData(Node node)
        {
            List<PortItemInfo> outputList = new List<PortItemInfo>();
            foreach (var output in node.outputPorts)
            {
                // 翻译输出节点
                var outputName = translateEventVarName(output?.define?.getDisplayName());
                outputList.Add(new PortItemInfo(outputName, game.editor.getColorByType(output.define.type), output.GetPortType() == PortType.Control));
            }
            return outputList.ToArray();
        }
        #endregion

        #region 编辑器环境
        private ICardPoolReference[] getCardPoolEnv() => game.editor.getEnvCardPoolRefs();
        private ICardPoolReference getCardPoolReference(long id) => game.editor.getCardPoolReference(id);
        private INodeDefiner getNodeDefiner() => game.card.getNodeDefiner(getCardPoolEnv());
        private EventDefine getEventDefine(EventReference eventRef, bool includeHidden = true)
        {
            if (eventRef == null)
                return null;
            return game.card.getEventDefine(getCardPoolReference(eventRef.cardPoolId), eventRef.eventName, includeHidden);
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
            _generatedRuleLogic = null;
            graph = null;
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

        #region 输入默认值
        private void removeInputPortDefaultValue(IPort port)
        {
            if (port is not ValueInput vi)
                return;
            var beforeValue = port.node.getInputDefaultValue(port.name, vi.paramIndex);
            if (port.node.removeInputDefaultValue(port.name, vi.paramIndex) > 0)
            {
                addOperationRecord(new ChangeNodeDefaultValueOperation(this, port.node, port.name, vi.paramIndex, beforeValue, null));
            }
        }
        private async void updateUINodeItemInputPorts(Node node)
        {
            var item = getActionNodeItem(node);
            var inputViewDatas = getNodeInputViewData(node);
            item.setInputs(inputViewDatas);

            // 更新卡牌引用。
            foreach (var defValue in node.inputDefaultValues)
            {
                await updateCardDefineDefaultValue(item, node, defValue.name, defValue.paramIndex, defValue.value);
            }
        }
        private Task updateCardDefineDefaultValue(ActionNodeItem item, Node node, string name, int paramIndex, object value)
        {
            var inputPort = paramIndex < 0 ? node.getInputPort(name) : node.getParamInputPort(name, paramIndex);
            if (inputPort == null)
                return Task.CompletedTask;
            var portIndex = inputPort.GetPortIndex();
            return updateCardDefineDefaultValue(item, portIndex, value);
        }
        private async Task updateCardDefineDefaultValue(ActionNodeItem item, int portIndex, object value)
        {
            ValueInputField cardInputField = item.getInputDefaultValueInputField(portIndex);
            var env = getCardPoolEnv();
            await cardInputField.setInputFieldCardByValue(value, env, game);
        }
        private bool tryGetValueTypeDefaultValue(ActionNodeValueType valueType, out object value)
        {
            value = null;
            switch (valueType)
            {
                case ActionNodeValueType.cardDefinePropertyNameGet:
                    value = EditorManager.cardProps.FirstOrDefault(p => p.canDefineGet).value;
                    break;
                case ActionNodeValueType.cardPropertyNameGet:
                    value = EditorManager.cardProps.FirstOrDefault(p => p.canGet).value;
                    break;
                case ActionNodeValueType.cardPropertyNameSet:
                    value = EditorManager.cardProps.FirstOrDefault(p => p.canSet).value;
                    break;
                case ActionNodeValueType.effectTag:
                    value = getAllEffectTagValues()[0];
                    break;
                case ActionNodeValueType.eventReference:
                    value = eventReferences[0];
                    break;
                case ActionNodeValueType.type:
                    value = typeValues[0];
                    break;
                case ActionNodeValueType.cardType:
                    value = cardTypeValues[0];
                    break;
                case ActionNodeValueType.pileName:
                    value = game.editor.getAllPileValues()[0];
                    break;
                case ActionNodeValueType.buffReference:
                    var buffDefine = game.editor.getCurrentBuffDefines().FirstOrDefault();
                    value = buffDefine != null ? new BuffReference(buffDefine.cardPoolId, buffDefine.id) : new BuffReference(0, 0);
                    break;
            }
            return value != null;
        }
        private object validateInputFieldValue(NodeValueType targetValueType, object value, bool forceHasValue)
        {
            if (!targetValueType.isArray())
            {
                // 单个值。
                var actualValueType = ActionNodeValueType.none;
                if (value != null)
                {
                    actualValueType = value.GetType().getNodeValueType().type;
                }
                else if (forceHasValue)
                {
                    // 强制设置一个值。
                    if (tryGetValueTypeDefaultValue(targetValueType.type, out var defValue))
                    {
                        value = defValue;
                    }
                    else
                    {
                        value = targetValueType.getDefaultValue();
                    }
                }
                if (targetValueType.type == ActionNodeValueType.buffReference || actualValueType == ActionNodeValueType.buffReference)
                {
                    if (validateSingleBuffReferenceValue(value, out var newBuffRef, out var needUpdateBuffs))
                        value = newBuffRef;
                    if (needUpdateBuffs)
                    {
                        updateEditorBuffs();
                    }
                }
            }
            else if (value is IList list)
            {
                // 数组。
                for (int i = 0; i < list.Count; i++)
                {
                    var listElement = list[i];
                    if (listElement == null)
                        continue;
                    // 这里的数组可能是object[]，也有可能是单一类型的数组，因此改为获取列表元素的类型。
                    var elementTargetType = listElement.GetType().getNodeValueType();
                    list[i] = validateInputFieldValue(elementTargetType, listElement, false);
                }
                value = list;
            }
            return value;
        }
        #endregion

        #region 事件类型
        /// <summary>
        /// 在“转换事件类型”节点的常量更改后，更新该节点的输出端口连接的节点的事件变量名列表。
        /// </summary>
        /// <param name="node">节点。</param>
        private void checkCastEventTypeNodeOutputs(Node node)
        {
            if (node is ActionNode actionNode && actionNode.defineRef.isBuiltinReference(BuiltinActionDefineID.CastEventType))
            {
                var output = actionNode.getOutputPort<ValueOutput>(EffectEditorLibrary.EVENT_CAST_EVENT_TYPE_NODE_CAST_EVENT_PARAM_KEY);
                foreach (var connection in output.connections)
                {
                    var destNode = connection?.destination?.node;
                    if (destNode == null)
                        continue;
                    checkEventVariableNodeItemVarNames(destNode);
                }
            }
        }
        /// <summary>
        /// 根据设置/获取事件变量节点的连接端口，更新该节点的事件变量名列表。
        /// </summary>
        /// <param name="node">节点。</param>
        private void checkEventVariableNodeItemVarNames(Node node)
        {
            checkEventVariableNodeItemVarNames(getActionNodeItem(node), node);
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
                !action.defineRef.isBuiltinReference(BuiltinActionDefineID.GetEventVariable))
                return;

            var inputPort = node.getInputPort<ValueInput>(EffectEditorLibrary.EVENT_VARIABLE_NODE_EVENT_PARAM_KEY);
            var eventRef = getConnectedEventReference(inputPort);
            var eventDefine = getEventDefine(eventRef);

            var varInfos = eventDefine?.afterVariableInfos;

            string[] eventVarNameValues = varInfos != null ? varInfos.Select(v => v.name).ToArray() : Array.Empty<string>();
            string[] eventVarNameNames = eventVarNameValues.Select(n => translateEventVarName(n)).ToArray();

            var properties = ValueInputFieldProperties.eventVar(eventVarNameNames, eventVarNameValues);
            item.initNodeInputFields(properties);
            var inputViewDatas = getNodeInputViewData(node);
            item.setInputs(inputViewDatas);

            // 如果该节点的该常量的值是null，则设置为名称列表的第一个值。
            var defaultValue = node.getInputDefaultValue<string>(EffectEditorLibrary.EVENT_VARIABLE_NODE_EVENT_NAME_PARAM_KEY);
            if (string.IsNullOrEmpty(defaultValue) && eventVarNameValues.Count() > 0)
            {
                setNodeDefaultValue(node, EffectEditorLibrary.EVENT_VARIABLE_NODE_EVENT_NAME_PARAM_KEY, -1, eventVarNameValues.FirstOrDefault());
            }
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
                if (connectedNode is ActionNode conAction)
                {
                    if (conAction.defineRef.isBuiltinReference(BuiltinActionDefineID.CastEventType))
                    {
                        return conAction.getInputDefaultValue<EventReference>(EffectEditorLibrary.EVENT_CAST_EVENT_TYPE_NODE_TYPE_REF_PARAM_KEY);
                    }
                    else if (conAction.defineRef.isBuiltinReference(BuiltinActionDefineID.ThisEvent))
                    {
                        if (_effect is GeneratedEventEffect eventEffect && eventEffect.eventEntry is EventEntryNode entry)
                        {
                            var triggerTime = entry.triggerTime;
                            return triggerTime.defineRef;
                        }
                    }
                }
                // 其他事件输出。
                var portType = connectedOutput.define?.type;
                if (portType != null)
                {
                    if (portType.IsArray)
                        portType = portType.GetElementType();
                    if (typeof(IEventArg).IsAssignableFrom(portType))
                    {
                        var meta = connectedOutput.define.metas?.OfType<EventTypePortMeta>()?.FirstOrDefault();
                        if (meta != null)
                        {
                            return new EventReference(0, EventHelper.getName(meta.eventType));
                        }
                    }
                }
            }
            return null;
        }
        #endregion
        private void updateInputParamsPort(IPort inputPort)
        {
            if (inputPort.node is ActionNode actionNode)
            {
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
                    updateNodePortModes(actionNode, _connectingPort);
                }
            }
        }
        private string[] getAllEffectTagNames()
        {
            return _activeEffectTagNames.Concat(_passiveEffectTagNames).Select(n => game.lang._p(StringTable.CONTEXT_EFFECT_TAG, n)).ToArray();
        }
        private string[] getAllEffectTagValues()
        {
            return _activeEffectTags.Concat(_passiveEffectTags).ToArray();
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
        public event Action<Node, string, int, int[]> onOpenNodeInputDefaultValueCardDefine;
        #endregion
        #region 属性字段
        
        #region 节点文档
        [TranslateMsg("节点帮助的标题，{0}是节点名称")]
        const string EDITOR_NODE_DOC_TITLE_TEMPLATE = "<h2>{0}</h2>";
        [TranslateMsg("节点参数列表的标题")]
        const string EDITOR_NODE_DOC_PARAMETER_TITLE = "<h3>参数列表</h3>";
        [TranslateMsg("节点输入参数")]
        const string EDITOR_NODE_DOC_INPUT_PARAMETERS = "<p>输入参数</p>";
        [TranslateMsg("节点输出参数")]
        const string EDITOR_NODE_DOC_OUTPUT_PARAMETERS = "<p>输出参数</p>";
        [TranslateMsg("参数模板。{0}: 参数名称, {1}: 参数类型, {2}: 数组, {3}: 参数, {4}: 注释")]
        const string EDITOR_NODE_DOC_PARAMETER_TEMPLATE = "<li>{0}({1}): {4}</li>";
        [TranslateMsg("节点说明模板, {0} 为节点自带说明", StringTable.CONTEXT_NODE_DOC_SUMMARY)]
        const string EDITOR_NODE_DOC_SUMMARY_TEMPLATE = "<p>{0}</p>";
        [TranslateMsg("节点分类模板, {0} 为分类名称")]
        const string EDITOR_NODE_DOC_CATEGORY_TEMPLATE = "<p><b>分类</b>: {0}</p>";
        [TranslateMsg("节点过期模板")]
        const string EDITOR_NODE_DOC_OBSOLETE_TEMPLATE = "<p><b>此节点已被废弃，可能在后续版本中删除。</b>{0}</p>";
        [TranslateMsg("节点示例标题")]
        const string EDITOR_NODE_DOC_EXAMPLE_TITLE = "<h3>示例</h3>";
        [TranslateMsg("节点示例模板, {0} 为节点自带说明", StringTable.CONTEXT_NODE_DOC_EXAMPLE)]
        const string EDITOR_NODE_DOC_EXAMPLE_TEMPLATE = "<p>{0}</p>";
        #endregion


        [TranslateMsg("编辑器-参数名称-目标增益（自定义光环-更新动作-输出值）")]
        public const string EDITOR_PARAM_NAME_TARGET_BUFF = "目标增益";
        [TranslateMsg("编辑器-节点名称-主动效果入口节点")]
        public const string EDITOR_NODE_NAME_ACTIVE_ENTRY_NODE = "主动效果入口";
        [TranslateMsg("编辑器-节点名称-被动效果入口节点")]
        public const string EDITOR_NODE_NAME_PASSIVE_ENTRY_NODE = "被动效果入口";
        [TranslateMsg("编辑器-节点名称-光环效果入口节点")]
        public const string EDITOR_NODE_NAME_AURA_ENTRY_NODE = "光环效果入口";

        [TranslateMsg("编辑器-效果类型-主动效果")]
        public const string EDITOR_ENUM_EFFECT_TYPE_ACTIVE = "主动效果";
        [TranslateMsg("编辑器-效果类型-被动效果")]
        public const string EDITOR_ENUM_EFFECT_TYPE_PASSIVE = "被动效果";
        [TranslateMsg("编辑器-效果类型-事件效果")]
        public const string EDITOR_ENUM_EFFECT_TYPE_EVENT = "事件效果";
        [TranslateMsg("编辑器-效果类型-光环效果")]
        public const string EDITOR_ENUM_EFFECT_TYPE_AURA = "光环效果";

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
        ActionGraph graph;
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
        /// 当前正在编辑的动作节点输入默认值名
        /// </summary>
        string _curEditNodeInputDefaultValueName = string.Empty;
        /// <summary>
        /// 当前正在编辑的动作节点的输入默认值的变长索引
        /// </summary>
        int _curEditNodeInputDefaultValueParamIndex = -1;
        /// <summary>
        /// 当前正在编辑的动作节点常量子项索引
        /// </summary>
        int[] _curEditNodeInputDefaultValueItemIndexes = null;
        string[] _targetCheckerTypes = new string[]
        {
            DEFINE.TARGET_TYPE_CHARACTER,
            DEFINE.TARGET_TYPE_SERVANT
        };

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

        [TranslateMsg]
        const string TITLE_NODE_OBSOLETE = "过时的节点";
        [TranslateMsg("参数0为节点自带的消息")]
        const string MESSAGE_NODE_OBSOLETE = "此节点已被弃用，很可能在之后的版本中删除。\n{0}";

        public static readonly Dictionary<string, string> _nameTextDict = new Dictionary<string, string>()
        {
            { DEFINE.TARGET_TYPE_CHARACTER, StringTable.GAME_TARGET_TYPE_CHARACTER },
            { DEFINE.TARGET_TYPE_SERVANT, StringTable.GAME_TARGET_TYPE_SERVANT },
            
            //节点名称
            { "IntegerOperation", StringTable.EDITOR_NODE_NAME_INTEGER_OPERATION },
            { "LogicOperation", StringTable.EDITOR_NODE_NAME_LOGIC_OPERATION },
            { "Compare", StringTable.EDITOR_NODE_NAME_COMPARE },
            { "IntegerConst", StringTable.EDITOR_NODE_NAME_INTEGER_CONST },
            { "StringConst", StringTable.EDITOR_NODE_NAME_STRING_CONST },
            { "BooleanConst", StringTable.EDITOR_NODE_NAME_BOOLEAN_CONST },
            { "ThisCard", StringTable.EDITOR_NODE_NAME_THIS_CARD },
            { "GetSpellDamage", StringTable.EDITOR_NODE_NAME_GET_SPELL_DAMAGE },
            { "GetOwner", StringTable.EDITOR_NODE_NAME_GET_OWNER },
            { "GetProperty", StringTable.EDITOR_NODE_NAME_GET_PROPERTY },
            { "SetProperty", StringTable.EDITOR_NODE_NAME_SET_PROPERTY },
            { "GetUniquePropertyName", StringTable.EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME },
            { "GetCount", StringTable.EDITOR_NODE_NAME_GET_COUNT },
            // 内置参数
            { "value", StringTable.EDITOR_PARAM_NAME_VALUE },
            { "Value", StringTable.EDITOR_PARAM_NAME_VALUE },
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
            { "TargetBuff", EDITOR_PARAM_NAME_TARGET_BUFF },
            
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


            { "Game.CardDefineSelect.Spell", StringTable.CARD_TYPE_SPELL },
            { "Game.CardDefineSelect.Servant",  StringTable.CARD_TYPE_SERVANT },
            { "Game.CardDefineSelect.Skill", StringTable.CARD_TYPE_SKILL },
            { "Game.CardDefineSelect.Master", StringTable.CARD_TYPE_MASTER },
            { "Game.CardDefineSelect.Item", StringTable.CARD_TYPE_ITEM },

            { "Game.EffectType.Active", EDITOR_ENUM_EFFECT_TYPE_ACTIVE },
            { "Game.EffectType.Passive", EDITOR_ENUM_EFFECT_TYPE_PASSIVE },
            { "Game.EffectType.Event", EDITOR_ENUM_EFFECT_TYPE_EVENT },
            { "Game.EffectType.Aura", EDITOR_ENUM_EFFECT_TYPE_AURA },
        };
        static readonly string[] _activeEffectTags = new string[]
        {
            EFFECTTAG.ACTIVE_PLAYED
        };
        static readonly string[] _activeEffectTagNames = new string[]
        {
            StringTable.EDITOR_EFFECT_TAG_NAME_PLAYED
        };
        static readonly string[] _passiveEffectTags = new string[]
        {
            EFFECTTAG.PASSIVE_LASTWORD,
            EFFECTTAG.EVENT_TRAP,
        };
        static readonly string[] _passiveEffectTagNames = new string[]
        {
            StringTable.EDITOR_EFFECT_TAG_NAME_LASTWORDS,
            StringTable.EDITOR_EFFECT_TAG_NAME_TRAP,
        };
        private long curCardPoolId => game.editor.currentCardPoolId;
        private string[] cardTypeValues;
        private string[] typeValues;
        private EventReference[] eventReferences => scene.editor.eventReferences;
        private string[] eventEditorNames => scene.editor.eventEditorNames;
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(object editObject)
            {
                this.editObject = editObject;
            }
            public object editObject;
        }
        #endregion
    }
}
