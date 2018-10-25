using System;

using UnityEngine;

namespace TouhouHeartstone
{
    [Serializable]
    class GetMissingWitnessRequest
    {
        public int min
        {
            get { return _min; }
        }
        [SerializeField]
        int _min;
        public int max
        {
            get { return _max; }
        }
        [SerializeField]
        int _max;
        public GetMissingWitnessRequest(int min, int max)
        {
            _min = min;
            _max = max;
        }
    }
}