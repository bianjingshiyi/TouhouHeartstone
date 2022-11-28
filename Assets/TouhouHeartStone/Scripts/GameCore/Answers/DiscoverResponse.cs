using System;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    [Serializable]
    public class DiscoverResponse : Response
    {
        public int cardId;
        public DefineReference cardDefRef;
    }
}
