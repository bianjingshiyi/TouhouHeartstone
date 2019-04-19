namespace TouhouHeartstone.Backend
{
    class StartEvent : VisibleEvent
    {
        public StartEvent() : base("onStart")
        {
        }
        public override void execute(CardEngine engine)
        {
            //空事件
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onStart");
            return witness;
        }
    }
}