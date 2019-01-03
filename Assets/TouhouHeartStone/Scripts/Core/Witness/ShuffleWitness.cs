using System;

namespace TouhouHeartstone
{
    /// <summary>
    /// 洗牌
    /// </summary>
    [Serializable]
    public struct ShuffleWitness : IWitness
    {
        public int number { get; set; }
        /// <summary>
        /// 玩家索引
        /// </summary>
        public int playerIndex { get; }
        /// <summary>
        /// 所洗的牌堆名
        /// </summary>
        public string pileName { get; }
        /// <summary>
        /// 洗牌的结果
        /// </summary>
        public CardInstance[] cards { get; }
        public ShuffleWitness(int playerIndex, string pileName, CardInstance[] cards)
        {
            number = 0;
            this.playerIndex = playerIndex;
            this.pileName = pileName;
            this.cards = cards;
        }
    }
}