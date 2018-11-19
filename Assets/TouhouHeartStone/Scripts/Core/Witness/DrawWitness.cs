using System;

namespace TouhouHeartstone
{
    [Serializable]
    public class UseWitness : Witness
    {
        public int playerId { get; }
        public CardInstance card { get; }
        public int position { get; }
        public CardInstance target { get; }
        public UseWitness(int playerId, CardInstance card, int position, CardInstance target)
        {
            this.playerId = playerId;
            this.card = card;
            this.position = position;
            this.target = target;
        }
        public override string ToString()
        {
            return "玩家" + playerId + "使用了" + card;
        }
    }
    [Serializable]
    public class DrawWitness : Witness
    {
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