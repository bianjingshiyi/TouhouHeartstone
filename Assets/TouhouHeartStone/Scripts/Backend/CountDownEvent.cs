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
            EventWitness witness = new EventWitness("onCountDown");
            witness.setVar("playerIndex", player);
            witness.setVar("time", time);
            return witness;
        }
    }
}