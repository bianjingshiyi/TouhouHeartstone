namespace TouhouHeartstone
{
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