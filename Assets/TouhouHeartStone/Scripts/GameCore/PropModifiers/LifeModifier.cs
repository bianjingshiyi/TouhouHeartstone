using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Threading.Tasks;
namespace TouhouHeartstone
{
    public class LifeModifier : IntPropModifier
    {
        #region 公有方法
        #region 构造方法
        public LifeModifier(int value) : base(DEFINE.LIFE, value)
        {
        }
        public LifeModifier(int value, bool isSet) : base(DEFINE.LIFE, value, isSet)
        {
        }
        public LifeModifier(string relatedPropName, bool isSet) : base(DEFINE.LIFE, relatedPropName, isSet)
        {
        }
        protected LifeModifier(LifeModifier origin) : base(origin)
        {
        }
        #endregion
        public override async Task updateModifierValue(IGame game, Card card, int beforeValue, int value)
        {
            bool resetCurrentLife = card.getCurrentLife(game) >= card.getLife(game);
            int changedValue = value - beforeValue;
            if (changedValue == 0)
                return;
            if (isSet)
            {
                await card.setCurrentLife(game, value);
            }
            else
            {
                int life = card.getLife(game);
                if (resetCurrentLife)
                {
                    game?.logger?.logTrace($"由于{card}没有受伤，修正值变为{value}后重置生命值为{life}");
                    await card.setCurrentLife(game, life);
                }
                else if (card.getCurrentLife(game) > life)
                {
                    game?.logger?.logTrace($"由于{card}当前生命值{card.getCurrentLife(game)}大于生命最大值{card.getLife(game)}，重置生命值为{life}");
                    await card.setCurrentLife(game, life);
                }
                else
                {
                    if (changedValue > 0)
                    {
                        game?.logger?.logTrace($"由于{card}受伤了，修正值变为{value}之后生命值变为{card.getCurrentLife(game) + changedValue}");
                        await card.setCurrentLife(game, card.getCurrentLife(game) + changedValue);
                    }
                    else
                    {
                        game?.logger?.logTrace($"由于{card}受伤了，修正值变为{value}之后生命值保持不变");
                    }
                }
            }
        }
        public override Task afterAdd(IGame game, Card card, int value)
        {
            if (isSet)
                return card.setCurrentLife(game, value);
            else
                return card.setCurrentLife(game, card.getCurrentLife(game) + value);
        }
        public override Task afterRemove(IGame game, Card card, int value)
        {
            int life = card.getLife(game);
            int lifeBefore = isSet ? value : life + value;
            if (card.getCurrentLife(game) > life || card.getCurrentLife(game) >= lifeBefore)
            {
                return card.setCurrentLife(game, life);
            }
            return Task.CompletedTask;
        }
        #endregion
    }
}