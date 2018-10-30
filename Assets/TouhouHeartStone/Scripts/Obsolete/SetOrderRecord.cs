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
        public override Dictionary<int, Witness> apply(GameLogic game)
        {
            if (game.players.orderedPlayers != null)
            {
                _originOrder = new int[game.players.orderedPlayers.Length];
                for (int i = 0; i < _originOrder.Length; i++)
                {
                    _originOrder[i] = game.players.orderedPlayers[i].id;
                }
            }
            else
                _originOrder = null;
            if (_targetOrder != null)
            {
                PlayerLogic[] orderedPlayers = new PlayerLogic[_targetOrder.Length];
                for (int i = 0; i < orderedPlayers.Length; i++)
                {
                    orderedPlayers[i] = game.players.getPlayer(_targetOrder[i]);
                }
                game.players.orderedPlayers = orderedPlayers;
            }
            else
                game.players.orderedPlayers = null;
            Dictionary<int, Witness> dicWitness = new Dictionary<int, Witness>();
            for (int i = 0; i < game.players.count; i++)
            {
                dicWitness.Add(game.players[i].id, new SetOrderWitness(_targetOrder));
            }
            return dicWitness;
        }
        public override Dictionary<int, Witness> revert(GameLogic game)
        {
            if (_originOrder != null)
            {
                PlayerLogic[] orderedPlayers = new PlayerLogic[_originOrder.Length];
                for (int i = 0; i < orderedPlayers.Length; i++)
                {
                    orderedPlayers[i] = game.players.getPlayer(_originOrder[i]);
                }
                game.players.orderedPlayers = orderedPlayers;
            }
            else
                game.players.orderedPlayers = null;
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