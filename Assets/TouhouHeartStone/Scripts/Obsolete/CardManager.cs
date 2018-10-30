using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

namespace TouhouHeartstone
{
    class CardsLogic : IEnumerable<CardLogic>
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
        public CardLogic create(CardInstance instance)
        {
            if (instance.instanceId > _lastId)
                _lastId = instance.instanceId;
            CardLogic card = new CardLogic(instance);
            _cardList.Add(card);
            return card;
        }
        int _lastId;
        public IEnumerator<CardLogic> GetEnumerator()
        {
            return ((IEnumerable<CardLogic>)_cardList).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<CardLogic>)_cardList).GetEnumerator();
        }
        List<CardLogic> _cardList = new List<CardLogic>();
    }
}