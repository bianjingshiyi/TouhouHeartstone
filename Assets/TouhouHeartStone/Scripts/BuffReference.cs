using MessagePack;
using System;
using TouhouCardEngine;

namespace Game
{
    [Serializable]
    public class BuffReference : DefineReference
    {
        public BuffReference(long cardPoolId, int defineId)
        {
            this.cardPoolId = cardPoolId;
            this.defineId = defineId;
        }
    }
}
