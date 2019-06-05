using System.Threading;

namespace TouhouHeartstone.Backend
{
    class CountDownEvent : VisibleEvent
    {
        public CountDownEvent(Player player, float time) : base("onCountDown")
        {
            this.player = player;
            this.time = time;
        }
        Player player { get; }
        float time { get; }
        public override void execute(CardEngine engine)
        {
            Thread.Sleep((int)time * 1000);
            //强制结束回合
            engine.doEvent(new TimeOutEvent());
            engine.turnEnd(player);
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new CountDownWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("time", time);
            return witness;
        }
    }
    /// <summary>
    /// 倒计时事件
    /// </summary>
    public class CountDownWitness : EventWitness
    {
        /// <summary>
        /// 倒计时的玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        /// <summary>
        /// 玩家当前回合的剩余时间
        /// </summary>
        public float time
        {
            get { return getVar<float>("time"); }
        }
        public CountDownWitness() : base("onCountDown")
        {
        }
    }
}