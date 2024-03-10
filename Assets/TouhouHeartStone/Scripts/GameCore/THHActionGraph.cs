using System;
using System.Linq;
using System.Numerics;
using TouhouCardEngine;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace TouhouHeartstone
{
    public static class THHActionGraph
    {
        public static Rect evaluateGraphRect(this ActionGraph graph)
        {
            //计算输入的宽度
            float maxX = float.MinValue;
            float minX = float.MaxValue;
            float maxY = float.MinValue;
            float minY = float.MaxValue;
            foreach (var node in graph.nodes)
            {
                maxX = Math.Max(node.posX, maxX);
                minX = Math.Min(node.posX, minX);
                maxY = Math.Max(node.posY, maxY);
                minY = Math.Min(node.posY, minY);
            }
            var width = maxX - minX;
            var height = maxY - minY;
            return new Rect(minX, minY, width, height);
        }
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
        public static GameRuleEntryNode createGameRuleEntryNode(this ActionGraph graph, GameRuleLogicType type, float posX = 0, float posY = 0)
        {
            int id = graph.getUniqueNodeId();
            var node = new GameRuleEntryNode(id, type);
            node.posX = posX;
            node.posY = posY;
            node.graph = graph;
            graph.addNode(node);
            return node;
        }
        public static EventRuleEntryNode createEventRuleEntryNode(this ActionGraph graph, EventTriggerTime triggerTime, float posX = 0, float posY = 0)
        {
            int id = graph.getUniqueNodeId();
            var node = new EventRuleEntryNode(triggerTime);
            node.id = id;
            node.posX = posX;
            node.posY = posY;
            node.graph = graph;
            graph.addNode(node);
            node.setInputDefaultValue(PassiveEntryNode.conditionPortName, true);
            return node;
        }
        public static EventRuleEntryNode createEventRuleEntryNode(this ActionGraph graph, EventDefine eventDefine, EventTriggerTimeType type, float posX = 0, float posY = 0)
        {
            if (eventDefine == null)
                return createEventRuleEntryNode(graph, new EventTriggerTime(new EventReference(0, null), EventTriggerTimeType.Before), posX, posY);
            var triggerTime = new EventTriggerTime(eventDefine, type);
            var node = createEventRuleEntryNode(graph, triggerTime, posX, posY);
            node.setEventDefine(eventDefine, type);
            node.Define();
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
        public static EventRuleEntryNode findEventRuleEntryNode(this ActionGraph graph)
        {
            return graph.nodes.OfType<EventRuleEntryNode>().FirstOrDefault();
        }
    }
}
