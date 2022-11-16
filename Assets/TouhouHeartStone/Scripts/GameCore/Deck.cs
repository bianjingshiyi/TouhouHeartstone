using System.Linq;
using System;
using System.Collections.Generic;
using TouhouCardEngine;

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
        [Obsolete]
        public void setCards(IEnumerable<int> cardsId)
        {
            clearCards();
            addCards(cardsId);
        }
        [Obsolete]
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
        [Obsolete]
        public void addCards(IEnumerable<int> cardsId)
        {
            if (cardsId == null)
                return;
            foreach (var cardId in cardsId)
            {
                addCard(cardId);
            }
        }
        public int getCardCount(int cardId)
        {
            var item = cardList.Find(e => e.id == cardId);
            return item != null ? item.count : 0;
        }
            public void setCards(IEnumerable<CardReference> cardsRef)
        {
            clearCards();
            addCards(cardsRef);
        }
        public void addCard(CardReference cardRef)
        {
            int index = cardRefList.FindIndex(e => e.cardRef == cardRef);
            if (index < 0)
            {
                cardRefList.Add(new DeckRefItem() { cardRef = cardRef, count = 1 });
            }
            else
            {
                cardRefList[index].count++;
            }
            addCard(cardRef.cardId);
        }
        public void addCards(IEnumerable<CardReference> cardsRef)
        {
            if (cardsRef == null)
                return;
            foreach (var cardId in cardsRef)
            {
                addCard(cardId);
            }
        }
        public void clearCards()
        {
            cardList.Clear();
            cardRefList.Clear();
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
        public CardReference[] toRefArray(bool includeMasterCard = true)
        {
            List<CardReference> list = includeMasterCard ? new List<CardReference>
            {
                new CardReference() { cardId = masterId, cardPoolId = CardDefineHelper.getCardPoolId(masterId) }
            } : new List<CardReference>();
            foreach (var item in cardRefList)
            {
                if (item.cardRef == null || item.cardRef.cardId == 0)
                    continue;
                if (item.count < 1)
                    continue;
                list.AddRange(Enumerable.Repeat(item.cardRef, item.count));
            }
            return list.ToArray();
        }
        public long[] getCardPools()
        {
            HashSet<long> poolList = new HashSet<long>();
            //检查是否有自定义卡牌，
            for (int i = 0; i < cardList.Count; i++)
            {
                poolList.Add(CardDefineHelper.getCardPoolId(cardList[i].id));
            }
            return poolList.ToArray();
        }        #endregion
        public int id;
        public string name;
        public int masterId;
        [Obsolete("使用cardRefList")]
        public List<DeckItem> cardList = new List<DeckItem>();
        public List<DeckRefItem> cardRefList = new List<DeckRefItem>();
    }
    [Serializable]
    public class DeckItem
    {
        [CardDefineID]
        public int id;
        public int count;
    }
    [Serializable]
    public class DeckRefItem
    {
        public CardReference cardRef;
        public int count;
    }
}
