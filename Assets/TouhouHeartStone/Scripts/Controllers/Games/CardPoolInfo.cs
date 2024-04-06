using System;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
using TouhouCardEngine.Shared;

namespace Controllers
{
    [Serializable]
    public class CardPoolInfo : ICardPoolReference
    {
        /// <summary>
        /// 序列化默认构造器
        /// </summary>
        public CardPoolInfo() { }
        /// <summary>
        /// 构造一个原卡池信息的深复制
        /// </summary>
        /// <param name="origin"></param>
        public CardPoolInfo(CardPoolInfo origin)
        {
            if (origin == null)
                throw new ArgumentNullException(nameof(origin));
            cardPoolId = origin.cardPoolId;
            cardPoolName = origin.cardPoolName;
            imageResPath = origin.imageResPath;
            version = origin.version;
            versionNumber = origin.versionNumber;
            editTime = origin.editTime;
            dependencyRefs = origin.dependencyRefs != null ? Array.ConvertAll(origin.dependencyRefs, r => new CardPoolReference(r)) : null;
            texturesDir = origin.texturesDir;
            dependencies = (long[])origin.dependencies?.Clone();
        }
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
}
