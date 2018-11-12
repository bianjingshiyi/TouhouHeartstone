using System;

namespace TouhouHeartstone
{
    /// <summary>
    /// 替换初始手牌的Witness
    /// </summary>
    [Serializable]
    public class InitReplaceWitness : Witness
    {
        public int playerId { get; }
        public CardInstance[] originCards { get; }
        public CardInstance[] replaceCards { get; }
        public InitReplaceWitness(int playerId, CardInstance[] originCards, CardInstance[] replaceCards)
        {
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