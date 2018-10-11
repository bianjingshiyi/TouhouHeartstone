using System;

namespace TouhouHeartstone
{
    [Serializable]
    class FirstPlayerDiff
    {
        private int _playerId;
        public FirstPlayerDiff(int playerId)
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