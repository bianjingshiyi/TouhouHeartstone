namespace TouhouHeartstone
{
    public abstract class CardDefine
    {
        public abstract int id { get; }
        public abstract CardDefineType type { get; }
        public abstract Effect[] effects { get; }
        public object this[string propName]
        {
            get { return getProp<object>(propName); }
        }
        public virtual T getProp<T>(string propName)
        {
            if (propName == nameof(id))
                return (T)(object)id;
            else
                return default(T);
        }
    }
}