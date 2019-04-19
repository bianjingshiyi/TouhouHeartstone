namespace TouhouHeartstone.Backend
{
    public abstract class MasterCardDefine : CardDefine
    {
        public abstract int category { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(category))
                return (T)(object)category;
            else
                return base.getProp<T>(propName);
        }
    }
}