using UnityEngine;
using Game.UndoRedo;
using TouhouCardEngine;
using System.Linq;
using TouhouHeartstone;
using TouhouCardEngine.Interfaces;

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
                ActionDefineFinder defineFinder = name => _graphCtrl.game.getActionDefine(name, _graphCtrl._curCardPoolId);
                EventTypeInfoFinder eventFinder = EventTypeInfoLoader.FindEventTypeInfo;

                var definer = new THHNodeDefiner(defineFinder, eventFinder);

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
                ActionDefineFinder defineFinder = name => _graphCtrl.game.getActionDefine(name, _graphCtrl._curCardPoolId);
                EventTypeInfoFinder eventFinder = EventTypeInfoLoader.FindEventTypeInfo;

                var definer = new THHNodeDefiner(defineFinder, eventFinder);

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
        class ConnectionOperation : GraphOperation
        {
            public ConnectionOperation(ActionNodeGraphCtrl graphCtrl, Node srcNode, string srcName, Node destNode, string destName) : base(graphCtrl)
            {
                _srcNodeId = srcNode.id;
                _srcName = srcName;
                _destNodeId = destNode.id;
                _destName = destName;
            }

            public override void undo()
            {
                Node outputActionNode = _graphCtrl.graph.findNodeById(_srcNodeId);
                Node inputActionNode = _graphCtrl.graph.findNodeById(_destNodeId);
                IPort outputPort = outputActionNode.getOutputPort(_srcName);
                IPort inputPort = inputActionNode.getInputPort(_destName);
                _graphCtrl.disconnectPorts(outputPort, inputPort, false);
            }
            public override void redo()
            {
                Node outputActionNode = _graphCtrl.graph.findNodeById(_srcNodeId);
                Node inputActionNode = _graphCtrl.graph.findNodeById(_destNodeId);
                IPort outputPort = outputActionNode.getOutputPort(_srcName);
                IPort inputPort = inputActionNode.getInputPort(_destName);
                _graphCtrl.connectPorts(outputPort, inputPort, false);
            }
            private int _srcNodeId;
            private string _srcName;
            private int _destNodeId;
            private string _destName;
        }
        class DisconnectionOperation : GraphOperation
        {
            public DisconnectionOperation(ActionNodeGraphCtrl graphCtrl, Node srcNode, string srcName, Node destNode, string destName) : base(graphCtrl)
            {
                _srcNodeId = srcNode.id;
                _srcName = srcName;
                _destNodeId = destNode.id;
                _destName = destName;
            }

            public override void undo()
            {
                Node outputActionNode = _graphCtrl.graph.findNodeById(_srcNodeId);
                Node inputActionNode = _graphCtrl.graph.findNodeById(_destNodeId);
                IPort outputPort = outputActionNode.getOutputPort(_srcName);
                IPort inputPort = inputActionNode.getInputPort(_destName);
                _graphCtrl.connectPorts(outputPort, inputPort, false);
            }
            public override void redo()
            {
                Node outputActionNode = _graphCtrl.graph.findNodeById(_srcNodeId);
                Node inputActionNode = _graphCtrl.graph.findNodeById(_destNodeId);
                IPort outputPort = outputActionNode.getOutputPort(_srcName);
                IPort inputPort = inputActionNode.getInputPort(_destName);
                _graphCtrl.disconnectPorts(outputPort, inputPort, false);
            }
            private int _srcNodeId;
            private string _srcName;
            private int _destNodeId;
            private string _destName;
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
