using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class LifeModifier : PropModifier<int>
    {
        public override string propName { get; } = nameof(ServantCardDefine.life);
        public int value { get; }
        public bool isSet { get; }
        public LifeModifier(int value)
        {
            this.value = value;
            isSet = false;
        }
        public LifeModifier(int value, bool isSet)
        {
            this.value = value;
            this.isSet = isSet;
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
    }
}