using TouhouCardEngine;
namespace TouhouHeartstone
{
    public abstract class ServantCardDefine : CardDefine
    {
        public override string type { get; set; } = CardDefineType.SERVANT;
        /// <summary>
        /// 是否是衍生物？
        /// </summary>
        public virtual bool isToken { get; set; } = false;
        public abstract int cost { get; set; }
        public abstract int attack { get; set; }
        public abstract int life { get; set; }
        public virtual int spellDamage { get; set; } = 0;
        public virtual string[] tags { get; set; } = new string[0];
        /// <summary>
        /// 关键词
        /// </summary>
        public virtual string[] keywords { get; set; } = new string[0];
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
            else if (propName == nameof(isToken))
                return (T)(object)isToken;
            else if (propName == nameof(tags))
                return (T)(object)tags;
            else if (propName == nameof(keywords))
                return (T)(object)keywords;
            else
                return base.getProp<T>(propName);
        }
        public override void setProp<T>(string propName, T value)
        {
            if (propName == nameof(cost))
                cost = (int)(object)value;
            else if (propName == nameof(attack))
                attack = (int)(object)value;
            else if (propName == nameof(life))
                life = (int)(object)value;
            else if (propName == nameof(spellDamage))
                spellDamage = (int)(object)value;
            else if (propName == nameof(isToken))
                isToken = value != null ? (bool)(object)value : false;
            else if (propName == nameof(tags))
                tags = (string[])(object)value;
            else if (propName == nameof(keywords))
                keywords = (string[])(object)value;
            else
                base.setProp(propName, value);
        }
        public override string isUsable(CardEngine engine, Player player, Card card)
        {
            return null;
        }
    }
}