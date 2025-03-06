using System;

namespace Game
{
    public class LocalizedString : LocalizedObject<string>, ICloneable
    {
        public LocalizedString(string value) : base(value) { }

        public LocalizedString(LocalizedString other) : base(other)
        {
        }
        public LocalizedString Clone()
        {
            return new LocalizedString(this);
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
