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
        public override int GetHashCode()
        {
            return instanceId;
        }
        public override bool Equals(object obj)
        {
            return obj is CardInstance && ((CardInstance)obj).instanceId == instanceId;
        }
        public override string ToString()
        {
            return "Card(" + instanceId + ")<" + cardId + ">";
        }
        public static bool operator ==(CardInstance a, CardInstance b)
        {
            return a.instanceId == b.instanceId;
        }
        public static bool operator !=(CardInstance a, CardInstance b)
        {
            return a.instanceId != b.instanceId;
        }
        public static implicit operator CardInstance[](CardInstance instance)
        {
            return new CardInstance[] { instance };
        }
    }
}