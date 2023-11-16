namespace Game
{
    public class PileName : NodeConstProxyString
    {
        public PileName(string name) : base(name)
        {
        }

        public static explicit operator PileName(string str)
        {
            return new PileName(str);
        }
    }
}
