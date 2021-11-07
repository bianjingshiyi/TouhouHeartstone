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
            return deck.cardList.Sum(i => i.count) < deckMaxSize;
        }
        #endregion
        #region 属性字段
        public int deckMaxSize => _deckMaxSize;
        List<Deck> _deckList;
        [SerializeField]
        int _deckMaxSize = 30;
        #endregion
    }
}
