namespace TouhouHeartstone
{
    class TiredEvent : VisibleEvent
    {
        public TiredEvent(Player player) : base("onTired")
        {
            this.player = player;
        }
        Player player { get; }
        public override void execute(CardEngine engine)
        {
            player.setProp("tired", PropertyChangeType.add, 1);
            (engine as HeartstoneCardEngine).damage(player["Master"][0], player.getProp<int>("tired"));
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new TiredWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            return witness;
        }
    }
    /// <summary>
    /// 疲劳事件
    /// </summary>
    public class TiredWitness : EventWitness
    {
        /// <summary>
        /// 感到疲劳的玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        public TiredWitness() : base("onTired")
        {
        }
    }
}