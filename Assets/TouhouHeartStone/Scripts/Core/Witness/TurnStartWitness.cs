using System;

namespace TouhouHeartstone
{
    [Serializable]
    public class TurnStartWitness : Witness
    {
        public int playerId { get; }
        public TurnStartWitness(int playerId)
        {
            this.playerId = playerId;
        }
        public override string ToString()
        {
            return "玩家" + playerId + "的回合开始";
        }
    }
}