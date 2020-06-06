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
        public override void merge(CardDefine newVersion)
        {
            if (newVersion.type != type)
                UberDebug.LogWarning(newVersion + "的类型与" + this + "不同，可能是一次非法的数据合并！");
            cost = newVersion.getProp<int>(nameof(cost));
        }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            return null;
        }
    }
}