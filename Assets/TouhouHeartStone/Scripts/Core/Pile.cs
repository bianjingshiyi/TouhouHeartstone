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
        public string name { get; } = null;
        public void createCard(Game game, Player owner, int[] cardDefs, int position, Visibility visibility)
        {
            createCards(game, this, owner, cardDefs, position, visibility);
        }
        static void createCards(Game game, Pile[] piles, Player[] owners, int[] cardDefs, int position, Visibility visibility)
        {
            Event @event = new Event("CreateCards");
            @event["piles"] = piles;
            @event["owners"] = owners;
            @event["cardDefs"] = cardDefs;
            @event["position"] = position;
            @event["visibility"] = visibility;
            game.doEvent(@event, (g, e) =>
             {
                 piles = e.getVar<Pile[]>("piles");
                 owners = e.getVar<Player[]>("owners");
                 cardDefs = e.getVar<int[]>("cardDefs");
                 position = e.getVar<int>("position");
                 visibility = e.getVar<Visibility>("visibility");
                 if (piles == null || piles.Length == 0)
                     return;
                 if (owners == null || owners.Length == 0)
                     return;
                 if (cardDefs == null || cardDefs.Length == 0)
                     return;
                 foreach (Pile pile in piles)
                 {
                     foreach (Player owner in owners)
                     {
                         var cards = cardDefs.Select(f => { return Card.create(game.cards.createInstance(f), owner); });
                         pile.cardList.InsertRange(position, cards);
                     }
                 }
             });
        }
        /// <summary>
        /// 将一张卡从当前牌堆中移动到另一个牌堆中。如果这张牌或者目标牌堆为空，则什么都不发生。
        /// </summary>
        /// <param name="card">这张牌</param>
        /// <param name="targetPile">目标牌堆</param>
        /// <param name="toTopOrRight">移动到目标牌堆的顶端还是低端</param>
        public void moveCardTo(Game game, Card[] cards, Pile targetPile, int position, Visibility visibility)
        {
            moveCardTo(game, cards, this, targetPile, position, visibility);
        }
        static void moveCardTo(Game game, Card[] cards, Pile pile, Pile targetPile, int position, Visibility visibility)
        {
            Event @event = new Event("MoveCardsTo");
            @event["cards"] = cards;
            @event["pile"] = pile;
            @event["targetPile"] = targetPile;
            @event["position"] = position;
            @event["visibility"] = visibility;
            game.doEvent(@event, (g, e) =>
            {
                cards = e.getVar<Card[]>("cards");
                pile = e.getVar<Pile>("pile");
                targetPile = e.getVar<Pile>("targetPile");
                position = e.getVar<int>("position");
                visibility = e.getVar<Visibility>("visibility");
                if (cards == null || cards.Length == 0)
                    return;
                if (pile == null)
                    return;
                if (targetPile == null)
                    return;
                List<Card> removedCards = new List<Card>();
                foreach (Card card in cards)
                {
                    for (int i = 0; i < pile.cardList.Count; i++)
                    {
                        if (pile.cardList[i] == card)
                        {
                            removedCards.Add(card);
                            pile.cardList.RemoveAt(i);
                            break;
                        }
                    }
                }
                if (removedCards.Count > 0)
                    targetPile.cardList.InsertRange(e.getVar<int>("position"), removedCards);
            });
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
        public void shuffle(Game game)
        {
            shuffle(game, this);
        }
        static void shuffle(Game game, Pile pile)
        {
            Event @event = new Event("Shuffle");
            @event["pile"] = pile;
            game.doEvent(@event, (g, e) =>
            {
                pile = e.getVar<Pile>("pile");
                if (pile == null)
                    return;
                Card[] shuffleArray = new Card[pile.cardList.Count];
                int currentIndex = 0;
                while (pile.cardList.Count > 0)
                {
                    int index = g.randomInt(0, pile.cardList.Count - 1);
                    shuffleArray[currentIndex] = pile.cardList[index];
                    pile.cardList.RemoveAt(index);
                    currentIndex++;
                }
                pile.cardList.AddRange(shuffleArray);
            });
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
        public override string ToString()
        {
            return name + "[" + cardList.Count + "]";
        }
        public static implicit operator Pile[] (Pile pile)
        {
            return new Pile[] { pile };
        }
    }
    public enum RegionType
    {
        none,
        deck,
        hand
    }
}