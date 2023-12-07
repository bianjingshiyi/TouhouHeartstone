using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
namespace Game.BattleTable
{
    using Arg = SetGemEventDefine;
    class GemChangeAnimation : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var value = eventArg.getVar<int>(EVENTVAR.VALUE);
            var gem = player == table.player ? table.table.SelfGem : table.table.EnemyGem;
            gem.setGem(value);
            yield break;
        }
    }
}
