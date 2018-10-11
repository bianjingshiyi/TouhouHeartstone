using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone
{
    public class PlayerManager : THManager
    {
        protected void Update()
        {
            //
        }
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
        Player[] players
        {
            get
            {
                if (_players == null || _players.Length <= 0)
                {
                    List<Player> playerList = new List<Player>();
                    for (int i = 0; i < game.network.connections.Length; i++)
                    {
                        if (game.network.connections[i] == game.network)
                        {
                            //本地
                            LocalPlayer player = LocalPlayer.create(this, game.network.connections[i].id);
                            playerList.Add(player);
                        }
                        else
                        {
                            //远端
                            RemotePlayer player = RemotePlayer.create(this, game.network.connections[i].id);
                            playerList.Add(player);
                        }
                    }
                    _players = playerList.ToArray();
                }
                return _players;
            }
        }
        [SerializeField]
        Player[] _players;
    }
}