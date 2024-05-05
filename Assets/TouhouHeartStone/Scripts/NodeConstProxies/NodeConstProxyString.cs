using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Union(0, typeof(CardPropertySetterName))]
    [Union(1, typeof(CardPropertyGetterName))]
    [Union(2, typeof(CardDefinePropertyGetterName))]
    [Union(3, typeof(CardType))]
    [Union(4, typeof(EffectTag))]
    [Union(5, typeof(EventArgReference))]
    [Union(6, typeof(EventVarName))]
    [Union(7, typeof(KeywordName))]
    [Union(8, typeof(NodeConstProxyString))]
    [Union(9, typeof(PileName))]
    [Union(10, typeof(TypeName))]
    public abstract class NodeConstProxyString : INodeConstProxy<string>
    {
        [Obsolete("仅用于序列化兼容，之后会移除该属性")]
        [BsonElement("<value>k__BackingField")]
        [BsonIgnoreIfDefault]
        private string valueBackingField
        {
            get => default;
            set => _value = value;
        }
        [BsonElement]
        private string _value;
        [BsonIgnore]
        [Key(0)]
        public string value
        {
            get => _value;
            set => _value = value;
        }
        public NodeConstProxyString(string value)
        {
            this.value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is INodeConstProxy<string> proxy)
                return value.Equals(proxy.value);
            if (obj is string proxyValue)
                return value.Equals(proxyValue);
            return value.Equals(obj);
        }
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public static bool operator ==(NodeConstProxyString lhs, NodeConstProxyString rhs)
        {
            if (lhs is null || lhs.value == null)
                return rhs is null || rhs.value == null;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(NodeConstProxyString lhs, NodeConstProxyString rhs)
        {
            return !(lhs == rhs);
        }
        public static implicit operator string(NodeConstProxyString proxy)
        {
            if (proxy == null)
                return default;
            return proxy.value;
        }
        public override string ToString()
        {
            return value;
        }
    }
}
