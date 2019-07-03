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
        /// <summary>
        /// 将一张不属于任何牌堆的卡牌插入该牌堆。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="position"></param>
        internal void insert(Card card, int position)
        {
            if (card.pile == null)
            {
                card.pile = this;
                cardList.Insert(position, card);
            }
            else
                throw new InvalidOperationException(card + "已经属于Pile[" + card.pile.name + "]");
        }
        /// <summary>
        /// 将位于该牌堆中的一张牌移动到其他的牌堆中。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="targetPile"></param>
        /// <param name="position"></param>
        public void moveTo(Card card, Pile targetPile, int position)
        {
            if (cardList.Remove(card))
            {
                card.pile = targetPile;
                targetPile.cardList.Insert(position, card);
            }
        }
        public void moveTo(Card[] cards, Pile targetPile, int position)
        {
            List<Card> removedCardList = new List<Card>(cards.Length);
            foreach (Card card in cards)
            {
                if (cardList.Remove(card))
                {
                    card.pile = targetPile;
                    removedCardList.Add(card);
                }
            }
            targetPile.cardList.InsertRange(position, removedCardList);
        }
        internal void remove(Card card)
        {
            if (cardList.Remove(card))
            {
                card.pile = null;
            }
        }
        public void shuffle(CardEngine engine)
        {
            for (int i = 0; i < cardList.Count; i++)
            {
                int index = engine.randomInt(i, cardList.Count - 1);
                Card card = cardList[i];
                cardList[i] = cardList[index];
                cardList[index] = card;
            }
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
            get { return cardList[index]; }
            internal set
            {
                cardList[index] = value;
            }
        }
        public Card[] this[int startIndex, int endIndex]
        {
            get
            {
                return cardList.GetRange(startIndex, endIndex - startIndex + 1).ToArray();
            }
            internal set
            {
                for (int i = 0; i < value.Length; i++)
                {
                    cardList[startIndex + i] = value[i];
                }
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
        public static implicit operator Card[] (Pile pile)
        {
            if (pile != null)
                return pile.cardList.ToArray();
            else
                return new Card[0];
        }
    }
    public enum RegionType
    {
        none,
        deck,
        hand
    }
}