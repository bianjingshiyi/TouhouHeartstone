using System;

namespace TouhouHeartstone
{
    [Serializable]
    public class RemoveCrystalWitness : Witness
    {
        public int playerId { get; }
        public int count { get; }
        public RemoveCrystalWitness(int playerId, int count)
        {
            this.playerId = playerId;
            this.count = count;
        }
        public override string ToString()
        {
            return "玩家" + playerId + "失去" + count + "个灵力珠";
        }
    }
}