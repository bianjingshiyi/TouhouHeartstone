using MessagePack;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    public class SerializableGameRuleEntryNode : SerializableNode
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableGameRuleEntryNode() : base()
        {
        }
        public SerializableGameRuleEntryNode(GameRuleEntryNode entryNode) : base(entryNode)
        {
            logicType = entryNode.logicType;
        }
        public GameRuleEntryNode ToActionNode(ActionGraph graph)
        {
            var entry = new GameRuleEntryNode(id, logicType)
            {
                posX = posX,
                posY = posY,
            };
            InitNode(entry, graph);
            return entry;
        }
        public override Node ToNode(ActionGraph graph) => ToActionNode(graph);
        [Key(5)]
        public GameRuleLogicType logicType;
    }
}
