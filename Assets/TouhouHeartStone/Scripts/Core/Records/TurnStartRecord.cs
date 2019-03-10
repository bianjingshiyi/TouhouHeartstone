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
        public override Dictionary<int, IWitness> apply(CardEngine game)
        {
            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new TurnStartWitness(playerId));
            }
            return dicWitness;
        }
        public override Dictionary<int, IWitness> revert(CardEngine game)
        {
            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new TurnStartWitness(playerId));
            }
            return dicWitness;
        }
    }
}