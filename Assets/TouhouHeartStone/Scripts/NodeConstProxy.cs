namespace Game
{
    public abstract class NodeConstProxy<T>
    {
        public T value { get; set; }
        public NodeConstProxy(T value)
        {
            this.value = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is NodeConstProxy<T> proxy)
                return value.Equals(proxy.value);
            if (obj is T proxyValue)
                return value.Equals(proxyValue);
            return value.Equals(obj);
        }
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public static bool operator ==(NodeConstProxy<T> lhs, NodeConstProxy<T> rhs)
        {
            if (lhs is null || lhs.value == null)
                return rhs is null || rhs.value == null;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(NodeConstProxy<T> lhs, NodeConstProxy<T> rhs)
        {
            return !(lhs == rhs);
        }
        public static implicit operator T(NodeConstProxy<T> proxy)
        {
            if (proxy == null)
                return default;
            return proxy.value;
        }
    }
    public abstract class NodeConstProxyString : NodeConstProxy<string>
    {
        public NodeConstProxyString(string value) : base(value)
        {
        }
        public override string ToString()
        {
            return value;
        }
    }
}
