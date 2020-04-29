using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class MasterCardDefine : CardDefine
    {
        public override string type { get; set; } = CardDefineType.MASTER;
        public abstract int life { get; set; }
        public abstract int skillID { get; set; }
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