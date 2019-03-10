using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class DuelStartRecord : Record
    {
        public override Dictionary<int, IWitness> apply(CardEngine game)
        {
            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new DuelStartWitness());
            }
            return dicWitness;
        }
        public override Dictionary<int, IWitness> revert(CardEngine game)
        {
            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new DuelStartWitness());
            }
            return dicWitness;
        }
    }
}