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
                await card.InitConsumableKeywords(value, game);
            }
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