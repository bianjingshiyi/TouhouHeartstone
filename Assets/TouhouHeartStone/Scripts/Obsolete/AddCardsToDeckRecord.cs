using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    [Serializable]
    class AddCardRecord : Record
    {
        [SerializeField]
        int _playerId;
        [SerializeField]
        RegionType _region;
        [SerializeField]
        CardInstance[] _cardInstances;
        public AddCardRecord(int playerId, RegionType region, CardInstance[] cardInstances)
        {
            _playerId = playerId;
            _region = region;
            _cardInstances = cardInstances;
        }
        public override Dictionary<int, Witness> apply(Game game)
        {
            _cards = _cardInstances.Select(e => { return game.cards.create(e); }).ToArray();
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
                foreach (Card card in _cards)
                {
                    UnityEngine.Object.Destroy(card.gameObject);
                }
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
    [Serializable]
    class SetDeckWitness : Witness
    {
        public int playerId
        {
            get { return _playerId; }
        }
        [SerializeField]
        int _playerId;
        public int count
        {
            get { return _count; }
        }
        [SerializeField]
        int _count;
        public SetDeckWitness(int playerId, int count)
        {
            _playerId = playerId;
            _count = count;
        }
        public override string ToString()
        {
            return "玩家" + _playerId + "的卡组大小设置为" + _count + "。";
        }
    }
}