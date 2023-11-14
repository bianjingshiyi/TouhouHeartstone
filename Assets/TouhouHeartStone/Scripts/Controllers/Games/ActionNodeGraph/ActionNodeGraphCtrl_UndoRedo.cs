using System;
using System.Linq;
using Game;
using Game.UndoRedo;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UnityEngine;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 公有方法
        public void undo()
        {
            _recordList.undo();
        }
        public void redo()
        {
            _recordList.redo();
        }
        public bool canUndo()
        {
            return _recordList.canUndo();
        }
        public bool canRedo()
        {
            return _recordList.canRedo();
        }
        public void addOperationRecord(OperationRecord record)
        {
            _recordList.addOperation(record);
            callOnEdited();
        }
        public void addRecordBreakpoint()
        {
            _recordList.addBreakpoint();
        }
        #endregion
        #region 嵌套类
        abstract class GraphOperation : OperationRecord
        {
            public GraphOperation(ActionNodeGraphCtrl graphCtrl)
            {
                _graphCtrl = graphCtrl;
            }
            protected ActionNodeGraphCtrl _graphCtrl;
        }
        class CreateNodeOperation : GraphOperation
        {
            public CreateNodeOperation(ActionNodeGraphCtrl graphCtrl, Node node) : base(graphCtrl)
            {
                _nodeID = node.id;
                _seriNode = node.ToSerializableNode();
            }

            public override void undo()
            {
                var node = _graphCtrl.graph.nodes.FirstOrDefault(x => x.id == _nodeID);
                if (node == null)
                    return;
                _graphCtrl.removeActionNode(node);
            }
            public override async void redo()
            {
                var definer = _graphCtrl.game.card.getNodeDefiner();

                var newNode = _seriNode.ToActionNode(_graphCtrl.graph);
                _graphCtrl.graph.addNode(newNode);
                definer.Define(newNode);
                await _graphCtrl.createNodeUI4Node(newNode);
            }
            private int _nodeID;
            private ISerializableNode _seriNode;
        }
        class DeleteNodeOperation : GraphOperation
        {
            public DeleteNodeOperation(ActionNodeGraphCtrl graphCtrl, Node node) : base(graphCtrl)
            {
                _nodeID = node.id;
                _seriNode = node.ToSerializableNode();
            }

            public override async void undo()
            {
                var definer = _graphCtrl.game.card.getNodeDefiner();

                var newNode = _seriNode.ToActionNode(_graphCtrl.graph);
                _graphCtrl.graph.addNode(newNode);
                definer.Define(newNode);
                await _graphCtrl.createNodeUI4Node(newNode);
            }
            public override void redo()
            {
                var node = _graphCtrl.graph.nodes.FirstOrDefault(x => x.id == _nodeID);
                if (node == null)
                    return;
                _graphCtrl.removeActionNode(node);
            }
            private int _nodeID;
            private ISerializableNode _seriNode;
        }
        class ConnectOperation : GraphOperation
        {
            public ConnectOperation(ActionNodeGraphCtrl graphCtrl, Node srcNode, string srcName, Node destNode, string destName, int destParamIndex) : base(graphCtrl)
            {
                _srcNodeId = srcNode.id;
                _srcName = srcName;
                _destNodeId = destNode.id;
                _destName = destName;
                _destParamIndex = destParamIndex;
            }
            public ConnectOperation(ActionNodeGraphCtrl graphCtrl, IPort source, IPort dest) : 
                this(graphCtrl, source.node, source.name, dest.node, dest.name,
                     dest is ValueInput vi ? vi.paramIndex : -1)
            {
            }
            public ConnectOperation(ActionNodeGraphCtrl graphCtrl, NodeConnection connection) : this(graphCtrl, connection.source, connection.destination)
            {
            }

            public override void undo()
            {
                Node outputActionNode = _graphCtrl.graph.findNodeById(_srcNodeId);
                Node inputActionNode = _graphCtrl.graph.findNodeById(_destNodeId);
                IPort outputPort = outputActionNode.getOutputPort(_srcName);
                IPort inputPort = _destParamIndex < 0 ?
                    inputActionNode.getInputPort(_destName) :
                    inputActionNode.getParamInputPort(_destName, _destParamIndex);
                _graphCtrl.disconnectPorts(outputPort, inputPort);
            }
            public override void redo()
            {
                Node outputActionNode = _graphCtrl.graph.findNodeById(_srcNodeId);
                Node inputActionNode = _graphCtrl.graph.findNodeById(_destNodeId);
                IPort outputPort = outputActionNode.getOutputPort(_srcName);
                IPort inputPort;
                if (_destParamIndex < 0)
                {
                    inputPort = inputActionNode.getInputPort(_destName);
                }
                else
                {
                    int length = inputActionNode.getParamInputPorts(_destName).Length;
                    for (int i = length; i <= _destParamIndex; i++)
                    {
                        inputActionNode.extendParamsPort(_destName);
                    }
                    inputPort = inputActionNode.getParamInputPort(_destName, _destParamIndex);
                    _graphCtrl.updateInputParamsPort(inputPort);
                }
                _graphCtrl.connectPorts(outputPort, inputPort);
            }
            private int _srcNodeId;
            private string _srcName;
            private int _destNodeId;
            private string _destName;
            private int _destParamIndex;
        }
        class DisconnectOperation : GraphOperation
        {
            public DisconnectOperation(ActionNodeGraphCtrl graphCtrl, Node srcNode, string srcName, Node destNode, string destName, int destParamIndex) : base(graphCtrl)
            {
                _srcNodeId = srcNode.id;
                _srcName = srcName;
                _destNodeId = destNode.id;
                _destName = destName;
                _destParamIndex = destParamIndex;
            }

            public override void undo()
            {
                Node outputActionNode = _graphCtrl.graph.findNodeById(_srcNodeId);
                Node inputActionNode = _graphCtrl.graph.findNodeById(_destNodeId);
                IPort outputPort = outputActionNode.getOutputPort(_srcName);
                IPort inputPort;
                if (_destParamIndex < 0)
                {
                    inputPort = inputActionNode.getInputPort(_destName);
                }
                else
                {
                    int length = inputActionNode.getParamInputPorts(_destName).Length;
                    for (int i = length; i <= _destParamIndex; i++)
                    {
                        inputActionNode.extendParamsPort(_destName);
                    }
                    inputPort = inputActionNode.getParamInputPort(_destName, _destParamIndex);
                    _graphCtrl.updateInputParamsPort(inputPort);
                }
                _graphCtrl.connectPorts(outputPort, inputPort);
            }
            public override void redo()
            {
                Node outputActionNode = _graphCtrl.graph.findNodeById(_srcNodeId);
                Node inputActionNode = _graphCtrl.graph.findNodeById(_destNodeId);
                IPort outputPort = outputActionNode.getOutputPort(_srcName);
                IPort inputPort = _destParamIndex < 0 ?
                    inputActionNode.getInputPort(_destName) :
                    inputActionNode.getParamInputPort(_destName, _destParamIndex);
                _graphCtrl.disconnectPorts(outputPort, inputPort);
            }
            private int _srcNodeId;
            private string _srcName;
            private int _destNodeId;
            private string _destName;
            private int _destParamIndex;
        }
        class ChangeNodeConstOperation : GraphOperation
        {
            public ChangeNodeConstOperation(ActionNodeGraphCtrl graphCtrl, Node node, string constName, object beforeValue, object afterValue) : base(graphCtrl)
            {
                _nodeId = node.id;
                _constName = constName;
                _beforeValue = recordValue(beforeValue);
                _afterValue = recordValue(afterValue);
            }
            private object recordValue(object originValue)
            {
                var value = originValue;
                if (originValue is ICloneable cloneable)
                {
                    return cloneable.Clone();
                }
                return value;
            }
            private void setValue(object value)
            {
                Node node = _graphCtrl.graph.findNodeById(_nodeId);
                if (value is ICloneable cloneable)
                {
                    node.setConst(_constName, cloneable.Clone());
                }
                else
                {
                    node.setConst(_constName, value);
                }
                _graphCtrl.updateNodeItemConsts(node);
            }
            public override void undo()
            {
                setValue(_beforeValue);
            }
            public override void redo()
            {
                setValue(_afterValue);
            }
            private int _nodeId;
            private string _constName;
            private object _beforeValue;
            private object _afterValue;
        }
        class MoveNodeOperation : GraphOperation
        {
            public MoveNodeOperation(ActionNodeGraphCtrl graphCtrl, int nodeID, Vector2 posBefore, Vector2 posAfter) : base(graphCtrl)
            {
                _nodeID = nodeID;
                _posBefore = posBefore;
                _posAfter = posAfter;
            }

            public override void undo()
            {
                _graphCtrl.moveNode(_nodeID, _posBefore);
            }
            public override void redo()
            {
                _graphCtrl.moveNode(_nodeID, _posAfter);
            }

            private int _nodeID;
            private Vector2 _posBefore;
            private Vector2 _posAfter;
        }
        #endregion
        #region 属性字段
        private OperationRecordList _recordList = new OperationRecordList();
        #endregion
    }
}
