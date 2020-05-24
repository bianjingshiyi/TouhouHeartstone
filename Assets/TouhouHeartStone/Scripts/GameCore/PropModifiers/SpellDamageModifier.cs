using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class SpellDamageModifier : PropModifier<int>
    {
        public override string propName { get; } = nameof(ServantCardDefine.spellDamage);
        public int value { get; }
        public SpellDamageModifier(int value)
        {
            this.value = value;
        }
        public override void beforeAdd(Card card)
        {
        }
        public override void beforeRemove(Card card)
        {
        }
        public override int calc(Card card, int value)
        {
            return value + this.value;
        }
        public override void afterAdd(Card card)
        {
        }
        public override void afterRemove(Card card)
        {
        }
    }
}