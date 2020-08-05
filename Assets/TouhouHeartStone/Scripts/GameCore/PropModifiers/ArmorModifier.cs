using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class ArmorModifier : IntPropModifier
    {
        public ArmorModifier(int value) : base(nameof(ServantCardDefine.Armor), value)
        {
        }
        public ArmorModifier(int value, bool isSet) : base(nameof(ServantCardDefine.Armor), value, isSet)
        {
        }
        protected ArmorModifier(IntPropModifier origin) : base(origin)
        {
        }
        public override PropModifier clone()
        {
            return new ArmorModifier(this);
        }
    }
}
