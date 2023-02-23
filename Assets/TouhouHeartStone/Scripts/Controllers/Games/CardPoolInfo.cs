using System;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;

namespace Controllers
{
    [Serializable]
    public class CardPoolInfo
    {
        public long cardPoolId;
        public string cardPoolName;
        public string imageResPath;
        public float version;
        public int versionNumber;
        public int editTime;
        public CardPoolReference[] dependencyRefs;
        [BsonIgnoreIfNull]
        public long[] dependencies;

        public CardPoolReference GetReference()
        {
            return new CardPoolReference(cardPoolId, editTime);
        }
        public bool IsSameCardPool(CardPoolReference other)
        {
            if (other == null) 
                return false;
            return cardPoolId == other.cardPoolId && editTime == other.version;
        }
        public bool IsSameCardPool(CardPoolInfo other)
        {
            if (other == null)
                return false;
            return cardPoolId == other.cardPoolId && editTime == other.editTime;
        }
    }
}
