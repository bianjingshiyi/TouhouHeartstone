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
            player.deck.moveTo(_cards, player.hand, true);

            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new DrawWitness(playerId, _cards.getInstances(playerId == game.players[i].id)));
            }
            return dicWitness;
        }
        [NonSerialized]
        Card[] _cards = null;
        public override Dictionary<int, Witness> revert(Game game)
        {
            Player player = game.players.getPlayer(playerId);
            player.hand.moveTo(_cards, player.deck, true);
            return null;
        }
    }
}