using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
namespace Game
{
    partial class DeckManager
    {
        #region 公有方法
        public async Task createPresetDecks()
        {
            var decks = await getPresetDecks();
            foreach (var deck in decks)
            {
                var masterRef = deck.masterCardRef;
                var poolInfo = main.card.getLocalCardPoolInfo(masterRef.cardPoolId);
                var cardName = main.card.getCardName(poolInfo, masterRef.defineId);
                var deckId = CreateDeck(cardName, masterRef);
                EnterTransaction();
                foreach (var cardRef in deck.toDefRefArray(includeMasterCard: false))
                {
                    AddCard(deckId, cardRef);
                }
                Commit();
            }
        }
        public async Task checkPresetDecks()
        {
            // 检测是否是初次启动并且拥有卡组。如果没有卡组，每个人物新建一个预设卡组。
            if (PlayerPrefs.GetInt(KEY_PRESET_DECKS_CHECKED, 0) <= 0)
            {
                PlayerPrefs.SetInt(KEY_PRESET_DECKS_CHECKED, 1);
                if (GetDecks().Count() <= 0)
                {
                    await createPresetDecks();
                }
            }
        }
        #endregion
        private async Task<Deck[]> getPresetDecks()
        {
            if (presetDecks == null)
            {
                presetDecks = await newPresetDeckInfos();
            }
            return presetDecks;
        }
        private async Task<Deck[]> newPresetDeckInfos()
        {
            List<Deck> list = new List<Deck>();

            var poolRef = main.card.getLocalCardPoolReference(CardManager.STANDARD_POOL_ID);
            var (_, pool) = await main.card.getOrLoadCardPool(poolRef);

            foreach (var master in pool.cardDefineList.Where(c => c.isMaster()).OrderBy(c => c.id))
            {
                var masterRef = master.getDefineRef();
                var deck = loadMasterPresetDeck(pool, masterRef);
                list.Add(deck);
            }
            return list.ToArray();
        }
        private Deck loadMasterPresetDeck(CardPool pool, DefineReference masterRef)
        {
            var notUniqueCards = pool.cardDefineList.Where(c => !c.hasKeyword(KEYWORD.UNIQUE));
            var cards = pool.cardDefineList
                .Concat(notUniqueCards)
                .Where(c => !c.isSkill() && !c.isMaster() && !c.isToken())
                .Where(c => c.getCharacterRef() == masterRef)
                .OrderBy(c => c.getCost())
                .ThenBy(c => c.id)
                .Take(30);
            return new Deck(masterRef, cards.Select(c => c.getDefineRef()));
        }

        #region 属性字段
        private const string KEY_PRESET_DECKS_CHECKED = "StartingPresetDecksChecked";
        private static Deck[] presetDecks;
        #endregion
    }
}
