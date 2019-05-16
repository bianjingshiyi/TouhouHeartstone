using System.Threading;
using System.Threading.Tasks;

namespace TouhouHeartstone.Backend
{
    class TimeOutEvent : VisibleEvent
    {
        public TimeOutEvent() : base("onTimeOut")
        {
        }
        public override void execute(CardEngine engine)
        {
            //空事件
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onTimeOut");
            return witness;
        }
    }
    class CountDownEvent : VisibleEvent
    {
        public CountDownEvent() : base("onCountDown")
        {
        }
        public override void execute(CardEngine engine)
        {
            //空事件
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onCountDown");
            return witness;
        }
    }
    class TurnStartEvent : VisibleEvent
    {
        public TurnStartEvent(Player player) : base("onTurnStart")
        {
            this.player = player;
        }
        Player player { get; }
        public override void execute(CardEngine engine)
        {
            //玩家的最大能量加1但是不超过10，充满玩家的能量。
            engine.setProp("currentPlayer", player);
            engine.setMaxGem(player, player.getProp<int>("maxGem") + 1);
            engine.setGem(player, player.getProp<int>("maxGem"));
            //抽一张牌
            engine.draw(player);
            //使随从可以攻击
            foreach (Card card in player["Field"])
            {
                card.setProp("isReady", true);
                card.setProp("attackTimes", 0);
            }
            //开始烧绳倒计时
            Task.Run(() =>
            {
                Thread.Sleep(20000);
                //开始烧绳
                engine.doEvent(new CountDownEvent());
                Thread.Sleep(10000);
                //强制结束回合
                engine.doEvent(new TimeOutEvent());
                engine.turnEnd(player);
            });
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onTurnStart");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            return witness;
        }
    }
}