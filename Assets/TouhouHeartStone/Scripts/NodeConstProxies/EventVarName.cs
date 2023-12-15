using System;
using TouhouCardEngine;

namespace Game
{
    [Serializable]
    public class EventVarName : NodeConstProxyString
    {
        public EventVarName(string value) : base(value)
        {

        }
        public static explicit operator EventVarName(string str)
        {
            return new EventVarName(str);
        }
    }
}
