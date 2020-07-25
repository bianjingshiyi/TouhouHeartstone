using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Linq;
namespace TouhouHeartstone
{
    public class TagsModifier : PropModifier<string[]>
    {
        public override string propName { get; } = nameof(ServantCardDefine.tags);
        public string[] tags { get; }
        public enum ModifyType
        {
            add, remove, set
        }
        public ModifyType modifyType { get; }
        public TagsModifier(ModifyType modifyType, params string[] tags)
        {
            this.modifyType = modifyType;
            this.tags = tags;
        }
        protected TagsModifier(TagsModifier origin)
        {
            tags = origin.tags;
            modifyType = origin.modifyType;
        }
        public override string[] calc(IGame game, Card card, string[] value)
        {
            switch (modifyType)
            {
                case ModifyType.set:
                    return tags;
                case ModifyType.remove:
                    return value.Except(tags).ToArray();
                default:
                    return value.Union(tags).ToArray();
            }
        }
        public override PropModifier clone()
        {
            return new TagsModifier(this);
        }
    }
}