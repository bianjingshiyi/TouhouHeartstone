using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public interface IEntryNodeEffect
    {
        Node entryNode { get; }
    }
    public interface IEntryNode
    {
        ControlOutput getActionOutputPort();
    }
}
