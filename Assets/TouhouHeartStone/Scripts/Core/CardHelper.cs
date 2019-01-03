using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    static class CardHelper
    {
        public static CardInstance getInstance(this Card card, bool isVisible)
        {
            return new CardInstance(card.instance.instanceId, isVisible ? card.instance.cardId : 0);
        }
        public static CardInstance[] getInstances(this IEnumerable<Card> cards, bool isVisible)
        {
            return cards.Select(e => { return new CardInstance(e.instance.instanceId, isVisible ? e.instance.cardId : 0); }).ToArray();
        }
        public static CardInstance[] getInstances(this IEnumerable<CardInstance> cards, bool isVisible)
        {
            return isVisible ? cards.ToArray() : cards.Select(e => { return new CardInstance(e.instanceId, 0); }).ToArray();
        }
    }
}