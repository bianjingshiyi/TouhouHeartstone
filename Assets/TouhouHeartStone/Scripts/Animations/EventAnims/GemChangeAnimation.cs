using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
namespace Game
{
    using Arg = HearthStoneRule.SetGemEventArg;
    class GemChangeAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;
            var value = arg.value;
            var gem = player == table.player ? table.ui.SelfGem : table.ui.EnemyGem;
            gem.Text.text = value.ToString();
            yield break;
        }
    }
}
