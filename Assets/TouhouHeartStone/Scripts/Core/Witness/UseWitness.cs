using System;

namespace TouhouHeartstone
{
    [Obsolete("除了EventWitness以外的其他Witness都被废除了")]
    [Serializable]
    public class UseWitness : IWitness
    {
        public int number { get; set; }
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
}