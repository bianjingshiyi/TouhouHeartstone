namespace TouhouHeartstone.Backend
{
    public abstract class ServantCardDefine : CardDefine, ICost
    {
        public abstract int cost { get; }
        public abstract int attack { get; }
        public abstract int life { get; }
        public abstract Effect[] getEffects();
        public abstract int category { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            else if (propName == nameof(attack))
                return (T)(object)attack;
            else if (propName == nameof(life))
                return (T)(object)life;
            else
                return base.getProp<T>(propName);
        }
    }
}