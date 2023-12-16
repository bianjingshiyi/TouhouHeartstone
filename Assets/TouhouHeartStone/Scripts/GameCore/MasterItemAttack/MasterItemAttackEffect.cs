using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public abstract class MasterItemAttackEffect : IEffect, IPileRangedEffect, IEventEffect
    {
        #region 公有方法
        #region 构造方法
        public MasterItemAttackEffect()
        {
        }
        #endregion
        public Task onEnable(CardEngine game, Card card, Buff buff)
        {
            if (!isDisabled(card, buff))
                return Task.CompletedTask;
            card.enableEffect(buff, this);

            game.logger.logTrace("Effect", $"{card}注册触发器{this}");
            Trigger trigger = new EffectTrigger(game, card, buff, this);
            var eventRef = new EventReference(0, EventHelper.getName(eventType));
            var triggerTime = new EventTriggerTime(eventRef, triggerType);
            game.triggers.registerDelayed(triggerTime, trigger);

            return Task.CompletedTask;
        }
        public Task onDisable(CardEngine game, Card card, Buff buff)
        {
            if (isDisabled(card, buff))
                return Task.CompletedTask;

            game.logger.logTrace("Effect", $"{card}注销触发器{this}");
            var eventRef = new EventReference(0, EventHelper.getName(eventType));
            var triggerTime = new EventTriggerTime(eventRef, triggerType);
            var triggers = game.triggers.getTriggers(triggerTime);
            var trigger = triggers.FirstOrDefault(t => t is EffectTrigger eventTrigger && eventTrigger.effect == this);
            game.triggers.remove(triggerTime, trigger);

            card.disableEffect(buff, this);
            return Task.CompletedTask;
        }
        public abstract bool checkCondition(EffectEnv env);
        public async Task execute(EffectEnv env)
        {
            var game = env.game;
            await game.updateMasterItemAttack(env.card.getOwner());
        }
        public bool isDisabled(ICard card, IBuff buff)
        {
            return !card.isEffectEnabled(buff, this);
        }
        #endregion

        Task IEventEffect.onTrigger(EffectEnv env) => execute(env);

        #region 属性字段
        public string[] piles => _piles;
        protected abstract Type eventType { get; }
        protected abstract EventTriggerTimeType triggerType { get; }
        private string[] _piles = new string[] { PILE.MASTER };
        #endregion
    }

    public sealed class MasterItemAttackEffectPropChanged : MasterItemAttackEffect
    {
        public override bool checkCondition(EffectEnv env)
        {
            var eventArg = env.eventArg;
            var card = env.card;
            var argCard = eventArg.getVar<Card>(CardPropChangeEventDefine.VAR_CARD);
            var propName = eventArg.getVar<string>(CardPropChangeEventDefine.VAR_PROP_NAME);
            return argCard == card.getOwner().getItem() && propName == DEFINE.ATTACK;
        }

        protected override Type eventType => typeof(CardPropChangeEventDefine);
        protected override EventTriggerTimeType triggerType => EventTriggerTimeType.After;
    }

    public sealed class MasterItemAttackEffectSetDefine : MasterItemAttackEffect
    {
        public override bool checkCondition(EffectEnv env)
        {
            var eventArg = env.eventArg;
            var card = env.card;
            var argCard = eventArg.getVar<Card>(SetCardDefineEventDefine.VAR_CARD);
            return argCard == card.getOwner().getItem();
        }
        protected override Type eventType => typeof(SetCardDefineEventDefine);
        protected override EventTriggerTimeType triggerType => EventTriggerTimeType.After;
    }
    public sealed class MasterItemAttackEffectMoveCard: MasterItemAttackEffect
    {
        public override bool checkCondition(EffectEnv env)
        {
            var eventArg = env.eventArg;
            var card = env.card;
            var player = card.getOwner();
            var argFrom = eventArg.getVar<Pile>(EVENTVAR.FROM_PILE);
            var argTo = eventArg.getVar<Pile>(MoveCardEventDefine.VAR_TO_PILE);
            return argFrom == player.getPile(PILE.ITEM) || argTo == player.getPile(PILE.ITEM);
        }
        protected override Type eventType => typeof(MoveCardEventDefine);
        protected override EventTriggerTimeType triggerType => EventTriggerTimeType.After;
    }
}