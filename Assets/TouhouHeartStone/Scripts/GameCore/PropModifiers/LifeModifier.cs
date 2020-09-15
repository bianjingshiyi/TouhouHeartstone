using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Threading.Tasks;
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
        public override async Task setValue(IGame game, Card card, int value)
        {
            bool resetCurrentLife = card.getCurrentLife(game) >= card.getLife(game);
            int changedValue = value - this.value;
            await base.setValue(game, card, value);
            if (isSet)
                await card.setCurrentLife(game, (value));
            else
            {
                int life = card.getLife(game);
                if (resetCurrentLife)
                {
                    game?.logger?.log("由于" + card + "没有受伤，修正值变为" + value + "后重置生命值为" + life);
                    await card.setCurrentLife(game, life);
                }
                else if (card.getCurrentLife(game) > life)
                    await card.setCurrentLife(game, life);
                else
                {
                    await card.setCurrentLife(game, (card.getCurrentLife(game) + changedValue));
                    game?.logger?.log("由于" + card + "受伤了，修正值变为" + value + "之后生命值变为" + card.getCurrentLife(game));
                }
            }
        }
        public override Task afterAdd(IGame game, Card card)
        {
            if (isSet)
                return card.setCurrentLife(game, (value));
            else
                return card.setCurrentLife(game, (card.getCurrentLife(game) + value));
        }
        bool resetCurrentLife { get; set; } = false;
        public override Task beforeRemove(IGame game, Card card)
        {
            resetCurrentLife = card.getCurrentLife(game) >= card.getLife(game);
            return base.beforeRemove(game, card);
        }
        public override Task afterRemove(IGame game, Card card)
        {
            int life = card.getLife(game);
            if (resetCurrentLife)
            {
                game?.logger?.log("由于" + card + "没有受伤，在移除" + this + "之后重置生命值为" + life);
                return card.setCurrentLife(game, life);
            }
            else if (card.getCurrentLife(game) > life)
                return card.setCurrentLife(game, life);
            else
            {
                game?.logger?.log("由于" + card + "受伤了，在移除" + this + "之后生命值仍为" + card.getCurrentLife(game));
                return Task.CompletedTask;
            }
        }
        public override PropModifier clone()
        {
            return new LifeModifier(this);
        }
    }
}