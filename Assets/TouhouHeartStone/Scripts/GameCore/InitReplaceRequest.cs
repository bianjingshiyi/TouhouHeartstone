using System;

namespace TouhouHeartstone.Backend
{
    [Serializable]
    class InitReplaceRequest
    {
        public int[] cards { get; }
        public InitReplaceRequest(int[] cards)
        {
            this.cards = cards;
        }
    }
}