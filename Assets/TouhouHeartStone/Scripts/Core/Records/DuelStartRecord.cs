using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class DuelStartRecord : Record
    {
        public override Dictionary<int, Witness> apply(Game game)
        {
            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new DuelStartWitness());
            }
            return dicWitness;
        }
        public override Dictionary<int, Witness> revert(Game game)
        {
            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new DuelStartWitness());
            }
            return dicWitness;
        }
    }
}