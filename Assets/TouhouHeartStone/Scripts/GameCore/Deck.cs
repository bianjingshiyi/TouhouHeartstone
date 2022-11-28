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
        public Deck(int id, string name, DefineReference masterCardRef, IEnumerable<DefineReference> cardRefs)
        {
            this.id = id;
            this.name = name;
            this.masterCardRef = masterCardRef;
            addCards(cardRefs);
        }
        public Deck(DefineReference masterCardRef, IEnumerable<DefineReference> cardRefs) : this(0, null, masterCardRef, cardRefs)
        {
        }
        public Deck() : this(0, null, null, null)
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
        public void setCards(IEnumerable<DefineReference> cardsRef)
        {
            clearCards();
            addCards(cardsRef);
        }
        public void addCard(DefineReference cardRef)
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
        }
        public void addCards(IEnumerable<DefineReference> cardsRef)
        {
            if (cardsRef == null)
                return;
            foreach (var cardRef in cardsRef)
            {
                addCard(cardRef);
            }
        }
        public void clearCards()
        {
            cardRefList.Clear();
        }
        public int getCardCount()
        {
            return cardRefList.Sum(item => item.count);
        }
        public DefineReference[] toDefRefArray(bool includeMasterCard = true)
        {
            List<DefineReference> list = includeMasterCard ? new List<DefineReference>
            {
                masterCardRef
            } : new List<DefineReference>();
            foreach (var item in cardRefList)
            {
                if (item.cardRef == null || item.cardRef.cardPoolId == 0 || item.cardRef.defineId == 0)
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
        public DefineReference masterCardRef;
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
        public DefineReference cardRef;
        public int count;
    }
}
