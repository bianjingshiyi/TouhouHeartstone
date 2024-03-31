using MessagePack;
using System;
using TouhouCardEngine;
using UI;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableSkinSoundRef
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableSkinSoundRef()
        {
        }
        public SerializableSkinSoundRef(SkinSoundRef soundRef)
        {
            name = soundRef.name;
            path = soundRef.path;
        }
        public SkinSoundRef toRef()
        {
            return new SkinSoundRef(name, path);
        }
        [Key(0)]
        public string name;
        [Key(1)]
        public string path;
    }
}
