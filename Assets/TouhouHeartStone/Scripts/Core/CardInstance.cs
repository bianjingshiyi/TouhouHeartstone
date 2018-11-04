using System;

namespace TouhouHeartstone
{
    [Serializable]
    public class CardInstance
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
            return obj is CardInstance && (obj as CardInstance).instanceId == instanceId;
        }
        public override string ToString()
        {
            return "Card(" + instanceId + ")<" + cardId + ">";
        }
    }
}