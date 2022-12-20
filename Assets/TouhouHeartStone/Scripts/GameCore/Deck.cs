using System.Linq;
using System;
using System.Collections.Generic;
using TouhouCardEngine;
using DeckPack = TouhouCardEngine.Shared.Deck;

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
        public Deck(DeckPack pack)
        {
            var items = pack.defineRefs;
            masterCardRef = DefineReference.FromDeckItem(items[0]);
            addCards(items.Skip(1).Select(i => DefineReference.FromDeckItem(i)));
        }
        public Deck() : this(0, null, null, null)
        {
        }
        public void setCards(IEnumerable<DefineReference> cardsRef)
        {
            clearCards();
            addCards(cardsRef);
        }
        public void addCard(DefineReference cardRef)
        {
            int index = cardList.FindIndex(e => e.cardRef == cardRef);
            if (index < 0)
            {
                cardList.Add(new DeckItem() { cardRef = cardRef, count = 1 });
            }
            else
            {
                cardList[index].count++;
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
            cardList.Clear();
        }
        public int getCardCount(long cardPoolId, int id)
        {
            return cardList.Find(c => c.cardRef.cardPoolId == cardPoolId && c.cardRef.defineId == id)?.count ?? 0;
        }
        public int getCardCount()
        {
            return cardList.Sum(item => item.count);
        }
        public DefineReference[] toDefRefArray(bool includeMasterCard = true)
        {
            List<DefineReference> list = includeMasterCard ? new List<DefineReference>
            {
                masterCardRef
            } : new List<DefineReference>();
            foreach (var item in cardList)
            {
                if (item.cardRef == null || item.cardRef.defineId == 0)
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
                if (cardList[i].cardRef.cardPoolId != 0)
                    poolList.Add(cardList[i].cardRef.cardPoolId);
                else if (cardList[i].cardRef.defineId < 0)
                    poolList.Add(CardDefineHelper.getCardPoolId(cardList[i].cardRef.defineId));
            }
            return poolList.ToArray();
        }
        public DeckPack ToDeckPack()
        {
            return new DeckPack(toDefRefArray(true).Select(r => r.ToDeckItem()).ToArray());
        }
        #endregion
        public int id;
        public string name;
        public DefineReference masterCardRef;
        public List<DeckItem> cardList = new List<DeckItem>();
    }
    [Serializable]
    public class DeckItem
    {
        public DefineReference cardRef;
        public int count;
    }
}
