using System;
using System.Collections.Generic;
using UnityEngine;

namespace TouhouHeartstone
{
    [Serializable]
    class SetOrderRecord : Record
    {
        [SerializeField]
        int[] _targetOrder = null;
        [SerializeField]
        int[] _originOrder = null;
        public SetOrderRecord(int[] orderedPlayerId)
        {
            _targetOrder = orderedPlayerId;
        }
        public override Dictionary<int, Witness> apply(GameContainer game)
        {
            if (game.orderedPlayers != null)
            {
                _originOrder = new int[game.orderedPlayers.Length];
                for (int i = 0; i < _originOrder.Length; i++)
                {
                    _originOrder[i] = game.orderedPlayers[i].id;
                }
            }
            else
                _originOrder = null;
            if (_targetOrder != null)
            {
                Player[] orderedPlayers = new Player[_targetOrder.Length];
                for (int i = 0; i < orderedPlayers.Length; i++)
                {
                    orderedPlayers[i] = game.players.getPlayer(_targetOrder[i]);
                }
                game.orderedPlayers = orderedPlayers;
            }
            else
                game.orderedPlayers = null;
            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new SetOrderWitness(_targetOrder));
            }
            return dicWitness;
        }
        public override Dictionary<int, Witness> revert(GameContainer game)
        {
            if (_originOrder != null)
            {
                Player[] orderedPlayers = new Player[_originOrder.Length];
                for (int i = 0; i < orderedPlayers.Length; i++)
                {
                    orderedPlayers[i] = game.players.getPlayer(_originOrder[i]);
                }
                game.orderedPlayers = orderedPlayers;
            }
            else
                game.orderedPlayers = null;
            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new SetOrderWitness(_originOrder));
            }
            return dicWitness;
        }
    }
    [Serializable]
    class SetOrderWitness : Witness
    {
        public int[] orderedPlayerId
        {
            get { return _orderedPlayerId; }
        }
        [SerializeField]
        int[] _orderedPlayerId = null;
        public SetOrderWitness(int[] orderedPlayerId)
        {
            _orderedPlayerId = orderedPlayerId;
        }
        public override string ToString()
        {
            string s = "玩家行动顺序为：";
            for (int i = 0; i < orderedPlayerId.Length; i++)
            {
                s += orderedPlayerId[i];
                if (i < orderedPlayerId.Length - 1)
                    s += "，";
                else
                    s += "。";
            }
            return s;
        }
    }
}