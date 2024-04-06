using MessagePack;
using System;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableGeneratedEntryNode : SerializableNode
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableGeneratedEntryNode() : base()
        {
        }
        public SerializableGeneratedEntryNode(GeneratedActionEntryNode node) : base(node)
        {
        }
        public GeneratedActionEntryNode ToGeneratedEntryNode(ActionGraph graph)
        {
            var node = new GeneratedActionEntryNode()
            {
                id = id,
                posX = posX,
                posY = posY
            };
            InitNode(node, graph);
            return node;
        }
        public override Node ToNode(ActionGraph graph) => ToGeneratedEntryNode(graph);
    }
}
