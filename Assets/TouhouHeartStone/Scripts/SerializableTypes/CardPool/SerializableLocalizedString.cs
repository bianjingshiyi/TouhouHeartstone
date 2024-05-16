using System;
using MessagePack;

namespace Game
{
    [Serializable]
    [MessagePackObject]
    public class SerializableLocalizedString : SerializableLocalizedObject<string>
    {
        public SerializableLocalizedString() : base()
        {
        }
        public SerializableLocalizedString(LocalizedString str) : base(str)
        {
        }
        public LocalizedString deserialize()
        {
            var result = new LocalizedString(value);
            applyDeserialize(result);
            return result;
        }
    }
}
