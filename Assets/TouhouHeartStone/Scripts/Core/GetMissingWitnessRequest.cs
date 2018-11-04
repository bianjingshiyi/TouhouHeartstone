using System;

namespace TouhouHeartstone
{
    [Serializable]
    class GetMissingWitnessRequest
    {
        public int min { get; }
        public int max { get; }
        public GetMissingWitnessRequest(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
    }
}