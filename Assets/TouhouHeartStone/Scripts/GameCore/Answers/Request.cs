using TouhouCardEngine.Interfaces;
using System;
using MessagePack;

namespace TouhouHeartstone
{
    [Serializable]
    [MessagePackObject]
    public abstract class Request : IRequest, INetworkData
    {
        [Key(0)]
        public int[] playersId { get; set; }
        [Key(1)]
        public bool isAny { get; set; }
        [Key(2)]
        public float timeout { get; set; }
        public abstract IResponse getDefaultResponse(IGame game, int playerId);
        public abstract bool isValidResponse(IResponse response);
    }
}