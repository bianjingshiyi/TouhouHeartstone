using MessagePack;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableGeneratedRuleGameLogic : SerializableGeneratedRuleLogic
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableGeneratedRuleGameLogic() : base()
        {
        }
        public SerializableGeneratedRuleGameLogic(GeneratedRuleGameLogic logic) : base(logic)
        {
            type = (int)logic.type;
        }
        public override GeneratedRuleLogic deserialize(INodeDefiner nodeDefiner)
        {
            var type = (GameRuleLogicType)this.type;
            var graph = this.graph.toActionGraph(nodeDefiner);
            var logic = new GeneratedRuleGameLogic(type, graph);
            initLogic(logic);
            return logic;
        }
        [Key(4)]
        public int type;
    }
}
