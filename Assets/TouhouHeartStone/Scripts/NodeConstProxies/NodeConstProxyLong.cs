using MessagePack;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Union(0, typeof(CardPoolIDProxy))]
    public abstract class NodeConstProxyLong : INodeConstProxy<long>
    {
        [Key(0)]
        public long value { get; set; }
        public NodeConstProxyLong(long value)
        {
            this.value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is INodeConstProxy<long> proxy)
                return value.Equals(proxy.value);
            if (obj is long proxyValue)
                return value.Equals(proxyValue);
            return value.Equals(obj);
        }
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public static bool operator ==(NodeConstProxyLong lhs, NodeConstProxyLong rhs)
        {
            if (lhs is null)
                return rhs is null;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(NodeConstProxyLong lhs, NodeConstProxyLong rhs)
        {
            return !(lhs == rhs);
        }
        public static implicit operator long(NodeConstProxyLong proxy)
        {
            if (proxy == null)
                return default;
            return proxy.value;
        }
        public override string ToString()
        {
            return value.ToString();
        }
    }
}
