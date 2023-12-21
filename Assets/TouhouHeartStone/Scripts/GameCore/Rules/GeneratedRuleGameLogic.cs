using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class GeneratedRuleGameLogic : GeneratedRuleLogic
    {
        public GeneratedRuleGameLogic(GameRuleLogicType type)
        {
            this.type = type;
            var graph = new ActionGraph();
            graph.createGameRuleEntryNode(type);
            this.graph = graph;
        }
        internal GeneratedRuleGameLogic(GameRuleLogicType type, ActionGraph graph)
        {
            this.type = type;
            this.graph = graph;
        }
        public Task execute(CardEngine game, Dictionary<string, object> arguments = null)
        {
            var entry = entryNode;
            if (entry != null)
            {
                var flowEnv = new FlowEnv(game, null, null, null, null);
                if (arguments != null)
                {
                    foreach (var arugment in arguments)
                    {
                        flowEnv.SetArgument(arugment.Key, arugment.Value);
                    }
                }
                var flow = new Flow(flowEnv);
                return game.runActions(flow, entry.getActionOutputPort());
            }
            return Task.CompletedTask;
        }
        public override SerializableGeneratedRuleLogic serialize()
        {
            return new SerializableGeneratedRuleGameLogic(this);
        }
        public GameRuleEntryNode entryNode => graph?.nodes?.OfType<GameRuleEntryNode>().FirstOrDefault();
        public GameRuleLogicType type { get; private set; }
    }
    [Serializable]
    public class SerializableGeneratedRuleGameLogic : SerializableGeneratedRuleLogic
    {
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
        public int type;
    }
    public enum GameRuleLogicType
    {
        GameStart,
        GameEnd,
        PlayerEnter,
        PlayerExit,
    }
}
