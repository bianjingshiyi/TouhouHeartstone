using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    public class Region : MonoBehaviour, IEnumerable<Card>
    {
        public void add(IEnumerable<Card> cards)
        {
            foreach (Card card in cards)
            {
                card.transform.parent = transform;
                _cards.Add(card);
            }
        }
        public void moveTo(IEnumerable<Card> cards, Region targetRegion)
        {
            foreach (Card card in cards)
            {
                _cards.Remove(card);
                card.transform.parent = targetRegion.transform;
                targetRegion._cards.Add(card);
            }
        }
        public void remove(IEnumerable<Card> cards)
        {
            foreach (Card card in cards)
            {
                _cards.Remove(card);
                card.transform.parent = null;
            }
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
        public Card[] cards
        {
            get { return _cards.ToArray(); }
        }
        [SerializeField]
        List<Card> _cards = new List<Card>();
    }
    public enum RegionType
    {
        none,
        deck,
        hand
    }
}