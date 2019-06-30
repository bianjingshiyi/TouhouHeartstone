namespace TouhouHeartstone
{
    public abstract class SkillCardDefine : CardDefine
    {
        public abstract int cost { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            return base.getProp<T>(propName);
        }
    }
}