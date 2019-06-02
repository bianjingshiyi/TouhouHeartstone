using System;

namespace TouhouHeartstone
{
    [Serializable]
    public struct CardInstance
    {
        public int instanceId { get; }
        public int cardId { get; }
        public CardInstance(int instanceId, int cardId)
        {
            this.instanceId = instanceId;
            this.cardId = cardId;
        }
        public static implicit operator CardInstance[](CardInstance instance)
        {
            return new CardInstance[] { instance };
        }
    }
}