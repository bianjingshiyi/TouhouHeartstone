using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Game
{
    [Serializable]
    public class EffectTag : NodeConstProxyString
    {
        [Obsolete("请使用value。该属性仅用于序列化兼容，之后会移除该属性")]
        [BsonElement]
        [BsonIgnoreIfNull]
        public string effectTag
        {
            get => null;
            set => this.value = value;
        }
        public EffectTag(string value) : base(value)
        {

        }
        public static explicit operator EffectTag(string value)
        {
            return new EffectTag(value);
        }
    }
}
