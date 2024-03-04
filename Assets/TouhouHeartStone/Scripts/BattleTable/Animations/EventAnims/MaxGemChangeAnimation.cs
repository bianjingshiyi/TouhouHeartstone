using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
namespace Game.BattleTable
{
    using Arg = SetMaxGemEventDefine;
    class MaxGemChangeAnimation : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var player = eventArg.getVar<Player>(Arg.VAR_PLAYER);
            var value = eventArg.getVar<int>(Arg.VAR_VALUE);
            var gem = player == table.player ? table.table.SelfGem : table.table.EnemyGem;
            gem.setMaxGem(value);
            yield break;
        }
    }
}
