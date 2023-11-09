using Controllers;
using TouhouCardEngine;

namespace Game.PVE
{
    /// <summary>
    /// 用于存储当前层信息的临时数据。
    /// </summary>
    public class StageData
    {
        /// <summary>
        /// 战斗时的随机种子。
        /// </summary>
        public int fightSeed;
        /// <summary>
        /// 本层的卡牌奖励。
        /// </summary>
        public DefineReference[][] cardRewards;
        /// <summary>
        /// 本层的插件奖励。
        /// </summary>
        public DefineReference[] pluginRewards;
        /// <summary>
        /// 本层的boss。
        /// </summary>
        public PVECharacter boss;
    }
}
