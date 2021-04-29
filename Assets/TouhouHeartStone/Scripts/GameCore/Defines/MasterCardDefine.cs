using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class MasterCardDefine : THHCardDefine
    {
        public override string type { get; set; } = DEFINE.MASTER;
        public abstract int skillID { get; set; }
        public override string isUsable(CardEngine engine, TouhouCardEngine.Player player, Card card)
        {
            return null;
        }
    }
}