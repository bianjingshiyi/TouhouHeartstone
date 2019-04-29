using System;

namespace TouhouHeartstone
{
    [Obsolete("除了EventWitness以外的其他Witness都被废除了")]
    [Serializable]
    public class DrawWitness : IWitness
    {
        public int number { get; set; }
        public int playerId { get; }
        public CardInstance[] cards { get; }
        public DrawWitness(int playerId, CardInstance[] cards)
        {
            this.playerId = playerId;
            this.cards = cards;
        }
        public override string ToString()
        {
            string s = "玩家" + playerId + "抽" + cards.Length + "张卡：";
            for (int i = 0; i < cards.Length; i++)
            {
                s += cards[i].ToString();
                if (i != cards.Length - 1)
                    s += "，";
                else
                    s += "。";
            }
            return s;
        }
    }
}