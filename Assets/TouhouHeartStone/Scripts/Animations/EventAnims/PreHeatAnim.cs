using BJSYGameCore.UI;
using TouhouHeartstone;
using UI;

namespace Game
{
    class PreHeatAnim : EventAnimation<THHCard.BeChargedEventArg>
    {
        public override bool update(TableManager table, THHCard.BeChargedEventArg eventArg, float time)
        {
            if (table.tryGetHand(eventArg.card, out var hand))
            {
                if (eventArg.value > 0)
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
            return true;
        }
    }
}