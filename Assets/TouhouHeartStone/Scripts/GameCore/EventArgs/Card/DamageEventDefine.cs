using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(CardPropChangeEventDefine))]
    public class DamageEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public DamageEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_TARGET,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_DAMAGE_SOURCE,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_DAMAGE_VALUE,type=typeof(int)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_TARGET,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_DAMAGE_SOURCE,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_DAMAGE_VALUE,type=typeof(int)},
                new EventVariableInfo(){name = VAR_DAMAGED_VALUE,type=typeof(int)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(VAR_TARGET, arg);
            record.setCardState(VAR_DAMAGE_SOURCE, arg);
            record.setVar(VAR_DAMAGE_VALUE, arg);
            record.setVar(VAR_DAMAGED_VALUE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var target = arg.getVar<Card>(VAR_TARGET);
            var source = arg.getVar<Card>(VAR_DAMAGE_SOURCE);
            var dmgValue = arg.getVar<int>(VAR_DAMAGE_VALUE);
            if (dmgValue < 1)
            {
                arg.isCanceled = true;
            }
            else if (target.isImmunity(game))
            {
                arg.isCanceled = true;
                game.logger.logTrace($"{target}免疫，不受伤害");
            }
            else if (target.isShield(game))
            {
                arg.isCanceled = true;
                game.logger.logTrace($"{target}受到伤害，失去圣盾");
                await target.setShield(game, false);
            }
            else
            {
                int damageReduce = target.getDamageReduce(game);
                int damage;
                if (damageReduce > 0)
                {
                    damage = dmgValue - damageReduce;
                    game.logger.logTrace($"{target}的伤害抵消{damageReduce}使伤害{dmgValue}=>{damage}");
                }
                else
                    damage = dmgValue;
                if (damage > 0)
                {
                    int armor = target.getArmor(game);
                    if (armor > 0)
                    {
                        if (armor >= damage)
                        {
                            await target.loseArmor(game, damage);
                            game.logger.logTrace($"{target}受到{damage}点伤害，护甲=>{target.getArmor(game)}");
                            await onDealKeywords(game, source, target, damage);
                            arg.setVar(VAR_DAMAGED_VALUE, damage);
                        }
                        else
                        {
                            int leakDamage = damage - armor;
                            await target.loseArmor(game, armor);
                            await target.setCurrentLife(game, target.getCurrentLife(game) - leakDamage);
                            game.logger.logTrace($"{target}受到{damage}点伤害，护甲=>{target.getArmor(game)}，生命值=>{target.getCurrentLife(game)}");
                            await onDealKeywords(game, source, target, damage);
                            arg.setVar(VAR_DAMAGED_VALUE, damage);
                        }
                    }
                    else
                    {
                        await target.setCurrentLife(game, target.getCurrentLife(game) - damage);
                        game.logger.logTrace($"{target}受到{damage}点伤害，生命值=>{target.getCurrentLife(game)}");
                        await onDealKeywords(game, source, target, damage);
                        arg.setVar(VAR_DAMAGED_VALUE, damage);
                    }
                }
                else
                {
                    arg.isCanceled = true;
                    game.logger.logTrace($"{target}的伤害{dmgValue}被减伤{damageReduce}抵消");
                }
            }
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(arg), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card target, Card source, int value)
        {
            var define = game.getEventDefine<DamageEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_TARGET, target);
            arg.setVar(VAR_DAMAGE_SOURCE, source);
            arg.setVar(VAR_DAMAGE_VALUE, value);
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return eventArg.getVar<Card>(VAR_TARGET);
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return new DescribableCard[] { eventArg.getVar<Card>(VAR_DAMAGE_SOURCE) };
        }
        ICard ICardEventDefine.getCard(IEventArg eventArg)
        {
            return eventArg.getVar<Card>(VAR_TARGET);
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString(eventArg);
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Card>(VAR_TARGET).getOwner();
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => new object[] { eventArg.getVar<int>(VAR_DAMAGED_VALUE) };
        #endregion

        private string localizeTemplateString(EventArg eventArg)
        {
            var source = eventArg.getVar<Card>(VAR_DAMAGE_SOURCE);
            return source != null ? TEMPLATE_ATK : TEMPLATE_RECV;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var source = eventArg.getVar<Card>(VAR_DAMAGE_SOURCE);
            var target = eventArg.getVar<Card>(VAR_TARGET);
            var damagedValue = eventArg.getVar<int>(VAR_DAMAGED_VALUE);
            if (source != null)
            {
                return new object[] {
                    source.getFormatString(),
                    target.getFormatString(),
                    damagedValue
                };
            }
            else
            {
                return new object[] {
                    target.getFormatString(),
                    damagedValue
                };
            }
        }
        private async Task onDealKeywords(CardEngine game, Card source, Card card, int damage)
        {
            if (source != null)
            {
                bool poisonous = source.isPoisonous(game);
                bool lifesteal = source.isDrain(game);
                if (source.isMaster() && game.getCurrentPlayer() == source.getOwner())
                {
                    var item = source.getOwner().getItem();
                    if (item != null)
                    {
                        poisonous |= item.isPoisonous(game);
                        lifesteal |= item.isDrain(game);
                    }
                }
                if (poisonous && card.isServant())
                    await card.kill(game);
                if (lifesteal && (card.isServant() || card.isMaster()))
                    await source.getOwner().getMaster().heal(game, card, damage);
            }
        }

        #endregion
        [TranslateMsg("{0}是卡，{1}是伤害值")]
        const string TEMPLATE_RECV = "{0} 受到 {1} 点伤害";
        [TranslateMsg("{0}{1}是卡，{2}是伤害值")]
        const string TEMPLATE_ATK = "{0} 对 {1} 造成 {2} 点伤害";
        [TranslateMsg("游戏事件日志项名称-伤害事件（{0}为失去的生命值）")]
        private const string LOG_ITEM_NAME = "-{0}";

        public static string VAR_TARGET => EVENTVAR.CARD;
        public const string VAR_DAMAGE_SOURCE = "伤害来源";
        public const string VAR_DAMAGE_VALUE = "伤害值";
        public const string VAR_DAMAGED_VALUE = "实际伤害值";
    }
}
