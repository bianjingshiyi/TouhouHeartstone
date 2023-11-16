using System;
using TouhouCardEngine;

namespace Game
{
    [Serializable]
    public class EventReferenceProxy : NodeConstProxy<EventReference>
    {
        public EventReferenceProxy(EventReference value) : base(value)
        {
        }
        public static explicit operator EventReferenceProxy(string str)
        {
            return new EventReferenceProxy(new EventReference(0, str));
        }
    }
}
