using System;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [Serializable]
    public class FreeActRequest : Request
    {
        public override IResponse getDefaultResponse(IGame game, int playerId)
        {
            return new TurnEndResponse()
            {
                playerId = playersId[0]
            };
        }
        public override bool isValidResponse(IResponse response)
        {
            return
                response is UseResponse ||
                response is AttackResponse ||
                response is TurnEndResponse;
        }
    }
}