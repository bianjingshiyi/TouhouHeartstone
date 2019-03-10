using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class AddCardRecord : Record
    {
        int _playerId;
        RegionType _region;
        CardInstance[] _cardInstances;
        public AddCardRecord(int playerId, RegionType region, CardInstance[] cardInstances)
        {
            _playerId = playerId;
            _region = region;
            _cardInstances = cardInstances;
        }
        public override Dictionary<int, IWitness> apply(CardEngine game)
        {
            _cards = _cardInstances.Select(e => { return game.cardManager.create(e, game.playerManager.getPlayer(_playerId)); }).ToArray();
            if (_region == RegionType.deck)
            {
                game.playerManager.getPlayer(_playerId).deck.add(_cards);
                Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
                for (int i = 0; i < game.playerManager.count; i++)
                {
                    dicWitness.Add(game.playerManager[i].id, new SetDeckWitness(_playerId, game.playerManager.getPlayer(_playerId).deck.count));
                }
                return dicWitness;
            }
            else
                throw new NotImplementedException();
        }
        [NonSerialized]
        Card[] _cards = null;
        public override Dictionary<int, IWitness> revert(CardEngine game)
        {
            if (_region == RegionType.deck)
            {
                game.playerManager.getPlayer(_playerId).deck.remove(_cards);
                Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
                for (int i = 0; i < game.playerManager.count; i++)
                {
                    dicWitness.Add(game.playerManager[i].id, new SetDeckWitness(_playerId, game.playerManager.getPlayer(_playerId).deck.count));
                }
                return dicWitness;
            }
            else
                throw new NotImplementedException();
        }
    }
}