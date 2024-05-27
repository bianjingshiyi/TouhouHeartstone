using System;
using System.Collections.Generic;
using System.Linq;
using Game;
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
            cardPoolNameKey = origin.cardPoolNameKey;
            imageResPathKey = origin.imageResPathKey;
            version = origin.version;
            versionNumber = origin.versionNumber;
            editTime = origin.editTime;
            dependencyRefs = origin.dependencyRefs != null ? Array.ConvertAll(origin.dependencyRefs, r => new CardPoolReference(r)) : null;
            texturesDir = origin.texturesDir;
            dependencies = (long[])origin.dependencies?.Clone();
            localizedStrings.AddRange(origin.localizedStrings.Select(s => new LocalizedString(s)));
            localizedResources.AddRange(origin.localizedResources.Select(s => new LocalizedResource(s)));
        }
        public string getCardPoolName(string language)
        {
            return localizedStrings.localize(cardPoolNameKey, language);
        }
        public string getCardPoolImagePath(string language)
        {
            return localizedResources.localize(imageResPathKey, language);
        }
        public LocalizedString getLocalizedString(string key)
        {
            return localizedStrings.getLocalizedObject<string, LocalizedString>(key);
        }
        public LocalizedResource getLocalizedResource(string key)
        {
            return localizedResources.getLocalizedObject<string, LocalizedResource>(key);
        }
        public long cardPoolId;
        public string cardPoolNameKey;
        public string imageResPathKey;
        [Obsolete("使用versionNumber。")]
        public float version;
        public int versionNumber;
        public int editTime;
        public CardPoolReference[] dependencyRefs;
        [BsonIgnoreIfNull]
        public string texturesDir;
        [BsonIgnoreIfNull]
        public string soundsDir;
        [BsonIgnoreIfNull]
        [Obsolete("使用dependencyRefs。")]
        public long[] dependencies;
        public List<LocalizedString> localizedStrings = new List<LocalizedString>();
        public List<LocalizedResource> localizedResources = new List<LocalizedResource>();
        long ICardPoolReference.CardPoolID => cardPoolId;

        int ICardPoolReference.CardPoolVersion => editTime;
    }
}
