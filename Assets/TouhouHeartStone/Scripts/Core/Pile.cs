using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TouhouHeartstone
{
    /// <summary>
    /// Pile（牌堆）表示一个可以容纳卡片的有序集合，比如卡组，手牌，战场等等。一个Region中可以包含可枚举数量的卡牌。
    /// 注意，卡片在Region中的顺序代表了它的位置。0是最左边（手牌），0也是最底部（卡组）。
    /// </summary>
    public class Pile : IEnumerable<Card>
    {
        public Pile(string name)
        {
            this.name = name;
        }
        public Pile(string name, Card[] cards)
        {
            this.name = name;
            foreach (Card card in cards)
            {
                card.pile = this;
            }
            cardList.AddRange(cards);
        }
        public Player owner { get; set; } = null;
        public string name { get; } = null;
        public void add(IEnumerable<Card> cards)
        {
            cardList.AddRange(cards);
        }
        public void moveTo(Card card, Pile targetPile, int position)
        {
            if (cardList.Remove(card))
            {
                targetPile.cardList.Insert(position, card);
            }
        }
        public void shuffle(CardEngine game)
        {
            Card[] shuffleArray = new Card[cardList.Count];
            int currentIndex = 0;
            while (cardList.Count > 0)
            {
                int index = game.randomInt(0, cardList.Count - 1);
                shuffleArray[currentIndex] = cardList[index];
                cardList.RemoveAt(index);
                currentIndex++;
            }
            cardList.AddRange(shuffleArray);
        }
        public Card[] getCards()
        {
            return cardList.ToArray();
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
        public Card top
        {
            get
            {
                if (cardList.Count < 1)
                    return null;
                return cardList[cardList.Count - 1];
            }
        }
        public int indexOf(Card card)
        {
            return cardList.IndexOf(card);
        }
        public int count
        {
            get { return cardList.Count; }
        }
        public Card this[int index]
        {
            get
            {
                if (0 <= index && index < cardList.Count)
                    return cardList[index];
                else
                    return null;
            }
        }
        public Card[] this[int startIndex, int endIndex]
        {
            get
            {
                return cardList.GetRange(startIndex, endIndex - startIndex + 1).ToArray();
            }
        }
        public IEnumerator<Card> GetEnumerator()
        {
            return ((IEnumerable<Card>)cardList).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Card>)cardList).GetEnumerator();
        }
        internal List<Card> cardList { get; } = new List<Card>();
        public override string ToString()
        {
            return name + "[" + cardList.Count + "]";
        }
        public static implicit operator Pile[] (Pile pile)
        {
            if (pile != null)
                return new Pile[] { pile };
            else
                return new Pile[0];
        }
    }
    public enum RegionType
    {
        none,
        deck,
        hand
    }
}