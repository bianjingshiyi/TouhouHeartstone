using System.Linq;

namespace Game
{
    public class LocalizedResource : LocalizedObject<string>
    {
        public LocalizedResource(string value) : base(value) { }
    }
}
