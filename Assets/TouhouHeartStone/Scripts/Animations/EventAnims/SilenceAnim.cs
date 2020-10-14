using TouhouHeartstone;
using UI;
namespace Game
{
    class SilenceAnim : EventAnimation<THHCard.SilenceEventArg>
    {
        public override bool update(TableManager table, THHCard.SilenceEventArg eventArg)
        {
            foreach (var card in eventArg.cards)
            {
                if (table.tryGetServant(card, out var servant))
                {
                    table.setServant(servant, card);
                }
            }
            return true;
        }
    }
}
