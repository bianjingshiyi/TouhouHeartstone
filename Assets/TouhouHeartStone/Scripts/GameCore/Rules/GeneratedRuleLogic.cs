using System;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class GeneratedRuleLogic
    {
        public abstract SerializableGeneratedRuleLogic serialize();
        public ActionGraph graph { get; protected set; }
        public long cardPoolId;
        public int id;
        public string name;
    }
    [Serializable]
    public abstract class SerializableGeneratedRuleLogic
    {
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
        public SerializableActionNodeGraph graph;
        public long cardPoolId;
        public int id;
        public string name;
    }
}
