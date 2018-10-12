using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    public class Deck : Region
    {
        public void set(IEnumerable<Card> cards)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                Destroy(_cards[i].gameObject);
            }
            _cards.Clear();
            foreach (Card card in cards)
            {
                card.transform.parent = transform;
            }
            _cards.AddRange(cards);
        }
        public Card[] cards
        {
            get { return _cards.ToArray(); }
        }
        [SerializeField]
        List<Card> _cards = new List<Card>();
    }
}