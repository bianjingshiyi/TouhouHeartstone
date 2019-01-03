using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    class PlayerManager : IEnumerable<Player>
    {
        public PlayerManager(int[] playersId)
        {
            players = playersId.Select(e => { return new Player(e); }).ToArray();
        }
        public int getPlayerIndex(Player player)
        {
            return Array.IndexOf(players, player);
        }
        public Player[] orderedPlayers { get; internal set; }
        public Player getPlayer(int playerId)
        {
            return players.FirstOrDefault(e => { return e.id == playerId; });
        }
        public Player this[int index]
        {
            get
            {
                if (0 <= index && index < count)
                    return players[index];
                else
                    return null;
            }
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