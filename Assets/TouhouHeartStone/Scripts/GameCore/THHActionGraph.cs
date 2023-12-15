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
            node.setInputDefaultValue(ActiveEntryNode.conditionPortName, true);
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
            node.setInputDefaultValue(PassiveEntryNode.conditionPortName, true);
            return node;
        }
        public static EventEntryNode createEventEntryNode(this ActionGraph graph, EventTriggerTime triggerTime, float posX = 0, float posY = 0)
        {
            int id = graph.getUniqueNodeId();
            var node = new EventEntryNode(triggerTime);
            node.id = id;
            node.posX = posX;
            node.posY = posY;
            node.graph = graph;
            graph.addNode(node);
            node.setInputDefaultValue(PassiveEntryNode.conditionPortName, true);
            return node;
        }
        public static EventEntryNode createEventEntryNode(this ActionGraph graph, EventDefine eventDefine, EventTriggerTimeType type, float posX = 0, float posY = 0)
        {
            if (eventDefine == null)
                return createEventEntryNode(graph, new EventTriggerTime(new EventReference(0, null), EventTriggerTimeType.Before), posX, posY);
            var triggerTime = new EventTriggerTime(eventDefine, type);
            var node = createEventEntryNode(graph, triggerTime, posX, posY);
            node.setEventDefine(eventDefine, type);
            node.Define();
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
        public static EventEntryNode findEventEntryNode(this ActionGraph graph, string triggerTimeName)
        {
            return graph.nodes.OfType<EventEntryNode>().FirstOrDefault(n => n.triggerTime.getTimeName() == triggerTimeName);
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
