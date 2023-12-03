using System.Collections;
using TouhouCardEngine.Histories;
using TouhouHeartstone;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    using Arg = THHCard.BeChargedEventArg;
    class PreHeatAnim : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = tEventArg.getVar<Card>(EVENTVAR.CARD).id;
            var valueBefore = tEventArg.getVar<int>(Arg.VAR_BEFORE_VALUE);
            var value = tEventArg.getVar<int>(EVENTVAR.VALUE);


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