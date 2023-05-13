using MessagePack;

namespace TouhouHeartstone
{
    /// <summary>
    /// 所有的网络Request/Response都必须在这里注册，否则将无法使用
    /// </summary>
    [Union(0, typeof(DiscoverRequest))]
    [Union(1, typeof(FreeActRequest))]
    [Union(2, typeof(InitReplaceRequest))]
    [Union(3, typeof(SurrenderRequest))]
    [Union(100, typeof(AttackResponse))]
    [Union(101, typeof(DiscoverResponse))]
    [Union(102, typeof(InitReplaceResponse))]
    [Union(103, typeof(SurrenderResponse))]
    [Union(104, typeof(TurnEndResponse))]
    [Union(105, typeof(UseResponse))]
    public interface INetworkData { }
}