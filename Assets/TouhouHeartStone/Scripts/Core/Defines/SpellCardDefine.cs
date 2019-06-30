namespace TouhouHeartstone
{
    public abstract class SpellCardDefine : CardDefine
    {
        public abstract int cost { get; }
        public abstract int category { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            else if (propName == nameof(category))
                return (T)(object)category;
            else
                return base.getProp<T>(propName);
        }
    }
}