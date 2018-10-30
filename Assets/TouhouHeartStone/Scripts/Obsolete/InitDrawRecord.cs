using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    [Serializable]
    class InitDrawRecord : Record
    {
        [SerializeField]
        int _playerId;
        [SerializeField]
        int _count;
        public InitDrawRecord(int playerId, int count)
        {
            _playerId = playerId;
            _count = count;
        }
        public override Dictionary<int, Witness> apply(GameLogic game)
        {
            PlayerLogic player = game.players.getPlayer(_playerId);
            _cards = player.deck.Take(_count).ToArray();
            player.deck.moveTo(_cards, player.hand);
            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new InitDrawWitness(_playerId, _cards.getInstances(_playerId == game.players[i].id)));
            }
            return dicWitness;
        }
        [NonSerialized]
        CardLogic[] _cards = null;
        public override Dictionary<int, Witness> revert(GameLogic game)
        {
            PlayerLogic player = game.players.getPlayer(_playerId);
            player.hand.moveTo(_cards, player.deck);
            return null;
        }
    }
    [Serializable]
    class InitDrawWitness : Witness
    {
        public int playerId
        {
            get { return _playerId; }
        }
        [SerializeField]
        int _playerId;
        public CardInstance[] cards
        {
            get { return _cards; }
        }
        [SerializeField]
        CardInstance[] _cards;
        public InitDrawWitness(int playerId, CardInstance[] cards)
        {
            _playerId = playerId;
            _cards = cards;
        }
        public override string ToString()
        {
            return "玩家" + playerId + "抽" + cards.Length + "张卡";
        }
    }
}