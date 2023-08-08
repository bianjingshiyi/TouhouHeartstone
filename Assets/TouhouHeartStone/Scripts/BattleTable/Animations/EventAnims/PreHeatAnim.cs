using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = THHCard.BeChargedEventArg;
    class PreHeatAnim : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            var card = arg.card;
            var value = arg.value;

            if (table.tryGetHand(card, out var hand))
            {
                hand.card.setPreheat(value > 0);
            }
            yield break;
        }
    }
}