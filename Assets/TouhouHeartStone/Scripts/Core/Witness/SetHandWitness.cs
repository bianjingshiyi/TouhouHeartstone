using System;

namespace TouhouHeartstone
{
    /// <summary>
    /// 直接设置手牌的Witness
    /// </summary>
    [Serializable]
    public class SetHandWitness : Witness
    {
        public int playerId { get; }
        public CardInstance[] cards { get; }
        public SetHandWitness(int playerId, CardInstance[] cards)
        {
            this.playerId = playerId;
            this.cards = cards;
        }
        public override string ToString()
        {
            string s = "玩家" + playerId + "的手牌为：";
            for (int i = 0; i < cards.Length; i++)
            {
                s += cards[i] + (i != cards.Length - 1 ? "，" : "。");
            }
            return s;
        }
    }
}