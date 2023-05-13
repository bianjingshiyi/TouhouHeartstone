using MessagePack;
using System;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    [Serializable]
    [MessagePackObject]
    public class DiscoverResponse : Response
    {
        [Key(3)]
        public int cardId;
        [Key(4)]
        public DefineReference cardDefRef;
    }
}
