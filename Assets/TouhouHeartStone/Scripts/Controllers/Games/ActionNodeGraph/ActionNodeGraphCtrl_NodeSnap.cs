using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UI;
using UnityEngine;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 公有方法
        public void showPreviewNode(NodeDefine define)
        {
            if (!_nodeSnapped)
            {
                _nodeSnapped = true;
                if (define.actionDefine != null)
                {
                    var inputInfos = new PortItemInfo[define.actionDefine.inputDefines.Count()];
                    for (int i = 0; i < inputInfos.Length; i++)
                    {
                        var portDefine = define.actionDefine.inputDefines.ElementAtOrDefault(i);
                        var color = game.editor.getColorByType(portDefine?.type);
                        string name = tryTranslateWithDict(portDefine?.getDisplayName() ?? string.Empty);
                        inputInfos[i] = new PortItemInfo(name, color, portDefine?.GetPortType() == PortType.Control);
                    }

                    var constsInfo = new ActionNodeValueInfo[define.actionDefine.constDefines.Count()];
                    for (int i = 0; i < constsInfo.Length; i++)
                    {
                        var portDefine = define.actionDefine.constDefines.ElementAtOrDefault(i);
                        constsInfo[i] = getConstValueInfoFromDefine(portDefine, null);
                    }

                    var outputInfos = new PortItemInfo[define.actionDefine.outputDefines.Count()];
                    for (int i = 0; i < outputInfos.Length; i++)
                    {
                        var portDefine = define.actionDefine.outputDefines.ElementAtOrDefault(i);
                        var color = game.editor.getColorByType(portDefine?.type);
                        string name = tryTranslateWithDict(portDefine?.getDisplayName() ?? string.Empty);
                        outputInfos[i] = new PortItemInfo(name, color, portDefine?.GetPortType() == PortType.Control);
                    }



                    int docKey = CanNodeDefineOpenDocument(define) ? define.actionDefine.defineId : -1;
                    _ui.setPreviewNode(tryTranslateWithDict(define.actionDefine.editorName),
                        inputInfos,
                        constsInfo,
                        outputInfos,
                        define.actionDefine.obsoleteMsg,
                        docKey);
                }
                else if (define.eventDefine != null)
                {
                    // 事件输入
                    var eventDefine = define.eventDefine;
                    var boolColor = game.editor.getColorByType(typeof(bool));
                    var inputInfos = new PortItemInfo[1]
                    {
                        new PortItemInfo(game.lang._(PassiveEntryNode.triggerConditionPortName), boolColor, false)
                    };
                    // 事件输出
                    var outputCount = eventDefine.variableInfos.Count();
                    var outputInfos = new PortItemInfo[1 + outputCount];
                    var controlColor = game.editor.getColorByType(typeof(ActionNode));
                    outputInfos[0] = new PortItemInfo(string.Empty, controlColor, true);
                    for (int i = 0; i < outputCount; i++)
                    {
                        var eventVar = eventDefine.variableInfos[i];
                        var color = game.editor.getColorByType(eventVar?.type);
                        string name = tryTranslateWithDict(eventVar?.name ?? string.Empty);
                        outputInfos[i + 1] = new PortItemInfo(name, color, false);
                    }


                    _ui.setPreviewEventNode(tryTranslateWithDict(EventTypeInfoLoader.translateEventTypeNameForEditor(game.lang, eventDefine.eventName)),
                        inputInfos,
                        outputInfos);
                }
            }
        }

        public void hidePreviewNode()
        {
            _nodeSnapped = false;
            _ui.hidePreviewNode();
            _ui.disconnectPreviewPorts();
        }
        public void showPreviewNodeByNode(Node node)
        {
            if (!_nodeSnapped)
            {
                _nodeSnapped = true;
                if (node is ActiveEntryNode)
                {
                    string nodeName = game.lang._(EDITOR_NODE_NAME_ACTIVE_ENTRY_NODE);
                    _ui.setPreviewNode(tryTranslateWithDict(nodeName),
                        getNodeInputViewData(node),
                        getNodeConstValueInfos(node),
                        getNodeOutputViewData(node));
                }
                else if (node is EventEntryNode eventEntry)
                {
                    var define = eventEntry.define;
                    _ui.setPreviewEventNode(tryTranslateWithDict(EventTypeInfoLoader.translateEventTypeNameForEditor(game.lang, define.eventName)),
                        getNodeInputViewData(node),
                        getNodeOutputViewData(node));
                }
                else if (node is PassiveEntryNode)
                {
                    string nodeName = game.lang._(EDITOR_NODE_NAME_PASSIVE_ENTRY_NODE);
                    _ui.setPreviewNode(tryTranslateWithDict(nodeName),
                        getNodeInputViewData(node),
                        getNodeConstValueInfos(node),
                        getNodeOutputViewData(node));
                }
                else if (node is AuraEntryNode)
                {
                    string nodeName = game.lang._(EDITOR_NODE_NAME_AURA_ENTRY_NODE);
                    _ui.setPreviewNode(tryTranslateWithDict(nodeName),
                        getNodeInputViewData(node),
                        getNodeConstValueInfos(node),
                        getNodeOutputViewData(node));
                }
                else if (node is ActionNode actionNode)
                {
                    var define = actionNode.define;
                    int docKey = CanNodeOpenDocument(actionNode) ? actionNode.defineRef.defineId : -1;
                    _ui.setPreviewNode(tryTranslateWithDict(define.editorName),
                        getNodeInputViewData(node),
                        getNodeConstValueInfos(node),
                        getNodeOutputViewData(node),
                        define.obsoleteMsg,
                        docKey);
                }
            }
        }
        /// <summary>
        /// 将要创建的节点拖拽到节点图上显示预览，检测节点吸附。
        /// </summary>
        /// <param name="nodeDefine"></param>
        /// <param name="position"></param>
        public void dragNodeItemToGraph(NodeDefine nodeDefine, Vector2 position)
        {
            Vector2 previewSize = _ui.getPreviewNodeSize();

            NodeSnapTarget snapResult = null;
            if (config.Config.NodeSnap)
            {
                Rect previewRect = new Rect(position - previewSize * 0.5f, previewSize);

                // 寻找合适的节点。
                var targets = findNodeSnapTargets(previewRect, nodeDefine);

                // 有左侧连接节点。
                snapResult = getNodeSnapResult(position, previewRect, targets);
            }
            showPreviewNode(nodeDefine);
            Vector2 previewPos = position;
            if (snapResult != null)
            {
                previewPos = snapResult.position;
                setNodeSnapConnections(snapResult);
                _currentNodeSnapTarget = snapResult;
            }
            else
            {
                _nodeSnapped = false;
                _ui.disconnectPreviewPorts();
                _currentNodeSnapTarget = null;
            }
            _ui.setPreviewNodePosition(previewPos);
        }
        /// <summary>
        /// 获取当前节点吸附预览的位置。
        /// </summary>
        /// <param name="input">原先的位置。</param>
        /// <returns></returns>
        public Vector2 getNodeSnapPreviewPosition(Vector2 input)
        {
            if (_nodeSnapped)
            {
                return _ui.getPreviewNodePosition();
            }
            return input;
        }
        public void connectSnapConnections(Node node)
        {
            if (_nodeSnapped && _currentNodeSnapTarget != null)
            {
                var result = _currentNodeSnapTarget;
                if (result.target != null)
                {
                    var connection = result.getConnection();
                    var item = result.target.item;
                    var targetNode = getActionNode(item);
                    IPort output, input;
                    if (connection.toOutput)
                    {
                        output = targetNode.getOutputPortAt(connection.targetIndex);
                        input = node.getInputPortAt(connection.sourceIndex);
                    }
                    else
                    {
                        output = node.getOutputPortAt(connection.sourceIndex);
                        input = targetNode.getInputPortAt(connection.targetIndex);
                    }
                    connectPorts(output, input);
                    _recordList.addBreakpoint();
                }
            }
        }
        #endregion
        #region 私有方法
        private void setNodeSnapConnections(NodeSnapTarget result)
        {
            _ui.disconnectPreviewPorts();
            if (result.target != null)
            {
                var connection = result.getConnection();
                var targetItem = result.target.item;
                var targetIndex = connection.targetIndex;
                _ui.connectPreviewPorts(targetItem, targetIndex, connection.sourceIndex, connection.toOutput);
            }
        }

        private void moveExistingNodeItem(ActionNodeItem item)
        {
            NodeSnapTarget result = null;
            var node = getActionNode(item);
            if (config.Config.NodeSnap)
            {
                Vector2 previewSize = _ui.getPreviewNodeSize();
                Vector2 position = new Vector2(item.posX, item.posY);
                Rect previewRect = new Rect(position - previewSize * 0.5f, previewSize);

                // 寻找合适的节点。
                var targets = findNodeSnapTargets(node);

                // 有左侧连接节点。
                result = getNodeSnapResult(position, previewRect, targets);
            }
            if (result != null)
            {
                Vector2 previewPos = result.position;
                showPreviewNodeByNode(node);
                setNodeSnapConnections(result);
                _currentNodeSnapTarget = result;
                _ui.setPreviewNodePosition(previewPos);
            }
            else
            {
                _nodeSnapped = false;
                _ui.disconnectPreviewPorts();
                _currentNodeSnapTarget = null;
                hidePreviewNode();
            }
        }
        private List<NodeSnapTarget> findNodeSnapTargets(Rect previewRect, NodeDefine nodeDefine)
        {
            // 获取此节点的输入和输出。
            PortDefine[] inputDefines = null;
            PortDefine[] outputDefines = null;
            if (nodeDefine.actionDefine != null)
            {
                inputDefines = nodeDefine.actionDefine.inputDefines.ToArray();
                outputDefines = nodeDefine.actionDefine.outputDefines.ToArray();
            }
            else if (nodeDefine.eventDefine != null)
            {
                inputDefines = new PortDefine[] { PassiveEntryNode.triggerConditionPortDefine };
                outputDefines = nodeDefine.eventDefine.variableInfos.Select(v => PortDefine.Value(v.type, v.name)).Prepend(PassiveEntryNode.actionPortDefine).ToArray();
            }
            return findNodeSnapTargets(previewRect, inputDefines, outputDefines);
        }
        private List<NodeSnapTarget> findNodeSnapTargets(Node node)
        {
            var item = getActionNodeItem(node);
            var nodeRect = item.getNodeItemRect();
            var inputDefines = node.inputPorts.Select(p => p.define).ToArray();
            var outputDefines = node.outputPorts.Select(p => p.define).ToArray();
            var connectedInputs = node.inputPorts.Select(p => p.connections.Any()).ToArray();
            var connectedOutputs = node.outputPorts.Select(p => p.connections.Any()).ToArray();
            return findNodeSnapTargets(nodeRect, inputDefines, outputDefines, connectedInputs, connectedOutputs, item);
        }
        /// <summary>
        /// 根据要创建的节点类型获取所有可以吸附的节点。
        /// </summary>
        /// <param name="previewRect"></param>
        /// <param name="inputDefines"></param>
        /// <param name="outputDefines"></param>
        /// <param name="excludingItem"></param>
        /// <returns></returns>
        private List<NodeSnapTarget> findNodeSnapTargets(Rect previewRect, PortDefine[] inputDefines, PortDefine[] outputDefines, bool[] connectedInputPorts = null, bool[] connectedOutputPorts = null, ActionNodeItem excludingItem = null)
        {
            List<NodeSnapTarget> targets = new List<NodeSnapTarget>(); // 符合连接条件的节点。
            Dictionary<ActionNodeItem, NodeSnapNode> nodeItemDict = new Dictionary<ActionNodeItem, NodeSnapNode>(); // 所有被包括进的节点，以及其对应的子树组。

            _ui.updateGraphFreePlaceCheckRegion(excludingItem); ;
            foreach (var nodeItem in _actionNodeItemList)
            {
                if (nodeItem == excludingItem)
                {
                    continue;
                }
                var rect = nodeItem.getNodeItemRect();
                // 这个节点在目标节点左边
                if (rect.xMax <= previewRect.xMin - _minNodeSnapHoriDistance)
                {
                    // 目标节点没有输入。
                    if (inputDefines == null)
                        continue;
                    // 超出横向范围。
                    if (rect.xMax <= previewRect.xMin - _maxNodeSnapHoriDistance)
                        continue;
                    // 超出纵向范围。
                    if (rect.yMin < previewRect.yMin - _maxNodeSnapVertDistance || rect.yMax > previewRect.yMax + _maxNodeSnapVertDistance)
                        continue;

                    // 有有效连接。
                    var node = getActionNode(nodeItem);
                    var validConnections = getSnapValidConnections(node.outputPorts, inputDefines, true, connectedInputPorts);

                    if (validConnections.Length > 0)
                    {
                        float distance = (new Vector2(nodeItem.posX, nodeItem.posY) - previewRect.center).sqrMagnitude;
                        var snapNode = getSnapNodeByItem(nodeItem);
                        foreach (var connection in validConnections)
                        {
                            var target = new NodeSnapTarget(snapNode, connection, false);
                            target.position = getNodeSnapResultPosition(target, previewRect);
                            targets.Add(target);
                        }
                    }
                }
                // 这个节点在目标节点右边
                else if (rect.xMin >= previewRect.xMax + _minNodeSnapHoriDistance)
                {
                    // 目标节点没有输出。
                    if (outputDefines == null)
                        continue;
                    // 超出横向范围。
                    if (rect.xMin >= previewRect.xMax + _maxNodeSnapHoriDistance)
                        continue;
                    // 超出纵向范围。
                    if (rect.yMin < previewRect.yMin - _maxNodeSnapVertDistance || rect.yMax > previewRect.yMax + _maxNodeSnapVertDistance)
                        continue;

                    // 有有效连接。
                    var node = getActionNode(nodeItem);
                    var validConnections = getSnapValidConnections(node.inputPorts, outputDefines, false, connectedOutputPorts);

                    if (validConnections.Length > 0)
                    {
                        float distance = (new Vector2(nodeItem.posX, nodeItem.posY) - previewRect.center).sqrMagnitude;
                        var snapNode = getSnapNodeByItem(nodeItem);
                        foreach (var connection in validConnections)
                        {
                            var target = new NodeSnapTarget(snapNode, connection, true);
                            target.position = getNodeSnapResultPosition(target, previewRect);
                            targets.Add(target);
                        }
                    }
                }
            }
            return targets;


            NodeSnapNode getSnapNodeByItem(ActionNodeItem item)
            {
                if (nodeItemDict.TryGetValue(item, out var node))
                {
                    return node;
                }
                node = new NodeSnapNode(item);
                nodeItemDict.Add(item, node);

                var children = new List<NodeSnapNode>();
                var itemNode = getActionNode(item);
                foreach (var input in itemNode.getInputPorts<ControlInput>())
                {
                    foreach (var connection in input.connections)
                    {
                        var outputItem = getActionNodeItem(connection.source.node);
                        children.Add(getSnapNodeByItem(outputItem));
                    }
                }
                foreach (var input in itemNode.getInputPorts<ValueInput>())
                {
                    var output = input.getConnectedOutputPort();
                    if (output != null)
                    {
                        var outputItem = getActionNodeItem(output.node);
                        children.Add(getSnapNodeByItem(outputItem));
                    }
                }
                node.children = children.ToArray();
                return node;
            }
        }
        private NodeSnapConnection[] getSnapValidConnections(IEnumerable<IPort> targets, PortDefine[] portDefines, bool toOutput, bool[] excludedPorts = null)
        {
            return portDefines.Select((i, index) =>
            {
                if (excludedPorts != null && excludedPorts[index])
                {
                    return new NodeSnapConnection(-1, -1, toOutput, false, false);
                }

                var target = targets.FirstOrDefault(o =>
                {
                    return o.connections.Count() <= 0 && PortDefine.CanTypeConvert(i.type, o.define.type) && i.GetPortType() == o.GetPortType();
                });
                if (i != null && target != null)
                {
                    bool needConversion = i.type != target.define.type;
                    return new NodeSnapConnection(target.GetPortIndex(), index, toOutput, target.GetPortType() == PortType.Control, needConversion);
                }
                return new NodeSnapConnection(-1, -1, toOutput, false, false);
            }).Where(c => c.targetIndex >= 0 && c.sourceIndex >= 0).ToArray();
        }
        /// <summary>
        /// 根据左侧和右侧的节点寻找节点吸附的位置。
        /// </summary>
        /// <param name="previewRect"></param>
        /// <param name="targets"></param>
        /// <param name="rightTargets"></param>
        /// <param name="previewPos"></param>
        /// <returns></returns>
        private NodeSnapTarget getNodeSnapResult(Vector2 targetPosition, Rect previewRect, List<NodeSnapTarget> targets)
        {
            if (targets.Count > 0)
            {
                var final = targets.Where(r =>
                {
                    if (r.isControl() && r.connectToRight) // 控制红线要连接，必须要向左连接。
                    {
                        return false;
                    }
                    return _ui.isNodePositionFree(r.position - previewRect.size * 0.5f, previewRect.size);
                });
                var ordered = final.OrderBy(r => r.isControl() ? 0 : 1);
                ordered = ordered.ThenBy(r => r.needTypeConversion() ? 1 : 0);
                ordered = ordered.ThenBy(r => (r.position - targetPosition).sqrMagnitude);
                return ordered.FirstOrDefault();
            }
            return null;
        }
        /// <summary>
        /// 根据连接两侧的节点获取吸附位置。
        /// </summary>
        /// <param name="previewRect"></param>
        /// <param name="leftNode"></param>
        /// <param name="rightNode"></param>
        /// <returns></returns>
        public Vector2 getNodeSnapResultPosition(NodeSnapTarget result, Rect previewRect)
        {
            Vector2 pos = previewRect.position;
            float snapDistance = _nodeSnapConfirmHoriDistance;
            var node = result.target?.item;

            var rect = node.getNodeItemRect();

            if (result.connectToRight) // 目标节点在预览节点右侧
            {
                pos.x = rect.xMin - snapDistance - previewRect.width * 0.5f;
            }
            else // 目标节点在预览节点左侧
            {
                pos.x = rect.xMax + snapDistance + previewRect.width * 0.5f;
            }

            if (result.isControl()) // 控制连接
            {
                // 顶部对齐
                pos.y = rect.yMax - previewRect.height * 0.5f;
            }
            else // 值连接
            {
                // 居中对齐
                pos.y = rect.center.y;
            }
            var checkRect = previewRect;
            checkRect.center = pos;

            float yMin = previewRect.yMin;
            float yMax = previewRect.yMax;
            foreach (var child in result.target.children)
            {
                var childRect = child.item.getNodeItemRect();
                yMin = Mathf.Min(yMin, childRect.yMin);
                yMax = Mathf.Max(yMax, childRect.yMax);
            }
            var checkPivot = previewRect.center;
            checkPivot.x = pos.x;
            checkPivot.y = Mathf.Clamp(checkPivot.y, yMin, yMax);
            return _ui.findFreeNodeCreatePosition(checkRect, previewRect.center);
        }
        #endregion
        #region 属性字段
        /// <summary>
        /// 节点吸附的最近水平判断距离。
        /// </summary>
        [Header("节点吸附")]
        [SerializeField]
        private float _minNodeSnapHoriDistance = 20;
        /// <summary>
        /// 节点吸附的最远水平判断距离。
        /// </summary>
        [SerializeField]
        private float _maxNodeSnapHoriDistance = 300;
        /// <summary>
        /// 节点吸附的最远垂直判断距离。
        /// </summary>
        [SerializeField]
        private float _maxNodeSnapVertDistance = 300;
        /// <summary>
        /// 节点吸附时的水平间隔。
        /// </summary>
        [SerializeField]
        private float _nodeSnapConfirmHoriDistance = 100;
        bool _nodeSnapped;
        NodeSnapTarget _currentNodeSnapTarget;
        #endregion
    }
    class NodeSnapTarget
    {
        public NodeSnapNode target;
        public NodeSnapConnection connection;
        public bool connectToRight;
        public Vector2 position;
        public NodeSnapTarget(NodeSnapNode target, NodeSnapConnection connection, bool connectToRight)
        {
            this.target = target;
            this.connection = connection;
            this.connectToRight = connectToRight;
        }

        public NodeSnapConnection getConnection()
        {
            return connection;
        }
        public bool isControl()
        {
            return getConnection().isControl;
        }

        public bool needTypeConversion()
        {
            return getConnection().needConversion;
        }
    }
    class NodeSnapNode
    {
        public ActionNodeItem item;
        public NodeSnapNode[] children;

        public NodeSnapNode(ActionNodeItem item) : this(item, null)
        {

        }
        public NodeSnapNode(ActionNodeItem item, NodeSnapNode[] children)
        {
            this.item = item;
            this.children = children;
        }
    }
    struct NodeSnapConnection
    {
        public int targetIndex;
        public int sourceIndex;
        public bool toOutput;
        public bool isControl;
        public bool needConversion;
        public NodeSnapConnection(int targetIndex, int sourceIndex, bool toOutput, bool isControl, bool needConversion)
        {
            this.targetIndex = targetIndex;
            this.sourceIndex = sourceIndex;
            this.toOutput = toOutput;
            this.isControl = isControl;
            this.needConversion = needConversion;
        }
    }
}
