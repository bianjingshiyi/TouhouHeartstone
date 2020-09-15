using System;
namespace UI
{
    [Serializable]
    public class SpaceNotEnoughException : Exception
    {
        public SpaceNotEnoughException() { }
        public SpaceNotEnoughException(string message) : base(message) { }
        public SpaceNotEnoughException(string message, Exception inner) : base(message, inner) { }
        protected SpaceNotEnoughException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
