using System;

namespace TouhouHeartstone
{
    [Obsolete("除了EventWitness以外的其他Witness都被废除了")]
    [Serializable]
    public struct SetOrderWitness : IWitness
    {
        public int number { get; set; }
        public int[] orderedPlayerId { get; }
        public SetOrderWitness(int[] orderedPlayerId)
        {
            number = 0;
            this.orderedPlayerId = orderedPlayerId;
        }
        public override string ToString()
        {
            string s = "玩家行动顺序为：";
            for (int i = 0; i < orderedPlayerId.Length; i++)
            {
                s += orderedPlayerId[i];
                if (i < orderedPlayerId.Length - 1)
                    s += "，";
                else
                    s += "。";
            }
            return s;
        }
    }
}