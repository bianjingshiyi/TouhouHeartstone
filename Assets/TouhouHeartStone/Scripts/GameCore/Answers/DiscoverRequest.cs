using MessagePack;
using System;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [Serializable]
    [MessagePackObject]
    public class DiscoverRequest : Request
    {
        [Key(3)]
        public string title;
        [Key(4)]
        public int[] cardIdArray;
        [Key(5)]
        public DefineReference[] cardRefArray;
        public override IResponse getDefaultResponse(IGame game, int playerId)
        {
            if (cardRefArray != null)
            {
                return new DiscoverResponse() { cardDefRef = cardRefArray[game.randomInt(0, cardRefArray.Length - 1)] };
            }
            else
            {

                return new DiscoverResponse() { cardId = cardIdArray[game.randomInt(0, cardIdArray.Length - 1)] };
            }
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
        public DiscoverRequest(DefineReference[] cardRefArray, string title)
        {
            this.cardRefArray = cardRefArray;
            this.title = title;
        }
    }
}
