using System;
using TouhouCardEngine;

namespace Game
{
    [Serializable]
    public class TypeName : NodeConstProxyString
    {
        public TypeName(string value) : base(value)
        {

        }
        public static explicit operator TypeName(string str)
        {
            return new TypeName(str);
        }
    }
}
