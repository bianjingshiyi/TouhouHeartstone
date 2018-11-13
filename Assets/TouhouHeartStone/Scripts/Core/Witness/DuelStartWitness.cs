using System;

namespace TouhouHeartstone
{
    [Serializable]
    public class DuelStartWitness : Witness
    {
        public DuelStartWitness()
        {
        }
        public override string ToString()
        {
            return "对战开始";
        }
    }
}