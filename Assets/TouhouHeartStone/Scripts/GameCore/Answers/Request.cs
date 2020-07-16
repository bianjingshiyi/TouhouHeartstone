using TouhouCardEngine.Interfaces;
using System;
namespace TouhouHeartstone
{
    [Serializable]
    public abstract class Request : IRequest
    {
        public int[] playersId { get; set; }
        public bool isAny { get; set; }
        public float timeout { get; set; }
        public abstract IResponse getDefaultResponse(IGame game, int playerId);
        public abstract bool isValidResponse(IResponse response);
    }
}