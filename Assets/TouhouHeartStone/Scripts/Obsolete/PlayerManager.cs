using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

namespace TouhouHeartstone
{
    [Serializable]
    class PlayersLogic : IEnumerable<PlayerLogic>
    {
        public PlayersLogic(int[] playersId)
        {
            players = playersId.Select(e => { return new PlayerLogic(e); }).ToArray();
        }
        public PlayerLogic[] orderedPlayers { get; internal set; }
        public PlayerLogic getPlayer(int playerId)
        {
            return players.FirstOrDefault(e => { return e.id == playerId; });
        }
        public PlayerLogic this[int index]
        {
            get { return players[index]; }
        }
        public int count
        {
            get { return players.Length; }
        }
        public IEnumerator<PlayerLogic> GetEnumerator()
        {
            return ((IEnumerable<PlayerLogic>)players).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PlayerLogic>)players).GetEnumerator();
        }
        public PlayerLogic[] players { get; private set; }
    }
}