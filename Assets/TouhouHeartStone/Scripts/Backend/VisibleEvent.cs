namespace TouhouHeartstone.Backend
{
    public abstract class VisibleEvent : Event
    {
        public VisibleEvent(string eventName) : base(eventName)
        {
        }
        public abstract EventWitness getWitness(CardEngine engine, Player player);
    }
}