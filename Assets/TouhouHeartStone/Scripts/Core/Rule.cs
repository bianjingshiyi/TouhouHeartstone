namespace TouhouHeartstone
{
    public abstract class Rule
    {
        public abstract CardPool pool { get; }
        public abstract void beforeEvent(CardEngine game, Event @event);
        public abstract void afterEvent(CardEngine game, Event @event);
    }
}