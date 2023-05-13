using TouhouCardEngine.Interfaces;
using System;
using MessagePack;

namespace TouhouHeartstone
{
    [Serializable]
    [MessagePackObject]
    public abstract class Response : IResponse, INetworkData
    {
        [Key(0)]
        public int playerId { get; set; }
        [Key(1)]
        public bool isUnasked { get; set; }
        [Key(2)]
        public float remainedTime { get; set; }
    }
}