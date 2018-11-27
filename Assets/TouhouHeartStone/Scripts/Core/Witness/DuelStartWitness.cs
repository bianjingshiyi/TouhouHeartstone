using System;

namespace TouhouHeartstone
{
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