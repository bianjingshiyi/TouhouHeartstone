using System;

namespace TouhouHeartstone
{
    /// <summary>
    /// 替换初始手牌的Witness
    /// </summary>
    [Obsolete("除了EventWitness以外的其他Witness都被废除了")]
    [Serializable]
    public struct OldInitReplaceWitness : IWitness
    {
        public int number { get; set; }
        public int playerId { get; }
        public CardInstance[] originCards { get; }
        public CardInstance[] replaceCards { get; }
        public OldInitReplaceWitness(int playerId, CardInstance[] originCards, CardInstance[] replaceCards)
        {
            number = 0;
            this.playerId = playerId;
            this.originCards = originCards;
            this.replaceCards = replaceCards;
        }
        public override string ToString()
        {
            string s = "玩家" + playerId + "将手牌：";
            for (int i = 0; i < originCards.Length; i++)
            {
                s += originCards[i] + "，";
            }
            s += "替换为：";
            for (int i = 0; i < replaceCards.Length; i++)
            {
                s += replaceCards[i] + (i != replaceCards.Length - 1 ? "，" : "。");
            }
            return s;
        }
    }
}