using System;
using TouhouCardEngine;

namespace Game
{
    public class CardPropertySetterName : NodeConstProxyString
    {
        public CardPropertySetterName(string name) : base(name)
        {
        }

        public static explicit operator CardPropertySetterName(string str)
        {
            return new CardPropertySetterName(str);
        }
    }
    public class CardPropertyGetterName : NodeConstProxyString
    {
        public CardPropertyGetterName(string name) : base(name)
        {
        }

        public static explicit operator CardPropertyGetterName(string str)
        {
            return new CardPropertyGetterName(str);
        }
    }
    public class CardDefinePropertyGetterName : NodeConstProxyString
    {
        public CardDefinePropertyGetterName(string name) : base(name)
        {
        }

        public static explicit operator CardDefinePropertyGetterName(string str)
        {
            return new CardDefinePropertyGetterName(str);
        }
    }
    public class CardDefinePropertySetterName : NodeConstProxyString
    {
        public CardDefinePropertySetterName(string name) : base(name)
        {
        }

        public static explicit operator CardDefinePropertySetterName(string str)
        {
            return new CardDefinePropertySetterName(str);
        }
    }
    [Serializable]
    public struct CardPropertyNameItem
    {
        public string value;
        public string name;
        public bool canGet;
        public bool canSet;
        public bool canDefineGet;
        public bool canDefineSet;
        public CardPropertyNameItem(string value, string name, bool canGet = true, bool canSet = false, bool canDefineGet = true, bool canDefineSet = true)
        {
            this.value = value;
            this.name = name;
            this.canGet = canGet;
            this.canSet = canSet;
            this.canDefineGet = canDefineGet;
            this.canDefineSet = canDefineSet;
        }
    }
}
