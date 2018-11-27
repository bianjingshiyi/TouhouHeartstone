using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class TurnStartRecord : Record
    {
        int playerId { get; } = 0;
        public TurnStartRecord(int playerId)
        {
            this.playerId = playerId;
        }
        public override Dictionary<int, IWitness> apply(Game game)
        {
            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new TurnStartWitness(playerId));
            }
            return dicWitness;
        }
        public override Dictionary<int, IWitness> revert(Game game)
        {
            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new TurnStartWitness(playerId));
            }
            return dicWitness;
        }
    }
}