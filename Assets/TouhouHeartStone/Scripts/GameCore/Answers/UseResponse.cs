using System;
namespace TouhouHeartstone
{
    [Serializable]
    public class UseResponse : Response
    {
        public int cardId { get; set; }
        public int position { get; set; }
        public int[] targetsId { get; set; }
    }
}