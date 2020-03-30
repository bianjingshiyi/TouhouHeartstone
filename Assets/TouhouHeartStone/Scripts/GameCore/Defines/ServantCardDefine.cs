using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class ServantCardDefine : CardDefine, ICost
    {
        public override CardDefineType type
        {
            get { return CardDefineType.servant; }
        }
        /// <summary>
        /// 是否是衍生物？
        /// </summary>
        public virtual bool isToken { get; } = false;
        public abstract int cost { get; }
        public abstract int attack { get; }
        public abstract int life { get; }
        public virtual int spellDamage { get; } = 0;
        public virtual string[] tags { get; } = new string[0];
        /// <summary>
        /// 关键词
        /// </summary>
        public virtual string[] keywords { get; } = new string[0];
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            else if (propName == nameof(attack))
                return (T)(object)attack;
            else if (propName == nameof(life))
                return (T)(object)life;
            else if (propName == nameof(spellDamage))
                return (T)(object)spellDamage;
            else
                return base.getProp<T>(propName);
        }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            return null;
        }
    }
}