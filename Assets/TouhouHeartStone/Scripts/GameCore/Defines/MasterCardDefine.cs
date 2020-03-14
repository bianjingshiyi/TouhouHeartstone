using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class MasterCardDefine : CardDefine
    {
        public override CardDefineType type
        {
            get { return CardDefineType.master; }
        }
        public abstract int skillID { get; }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            return null;
        }
    }
}