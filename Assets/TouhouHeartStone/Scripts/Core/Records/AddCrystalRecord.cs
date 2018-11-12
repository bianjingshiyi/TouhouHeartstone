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
        public override Dictionary<int, Witness> apply(Game game)
        {
            game.players.getPlayer(playerId).addCrystal(count, state);

            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new AddCrystalWitness(playerId, count, state));
            }
            return dicWitness;
        }
        public override Dictionary<int, Witness> revert(Game game)
        {
            game.players.getPlayer(playerId).removeCrystal(count);

            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new RemoveCrystalWitness(playerId, count));
            }
            return dicWitness;
        }
    }
}