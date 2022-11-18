using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace TouhouHeartstone
{
    public class KeywordsModifier : PropModifier<string[]>
    {
        public KeywordsModifier(ModifyType modifyType, params string[] keywords)
        {
            propertyName = DEFINE.KEYWORDS;
            this.modifyType = modifyType;
            value = keywords;
        }
        protected KeywordsModifier(KeywordsModifier origin)
        {
            value = origin.value;
            modifyType = origin.modifyType;
        }
        public override Task afterAdd(IGame game, Card card)
        {
            Task task = base.afterAdd(game, card);
            if (value.Contains(KEYWORD.SHIELD))
            {
                card.setShield(game, true);
            }
            if (value.Contains(KEYWORD.STEALTH))
            {
                card.setStealth(game, true);
            }
            return task;
        }
        public override string[] calc(IGame game, Card card, string[] value)
        {
            switch (modifyType)
            {
                case ModifyType.set:
                    return this.value;
                case ModifyType.remove:
                    return value != null ? value.Except(this.value).ToArray() : new string[0];
                default:
                    return value != null ? value.Union(this.value).ToArray() : this.value;
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
                    return "keywords = " + (value.Length > 0 ? string.Join(",", value) : "empty");
                case ModifyType.remove:
                    return "keywords - " + (value.Length > 0 ? string.Join(",", value) : "empty");
                default:
                    return "keywords + " + (value.Length > 0 ? string.Join(",", value) : "empty");
            }
        }
        public ModifyType modifyType;
        public enum ModifyType
        {
            add, remove, set
        }
    }
}