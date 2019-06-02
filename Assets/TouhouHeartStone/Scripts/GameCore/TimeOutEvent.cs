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
            EventWitness witness = new TimeOutWitness();
            return witness;
        }
    }
    /// <summary>
    /// 时间耗尽事件
    /// </summary>
    public class TimeOutWitness : EventWitness
    {
        public TimeOutWitness() : base("onTimeOut")
        {
        }
    }
}