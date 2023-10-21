using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System;
using ExcelLibrary.BinaryFileFormat;

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
            propertyName = origin.propertyName;
        }
        public override async Task afterAdd(IGame game, Card card)
        {
            await base.afterAdd(game, card);
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
        public override async Task afterRemove(IGame game, Card card)
        {
            await base.afterRemove(game, card);
            await card.updateConsumableKeywords(game);
        }
        public override async Task<IPropChangeEventArg> setValue(IGame game, Card card, string[] value)
        {
            var arg = await base.setValue(game, card, value);
            await card.updateConsumableKeywords(game);
            return arg;
        }
        public override string[] calcGeneric(IGame game, Card card, string[] value)
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