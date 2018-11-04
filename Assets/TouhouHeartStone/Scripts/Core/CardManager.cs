using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
        public Card getCard(int instanceId)
        {
            return _cardList.Find(e => { return e.instance.instanceId == instanceId; });
        }
        public Card[] getCards(int[] instanceId)
        {
            return instanceId.Select(e => { return _cardList.Find(f => { return f.instance.instanceId == e; }); }).ToArray();
        }
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