using Game.Animations;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using UI;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializablePersistentAnimationRef
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializablePersistentAnimationRef()
        {
        }
        public SerializablePersistentAnimationRef(PersistentAnimationRef animRef)
        {
            this.animRef = animRef.animRef;
            colorModifies = animRef.colorModifies?.Select(d => new SerializableColorModifyData(d))?.ToArray();
        }
        public PersistentAnimationRef toRef()
        {
            return new PersistentAnimationRef(animRef)
            {
                colorModifies = colorModifies?.Select(d => d.deserialize())?.ToList() ?? new List<ColorModifyData>(),
            };
        }
        [Key(0)]
        public DefineReference animRef;
        [Key(1)]
        public SerializableColorModifyData[] colorModifies;
    }
}
