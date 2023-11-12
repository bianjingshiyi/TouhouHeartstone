using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.TurnEndEventArg;
    class TurnEndAnimation : EventAnimation<Arg>
    {
        public override void postEvent(BattleTableManager table, Arg eventArg)
        {
            base.postEvent(table, eventArg);
            if (eventArg.player == table.player)
            {
                table.endLocalTurn();
            }
        }
    }
}
