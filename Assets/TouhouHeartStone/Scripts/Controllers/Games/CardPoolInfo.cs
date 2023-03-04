using System;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
using TouhouCardEngine.Shared;

namespace Controllers
{
    [Serializable]
    public class CardPoolInfo : ICardPoolReference
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
        long ICardPoolReference.CardPoolID => cardPoolId;

        int ICardPoolReference.CardPoolVersion => editTime;
    }
}
