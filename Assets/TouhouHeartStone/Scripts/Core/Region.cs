using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    /// <summary>
    /// Region表示一个容纳卡牌的区域，比如卡组，手牌，战场等等。一个Region中可以包含可枚举数量的卡牌。
    /// 注意，卡片在Region中的顺序代表了它的位置。0是最左边（手牌），0也是最底部（卡组）。
    /// </summary>
    class Region : IEnumerable<Card>
    {
        public void add(IEnumerable<Card> cards)
        {
            cardList.AddRange(cards);
        }
        public void moveTo(IEnumerable<Card> cards, Region targetRegion, bool toTopOrRight)
        {
            cardList.RemoveAll(e => { return cards.Contains(e); });
            if (toTopOrRight)
                targetRegion.cardList.InsertRange(0, cards);
            else
                targetRegion.cardList.AddRange(cards);
        }
        public void replace(IEnumerable<Card> originCards, IEnumerable<Card> targetCards)
        {
            Card[] originArray = originCards.ToArray();
            Card[] targetArray = targetCards.ToArray();
            for (int i = 0; i < originArray.Length; i++)
            {
                int index = cardList.IndexOf(originArray[i]);
                cardList[index] = targetArray[i];
            }
        }
        /// <summary>
        /// 洗牌
        /// </summary>
        /// <param name="game">游戏本体，用于提供随机</param>
        public void shuffle(Game game)
        {
            List<Card> shuffleList = new List<Card>(cardList.Count);
            for (int i = 0; i < cardList.Count; i++)
            {
                if (shuffleList.Count < 1)
                    shuffleList.Add(cardList[i]);
                else
                    shuffleList.Insert(game.randomInt(0, shuffleList.Count), cardList[i]);
            }
            cardList.Clear();
            cardList.AddRange(shuffleList);
        }
        public void remove(IEnumerable<Card> cards)
        {
            cardList.RemoveAll(e => { return cards.Contains(e); });
        }
        public Card[] getCards()
        {
            return cardList.ToArray();
        }
        public Card[] getCards(CardInstance[] instances)
        {
            return instances.Select(e => { return cardList.Find(f => { return f.instance.Equals(e); }); }).ToArray();
        }
        public void setCards(IEnumerable<Card> cards)
        {
            cardList.Clear();
            cardList.AddRange(cards.ToList());
        }
        /// <summary>
        /// 获取左边或者底端的卡牌。注意如果没有足够的卡牌的话，返回的卡牌数量会比参数更少。
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Card[] getBottomOrLeft(int count)
        {
            if (count <= this.count)
            {
                Card[] cards = new Card[count];
                for (int i = 0; i < cards.Length; i++)
                {
                    cards[i] = cardList[i];
                }
                return cards;
            }
            else
                return cardList.ToArray();
        }
        /// <summary>
        /// 获取右边或者顶端的卡牌。注意如果没有足够的卡牌的话，返回的卡牌数量会比参数更少。
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Card[] getTopOrRight(int count)
        {
            if (count <= this.count)
            {
                Card[] cards = new Card[count];
                for (int i = 0; i < cards.Length; i++)
                {
                    cards[i] = cardList[this.count - count + i];
                }
                return cards;
            }
            else
                return cardList.ToArray();
        }
        public int count
        {
            get { return cardList.Count; }
        }
        public Card this[int index]
        {
            get { return cardList[index]; }
        }
        public IEnumerator<Card> GetEnumerator()
        {
            return ((IEnumerable<Card>)cardList).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Card>)cardList).GetEnumerator();
        }
        List<Card> cardList { get; } = new List<Card>();
    }
    public enum RegionType
    {
        none,
        deck,
        hand
    }
}