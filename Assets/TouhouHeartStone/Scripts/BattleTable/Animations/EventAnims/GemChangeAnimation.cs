using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
namespace Game.BattleTable
{
    using Arg = HearthStoneRule.SetGemEventArg;
    class GemChangeAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;
            var value = arg.value;
            var gem = player == table.player ? table.table.SelfGem : table.table.EnemyGem;
            gem.setGem(value);
            yield break;
        }
    }
}
