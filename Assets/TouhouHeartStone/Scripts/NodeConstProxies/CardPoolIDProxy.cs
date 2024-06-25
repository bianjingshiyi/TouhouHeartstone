using System;
using TouhouCardEngine;

namespace Game
{
    public class CardPoolIDProxy : NodeConstProxyLong
    {
        public CardPoolIDProxy(long value) : base(value)
        {
        }

        public static explicit operator CardPoolIDProxy(long value)
        {
            return new CardPoolIDProxy(value);
        }
    }
}
