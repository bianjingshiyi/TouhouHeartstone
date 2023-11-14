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
        [Obsolete("使用versionNumber。")]
        public float version;
        public int versionNumber;
        public int editTime;
        public CardPoolReference[] dependencyRefs;
        [BsonIgnoreIfNull]
        public string texturesDir;
        [BsonIgnoreIfNull]
        [Obsolete("使用dependencyRefs。")]
        public long[] dependencies;
        long ICardPoolReference.CardPoolID => cardPoolId;

        int ICardPoolReference.CardPoolVersion => editTime;
    }
    [Serializable]
    public class SerializableCardPoolInfo
    {
        public SerializableCardPoolInfo(CardPoolInfo info)
        {
            cardPoolId = info.cardPoolId;
            cardPoolName = info.cardPoolName;
            imageResPath = info.imageResPath;
            versionNumber = info.versionNumber;
            editTime = info.editTime;
            dependencyRefs = info.dependencyRefs;
            texturesDir = info.texturesDir;
        }
        public CardPoolInfo toInfo()
        {
            return new CardPoolInfo()
            {
                cardPoolId = cardPoolId,
                cardPoolName = cardPoolName,
                imageResPath = imageResPath,
                versionNumber = versionNumber,
                editTime = editTime,
                dependencyRefs = dependencyRefs,
                texturesDir = texturesDir
            };
        }
        public long cardPoolId;
        public string cardPoolName;
        public string imageResPath;
        public int versionNumber;
        public int editTime;
        public CardPoolReference[] dependencyRefs;
        public string texturesDir;
    }
}
