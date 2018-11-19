using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class UseRecord : Record
    {
        int playerId { get; }
        CardInstance instance { get; }
        int position { get; }
        CardInstance target { get; }
        public UseRecord(int playerId, CardInstance instance, int position, CardInstance target)
        {
            this.playerId = playerId;
            this.instance = instance;
            this.position = position;
            this.target = target;
        }
        public override Dictionary<int, Witness> apply(Game game)
        {
            Player player = game.players.getPlayer(playerId);
            Card card = game.cards.getCard(instance);
            card.use(game, position, game.cards.getCard(target));
            player.hand.moveTo(card, player.grave, true);

            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new UseWitness(playerId, instance, position, target));
            }
            return dicWitness;
        }
        public override Dictionary<int, Witness> revert(Game game)
        {
            throw new NotImplementedException();
        }
    }
}