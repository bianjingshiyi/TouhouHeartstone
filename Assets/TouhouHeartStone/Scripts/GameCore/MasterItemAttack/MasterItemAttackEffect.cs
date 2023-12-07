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
        public Task onEnable(IGame game, ICard card, IBuff buff)
        {
            if (!isDisabled(card, buff))
                return Task.CompletedTask;
            card.enableEffect(buff, this);

            game.logger.logTrace("Effect", $"{card}注册触发器{this}");
            Trigger trigger = new EffectTrigger(game, card, buff, this);
            game.triggers.registerDelayed(triggerName, trigger);

            return Task.CompletedTask;
        }
        public Task onDisable(IGame game, ICard card, IBuff buff)
        {
            if (isDisabled(card, buff))
                return Task.CompletedTask;

            game.logger.logTrace("Effect", $"{card}注销触发器{this}");
            var eventName = triggerName;
            var triggers = game.triggers.getTriggers(eventName);
            var trigger = triggers.FirstOrDefault(t => t is EffectTrigger eventTrigger && eventTrigger.effect == this);
            game.triggers.remove(eventName, trigger);

            card.disableEffect(buff, this);
            return Task.CompletedTask;
        }
        public abstract bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg);
        public async Task execute(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var engine = game as CardEngine;
            await engine.updateMasterItemAttack(card.getOwner());
        }
        public bool isDisabled(ICard card, IBuff buff)
        {
            return !card.isEffectEnabled(buff, this);
        }
        #endregion

        Task IEventEffect.onTrigger(IGame game, ICard card, IBuff buff, IEventArg arg) => execute(game, card, buff, arg);

        #region 属性字段
        public string[] piles => _piles;
        protected abstract string triggerName { get; }
        private string[] _piles = new string[] { PILE.MASTER };
        #endregion
    }

    public sealed class MasterItemAttackEffectPropChanged : MasterItemAttackEffect
    {
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var argCard = eventArg.getVar<Card>(CardPropChangeEventDefine.VAR_CARD);
            var propName = eventArg.getVar<string>(CardPropChangeEventDefine.VAR_PROP_NAME);
            return argCard == card.getOwner().getItem() && propName == DEFINE.ATTACK;
        }

        protected override string triggerName => EventHelper.getNameAfter(EventHelper.getName(typeof(CardPropChangeEventDefine)));
    }

    public sealed class MasterItemAttackEffectSetDefine : MasterItemAttackEffect
    {
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var argCard = eventArg.getVar<Card>(SetCardDefineEventDefine.VAR_CARD);
            return argCard == card.getOwner().getItem();
        }
        protected override string triggerName => EventHelper.getNameAfter(EventHelper.getName(typeof(SetCardDefineEventDefine)));
    }
    public sealed class MasterItemAttackEffectMoveCard: MasterItemAttackEffect
    {
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var player = card.getOwner();
            var argFrom = eventArg.getVar<Pile>(EVENTVAR.FROM_PILE);
            var argTo = eventArg.getVar<Pile>(MoveCardEventDefine.VAR_TO_PILE);
            return argFrom == player.getPile(PILE.ITEM) || argTo == player.getPile(PILE.ITEM);
        }
        protected override string triggerName => EventHelper.getNameAfter(EventHelper.getName(typeof(MoveCardEventDefine)));
    }
}