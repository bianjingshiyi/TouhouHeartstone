using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

namespace TouhouHeartstone
{
    [Serializable]
    class PlayerManager : IEnumerable<Player>
    {
        public PlayerManager(int[] playersId)
        {
            players = playersId.Select(e => { return new Player(e); }).ToArray();
        }
        public Player[] orderedPlayers { get; internal set; }
        public Player getPlayer(int playerId)
        {
            return players.FirstOrDefault(e => { return e.id == playerId; });
        }
        public Player this[int index]
        {
            get { return players[index]; }
        }
        public int count
        {
            get { return players.Length; }
        }
        public IEnumerator<Player> GetEnumerator()
        {
            return ((IEnumerable<Player>)players).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Player>)players).GetEnumerator();
        }
        public Player[] players { get; private set; }
    }
}