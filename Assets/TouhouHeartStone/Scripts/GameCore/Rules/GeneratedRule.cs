using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class GeneratedRule : Rule
    {
        public GeneratedRule(IEnumerable<GeneratedRuleLogic> logics)
        {
            this.logics = logics.ToArray();
        }
        public override async Task onGameInit(CardEngine game, GameOption options)
        {
            foreach (var logic in logics.OfType<GeneratedRuleGameLogic>().Where(s => s.type == GameRuleLogicType.GameStart))
            {
                await logic.execute(game);
            }
            foreach (var logic in logics.OfType<GeneratedRuleEventLogic>())
            {
                logic.enable(game);
            }
        }
        public override async Task onGameClose(CardEngine game)
        {
            foreach (var logic in logics.OfType<GeneratedRuleGameLogic>().Where(s => s.type == GameRuleLogicType.GameEnd))
            {
                await logic.execute(game);
            }
            foreach (var logic in logics.OfType<GeneratedRuleEventLogic>())
            {
                logic.disable(game);
            }
        }
        public override async Task onPlayerInit(CardEngine game, Player player)
        {
            foreach (var logic in logics.OfType<GeneratedRuleGameLogic>().Where(s => s.type == GameRuleLogicType.PlayerEnter))
            {
                var arguments = new Dictionary<string, object>()
                {
                    { GameRuleEntryNode.playerPortName, player }
                };
                await logic.execute(game);
            }
        }
        public override async Task onPlayerExit(CardEngine game, Player player)
        {
            foreach (var logic in logics.OfType<GeneratedRuleGameLogic>().Where(s => s.type == GameRuleLogicType.PlayerExit))
            {
                var arguments = new Dictionary<string, object>()
                {
                    { GameRuleEntryNode.playerPortName, player }
                };
                await logic.execute(game);
            }
        }
        public override Task onGameRun(CardEngine game)
        {
            return Task.CompletedTask;
        }
        public GeneratedRuleLogic[] logics;
    }
}
