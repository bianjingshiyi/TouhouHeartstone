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



                    string docKey = CanNodeDefineOpenDocument(define) ? define.actionDefine.defineName : null;
                    _ui.setPreviewNode(tryTranslateWithDict(define.actionDefine.defineName),
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
        /// <summary>
        /// 将要创建的节点拖拽到节点图上显示预览，检测节点吸附。
        /// </summary>
        /// <param name="nodeDefine"></param>
        /// <param name="position"></param>
        public void dragNodeItemToGraph(NodeDefine nodeDefine, Vector2 position)
        {
            Vector2 previewSize = _ui.getPreviewNodeSize();
            Rect previewRect = new Rect(position - previewSize * 0.5f, previewSize);

            // 寻找合适的节点。
            var (leftTargets, rightTargets) = findNodeSnapTargets(previewRect, nodeDefine);

            // 有左侧连接节点。
            var pair = getNodeSnapResult(position, previewRect, leftTargets, rightTargets);

            showPreviewNode(nodeDefine);
            Vector2 previewPos = position;
            if (pair != null)
            {
                previewPos = getNodeSnapResultPosition(pair, previewRect);
                setNodeSnapConnections(pair);
                _currentNodeSnapResult = pair;
            }
            else
            {
                _nodeSnapped = false;
                _ui.disconnectPreviewPorts();
                _currentNodeSnapResult = null;
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
            if (_nodeSnapped && _currentNodeSnapResult != null)
            {
                var result = _currentNodeSnapResult;
                if (result.target != null)
                {
                    var connection = result.getConnection();
                    var item = result.target.node.item;
                    var curNodeItem = getActionNodeItem(node);
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
                }
            }
        }
        #endregion
        #region 私有方法
        private void setNodeSnapConnections(NodeSnapResult result)
        {
            _ui.disconnectPreviewPorts();
            if (result.target != null)
            {
                var connection = result.getConnection();
                var targetItem = result.target.node.item;
                var targetIndex = connection.targetIndex;
                _ui.connectPreviewPorts(targetItem, targetIndex, connection.sourceIndex, connection.toOutput);
            }
        }
        /// <summary>
        /// 根据要创建的节点类型获取所有可以吸附的节点。
        /// </summary>
        /// <param name="previewRect"></param>
        /// <param name="nodeDefine"></param>
        /// <returns></returns>
        private (List<NodeSnapTarget> leftTargets, List<NodeSnapTarget> rightTargets) findNodeSnapTargets(Rect previewRect, NodeDefine nodeDefine)
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
                outputDefines = nodeDefine.eventDefine.variableInfos.Select(v => PortDefine.Value(v.type, v.name)).ToArray();
            }

            List<NodeSnapTarget> leftTargets = new List<NodeSnapTarget>(); // 位于左侧，并且符合连接条件的节点。
            List<NodeSnapTarget> rightTargets = new List<NodeSnapTarget>(); // 位于右侧，并且符合连接条件的节点。
            Dictionary<ActionNodeItem, NodeSnapNode> nodeItemDict = new Dictionary<ActionNodeItem, NodeSnapNode>(); // 所有被包括进的节点，以及其对应的子树组。
            foreach (var nodeItem in _actionNodeItemList)
            {
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
                    var validConnnections = getSnapValidConnections(node.outputPorts, inputDefines, true);

                    if (validConnnections.Length > 0)
                    {
                        float distance = (new Vector2(nodeItem.posX, nodeItem.posY) - previewRect.center).sqrMagnitude;
                        var snapNode = getSnapNodeByItem(nodeItem);
                        leftTargets.Add(new NodeSnapTarget(snapNode, validConnnections, distance));
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
                    var validConnnections = getSnapValidConnections(node.inputPorts, outputDefines, false);

                    if (validConnnections.Length > 0)
                    {
                        float distance = (new Vector2(nodeItem.posX, nodeItem.posY) - previewRect.center).sqrMagnitude;
                        var snapNode = getSnapNodeByItem(nodeItem);
                        rightTargets.Add(new NodeSnapTarget(snapNode, validConnnections, distance));
                    }
                }
            }
            // 按照距离目标位置的距离排序。
            leftTargets.OrderBy(n => n.distance);
            rightTargets.OrderBy(n => n.distance);
            return (leftTargets, rightTargets);


            NodeSnapNode getSnapNodeByItem(ActionNodeItem item)
            {
                if (nodeItemDict.TryGetValue(item, out var node))
                {
                    return node;
                }

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
                node = new NodeSnapNode(item, children.ToArray());
                nodeItemDict.Add(item, node);
                return node;
            }
        }
        private NodeSnapConnection[] getSnapValidConnections(IEnumerable<IPort> targets, PortDefine[] portDefines, bool toOutput)
        {
            return portDefines.Select((i, index) =>
            {
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
        /// <param name="leftTargets"></param>
        /// <param name="rightTargets"></param>
        /// <param name="previewPos"></param>
        /// <returns></returns>
        private NodeSnapResult getNodeSnapResult(Vector2 targetPosition, Rect previewRect, List<NodeSnapTarget> leftTargets, List<NodeSnapTarget> rightTargets)
        {
            List<NodeSnapResult> results = new List<NodeSnapResult>();
            if (leftTargets.Count > 0)//只有左侧有。
            {
                results.AddRange(leftTargets.SelectMany(t => t.connections.Select((c, i) => new NodeSnapResult(t, i, false))));
            }
            if (rightTargets.Count > 0) // 只有右侧连接节点。
            {
                results.AddRange(rightTargets.SelectMany(t => t.connections.Select((c, i) => new NodeSnapResult(t, i, true))));
            }
            if (results.Count > 0)
            {
                var final = results.Where(r =>
                {
                    if (r.isControl() && r.connectToRight) // 控制红线要连接，必须要向左连接。
                    {
                        return false;
                    }
                    var pos = getNodeSnapResultPosition(r, previewRect);
                    return _ui.isNodePositionFree(pos - previewRect.size * 0.5f, previewRect.size);
                });
                var ordered = final.OrderBy(r => r.isControl() ? 0 : 1);
                ordered = ordered.ThenBy(r => r.needTypeConversion() ? 1 : 0);
                ordered = ordered.ThenBy(r => (getNodeSnapResultPosition(r, previewRect) - targetPosition).sqrMagnitude);
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
        public Vector2 getNodeSnapResultPosition(NodeSnapResult result, Rect previewRect)
        {
            Vector2 pos = previewRect.position;
            float snapDistance = _nodeSnapConfirmHoriDistance;
            var node = result.target?.node?.item;

            var rect = node.getNodeItemRect();

            if (!result.isControl() && result.connectToRight) // 特殊：向右连接值
            {
                pos.x = rect.xMin - snapDistance - previewRect.width * 0.5f;
                pos.y = rect.center.y;
                var children = result.target.node.children;
                if (children.Length > 0)
                {
                    // 获取所有目标节点直接子树的节点，然后将位置设置为子树节点的上方或下方。
                    float maxY = pos.y, minY = pos.y;
                    foreach (var childNode in children)
                    {
                        var itemRect = childNode.item.getNodeItemRect();
                        maxY = Mathf.Max(itemRect.yMax, maxY);
                        minY = Mathf.Min(itemRect.yMin, minY);
                    }
                    float centerY = (maxY + minY) / 2;
                    if (previewRect.center.y > centerY)
                    {
                        pos.y = maxY + 50 + previewRect.width / 2;
                    }
                    else
                    {
                        pos.y = minY - 50 - previewRect.width / 2;
                    }
                }
                return pos;
            }

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
            return pos;
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
        private float _maxNodeSnapHoriDistance = 500;
        /// <summary>
        /// 节点吸附的最远垂直判断距离。
        /// </summary>
        [SerializeField]
        private float _maxNodeSnapVertDistance = 500;
        /// <summary>
        /// 节点吸附时的水平间隔。
        /// </summary>
        [SerializeField]
        private float _nodeSnapConfirmHoriDistance = 100;
        bool _nodeSnapped;
        NodeSnapResult _currentNodeSnapResult;
        #endregion
    }
    class NodeSnapResult
    {
        public NodeSnapTarget target;
        public int connectionIndex;
        public bool connectToRight;
        public NodeSnapResult(NodeSnapTarget target, int connectionIndex, bool isRight)
        {
            this.target = target;
            this.connectionIndex = connectionIndex;
            this.connectToRight = isRight;
        }


        public NodeSnapConnection getConnection()
        {
            return target.connections[connectionIndex];
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
    class NodeSnapTarget
    {
        public NodeSnapNode node;
        public NodeSnapConnection[] connections;
        public float distance;
        public NodeSnapTarget(NodeSnapNode item, NodeSnapConnection[] connections, float distance)
        {
            this.node = item;
            this.connections = connections;
            this.distance = distance;
        }
    }
    class NodeSnapNode
    {
        public ActionNodeItem item;
        public NodeSnapNode[] children;

        public NodeSnapNode(ActionNodeItem item) : this(item, new NodeSnapNode[0])
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
