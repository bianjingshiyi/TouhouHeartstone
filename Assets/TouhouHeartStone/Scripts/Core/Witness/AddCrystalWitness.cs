using System;

namespace TouhouHeartstone
{
    [Serializable]
    class AddCrystalWitness : Witness
    {
        public int playerId { get; }
        public int count { get; }
        public CrystalState state { get; }
        public AddCrystalWitness(int playerId, int count, CrystalState state)
        {
            this.playerId = playerId;
            this.count = count;
            this.state = state;
        }
        public override string ToString()
        {
            string s;
            switch (state)
            {
                case CrystalState.empty:
                    s = "空的";
                    break;
                case CrystalState.locked:
                    s = "锁住的";
                    break;
                default:
                    s = "";
                    break;
            }
            return "玩家" + playerId + "获得" + count + "个" + s + "灵力珠";
        }
    }
}