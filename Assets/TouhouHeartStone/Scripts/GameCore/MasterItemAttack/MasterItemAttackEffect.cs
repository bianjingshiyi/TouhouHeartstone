using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public abstract class MasterItemAttackEffect : Effect, IPileRangedEffect, IEventEffect
    {
        #region 公有方法
        #region 构造方法
        public MasterItemAttackEffect(DefineReference buffDefineRef)
        {
            this.buffDefineRef = buffDefineRef;
        }
        #endregion
        public override async Task execute(EffectEnv env)
        {
            var game = env.game;
            await game.updateMasterItemAttack(env.card.getOwner());
        }
        public bool isDisabled(ICard card, IBuff buff)
        {
            return !card.isEffectEnabled(buff, this);
        }
        public string[] getPiles() => _piles;
        public override int getPriority() => 0;
        #endregion

        protected override Task onEnable(EffectEnv env)
        {
            var game = env.game;
            var card = env.card;
            var buff = env.buff;
            game.logger.logTrace("Effect", $"{card}注册触发器{this}");
            Trigger trigger = new EffectTrigger(game, card, buff, this);
            var eventRef = new EventReference(0, EventHelper.getName(eventType));
            var triggerTime = new EventTriggerTime(eventRef, triggerType);
            game.triggers.registerDelayed(triggerTime, trigger);
            return Task.CompletedTask;
        }
        protected override Task onDisable(EffectEnv env)
        {
            var game = env.game;
            var card = env.card;
            var buff = env.buff;
            game.logger.logTrace("Effect", $"{card}注销触发器{this}");
            var eventRef = new EventReference(0, EventHelper.getName(eventType));
            var triggerTime = new EventTriggerTime(eventRef, triggerType);
            var triggers = game.triggers.getTriggers(triggerTime);
            var trigger = triggers.FirstOrDefault(t => t is EffectTrigger eventTrigger && eventTrigger.effect == this);
            game.triggers.remove(triggerTime, trigger);
            return Task.CompletedTask;
        }

        Task IEventEffect.onTrigger(EffectEnv env)
        {
            return EffectTriggerEventDefine.doEvent(env);
        }

        #region 属性字段
        protected abstract Type eventType { get; }
        protected abstract EventTriggerTimeType triggerType { get; }
        private string[] _piles = new string[] { PILE.MASTER };
        #endregion
    }

    public sealed class MasterItemAttackEffectPropChanged : MasterItemAttackEffect
    {
        #region 构造方法
        public MasterItemAttackEffectPropChanged(DefineReference buffDefineRef) : base(buffDefineRef)
        {
        }
        #endregion
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
        #region 构造方法
        public MasterItemAttackEffectSetDefine(DefineReference buffDefineRef) : base(buffDefineRef)
        {
        }
        #endregion
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
        #region 构造方法
        public MasterItemAttackEffectMoveCard(DefineReference buffDefineRef) : base(buffDefineRef)
        {
        }
        #endregion
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