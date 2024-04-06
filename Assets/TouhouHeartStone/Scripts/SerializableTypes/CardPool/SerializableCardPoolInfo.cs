using Controllers;
using MessagePack;
using System;
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
        [Key(0)]
        public long cardPoolId;
        [Key(1)]
        public string cardPoolName;
        [Key(2)]
        public string imageResPath;
        [Key(3)]
        public int versionNumber;
        [Key(4)]
        public int editTime;
        [Key(5)]
        public CardPoolReference[] dependencyRefs;
        [Key(6)]
        public string texturesDir;
    }
}
