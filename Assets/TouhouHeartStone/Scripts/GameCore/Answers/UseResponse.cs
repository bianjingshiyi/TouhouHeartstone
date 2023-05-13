using MessagePack;
using System;
namespace TouhouHeartstone
{
    [Serializable]
    [MessagePackObject]
    public class UseResponse : Response
    {
        [Key(3)]
        public int cardId { get; set; }
        [Key(4)]
        public int position { get; set; }
        [Key(5)]
        public int[] targetsId { get; set; } = new int[0];
    }
}