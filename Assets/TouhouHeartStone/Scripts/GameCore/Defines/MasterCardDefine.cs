using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class MasterCardDefine : CardDefine
    {
        public override CardDefineType type { get; set; } = CardDefineType.master;
        public abstract int life { get; }
        public abstract int skillID { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(life))
                return (T)(object)life;
            return base.getProp<T>(propName);
        }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            return null;
        }
    }
}