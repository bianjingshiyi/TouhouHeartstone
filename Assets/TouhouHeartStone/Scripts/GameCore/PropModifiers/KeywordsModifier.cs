using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class KeywordsModifier : StringArrayModifier
    {
        public KeywordsModifier(ModifyType modifyType, params string[] keywords) : base(DEFINE.KEYWORDS, modifyType, keywords)
        {
        }
        public override async Task afterAdd(IGame game, Card card, Buff buff)
        {
            await base.afterAdd(game, card, buff);
            var value = getValueGeneric(buff);
            if (modifyType != ModifyType.remove)
            {
                await card.InitConsumableKeywords(value, game);
            }
        }
        public override PropModifier Clone()
        {
            return new KeywordsModifier(modifyType, CollectionHelper.Clone(defaultValue));
        }
        public override string ToString()
        {
            var value = defaultValue;
            switch (modifyType)
            {
                case ModifyType.set:
                    return $"keywords = {(value.Length > 0 ? string.Join(",", value) : "empty")}";
                case ModifyType.remove:
                    return $"keywords - {(value.Length > 0 ? string.Join(",", value) : "empty")}";
                default:
                    return $"keywords + {(value.Length > 0 ? string.Join(",", value) : "empty")}";
            }
        }
    }
}