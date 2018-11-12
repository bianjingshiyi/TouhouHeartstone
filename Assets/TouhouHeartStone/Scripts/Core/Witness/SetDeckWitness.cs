using System;

namespace TouhouHeartstone
{
    [Serializable]
    public class SetDeckWitness : Witness
    {
        public int playerId
        {
            get { return _playerId; }
        }
        int _playerId;
        public int count
        {
            get { return _count; }
        }
        int _count;
        public SetDeckWitness(int playerId, int count)
        {
            _playerId = playerId;
            _count = count;
        }
        public override string ToString()
        {
            return "玩家" + _playerId + "的卡组大小设置为" + _count + "。";
        }
    }
}