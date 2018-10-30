using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    static class CardHelper
    {
        public static CardInstance getInstance(this CardLogic card, bool isVisible)
        {
            return new CardInstance(card.instance.instanceId, isVisible ? card.instance.cardId : 0);
        }
        public static CardInstance[] getInstances(this IEnumerable<CardLogic> cards, bool isVisible)
        {
            return cards.Select(e => { return new CardInstance(e.instance.instanceId, isVisible ? e.instance.cardId : 0); }).ToArray();
        }
    }
}