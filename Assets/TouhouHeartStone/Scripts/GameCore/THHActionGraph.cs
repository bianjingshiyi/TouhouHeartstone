using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public static class THHActionGraph
    {
        public static ActiveEntryNode createActiveEntryNode(this ActionGraph graph, float posX = 0, float posY = 0)
        {
            int id = graph.getUniqueNodeId();
            var node = new ActiveEntryNode(id);
            node.posX = posX;
            node.posY = posY;
            node.graph = graph;
            graph.addNode(node);
            graph.updateSize();
            return node;
        }
        public static PassiveEntryNode createPassiveEntryNode(this ActionGraph graph, float posX = 0, float posY = 0)
        {
            int id = graph.getUniqueNodeId();
            var node = new PassiveEntryNode();
            node.id = id;
            node.posX = posX;
            node.posY = posY;
            node.graph = graph;
            graph.addNode(node);
            graph.updateSize();
            return node;
        }
        public static ActiveEntryNode findActiveEntryNode(this ActionGraph graph)
        {
            return graph.nodes.OfType<ActiveEntryNode>().FirstOrDefault();
        }
    }
}
