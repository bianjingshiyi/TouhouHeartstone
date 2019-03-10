using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class InitReplaceRecord : Record
    {
        int _playerId = 0;
        CardInstance[] _originCards = null;
        public InitReplaceRecord(int playerId, CardInstance[] originCards)
        {
            _playerId = playerId;
            _originCards = originCards;
        }
        public override Dictionary<int, IWitness> apply(CardEngine game)
        {
            //记录当前手牌和卡组状态
            Player player = game.playerManager.getPlayer(_playerId);
            _originDeck = player.deck.getCards();
            _originHand = player.hand.getCards();
            //将原本的手牌替换为从卡组中抽出的牌，再重新洗牌。
            Card[] originCards = player.hand.getCards(_originCards);
            Card[] targetCards = player.deck.Take(originCards.Length).ToArray();
            player.deck.remove(targetCards);
            player.hand.replace(originCards, targetCards);
            player.deck.add(originCards);
            player.deck.shuffle(game);

            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new InitReplaceWitness(_playerId, originCards.getInstances(game.playerManager[i].id == _playerId),
                                                                                     targetCards.getInstances(game.playerManager[i].id == _playerId)));
            }
            return dicWitness;
        }
        [NonSerialized]
        Card[] _originDeck;
        [NonSerialized]
        Card[] _originHand;
        public override Dictionary<int, IWitness> revert(CardEngine game)
        {
            Player player = game.playerManager.getPlayer(_playerId);
            player.deck.setCards(_originDeck);
            player.hand.setCards(_originHand);

            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new SetHandWitness(_playerId, _originHand.getInstances(game.playerManager[i].id == _playerId)));
            }
            return dicWitness;
        }
    }
}