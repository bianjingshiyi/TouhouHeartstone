using System;

using UnityEngine;

namespace TouhouHeartstone
{
    [Serializable]
    public class Record
    {
        public int number
        {
            get { return _number; }
            private set { _number = value; }
        }
        [SerializeField]
        int _number;
        public Record(int number)
        {
            this.number = number;
        }
    }
    [Serializable]
    class FirstPlayerRecord : Record
    {
        private int _playerId;
        public FirstPlayerRecord(int number, int playerId) : base(number)
        {
            _playerId = playerId;
        }
        public void apply(Game game)
        {
            game.firstPlayer = game.players.getPlayer(_playerId);
            game.log.msg(game.firstPlayer + "获得了先手");
        }
    }
}