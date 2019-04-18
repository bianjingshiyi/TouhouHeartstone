using System.Linq;

namespace TouhouHeartstone.Backend
{
    class GameEndEvent : VisibleEvent
    {
        public GameEndEvent(Player[] winnerPlayers) : base("onGameEnd")
        {
            this.winnerPlayers = winnerPlayers;
        }
        Player[] winnerPlayers { get; }
        public override void execute(CardEngine engine)
        {
            //空事件
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onGameEnd");
            witness.setVar("winnerPlayersIndex", winnerPlayers.Select(p => { return engine.getPlayerIndex(p); }).ToArray());
            return witness;
        }
    }
}