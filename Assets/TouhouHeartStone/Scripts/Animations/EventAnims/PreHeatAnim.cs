using UI;
using TouhouHeartstone.Builtin;

using BJSYGameCore.UI;
namespace Game
{
    class PreHeatAnim : EventAnimation<Youmu.PreHeatEventArg>
    {
        public override bool update(TableManager table, Youmu.PreHeatEventArg eventArg, float time)
        {
            if (table.tryGetHand(eventArg.card, out var hand))
            {
                if (eventArg.value)
                {
                    hand.Card.CardBackEffectImage.display();
                    hand.Card.CardFaceEffectImage.display();
                }
                else
                {
                    hand.Card.CardBackEffectImage.hide();
                    hand.Card.CardFaceEffectImage.hide();
                }
            }
            return true;
        }
    }
}