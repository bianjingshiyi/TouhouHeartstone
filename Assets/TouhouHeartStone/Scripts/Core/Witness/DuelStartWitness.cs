using System;

namespace TouhouHeartstone
{
    [Obsolete("除了EventWitness以外的其他Witness都被废除了")]
    [Serializable]
    public struct DuelStartWitness : IWitness
    {
        public int number { get; set; }
        public override string ToString()
        {
            return "对战开始";
        }
    }
}