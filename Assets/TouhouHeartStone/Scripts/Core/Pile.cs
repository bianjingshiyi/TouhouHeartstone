using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TouhouHeartstone
{
    class ShuffleEvent : Event
    {
        public ShuffleEvent() : base("Shuffle")
        {
        }
        public override void execute(CardEngine core)
        {

        }
    }
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
            cardList.AddRange(cards);
        }
        public Pile(Player owner, string name)
        {
            this.owner = owner;
            this.name = name;
        }
        public Player owner { get; set; } = null;
        public string name { get; } = null;
        /// <summary>
        /// 将一张卡从当前牌堆中移动到另一个牌堆中。如果这张牌或者目标牌堆为空，则什么都不发生。
        /// </summary>
        /// <param name="card">这张牌</param>
        /// <param name="targetPile">目标牌堆</param>
        /// <param name="toTopOrRight">移动到目标牌堆的顶端还是低端</param>
        public void moveCardTo(Card[] cards, Pile targetPile, int position)
        {
            if (cards == null || cards.Length == 0)
                return;
            if (targetPile == null)
                return;
            List<Card> removedCards = new List<Card>();
            foreach (Card card in cards)
            {
                for (int i = 0; i < cardList.Count; i++)
                {
                    if (cardList[i] == card)
                    {
                        removedCards.Add(card);
                        cardList.RemoveAt(i);
                        break;
                    }
                }
            }
            if (removedCards.Count > 0)
                targetPile.cardList.InsertRange(position, removedCards);
        }
        public Card[] getCards(int count)
        {
            return cardList.GetRange(cardList.Count - count, count).ToArray();
        }
        public void add(IEnumerable<Card> cards)
        {
            cardList.AddRange(cards);
        }
        public void moveTo(Card card, Pile targetRegion, bool toTopOrRight)
        {
            cardList.Remove(card);
            if (toTopOrRight)
                targetRegion.cardList.Add(card);
            else
                targetRegion.cardList.Insert(0, card);
        }
        public void moveTo(IEnumerable<Card> cards, Pile targetRegion, bool toTopOrRight)
        {
            cardList.RemoveAll(e => { return cards.Contains(e); });
            if (toTopOrRight)
                targetRegion.cardList.AddRange(cards);
            else
                targetRegion.cardList.InsertRange(0, cards);
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
        internal void shuffle(CardEngine game)
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
            Card[] cards = new Card[instances.Length];
            for (int i = 0; i < instances.Length; i++)
            {
                cards[i] = cardList.Find(e => { return e.instance.Equals(instances[i]); });
            }
            return cards;
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
        List<Card> cardList { get; } = new List<Card>();
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