using TouhouHeartstone;
using System;
using System.Collections.Generic;
using UnityEngine;
using TouhouCardEngine.Interfaces;
using System.Linq;
using MongoDB.Bson;
using NitoriNetwork.Common;
using TouhouCardEngine;

namespace Game
{
    [Serializable]
    public class Replay
    {
        public HearthstoneOption option;
        public RoomPlayerInfo[] players;
        //public ReplayPlayer[] players;
        public List<ReplayLogLine> logs = new List<ReplayLogLine>();
        [Serializable]
        public class ReplayPlayer
        {
            public int MasterCard;
            public int[] Deck;
            public int ID;
            public string Name;
            public ReplayPlayer(Player p)
            {
                MasterCard = p.getMaster().define.id;
                Deck = p.getDeck().Select(d => d.define.id).ToArray();
                ID = p.id;
                Name = p.name;
            }
        }
        [Serializable]
        public class ReplayLogLine
        {
            public int player;
            public float time;
            public IResponse response;

            public ReplayLogLine(int id, IResponse response, float time)
            {
                player = id;
                this.response = response;
                this.time = time;
            }
        }
        public Replay(CardEngine game)
        {
            option = game.getOption();
            players = game.players.Select(p =>
            {
                var player = new RoomPlayerInfo() { RoomID = p.id, name = p.name };
                player.setDeck(new Deck(p.getMaster().define.id, p.getDeck().Select(c => c.define.id).ToArray()));
                return player;
            }).ToArray();
        }
#if UNITY_EDITOR
        [SerializeField]
        string[] _contents;
        public void showContents()
        {
            _contents = logs.Select(l => l.response.ToJson()).ToArray();
        }
#endif
    }
}
