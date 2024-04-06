using MessagePack;
using System;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableGeneratedReturnNode : SerializableNode
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableGeneratedReturnNode() : base()
        {
        }
        public SerializableGeneratedReturnNode(GeneratedActionReturnNode node) : base(node)
        {
        }
        public GeneratedActionReturnNode ToGeneratedEntryNode(ActionGraph graph)
        {
            var node = new GeneratedActionReturnNode()
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
