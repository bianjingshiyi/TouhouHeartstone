namespace TouhouHeartstone.Backend
{
    public enum ErrorCode
    {
        //0x001~099为内部错误
        /// <summary>
        /// 抛出异常
        /// </summary>
        exception = 0x001,
        //末尾两位是错误序号，2位以上代表发生错误的事件类型
        //使用卡牌:0x100~199
        /// <summary>
        /// 没有足够的法力值
        /// </summary>
        use_NoEnoughGem = 0x101,
        /// <summary>
        /// 不是有效的使用目标
        /// </summary>
        use_InvalidTarget = 0x102,
        /// <summary>
        /// 必须等到你的回合才能使用卡牌
        /// </summary>
        use_NotYourTurn = 0x103,
        //攻击:0x200~299
        /// <summary>
        /// 必须等待一个回合才能进行攻击
        /// </summary>
        attack_waitOneTurn = 0x201,
        /// <summary>
        /// 这个回合已经攻击过了
        /// </summary>
        attack_AlreadyAttacked = 0x202,
        /// <summary>
        /// 不是有效的攻击目标
        /// </summary>
        attack_InvalidTarget = 0x203,
        /// <summary>
        /// 必须等到你的回合才能进行攻击
        /// </summary>
        attack_NotYourTurn = 0x204,
    }
}