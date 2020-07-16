using System;

namespace TouhouHeartstone
{
    [Serializable]
    public class InitReplaceResponse : Response
    {
        public int[] cardsId { get; set; }
    }
}