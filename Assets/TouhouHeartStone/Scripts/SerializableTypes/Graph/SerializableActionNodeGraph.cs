using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public sealed class SerializableActionNodeGraph
    {
        #region 公有方法
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableActionNodeGraph()
        {
        }
        public SerializableActionNodeGraph(ActionGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            nodes.AddRange(graph.nodes.Select(n => SerializeHelper.ToSerializable(n)));
            connections.AddRange(graph.connections.Select(c => new SerializableConnection(c)));
        }
        public ActionGraph toActionGraph(INodeDefiner nodeDefiner)
        {
            ActionGraph graph = new ActionGraph();
            graph.AddNodes(GetNodes(graph));
            graph.DefineNodes(nodeDefiner);
            graph.AddConnections(GetConnections(graph));
            return graph;
        }
        public Node[] GetNodes(ActionGraph graph)
        {
            return nodes.ConvertAll(n => n.ToNode(graph)).ToArray();
        }
        public NodeConnection[] GetConnections(ActionGraph graph)
        {
            return connections.ConvertAll(n => n.ToNodeConnection(graph)).Where(c => c != null).ToArray();
        }
        #endregion
        #region 属性字段
        [Key(0)]
        public List<SerializableNode> nodes = new List<SerializableNode>();
        [Key(1)]
        public List<SerializableConnection> connections = new List<SerializableConnection>();
        #endregion
    }
}
