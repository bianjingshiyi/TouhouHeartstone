using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class TagModifier : StringArrayModifier
    {
        public TagModifier(ModifyType modifyType, params string[] keywords) : base(DEFINE.TAGS, modifyType, keywords)
        {
        }
        public override string ToString()
        {
            var value = defaultValue;
            switch (modifyType)
            {
                case ModifyType.set:
                    return $"tags = {(value.Length > 0 ? string.Join(",", value) : "empty")}";
                case ModifyType.remove:
                    return $"tags - {(value.Length > 0 ? string.Join(",", value) : "empty")}";
                default:
                    return $"tags + {(value.Length > 0 ? string.Join(",", value) : "empty")}";
            }
        }
    }
}