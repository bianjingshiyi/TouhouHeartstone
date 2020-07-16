using System;
namespace TouhouHeartstone
{
    [Serializable]
    public class AttackResponse : Response
    {
        public int cardId { get; set; }
        public int targetId { get; set; }
    }
}