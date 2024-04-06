using TouhouCardEngine.Interfaces;
using System;
using MessagePack;

namespace TouhouHeartstone
{
    [Union(0, typeof(AttackResponse))]
    [Union(1, typeof(DiscoverResponse))]
    [Union(2, typeof(InitReplaceResponse))]
    [Union(3, typeof(SurrenderResponse))]
    [Union(4, typeof(TurnEndResponse))]
    [Union(5, typeof(UseResponse))]
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