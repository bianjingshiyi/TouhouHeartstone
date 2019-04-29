namespace TouhouHeartstone.Backend
{
    abstract class SpellCardDefine : CardDefine
    {
        public abstract int cost { get; }
        public abstract int category { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            else if (propName == nameof(category))
                return (T)(object)category;
            else
                return base.getProp<T>(propName);
        }
    }
    /// <summary>
    /// 效果
    /// </summary>
    public abstract class Effect
    {
        /// <summary>
        /// 效果的作用域
        /// </summary>
        public abstract string pile { get; }
        /// <summary>
        /// 效果的作用时机
        /// </summary>
        public abstract string trigger { get; }
        /// <summary>
        /// 执行效果
        /// </summary>
        /// <param name="engine"></param>
        public abstract void execute(CardEngine engine, Player player, Card card, Card[] targetCards);
    }
}