using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class InitDrawRecord : Record
    {
        int _playerId;
        int _count;
        public InitDrawRecord(int playerId, int count)
        {
            _playerId = playerId;
            _count = count;
        }
        public override Dictionary<int, Witness> apply(Game game)
        {
            Player player = game.players.getPlayer(_playerId);
            _cards = player.deck.Take(_count).ToArray();
            player.deck.moveTo(_cards, player.hand);

            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new InitDrawWitness(_playerId, _cards.getInstances(_playerId == game.players[i].id)));
            }
            return dicWitness;
        }
        [NonSerialized]
        Card[] _cards = null;
        public override Dictionary<int, Witness> revert(Game game)
        {
            Player player = game.players.getPlayer(_playerId);
            player.hand.moveTo(_cards, player.deck);
            return null;
        }
    }
    [Serializable]
    class InitDrawWitness : Witness
    {
        public int playerId { get; }
        public CardInstance[] cards { get; }
        public InitDrawWitness(int playerId, CardInstance[] cards)
        {
            this.playerId = playerId;
            this.cards = cards;
        }
        public override string ToString()
        {
            return "玩家" + playerId + "抽" + cards.Length + "张卡";
        }
    }
}