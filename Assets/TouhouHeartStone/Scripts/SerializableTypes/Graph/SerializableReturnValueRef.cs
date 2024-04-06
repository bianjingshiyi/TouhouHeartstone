using MessagePack;
using System;

namespace Game
{
    [MessagePackObject]
    [Obsolete]
    [Serializable]
    public class SerializableReturnValueRef
    {
        [Key(0)]
        public SerializableActionValueRef valueRef;
        [Key(1)]
        public int returnIndex;
    }
}
