using System;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [Serializable]
    public class InitReplaceRequest : Request
    {
        public override IResponse getDefaultResponse(IGame game, int playerId)
        {
            return new InitReplaceResponse()
            {
                playerId = playersId[0],
                cardsId = new int[0]
            };
        }
        public override bool isValidResponse(IResponse response)
        {
            return response is InitReplaceResponse;
        }
    }
}