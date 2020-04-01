using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class AttackModifier : PropModifier<int>
    {
        public override string propName { get; } = nameof(ServantCardDefine.attack);
        public int value { get; }
        public bool isSet { get; }
        public AttackModifier(int value)
        {
            this.value = value;
            isSet = false;
        }
        public AttackModifier(int value, bool isSet)
        {
            this.value = value;
            this.isSet = isSet;
        }
        public override void beforeAdd(Card card)
        {
        }
        public override void afterAdd(Card card)
        {
        }
        public override int calc(Card card, int value)
        {
            if (isSet)
                return this.value;
            else
                return value + this.value;
        }
        public override void beforeRemove(Card card)
        {
        }
        public override void afterRemove(Card card)
        {
        }
    }
}