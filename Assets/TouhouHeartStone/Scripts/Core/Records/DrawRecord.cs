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
        public override Dictionary<int, IWitness> apply(CardEngine game)
        {
            Player player = game.playerManager.getPlayer(playerId);
            _cards = player.deck.Take(count).ToArray();
            player.deck.moveTo(_cards, player.hand, true);

            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new DrawWitness(playerId, _cards.getInstances(playerId == game.playerManager[i].id)));
            }
            return dicWitness;
        }
        [NonSerialized]
        Card[] _cards = null;
        public override Dictionary<int, IWitness> revert(CardEngine game)
        {
            Player player = game.playerManager.getPlayer(playerId);
            player.hand.moveTo(_cards, player.deck, true);
            return null;
        }
    }
}