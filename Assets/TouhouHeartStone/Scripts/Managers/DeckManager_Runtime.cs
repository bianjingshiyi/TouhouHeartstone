using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UnityEngine;
namespace Game
{
    partial class DeckManager
    {
        #region 公有方法
        public List<Deck> getOrLoadDeckList()
        {
            if (_deckList == null)
            {
                reloadDeckList();
            }
            return _deckList;
        }

        public void reloadDeckList()
        {
            _deckList = new List<Deck>();
            foreach (DeckDefine deckData in GetDecks())
            {
                Deck deck = new Deck()
                {
                    id = (int)deckData.ID,
                    name = deckData.Name,
                    masterCardRef = deckData.MasterCardRef != null ? deckData.MasterCardRef : getCardReference(0, deckData.MasterID),
                };
                deck.addCards(GetDeckCards(deckData.ID).Values);
                _deckList.Add(deck);
            }
        }

        /// <summary>
        /// 卡组是否合法
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public bool isValidDeck(Deck deck)
        {
            //检查数量是否合法，合法数量是可编辑的数量加上一张角色卡
            DebugManager debug = getManager<DebugManager>();
            if (!debug.SkipCardNumberCheck && deck.getCardCount() != deckValidSize)
            {
                return false;
            }
            // 英雄是否没有技能
            var master = main.card.getLocalCardDefine(deck.masterCardRef);
            var skillRef = master?.getSkillRef();
            var skill = skillRef != null ? main.card.getLocalCardDefine(skillRef) : null;
            if (skill == null)
                return false;

            //检查是否使用了不合法的卡牌
            for (int i = 0; i < deck.cardList.Count; i++)
            {
                var deckItem = deck.cardList[i];
                // 未知卡牌。
                var cardDefine = main.card.getLocalCardDefine(deckItem.cardRef);
                if (cardDefine == null)
                {
                    return false;
                }
            }
            return true;
        }
        public async Task<DefineReference> getDeckCardReferenceByIndex(Deck deck, int index, IEnumerable<ICardPoolReference> poolSet)
        {
            if (deck == null)
                return null;
            var deckItems = await main.card.orderDeckItemsByPoolsInEnv(deck.cardList, poolSet);
            var deckItem = deckItems[index];
            var defRef = deckItem.cardRef;
            return defRef;
        }
        /// <summary>
        /// 卡组是否能用于线上对战
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public bool isValidDeckForOnlineGame(Deck deck)
        {
            return isValidDeck(deck);
        }
        /// <summary>
        /// 卡组中是否有自定义卡牌。
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        #endregion
        #region 属性字段
        public int deckValidSize => _deckMaxSize;
        List<Deck> _deckList;
        [SerializeField]
        int _deckMaxSize = 30;
        #endregion
    }
}
