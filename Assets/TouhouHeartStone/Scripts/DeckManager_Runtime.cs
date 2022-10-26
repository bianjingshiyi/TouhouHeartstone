using System.Collections.Generic;
using System.Linq;
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
                _deckList = new List<Deck>();
                foreach (var deckData in GetDecks())
                {
                    Deck deck = new Deck()
                    {
                        id = (int)deckData.ID,
                        name = deckData.Name,
                        masterId = deckData.MasterID,
                    };
                    deck.addCards(GetDeckCards(deckData.ID).Values.Select(id => (int)id));
                    _deckList.Add(deck);
                }
            }
            return _deckList;
        }
        /// <summary>
        /// 卡组是否合法
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public bool isValidDeck(Deck deck)
        {
            //检查数量是否合法，合法数量是可编辑的数量加上一张角色卡
            if (deck.getCardCount() != deckValidSize)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 卡组是否能用于线上对战
        /// </summary>
        /// <param name="deck"></param>
        /// <returns></returns>
        public bool isValidDeckForOnlineGame(Deck deck)
        {
            //检查是否使用了不合法的卡牌
            MainManager game = getManager<MainManager>();
            for (int i = 0; i < deck.cardList.Count; i++)
            {
                if (!game.card.isBuiltinCard(deck.cardList[i].id))
                {
                    return false;
                }
            }
            return isValidDeck(deck);
        }
        #endregion
        #region 属性字段
        public int deckValidSize => _deckMaxSize;
        List<Deck> _deckList;
        [SerializeField]
        int _deckMaxSize = 30;
        #endregion
    }
}
