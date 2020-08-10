using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Linq;
namespace TouhouHeartstone
{
    public class KeywordsModifier : PropModifier<string[]>
    {
        public override string propName { get; } = nameof(ServantCardDefine.keywords);
        public string[] keywords { get; }
        public enum ModifyType
        {
            add, remove, set
        }
        public ModifyType modifyType { get; }
        public KeywordsModifier(ModifyType modifyType, params string[] keywords)
        {
            this.modifyType = modifyType;
            this.keywords = keywords;
        }
        protected KeywordsModifier(KeywordsModifier origin)
        {
            keywords = origin.keywords;
            modifyType = origin.modifyType;
        }
        public override string[] calc(IGame game, Card card, string[] value)
        {
            switch (modifyType)
            {
                case ModifyType.set:
                    return keywords;
                case ModifyType.remove:
                    return value != null ? value.Except(keywords).ToArray() : new string[0];
                default:
                    return value != null ? value.Union(keywords).ToArray() : keywords;
            }
        }
        public override PropModifier clone()
        {
            return new KeywordsModifier(this);
        }
        public override string ToString()
        {
            switch (modifyType)
            {
                case ModifyType.set:
                    return "keywords = " + (keywords.Length > 0 ? string.Join(",", keywords) : "empty");
                case ModifyType.remove:
                    return "keywords - " + (keywords.Length > 0 ? string.Join(",", keywords) : "empty");
                default:
                    return "keywords + " + (keywords.Length > 0 ? string.Join(",", keywords) : "empty");
            }
        }
    }
}