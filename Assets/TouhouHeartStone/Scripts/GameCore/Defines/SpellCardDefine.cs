using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class SpellCardDefine : CardDefine
    {
        public override string type { get; set; } = CardDefineType.SPELL;
        public abstract int cost { get; set; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            else
                return base.getProp<T>(propName);
        }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            return null;
        }
    }
}