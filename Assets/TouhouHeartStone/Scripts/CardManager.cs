using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    public class CardManager : THManager
    {
        public CardInstance createInstance(int cardId)
        {
            _lastId++;
            return new CardInstance(_lastId, cardId);
        }
        public CardInstance[] createInstances(int[] cardsId)
        {
            return cardsId.Select(e =>
            {
                _lastId++;
                return new CardInstance(_lastId, e);
            }).ToArray();
        }
        [SerializeField]
        int _lastId = 0;
        public Card create(CardInstance instance)
        {
            if (instance.instanceId > _lastId)
                _lastId = instance.instanceId;
            Card card = Card.create(instance);
            _cards.Add(card);
            return card;
        }
        [SerializeField]
        List<Card> _cards = new List<Card>();
    }
}