using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class LifeModifier : IntPropModifier
    {
        public LifeModifier(int value) : base(nameof(ServantCardDefine.life), value)
        {
        }
        public LifeModifier(int value, bool isSet) : base(nameof(ServantCardDefine.life), value, isSet)
        {
        }
        LifeModifier(LifeModifier origin) : base(origin)
        {

        }
        public override void beforeAdd(Card card)
        {
        }
        public override void afterAdd(Card card)
        {
            if (isSet)
                card.setCurrentLife(value);
            else
                card.setCurrentLife(card.getCurrentLife() + value);
        }
        public override int calc(Card card, int value)
        {
            if (isSet)
                return this.value;
            else
                return value + this.value;
        }
        bool resetCurrentLife { get; set; } = false;
        public override void beforeRemove(Card card)
        {
            resetCurrentLife = card.getCurrentLife() >= card.getLife();
        }
        public override void afterRemove(Card card)
        {
            if (resetCurrentLife)
            {
                card.setCurrentLife(card.getLife());
            }
        }
        public override PropModifier clone()
        {
            return new LifeModifier(this);
        }
    }
}