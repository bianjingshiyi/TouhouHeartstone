using System;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [Serializable]
    public class DiscoverRequest : Request
    {
        public string title;
        public int[] cardIdArray;
        public override IResponse getDefaultResponse(IGame game, int playerId)
        {
            return new DiscoverResponse() { cardId = cardIdArray[game.randomInt(0, cardIdArray.Length - 1)] };
        }
        public override bool isValidResponse(IResponse response)
        {
            return response is DiscoverResponse;
        }
        public DiscoverRequest(int[] cardIdArray, string title)
        {
            this.cardIdArray = cardIdArray;
            this.title = title;
        }
    }
}
