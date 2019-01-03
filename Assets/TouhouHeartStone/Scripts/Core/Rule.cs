namespace TouhouHeartstone
{
    public abstract class Rule
    {
        public abstract void beforeEvent(Game game, Event eventName);
        public abstract void afterEvent(Game game, Event eventName);
    }
}