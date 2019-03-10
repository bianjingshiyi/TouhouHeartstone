using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class SetOrderRecord : Record
    {
        int[] _targetOrder = null;
        int[] _originOrder = null;
        public SetOrderRecord(int[] orderedPlayerId)
        {
            _targetOrder = orderedPlayerId;
        }
        public override Dictionary<int, IWitness> apply(CardEngine game)
        {
            if (game.playerManager.orderedPlayers != null)
            {
                _originOrder = new int[game.playerManager.orderedPlayers.Length];
                for (int i = 0; i < _originOrder.Length; i++)
                {
                    _originOrder[i] = game.playerManager.orderedPlayers[i].id;
                }
            }
            else
                _originOrder = null;
            if (_targetOrder != null)
            {
                Player[] orderedPlayers = new Player[_targetOrder.Length];
                for (int i = 0; i < orderedPlayers.Length; i++)
                {
                    orderedPlayers[i] = game.playerManager.getPlayer(_targetOrder[i]);
                }
                game.playerManager.orderedPlayers = orderedPlayers;
            }
            else
                game.playerManager.orderedPlayers = null;
            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new SetOrderWitness(_targetOrder));
            }
            return dicWitness;
        }
        public override Dictionary<int, IWitness> revert(CardEngine game)
        {
            if (_originOrder != null)
            {
                Player[] orderedPlayers = new Player[_originOrder.Length];
                for (int i = 0; i < orderedPlayers.Length; i++)
                {
                    orderedPlayers[i] = game.playerManager.getPlayer(_originOrder[i]);
                }
                game.playerManager.orderedPlayers = orderedPlayers;
            }
            else
                game.playerManager.orderedPlayers = null;
            Dictionary<int, IWitness> dicWitness = new Dictionary<int, IWitness>();
            for (int i = 0; i < game.playerManager.count; i++)
            {
                dicWitness.Add(game.playerManager[i].id, new SetOrderWitness(_originOrder));
            }
            return dicWitness;
        }
    }
}