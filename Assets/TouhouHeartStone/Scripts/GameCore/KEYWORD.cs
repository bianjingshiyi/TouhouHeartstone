using System;

namespace TouhouHeartstone
{
    public class KEYWORD
    {
        /// <summary>
        /// 嘲讽
        /// </summary>
        public const string TAUNT = "taunt";
        /// <summary>
        /// 冲锋
        /// </summary>
        public const string CHARGE = "charge";
        /// <summary>
        /// 突袭
        /// </summary>
        public const string RUSH = "rush";
        /// <summary>
        /// 圣盾
        /// </summary>
        public const string SHIELD = "shield";
        /// <summary>
        /// 潜行
        /// </summary>
        public const string STEALTH = "stealth";
        /// <summary>
        /// 永久潜行
        /// </summary>
        public const string PERMANENTSTEALTH = "permanentStealth";
        /// <summary>
        /// 吸血
        /// </summary>
        public const string DRAIN = "drain";
        /// <summary>
        /// 剧毒
        /// </summary>
        public const string POISONOUS = "poisonous";
        /// <summary>
        /// 魔免
        /// </summary>
        public const string ELUSIVE = "elusive";
        /// <summary>
        /// 被冰冻的
        /// </summary>
        public const string FROZEN = "frozen";
        /// <summary>
        /// 唯一
        /// </summary>
        public const string UNIQUE = "unique";
        /// <summary>
        /// 抽到时自动施放
        /// </summary>
        public const string AUTOCAST = "autocast";
        /// <summary>
        /// 无法攻击
        /// </summary>
        public const string CANTATTACK = "cantattack";
        /// <summary>
        /// 风怒
        /// </summary>
        public const string WINDFURY = "windfury";
        /// <summary>
        /// 免疫
        /// </summary>
        public const string IMMUNITY = "immunity";
        /// <summary>
        /// 奥秘
        /// </summary>
        [Obsolete]
        public const string SECRET = "secret";
        /// <summary>
        /// 任务，委托，或者别的什么东西
        /// </summary>
        public const string MISSION = "mission";
        /// <summary>
        /// 永久禁锢
        /// </summary>
        public const string PERMANENT_FROZEN = "permanentFrozen";
        /// <summary>
        /// 可蓄力
        /// </summary>
        public const string CAN_BE_CHARGED = "preheat";
        /// <summary>
        /// 不能被使用
        /// </summary>
        public const string UNPLAYABLE = "unplayable";
        /// <summary>
        /// 幻象，只是作为卡牌关键词说明的占位符
        /// </summary>
        public const string ILLUSIVE = "illusive";
        /// <summary>
        /// 消失，只是作为卡牌关键词说明的占位符
        /// </summary>
        public const string VANISH = "vanish";

        #region 效果关键词
        /// <summary>
        /// 宣言
        /// </summary>
        public const string BATTLECRY = "battleCry";
        /// <summary>
        /// 遗言
        /// </summary>
        public const string DEATHRATTLE = "deathRattle";
        /// <summary>
        /// 陷阱
        /// </summary>
        public const string TRAP = "trap";
        /// <summary>
        /// 蓄力
        /// </summary>
        public const string CHARGED = "charged";
        #endregion

        #region 动作关键词
        public const string SHACKLE = "shackle";
        public const string SEAL = "seal";
        public const string FAITHDUEL = "faithduel";
        #endregion
    }
}
