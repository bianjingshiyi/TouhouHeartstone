using System.Collections;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game
{
    using Arg = HearthStoneRule.GameEndEventArg;
    public class GameEndAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            if (record == null)
                yield break;
            var winners = arg.winners;

            var grd = table.ui.ui.getObject<GameResultDialog>();
            grd.ShowGameResult(winners.Contains(table.player));
        }
    }
}
