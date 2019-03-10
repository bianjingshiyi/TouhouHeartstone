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
        public override Dictionary<int, IWitness> apply(CardEngine game)
        {
            Player player = game.playerManager.getPlayer(playerId);
            Card card = game.cardManager.getCard(instance);
            card.use(game, position, game.cardManager.getCard(target));
            player.hand.moveTo(card, player.grave, true);

            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new UseWitness(playerId, instance, position, target));
            }
            return dicWitness;
        }
        public override Dictionary<int, IWitness> revert(CardEngine game)
        {
            throw new NotImplementedException();
        }
    }
}