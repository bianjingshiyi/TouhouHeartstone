using System;

namespace TouhouHeartstone
{
    [Obsolete("除了EventWitness以外的其他Witness都被废除了")]
    [Serializable]
    public struct SetDeckWitness : IWitness
    {
        public int number { get; set; }
        public int playerId { get; }
        public int count { get; }

        public SetDeckWitness(int playerId, int count)
        {
            number = 0;
            this.playerId = playerId;
            this.count = count;
        }
        public override string ToString()
        {
            return "玩家" + playerId + "的卡组大小设置为" + count + "。";
        }
    }
}