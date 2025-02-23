using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class StringArrayModifier : PropModifier<string[]>
    {
        public StringArrayModifier(string propertyName, ModifyType modifyType, params string[] stringArray)
        {
            this.propertyName = propertyName;
            this.modifyType = modifyType;
            defaultValue = stringArray;
        }
        public override string[] calcGeneric(IGame game, ICardData card, string[] prop, string[] value)
        {
            switch (modifyType)
            {
                case ModifyType.set:
                    return value;
                case ModifyType.remove:
                    return prop != null ? prop.Except(value).ToArray() : new string[0];
                default:
                    return prop != null ? prop.Union(value).ToArray() : value;
            }
        }
        public override PropModifier Clone()
        {
            return new StringArrayModifier(propertyName, modifyType, defaultValue);
        }
        public ModifyType modifyType;
        public enum ModifyType
        {
            add, remove, set
        }
    }
}
