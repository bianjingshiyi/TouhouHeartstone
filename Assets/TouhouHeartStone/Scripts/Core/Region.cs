using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
        public void replace(IEnumerable<Card> originCards, IEnumerable<Card> targetCards)
        {
            Card[] originArray = originCards.ToArray();
            Card[] targetArray = targetCards.ToArray();
            for (int i = 0; i < originArray.Length; i++)
            {
                int index = _cards.IndexOf(originArray[i]);
                _cards[index] = targetArray[i];
            }
        }
        /// <summary>
        /// 洗牌
        /// </summary>
        /// <param name="game">游戏本体，用于提供随机</param>
        public void shuffle(Game game)
        {
            List<Card> shuffleList = new List<Card>(_cards.Count);
            for (int i = 0; i < _cards.Count; i++)
            {
                if (shuffleList.Count < 1)
                    shuffleList.Add(_cards[i]);
                else
                    shuffleList.Insert(game.randomInt(0, shuffleList.Count), _cards[i]);
            }
            _cards = shuffleList;
        }
        public void remove(IEnumerable<Card> cards)
        {
            _cards.RemoveAll(e => { return cards.Contains(e); });
        }
        public Card[] getCards()
        {
            return _cards.ToArray();
        }
        public Card[] getCards(CardInstance[] instances)
        {
            return instances.Select(e => { return _cards.Find(f => { return f.instance == e; }); }).ToArray();
        }
        public void setCards(IEnumerable<Card> cards)
        {
            _cards = cards.ToList();
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