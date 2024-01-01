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
                var choice = game.responseRandomInt(0, cardRefArray.Length);
                return new DiscoverResponse() 
                { 
                    playerId = playerId,
                    cardDefRef = cardRefArray[choice] 
                };
            }
            else
            {
                var choice = game.responseRandomInt(0, cardIdArray.Length);
                return new DiscoverResponse()
                {
                    playerId = playerId,
                    cardId = cardIdArray[choice] 
                };
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
