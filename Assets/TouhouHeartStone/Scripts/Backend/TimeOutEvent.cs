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
}