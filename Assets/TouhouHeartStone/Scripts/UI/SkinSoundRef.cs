using System;
using TMPro;
using TouhouCardEngine;

namespace UI
{
    public class SkinSoundRef : ICloneable
    {
        /// <summary>
        /// 构造一个特效引用。
        /// </summary>
        public SkinSoundRef(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
        public SkinSoundRef(SkinSoundRef other)
        {
            name = other.name;
            path = other.path;
        }
        public SkinSoundRef Clone()
        {
            return new SkinSoundRef(this);
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public string name;
        public string path;
    }
}
