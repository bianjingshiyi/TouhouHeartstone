namespace Game
{
    public abstract class CardPropertyName
    {
        public string name { get; set; }
        public CardPropertyName(string name)
        {
            this.name = name;
        }
        public override bool Equals(object obj)
        {
            return name.Equals(obj);
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
        public override string ToString()
        {
            return name;
        }
        public static implicit operator string(CardPropertyName propName)
        {
            return propName.name;
        }
    }
    public class CardPropertySetterName : CardPropertyName
    {
        public CardPropertySetterName(string name) : base(name)
        {
        }

        public static explicit operator CardPropertySetterName(string str)
        {
            return new CardPropertySetterName(str);
        }
    }
    public class CardPropertyGetterName : CardPropertyName
    {
        public CardPropertyGetterName(string name) : base(name)
        {
        }

        public static explicit operator CardPropertyGetterName(string str)
        {
            return new CardPropertyGetterName(str);
        }
    }
    public struct CardPropertyNameItem
    {
        public string value;
        public string name;
        public bool canGet;
        public bool canSet;
        public CardPropertyNameItem(string value, string name, bool canGet = true, bool canSet = false)
        {
            this.value = value;
            this.name = name;
            this.canGet = canGet;
            this.canSet = canSet;
        }
    }
}
