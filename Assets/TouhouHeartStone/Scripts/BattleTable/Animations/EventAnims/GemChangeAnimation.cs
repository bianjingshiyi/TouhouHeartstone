using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
namespace Game.BattleTable
{
    using Arg = HearthStoneRule.SetGemEventArg;
    class GemChangeAnimation : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var player = record.getVar<Player>(EVENTVAR.PLAYER);
            var value = record.getVar<int>(EVENTVAR.VALUE);
            var gem = player == table.player ? table.table.SelfGem : table.table.EnemyGem;
            gem.setGem(value);
            yield break;
        }
    }
}
