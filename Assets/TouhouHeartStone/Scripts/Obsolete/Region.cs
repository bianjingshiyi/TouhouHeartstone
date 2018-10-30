using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    class RegionLogic : IEnumerable<CardLogic>
    {
        public void add(IEnumerable<CardLogic> cards)
        {
            _cards.AddRange(cards);
        }
        public void moveTo(IEnumerable<CardLogic> cards, RegionLogic targetRegion)
        {
            _cards.RemoveAll(e => { return cards.Contains(e); });
            targetRegion._cards.AddRange(cards);
        }
        public void remove(IEnumerable<CardLogic> cards)
        {
            _cards.RemoveAll(e => { return cards.Contains(e); });
        }
        public int count
        {
            get { return _cards.Count; }
        }
        public CardLogic this[int index]
        {
            get { return _cards[index]; }
        }
        public IEnumerator<CardLogic> GetEnumerator()
        {
            return ((IEnumerable<CardLogic>)_cards).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<CardLogic>)_cards).GetEnumerator();
        }
        List<CardLogic> _cards = new List<CardLogic>();
    }
    public enum RegionType
    {
        none,
        deck,
        hand
    }
}