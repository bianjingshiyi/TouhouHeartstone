using System.Linq;

namespace Game
{
    public class LocalizedResource : LocalizedObject<string>
    {
        public LocalizedResource(string value) : base(value) { }

        public LocalizedResource(LocalizedResource other) : base(other)
        {
        }
    }
}
