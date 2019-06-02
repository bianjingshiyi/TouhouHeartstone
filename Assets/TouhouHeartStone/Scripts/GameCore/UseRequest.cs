using System;

namespace TouhouHeartstone.Backend
{
    [Serializable]
    class UseRequest
    {
        public int instance { get; }
        public int position { get; }
        public int target { get; }
        public UseRequest(int instance, int position, int target)
        {
            this.instance = instance;
            this.position = position;
            this.target = target;
        }
    }
}