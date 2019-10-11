using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class SkillCardDefine : CardDefine
    {
        public override CardDefineType type
        {
            get { return CardDefineType.skill; }
        }
        public abstract int cost { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            return base.getProp<T>(propName);
        }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            return null;
        }
    }
}