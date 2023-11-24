using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.TurnEndEventArg;
    class TurnEndAnimation : EventAnimation<Arg>
    {
        public override void postEvent(BattleTableManager table)
        {
            base.postEvent(table);
            var player = tEventArg.player;
            if (player == table.player)
            {
                table.endLocalTurn();
            }
        }
    }
}
