using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class KeywordsModifier : PropModifier<string[]>
    {
        public KeywordsModifier(ModifyType modifyType, params string[] keywords)
        {
            propertyName = DEFINE.KEYWORDS;
            this.modifyType = modifyType;
            defaultValue = keywords;
        }
        public override async Task afterAdd(IGame game, Card card, Buff buff)
        {
            await base.afterAdd(game, card, buff);
            var value = getValueGeneric(buff);
            if (modifyType != ModifyType.remove)
            {
                if (value.Contains(KEYWORD.SHIELD))
                {
                    await card.addKeywords(game, KEYWORD.HAS_SHIELD);
                }
                if (value.Contains(KEYWORD.STEALTH))
                {
                    await card.addKeywords(game, KEYWORD.HAS_STEALTH);
                }
                if (value.Contains(KEYWORD.FROZEN))
                {
                    await card.addKeywords(game, KEYWORD.IS_FROZEN);
                }
            }
            await card.updateConsumableKeywords(game);
        }
        public override async Task afterRemove(IGame game, Card card, Buff buff)
        {
            await base.afterRemove(game, card, buff);
            await card.updateConsumableKeywords(game);
        }
        public override async Task updateModifierValue(IGame game, Card card, Buff buff, string[] beforeValue)
        {
            await card.updateConsumableKeywords(game);
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
        public ModifyType modifyType;
        public enum ModifyType
        {
            add, remove, set
        }
    }
}