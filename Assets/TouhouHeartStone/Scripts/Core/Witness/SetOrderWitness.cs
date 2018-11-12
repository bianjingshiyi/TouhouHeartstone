using System;

namespace TouhouHeartstone
{
    [Serializable]
    public class SetOrderWitness : Witness
    {
        public int[] orderedPlayerId { get; } = null;
        public SetOrderWitness(int[] orderedPlayerId)
        {
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