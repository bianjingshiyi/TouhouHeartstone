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
        public override Dictionary<int, Witness> apply(Game game)
        {
            //记录当前手牌和卡组状态
            Player player = game.players.getPlayer(_playerId);
            _originDeck = player.deck.getCards();
            _originHand = player.hand.getCards();
            //将原本的手牌替换为从卡组中抽出的牌，再重新洗牌。
            Card[] originCards = player.hand.getCards(_originCards);
            Card[] targetCards = player.deck.Take(originCards.Length).ToArray();
            player.deck.remove(targetCards);
            player.hand.replace(originCards, targetCards);
            player.deck.add(originCards);
            player.deck.shuffle(game);

            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new InitReplaceWitness(_playerId, originCards.getInstances(game.players[i].id == _playerId),
                                                                                     targetCards.getInstances(game.players[i].id == _playerId)));
            }
            return dicWitness;
        }
        [NonSerialized]
        Card[] _originDeck;
        [NonSerialized]
        Card[] _originHand;
        public override Dictionary<int, Witness> revert(Game game)
        {
            Player player = game.players.getPlayer(_playerId);
            player.deck.setCards(_originDeck);
            player.hand.setCards(_originHand);

            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new SetHandWitness(_playerId, _originHand.getInstances(game.players[i].id == _playerId)));
            }
            return dicWitness;
        }
    }
    /// <summary>
    /// 替换初始手牌的Witness
    /// </summary>
    [Serializable]
    class InitReplaceWitness : Witness
    {
        public int playerId { get; }
        public CardInstance[] originCards { get; }
        public CardInstance[] replaceCards { get; }
        public InitReplaceWitness(int playerId, CardInstance[] originCards, CardInstance[] replaceCards)
        {
            this.playerId = playerId;
            this.originCards = originCards;
            this.replaceCards = replaceCards;
        }
    }
    /// <summary>
    /// 直接设置手牌的Witness
    /// </summary>
    [Serializable]
    class SetHandWitness : Witness
    {
        public int playerId { get; }
        public CardInstance[] cards { get; }
        public SetHandWitness(int playerId, CardInstance[] cards)
        {
            this.playerId = playerId;
            this.cards = cards;
        }
    }
}