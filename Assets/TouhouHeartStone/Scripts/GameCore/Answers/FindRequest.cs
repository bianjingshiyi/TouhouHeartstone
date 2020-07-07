using System;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [Serializable]
    public class FindRequest : Request
    {
        int cards;
        Random r = new Random();

        public override IResponse getDefaultResponse(IGame game, int playerId)
        {
            return new FindResponse()
            {
                selectId = r.Next(0, cards)
            };
        }
        public override bool isValidResponse(IResponse response)
        {
            return response is FindResponse;
        }

        public FindRequest(int cards)
        {
            this.cards = cards;
        }
    }
}
