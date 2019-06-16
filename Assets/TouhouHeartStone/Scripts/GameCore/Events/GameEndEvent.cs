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
            EventWitness witness = new GameEndWitness();
            witness.setVar("winnerPlayersIndex", winnerPlayers.Select(p => { return engine.getPlayerIndex(p); }).ToArray());
            return witness;
        }
    }
    /// <summary>
    /// 游戏结束事件
    /// </summary>
    public class GameEndWitness : EventWitness
    {
        /// <summary>
        /// 所有获胜玩家的索引
        /// </summary>
        public int[] winnerPlayersIndex
        {
            get { return getVar<int[]>("winnerPlayersIndex"); }
        }
        public GameEndWitness() : base("onGameEnd")
        {
        }
    }
}