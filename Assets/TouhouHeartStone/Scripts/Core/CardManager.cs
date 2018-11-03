using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

namespace TouhouHeartstone
{
    class CardManager : IEnumerable<Card>
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
        public Card create(CardInstance instance)
        {
            if (instance.instanceId > _lastId)
                _lastId = instance.instanceId;
            Card card = new Card(instance);
            _cardList.Add(card);
            return card;
        }
        int _lastId;
        public IEnumerator<Card> GetEnumerator()
        {
            return ((IEnumerable<Card>)_cardList).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Card>)_cardList).GetEnumerator();
        }
        List<Card> _cardList = new List<Card>();
    }
}