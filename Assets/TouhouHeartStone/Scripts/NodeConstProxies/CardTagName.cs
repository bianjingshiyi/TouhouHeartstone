using TouhouCardEngine;

namespace Game
{
    public class CardTagName : NodeConstProxyString
    {
        public CardTagName(string name) : base(name)
        {
        }

        public static explicit operator CardTagName(string str)
        {
            return new CardTagName(str);
        }
    }
}
