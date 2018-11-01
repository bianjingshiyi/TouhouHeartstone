using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    class Region : IEnumerable<Card>
    {
        public void add(IEnumerable<Card> cards)
        {
            _cards.AddRange(cards);
        }
        public void moveTo(IEnumerable<Card> cards, Region targetRegion)
        {
            _cards.RemoveAll(e => { return cards.Contains(e); });
            targetRegion._cards.AddRange(cards);
        }
        public void remove(IEnumerable<Card> cards)
        {
            _cards.RemoveAll(e => { return cards.Contains(e); });
        }
        public int count
        {
            get { return _cards.Count; }
        }
        public Card this[int index]
        {
            get { return _cards[index]; }
        }
        public IEnumerator<Card> GetEnumerator()
        {
            return ((IEnumerable<Card>)_cards).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Card>)_cards).GetEnumerator();
        }
        List<Card> _cards = new List<Card>();
    }
    public enum RegionType
    {
        none,
        deck,
        hand
    }
}