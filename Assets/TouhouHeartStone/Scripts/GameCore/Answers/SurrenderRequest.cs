using System;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [Serializable]
    public class SurrenderRequest : Request
    {
        public override IResponse getDefaultResponse(IGame game, int playerId)
        {
            return null;
        }
        public override bool isValidResponse(IResponse response)
        {
            return response is SurrenderResponse;
        }
    }
}