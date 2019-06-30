namespace TouhouHeartstone
{
    class TurnEndEvent : VisibleEvent
    {
        public TurnEndEvent(Player player) : base("onTurnEnd")
        {
            this.player = player;
        }
        Player player { get; }
        public override void execute(CardEngine engine)
        {
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new TurnEndWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            return witness;
        }
    }
    /// <summary>
    /// 结束回合事件
    /// </summary>
    public class TurnEndWitness : EventWitness
    {
        /// <summary>
        /// 结束回合的玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        public TurnEndWitness () : base("onTurnEnd")
        {
        }
    }
}