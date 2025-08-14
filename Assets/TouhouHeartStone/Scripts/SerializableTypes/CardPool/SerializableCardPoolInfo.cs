using Controllers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableCardPoolInfo
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableCardPoolInfo()
        {
        }
        public SerializableCardPoolInfo(CardPoolInfo info)
        {
            cardPoolId = info.cardPoolId;
            cardPoolName = info.cardPoolNameKey;
            imageResPath = info.imageResPathKey;
            versionNumber = info.versionNumber;
            editTime = info.editTime;
            dependencyRefs = info.dependencyRefs;
            texturesDir = info.texturesDir;
            soundsDir = info.soundsDir;
            localizedStrings = info.localizedStrings.Count >= 0 ? info.localizedStrings.ConvertAll(r => new SerializableLocalizedString(r)) : null;
            localizedStrings?.Sort((a, b) => GBStringComparer.Default.Compare(a.value, b.value));
            localizedResources = info.localizedResources.Count >= 0 ? info.localizedResources.ConvertAll(r => new SerializableLocalizedResource(r)) : null;
            localizedResources?.Sort((a, b) => GBStringComparer.Default.Compare(a.value, b.value));
        }
        public CardPoolInfo toInfo()
        {
            return new CardPoolInfo()
            {
                cardPoolId = cardPoolId,
                cardPoolNameKey = cardPoolName,
                imageResPathKey = imageResPath,
                versionNumber = versionNumber,
                editTime = editTime,
                dependencyRefs = dependencyRefs,
                texturesDir = texturesDir,
                soundsDir = soundsDir,
                localizedStrings = localizedStrings?.ConvertAll(s => s.deserialize()) ?? new List<LocalizedString>(),
                localizedResources = localizedResources?.ConvertAll(s => s.deserialize()) ?? new List<LocalizedResource>(),
            };
        }
        [Key(0)]
        public int versionNumber;
        [Key(1)]
        public long cardPoolId;
        [Key(2)]
        public int editTime;
        [Key(3)]
        public string cardPoolName;
        [Key(4)]
        public string imageResPath;
        [Key(5)]
        public CardPoolReference[] dependencyRefs;
        [Key(6)]
        public List<SerializableLocalizedString> localizedStrings;
        [Key(7)]
        public List<SerializableLocalizedResource> localizedResources;
        [IgnoreMember]
        public string texturesDir;
        [IgnoreMember]
        public string soundsDir;
    }
}
