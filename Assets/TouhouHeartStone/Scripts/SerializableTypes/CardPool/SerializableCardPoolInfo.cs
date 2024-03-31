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
            soundsDir = info.soundsDir;
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
                texturesDir = texturesDir,
                soundsDir = soundsDir
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
        [IgnoreMember]
        public string texturesDir;
        [IgnoreMember]
        public string soundsDir;
    }
}
