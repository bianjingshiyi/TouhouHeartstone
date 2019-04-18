using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    class CardManager : IEnumerable<Card>
    {
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