using MessagePack;
using System;
namespace TouhouHeartstone
{
    [Serializable]
    [MessagePackObject]
    public class AttackResponse : Response
    {
        [Key(3)]
        public int cardId { get; set; }
        [Key(4)]
        public int targetId { get; set; }
    }
}