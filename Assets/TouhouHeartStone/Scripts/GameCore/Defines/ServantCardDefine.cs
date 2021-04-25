using TouhouCardEngine;
namespace TouhouHeartstone
{
    public abstract class ServantCardDefine : THHCardDefine
    {
        public override string type { get; set; } = DEFINE.SERVANT;
        public override string isUsable(CardEngine engine, TouhouCardEngine.Player player, Card card)
        {
            return null;
        }
    }
}