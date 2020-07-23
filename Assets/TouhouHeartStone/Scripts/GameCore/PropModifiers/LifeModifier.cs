using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
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
        public override void beforeAdd(IGame game, Card card)
        {
        }
        public override void afterAdd(IGame game, Card card)
        {
            if (isSet)
                card.setCurrentLife(value);
            else
                card.setCurrentLife(card.getCurrentLife(game) + value);
        }
        public override int calc(IGame game, Card card, int value)
        {
            if (isSet)
                return this.value;
            else
                return value + this.value;
        }
        bool resetCurrentLife { get; set; } = false;
        public override void beforeRemove(IGame game, Card card)
        {
            resetCurrentLife = card.getCurrentLife(game) >= card.getLife(game);
        }
        public override void afterRemove(IGame game, Card card)
        {
            int life = card.getLife(game);
            if (resetCurrentLife)
            {
                game?.logger?.log("由于" + card + "没有受伤，在移除" + this + "之后重置生命值为" + life);
                card.setCurrentLife(life);
            }
            if (card.getCurrentLife(game) > life)
                card.setCurrentLife(life);
        }
        public override PropModifier clone()
        {
            return new LifeModifier(this);
        }
    }
}