using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public abstract class MasterItemAttackEffect<T> : IEffect, IPileRangedEffect, IEventEffect where T : IEventArg
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
            game.triggers.registerDelayed(game.triggers.getNameAfter<T>(), trigger);

            return Task.CompletedTask;
        }
        public Task onDisable(IGame game, ICard card, IBuff buff)
        {
            if (isDisabled(card, buff))
                return Task.CompletedTask;

            game.logger.logTrace("Effect", $"{card}注销触发器{this}");
            var eventName = game.triggers.getNameAfter<T>();
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

        #region 属性字段
        public string[] piles => _piles;
        private string[] _piles = new string[] { PILE.MASTER };
        #endregion
    }

    public sealed class MasterItemAttackEffectPropChanged : MasterItemAttackEffect<Card.PropChangeEventArg>
    {
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var arg = eventArg as Card.PropChangeEventArg;
            return arg.card == card.getOwner().getItem() && arg.propName == DEFINE.ATTACK;
        }
    }

    public sealed class MasterItemAttackEffectSetDefine : MasterItemAttackEffect<Card.SetDefineEventArg>
    {
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var arg = eventArg as Card.SetDefineEventArg;
            return arg.card == card.getOwner().getItem();
        }
    }
    public sealed class MasterItemAttackEffectMoveCard: MasterItemAttackEffect<THHPile.MoveCardEventArg>
    {
        public override bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg eventArg)
        {
            var arg = eventArg as THHPile.MoveCardEventArg;
            var player = card.getOwner();
            return arg.from == player.getPile(PILE.ITEM) || arg.to == player.getPile(PILE.ITEM);
        }
    }
}