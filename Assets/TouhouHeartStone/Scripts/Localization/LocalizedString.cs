namespace Game
{
    public class LocalizedString : LocalizedObject<string>
    {
        public LocalizedString(string value) : base(value) { }

        public LocalizedString(LocalizedString other) : base(other)
        {
        }
    }
}
