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
        public override Dictionary<int, Witness> apply(Game game)
        {
            _cards = _cardInstances.Select(e => { return game.cards.create(e, game.players.getPlayer(_playerId)); }).ToArray();
            if (_region == RegionType.deck)
            {
                game.players.getPlayer(_playerId).deck.add(_cards);
                Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
                for (int i = 0; i < game.players.count; i++)
                {
                    dicWitness.Add(game.players[i].id, new SetDeckWitness(_playerId, game.players.getPlayer(_playerId).deck.count));
                }
                return dicWitness;
            }
            else
                throw new NotImplementedException();
        }
        [NonSerialized]
        Card[] _cards = null;
        public override Dictionary<int, Witness> revert(Game game)
        {
            if (_region == RegionType.deck)
            {
                game.players.getPlayer(_playerId).deck.remove(_cards);
                Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
                for (int i = 0; i < game.players.count; i++)
                {
                    dicWitness.Add(game.players[i].id, new SetDeckWitness(_playerId, game.players.getPlayer(_playerId).deck.count));
                }
                return dicWitness;
            }
            else
                throw new NotImplementedException();
        }
    }
}