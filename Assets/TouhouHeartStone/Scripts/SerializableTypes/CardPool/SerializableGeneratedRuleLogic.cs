using MessagePack;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [Union(0, typeof(SerializableGeneratedRuleGameLogic))]
    [Union(1, typeof(SerializableGeneratedRuleEventLogic))]
    [MessagePackObject]
    [Serializable]
    public abstract class SerializableGeneratedRuleLogic
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableGeneratedRuleLogic()
        {
        }
        public SerializableGeneratedRuleLogic(GeneratedRuleLogic logic)
        {
            graph = new SerializableActionNodeGraph(logic.graph);
            cardPoolId = logic.cardPoolId;
            id = logic.id;
            name = logic.name;
        }
        public abstract GeneratedRuleLogic deserialize(INodeDefiner nodeDefiner);
        protected void initLogic(GeneratedRuleLogic logic)
        {
            logic.cardPoolId = cardPoolId;
            logic.id = id;
            logic.name = name;
        }
        [Key(0)]
        public long cardPoolId;
        [Key(1)]
        public int id;
        [Key(2)]
        public string name;
        [Key(3)]
        public SerializableActionNodeGraph graph;
    }
}
