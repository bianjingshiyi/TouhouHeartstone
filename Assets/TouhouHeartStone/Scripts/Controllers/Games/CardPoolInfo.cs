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
    }
}
