using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = TurnEndEventDefine;
    class TurnEndAnimation : EventAnimation<Arg>
    {
        public override void postEvent(BattleTableManager table)
        {
            base.postEvent(table);
            var player = eventArg.getVar<Player>(Arg.VAR_PLAYER);
            if (player == table.player)
            {
                table.endLocalTurn();
            }
        }
    }
}
