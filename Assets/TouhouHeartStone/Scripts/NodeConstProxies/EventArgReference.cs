using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Game
{
    [Serializable]
    public class EventArgReference : NodeConstProxyString
    {
        [Obsolete("请使用value。该属性仅用于序列化兼容，之后会移除该属性")]
        [BsonElement]
        [BsonIgnoreIfNull]
        public string eventName
        {
            get => null;
            set => this.value = value;
        }
        public EventArgReference(string value) : base(value)
        {

        }
        public static explicit operator EventArgReference(string str)
        {
            return new EventArgReference(str);
        }
    }
}
