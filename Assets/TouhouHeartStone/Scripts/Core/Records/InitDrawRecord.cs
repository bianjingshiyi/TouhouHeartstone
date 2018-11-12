using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class DrawRecord : Record
    {
        int playerId { get; }
        int count { get; }
        public DrawRecord(int playerId, int count)
        {
            this.playerId = playerId;
            this.count = count;
        }
        public override Dictionary<int, Witness> apply(Game game)
        {
            Player player = game.players.getPlayer(playerId);
            _cards = player.deck.Take(count).ToArray();
            player.deck.moveTo(_cards, player.hand);

            return null;
        }
        [NonSerialized]
        Card[] _cards = null;
        public override Dictionary<int, Witness> revert(Game game)
        {
            throw new NotImplementedException();
        }
    }
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
}