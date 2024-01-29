using TouhouCardEngine;

namespace Game
{
    public class CardType : NodeConstProxyString
    {
        public CardType(string name) : base(name)
        {
        }

        public static explicit operator CardType(string str)
        {
            return new CardType(str);
        }
    }
}