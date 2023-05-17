using System.Collections;
using BJSYGameCore.UI;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game
{
    using Arg = THHCard.BeChargedEventArg;
    class PreHeatAnim : EventAnimation<Arg>
    {
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            var card = arg.card;
            var value = arg.value;

            if (table.tryGetHand(card, out var hand))
            {
                if (value > 0)
                {
                    hand.Card.cardBackEffectImage.display();
                    hand.Card.cardFaceEffectImage.display();
                }
                else
                {
                    hand.Card.cardBackEffectImage.hide();
                    hand.Card.cardFaceEffectImage.hide();
                }
            }
            yield break;
        }
    }
}