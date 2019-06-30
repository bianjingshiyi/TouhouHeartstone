namespace TouhouHeartstone
{
    class TimeOutEvent : VisibleEvent
    {
        Player player { get; }
        public TimeOutEvent(Player player) : base("onTimeOut")
        {
            this.player = player;
        }
        public override void execute(CardEngine engine)
        {
            //空事件
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new TimeOutWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            return witness;
        }
    }
    /// <summary>
    /// 时间耗尽事件
    /// </summary>
    public class TimeOutWitness : EventWitness
    {
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        public TimeOutWitness() : base("onTimeOut")
        {
        }
    }
}