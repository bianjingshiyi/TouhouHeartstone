using System.Collections;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.GameEndEventArg;
    public class GameEndAnimation : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var winners = record.getVar<Player[]>(Arg.VAR_WINNERS);

            table.showGameResult(winners.Contains(table.player));
            yield break;
        }
    }
}
