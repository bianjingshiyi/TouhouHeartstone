using TouhouCardEngine.Interfaces;
using System;
namespace TouhouHeartstone
{
    [Serializable]
    public abstract class Response : IResponse
    {
        public int playerId { get; set; }
        public bool isUnasked { get; set; }
        public float remainedTime { get; set; }
    }
}