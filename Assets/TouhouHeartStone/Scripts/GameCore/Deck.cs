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
        public Deck(DefineReference[] cardRefs): this(cardRefs[0], cardRefs.Skip(1))
        {

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

            //英雄牌。
            addDefineRef(masterCardRef);

            //套牌内的卡牌。
            for (int i = 0; i < cardList.Count; i++)
            {
                addDefineRef(cardList[i].cardRef);
            }
            return poolList.ToArray();

            void addDefineRef(DefineReference defRef)
            {
                if (defRef.defineId < 0)
                {
                    poolList.Add(CardDefineHelper.getCardPoolId(defRef.defineId));
                }
                else
                {
                    poolList.Add(defRef.cardPoolId);
                }
            }
        }
        #endregion
        public int id;
        public string name;
        public DefineReference masterCardRef;
        public List<DeckCardPoolItem> cardPoolList = new List<DeckCardPoolItem>();
        public List<DeckItem> cardList = new List<DeckItem>();
    }
    [Serializable]
    public class DeckItem
    {
        public DefineReference cardRef;
        public int count;
    }
    [Serializable]
    public class DeckCardPoolItem
    {
        public long cardPoolId;
        public int edition;
    }
}
