using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class AddCrystalRecord : Record
    {
        int playerId { get; }
        int count { get; }
        CrystalState state { get; }
        public AddCrystalRecord(int playerId, int count, CrystalState state)
        {
            this.playerId = playerId;
            this.count = count;
            this.state = state;
        }
        public override Dictionary<int, IWitness> apply(CardEngine game)
        {
            game.playerManager.getPlayer(playerId).addCrystal(count, state);

            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new AddCrystalWitness(playerId, count, state));
            }
            return dicWitness;
        }
        public override Dictionary<int, IWitness> revert(CardEngine game)
        {
            game.playerManager.getPlayer(playerId).removeCrystal(count);

            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new RemoveCrystalWitness(playerId, count));
            }
            return dicWitness;
        }
    }
}