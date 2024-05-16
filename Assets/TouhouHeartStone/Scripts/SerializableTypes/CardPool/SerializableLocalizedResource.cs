using System;
using MessagePack;

namespace Game
{
    [Serializable]
    [MessagePackObject]
    public class SerializableLocalizedResource : SerializableLocalizedObject<string>
    {
        public SerializableLocalizedResource() : base()
        {

        }
        public SerializableLocalizedResource(LocalizedResource str) : base(str)
        {
        }
        public LocalizedResource deserialize()
        {
            var result = new LocalizedResource(value);
            applyDeserialize(result);
            return result;
        }
    }
}
