using MessagePack;
using System;

namespace TouhouHeartstone
{
    [Serializable]
    [MessagePackObject]
    public class InitReplaceResponse : Response
    {
        [Key(3)]
        public int[] cardsId { get; set; }
    }
}