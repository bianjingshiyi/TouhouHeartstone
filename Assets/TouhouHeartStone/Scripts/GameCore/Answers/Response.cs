using TouhouCardEngine.Interfaces;
using System;
namespace TouhouHeartstone
{
    [Serializable]
    public class Response : IResponse
    {
        public int playerId { get; set; }
        public bool isUnasked { get; set; }
    }
}