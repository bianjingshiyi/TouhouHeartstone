using System.Collections;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.GameEndEventArg;
    public class GameEndAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            if (record == null)
                yield break;
            var winners = arg.winners;

            table.showGameResult(winners.Contains(table.player));
        }
    }
}
