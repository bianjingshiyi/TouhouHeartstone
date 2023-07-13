using System.Linq;
using TouhouCardEngine;
using UnityEngine.UIElements;

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
        public static EventEntryNode createEventEntryNode(this ActionGraph graph, string eventName, float posX = 0, float posY = 0)
        {
            int id = graph.getUniqueNodeId();
            var node = new EventEntryNode(eventName);
            node.id = id;
            node.posX = posX;
            node.posY = posY;
            node.graph = graph;
            graph.addNode(node);
            graph.updateSize();
            return node;
        }
        public static EventEntryNode createEventEntryNode(this ActionGraph graph, EventDefine eventInfo, float posX = 0, float posY = 0)
        {
            var node = graph.createEventEntryNode(eventInfo?.eventName, posX, posY);
            if (eventInfo != null)
            {
                node.define = eventInfo;
                node.Define();
            }
            return node;
        }
        public static AuraEntryNode createAuraEntryNode(this ActionGraph graph, float posX = 0, float posY = 0)
        {
            int id = graph.getUniqueNodeId();
            var node = new AuraEntryNode();
            node.id = id;
            node.posX = posX;
            node.posY = posY;
            node.graph = graph;
            graph.addNode(node);
            graph.updateSize();
            return node;
        }
        public static Node findNodeById(this ActionGraph graph, int id)
        {
            return graph.nodes.FirstOrDefault(n => n.id == id);
        }
        public static ActiveEntryNode findActiveEntryNode(this ActionGraph graph)
        {
            return graph.nodes.OfType<ActiveEntryNode>().FirstOrDefault();
        }
        public static PassiveEntryNode findPassiveEntryNode(this ActionGraph graph)
        {
            return graph.nodes.OfType<PassiveEntryNode>().FirstOrDefault();
        }
        public static EventEntryNode findEventEntryNode(this ActionGraph graph, string eventName)
        {
            return graph.nodes.OfType<EventEntryNode>().FirstOrDefault(n => n.eventName == eventName);
        }
        public static EventEntryNode findEventEntryNode(this ActionGraph graph)
        {
            return graph.nodes.OfType<EventEntryNode>().FirstOrDefault();
        }
        public static AuraEntryNode findAuraEntryNode(this ActionGraph graph)
        {
            return graph.nodes.OfType<AuraEntryNode>().FirstOrDefault();
        }
    }
}
