using System.Linq;
using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public class Deck
    {
        #region 公有方法
        public Deck(int id, string name, int masterId, IEnumerable<int> cardsId)
        {
            this.id = id;
            this.name = name;
            this.masterId = masterId;
            addCards(cardsId);
        }
        public Deck(int masterId, IEnumerable<int> cardsId) : this(0, null, masterId, cardsId)
        {
        }
        public Deck() : this(0, null, 0, null)
        {
        }
        public void setCards(IEnumerable<int> cardsId)
        {
            clearCards();
            addCards(cardsId);
        }
        public void addCard(int cardId)
        {
            int index = cardList.FindIndex(e => e.id == cardId);
            if (index < 0)
            {
                cardList.Add(new DeckItem() { id = cardId, count = 1 });
            }
            else
            {
                cardList[index].count++;
            }
        }
        public void addCards(IEnumerable<int> cardsId)
        {
            if (cardsId == null)
                return;
            foreach (var cardId in cardsId)
            {
                addCard(cardId);
            }
        }
        public void clearCards()
        {
            cardList.Clear();
        }
        public int getCardCount(int cardId)
        {
            var item = cardList.Find(e => e.id == cardId);
            return item != null ? item.count : 0;
        }
        public int getCardCount()
        {
            return cardList.Sum(item => item.count);
        }
        public int[] toIdArray(bool includeMasterCard = true)
        {
            List<int> list = includeMasterCard ? new List<int>
            {
                masterId
            } : new List<int>();
            foreach (var item in cardList)
            {
                if (item.id == 0)
                    continue;
                if (item.count < 1)
                    continue;
                list.AddRange(Enumerable.Repeat(item.id, item.count));
            }
            return list.ToArray();
        }
        #endregion
        public int id;
        public string name;
        public int masterId;
        public List<DeckItem> cardList = new List<DeckItem>();
    }
    [Serializable]
    public class DeckItem
    {
        [CardDefineID]
        public int id;
        public int count;
    }
}
