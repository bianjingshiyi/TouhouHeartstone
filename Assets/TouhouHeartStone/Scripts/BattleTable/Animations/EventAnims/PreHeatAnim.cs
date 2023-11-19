using System.Collections;
using TouhouCardEngine.Histories;
using TouhouHeartstone;

namespace Game.BattleTable
{
    using Arg = THHCard.BeChargedEventArg;
    class PreHeatAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = record.getVar<CardState>(EVENTVAR.CARD).card.id;
            var valueBefore = record.getVar<int>(Arg.VAR_BEFORE_VALUE);
            var value = record.getVar<int>(EVENTVAR.VALUE);


            if (table.tryGetHand(card, out var hand))
            {
                if (valueBefore <= 0 && value > 0)
                {
                    table.sounds.PlaySE("BeCharged");
                }
                hand.card.setPreheat(value > 0);
            }
            yield break;
        }
    }
}