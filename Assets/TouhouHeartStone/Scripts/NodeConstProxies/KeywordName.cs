using TouhouCardEngine;

namespace Game
{
    public class KeywordName : NodeConstProxyString
    {
        public KeywordName(string name) : base(name)
        {
        }

        public static explicit operator KeywordName(string str)
        {
            return new KeywordName(str);
        }
    }
}
