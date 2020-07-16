using System;
namespace UI
{
    [Serializable]
    public class ActorNotFoundException : Exception
    {
        public ActorNotFoundException() { }
        public ActorNotFoundException(TouhouCardEngine.Card card) : base("没有找到与" + card + "对应的Actor") { }
        public ActorNotFoundException(string message) : base(message) { }
        public ActorNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected ActorNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
