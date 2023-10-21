using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    public class THHEffect : ITriggerEffect, IPileRangedEffect
    {
        protected string _name;
        public TriggerTime[] triggers { get; }

        string[] ITriggerEffect.events
        {
            get { throw new NotImplementedException(); }
        }

        string[] ITriggerEffect.getEvents(ITriggerManager manager)
        {
            return triggers.Select(t => t.getEventName(manager)).ToArray();
        }

        public string[] piles { get; }

        public delegate bool CheckConditionDelegate(CardEngine game, Card card, Buff buff, object[] vars);

        CheckConditionDelegate onCheckCondition { get; }

        bool ITriggerEffect.checkCondition(IGame game, ICard card, IBuff buff, object[] vars)
        {
            if (onCheckCondition != null)
                return onCheckCondition.Invoke(game as CardEngine, card as Card, buff as Buff, vars);
            else
                return true;
        }

        public delegate bool CheckTargetDelegate(CardEngine game, Card card, Buff buff, object[] targets);

        CheckTargetDelegate onCheckTarget { get; }

        bool ITriggerEffect.checkTargets(IGame game, ICard card, IBuff buff, object[] vars, object[] targets)
        {
            if (onCheckTarget != null)
                return onCheckTarget.Invoke(game as CardEngine, card as Card, buff as Buff, targets);
            else
                return true;
        }

        public delegate Task ExecuteDelegate(CardEngine game, Card card, Buff buff, object[] vars, object[] targets);

        ExecuteDelegate onExecute { get; }

        Task ITriggerEffect.execute(IGame game, ICard card, IBuff buff, object[] vars, object[] targets)
        {
            if (onExecute != null)
                return onExecute.Invoke(game as CardEngine, card as Card, buff as Buff, vars, targets);
            else
                return Task.CompletedTask;
        }

        public THHEffect(TriggerTime trigger, string pile, CheckConditionDelegate onCheckCondition,
            CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute, string name = null)
        {
            _name = name;
            triggers = new TriggerTime[] { trigger };
            piles = new string[] { pile };
            this.onCheckCondition = onCheckCondition;
            this.onCheckTarget = onCheckTarget;
            this.onExecute = onExecute;
        }

        public THHEffect(TriggerTime[] triggers, string[] piles, CheckConditionDelegate onCheckCondition,
            CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute, string name = null)
        {
            _name = name;
            this.triggers = triggers;
            this.piles = piles;
            this.onCheckCondition = onCheckCondition;
            this.onCheckTarget = onCheckTarget;
            this.onExecute = onExecute;
        }

        protected Action<IGame, ICard, IBuff> onRegisterTrigger = null;

        public async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger trigger = new Trigger(action: args => triggerAction(game, card, buff, args), name: effectName);
                await card.setProp(null, effectName, trigger);
                game.logger.logTrace("Effect", $"{card}注册效果{effectName}");
                onRegisterTrigger?.Invoke(game, card, buff);
                game.triggers.registerDelayed(time.getEventName(game.triggers), trigger);
            }
        }

        protected Action<IGame, ICard, IBuff> onUnRegisterTrigger = null;

        public Task onDisable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger trigger = card.getProp<Trigger>(game, effectName);
                game.logger.logTrace("Effect", $"{card}注销效果{effectName}");
                onUnRegisterTrigger?.Invoke(game, card, buff);
                game.triggers.remove(time.getEventName(game.triggers), trigger);
                card.setProp(null, effectName, null);
            }

            return Task.CompletedTask;
        }

        protected virtual Task triggerAction(IGame game, ICard card, IBuff buff, object[] args)
        {
            if ((this as ITriggerEffect).checkCondition(game, card, buff, args))
                return (this as ITriggerEffect).execute(game, card, buff, args, new object[0]);
            else
                return Task.CompletedTask;
        }

        protected virtual string getEffectName(IGame game, ICard card, IBuff buff, TriggerTime time)
        {
            return string.IsNullOrEmpty(_name) 
               ? $"{(buff != null ? buff.instanceID.ToString() : string.Empty)}Effect{Array.IndexOf(card.define.getEffects(), this)}{time.getEventName(game.triggers)}" 
               : _name;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(_name) ? base.ToString() : _name;
        }
    }

    public abstract class THHEffect<T> : THHEffect where T : IEventArg
    {
        public new delegate bool CheckConditionDelegate(CardEngine game, Card card, Buff buff, T eventArg);

        public new delegate Task ExecuteDelegate(CardEngine game, Card card, Buff buff, T eventArg, object[] targets);

        public delegate bool CheckConditionNoBuffDelegate(CardEngine game, Card card, T eventArg);

        public delegate bool CheckTargetNoBuffDelegate(CardEngine game, Card card, object[] targets);

        public delegate Task ExecuteNoBuffDelegate(CardEngine game, Card card, T eventArg, object[] targets);

        public delegate Task ExecuteNoTargetDelegate(CardEngine game, Card card, Buff buff, T eventArg);

        public delegate Task ExecuteNoBuffNoTargetDelegate(CardEngine game, Card card, T eventArg);

        protected THHEffect(TriggerTime trigger, PileFlag pile, CheckConditionDelegate onCheckCondition,
            CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute, string name = null) :
            base(new[] { trigger }, pile.getPiles(), (game, card, buff, vars) =>
              {
                  if (onCheckCondition != null)
                      return onCheckCondition.Invoke(game, card, buff, (T)vars[0]);
                  else
                      return true;
              }, onCheckTarget, (game, card, buff, vars, targets) =>
              {
                  if (onExecute != null && vars.Length > 0 && vars[0] is T t)
                      return onExecute.Invoke(game, card, buff, t, targets);
                  else
                      return Task.CompletedTask;
              })
        {
        }

        protected THHEffect(TriggerTime trigger, PileFlag pile, CheckConditionNoBuffDelegate onCheckCondition,
            CheckTargetNoBuffDelegate onCheckTarget, ExecuteNoBuffDelegate onExecute) :
            this(trigger, pile, (game, card, buff, arg) =>
                {
                    if (onCheckCondition != null)
                        return onCheckCondition(game, card, arg);
                    else
                        return true;
                },
                (game, card, buff, targets) =>
                {
                    return onCheckTarget != null ? onCheckTarget(game, card, targets) : true;
                },
                (game, card, buff, arg, targets) =>
                {
                    return onExecute != null ? onExecute(game, card, arg, targets) : Task.CompletedTask;
                })
        {
        }

        protected THHEffect(TriggerTime trigger, PileFlag pile, CheckConditionDelegate onCheckCondition,
            ExecuteNoTargetDelegate onExecute) :
            this(trigger, pile, (game, card, buff, arg) =>
                {
                    if (onCheckCondition != null)
                        return onCheckCondition(game, card, buff, arg);
                    else
                        return true;
                }, null,
                (game, card, buff, arg, targets) =>
                {
                    return onExecute != null ? onExecute(game, card, buff, arg) : Task.CompletedTask;
                })
        {
        }

        protected THHEffect(TriggerTime trigger, PileFlag pile, CheckConditionNoBuffDelegate onCheckCondition,
            ExecuteNoBuffNoTargetDelegate onExecute) :
            this(trigger, pile, (game, card, buff, arg) =>
                {
                    if (onCheckCondition != null)
                        return onCheckCondition(game, card, arg);
                    else
                        return true;
                }, null,
                (game, card, buff, arg, targets) =>
                {
                    return onExecute != null ? onExecute(game, card, arg) : Task.CompletedTask;
                })
        {
        }

        protected override Task triggerAction(IGame game, ICard card, IBuff buff, object[] args)
        {
            if (args[0] is T eventArg)
            {
                return this.triggerAction(game, card, buff, eventArg);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 效果将会注册的触发器内容，大部分的时候你是不需要重载这个函数的，请三思
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Task triggerAction(IGame game, ICard card, IBuff buff, T arg)
        {
            if ((this as ITriggerEffect).checkCondition(game, card, buff, new object[] { arg }))
                return (this as ITriggerEffect).execute(game, card, buff, new object[] { arg }, new object[0]);
            else
                return Task.CompletedTask;
        }
    }

    public class THHEffectOn<T> : THHEffect<T> where T : IEventArg
    {
        // public new delegate bool CheckConditionDelegate(CardEngine game, Card card, Buff buff, T eventArg);
        //
        // public new delegate Task ExecuteDelegate(CardEngine game, Card card, Buff buff, T eventArg, object[] targets);
        //
        // public delegate bool CheckConditionNoBuffDelegate(CardEngine game, Card card, T eventArg);
        //
        // public delegate bool CheckTargetNoBuffDelegate(CardEngine game, Card card, object[] targets);
        //
        // public delegate Task ExecuteNoBuffDelegate(CardEngine game, Card card, T eventArg, object[] targets);
        public THHEffectOn(PileFlag pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget,
            ExecuteDelegate onExecute) :
            base(new On<T>(), pile, onCheckCondition, onCheckTarget, onExecute)
        {
        }

        public THHEffectOn(PileFlag pile, CheckConditionNoBuffDelegate onCheckCondition,
            CheckTargetNoBuffDelegate onCheckTarget, ExecuteNoBuffDelegate onExecute) :
            base(new On<T>(), pile, onCheckCondition, onCheckTarget, onExecute)
        {
        }
    }

    public class THHEffectBefore<T> : THHEffect<T> where T : IEventArg
    {
        public THHEffectBefore(PileFlag pile, CheckConditionDelegate onCheckCondition,
            CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute) :
            base(new Before<T>(), pile, onCheckCondition, onCheckTarget, onExecute)
        {
        }

        public THHEffectBefore(PileFlag pile, CheckConditionNoBuffDelegate onCheckCondition,
            CheckTargetNoBuffDelegate onCheckTarget, ExecuteNoBuffDelegate onExecute) :
            base(new Before<T>(), pile, onCheckCondition, onCheckTarget, onExecute)
        {
        }

        public THHEffectBefore(PileFlag pile, CheckConditionDelegate onCheckCondition,
            ExecuteNoTargetDelegate onExecute) :
            base(new Before<T>(), pile, onCheckCondition, onExecute)
        {
        }

        public THHEffectBefore(PileFlag pile, CheckConditionNoBuffDelegate onCheckCondition,
            ExecuteNoBuffNoTargetDelegate onExecute) :
            base(new Before<T>(), pile, onCheckCondition, onExecute)
        {
        }
    }

    public class THHEffectAfter<T> : THHEffect<T> where T : IEventArg
    {
        public THHEffectAfter(PileFlag pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget,
            ExecuteDelegate onExecute) :
            base(new After<T>(), pile, onCheckCondition, onCheckTarget, onExecute)
        {
        }

        public THHEffectAfter(PileFlag pile, CheckConditionNoBuffDelegate onCheckCondition,
            CheckTargetNoBuffDelegate onCheckTarget, ExecuteNoBuffDelegate onExecute) :
            base(new After<T>(), pile, onCheckCondition, onCheckTarget, onExecute)
        {
        }

        public THHEffectAfter(PileFlag pile, CheckConditionDelegate onCheckCondition,
            ExecuteNoTargetDelegate onExecute) :
            base(new After<T>(), pile, onCheckCondition, onExecute)
        {
        }

        public THHEffectAfter(PileFlag pile, CheckConditionNoBuffDelegate onCheckCondition,
            ExecuteNoBuffNoTargetDelegate onExecute) :
            base(new After<T>(), pile, onCheckCondition, onExecute)
        {
        }
    }

    public abstract class PassiveEffect : IPassiveEffect, IPileRangedEffect 
    {
        public string[] events => throw new NotImplementedException();
        public abstract string[] piles { get; }
        public bool enabled { get; set; }
        public string[] getEvents(ITriggerManager manager)
        {
            return new string[0];
        }

        Task IEffect.onEnable(IGame game, ICard card, IBuff buff)
        {
            return onEnable(game as CardEngine, card as Card, buff as Buff);
        }

        public abstract Task onEnable(CardEngine game, Card card, Buff buff);

        Task IEffect.onDisable(IGame game, ICard card, IBuff buff)
        {
            return onDisable(game as CardEngine, card as Card, buff as Buff);
        }

        public abstract Task onDisable(CardEngine game, Card card, Buff buff);
    }

    public abstract class ActiveEffect : IActiveEffect
    {
        string _name;

        public ActiveEffect(string name = null)
        {
            _name = name;
        }

        Task IEffect.onEnable(IGame game, ICard card, IBuff buff)
        {
            return Task.CompletedTask;
        }

        Task IEffect.onDisable(IGame game, ICard card, IBuff buff)
        {
            return Task.CompletedTask;
        }
        public string[] getEvents(ITriggerManager manager)
        {
            return new string[] { manager.getName<HearthStoneRule.ActiveEventArg>() };
        }

        public bool checkCondition(IGame game, ICard card, IBuff buff, IEventArg arg)
        {
            return checkCondition(game as CardEngine, card as Card);
        }

        public abstract bool checkCondition(CardEngine game, Card card);
        public abstract Task execute(IGame game, ICard card, IBuff buff, IEventArg arg);

        public override string ToString()
        {
            return string.IsNullOrEmpty(_name) ? base.ToString() : _name;
        }
    }

    public delegate Task ExecuteDelegate(CardEngine game, Card card);

    public class NoTargetEffect : ActiveEffect
    {
        CheckConditionDelegate _onCheckCondition;
        ExecuteDelegate _onExecute;

        public NoTargetEffect(ExecuteDelegate onExecute, CheckConditionDelegate onCheckCondition = null,
            string name = null) : base(name)
        {
            _onCheckCondition = onCheckCondition;
            _onExecute = onExecute;
        }

        public override bool checkCondition(CardEngine game, Card card)
        {
            return _onCheckCondition == null || _onCheckCondition(game, card);
        }

        public override Task execute(IGame game, ICard card, IBuff buff, IEventArg arg)
        {
            if (_onExecute != null)
                return _onExecute(game as CardEngine, card as Card);
            return Task.CompletedTask;
        }
    }

    public abstract class SingleTargetEffect : ActiveEffect, ITargetEffect
    {
        /// <summary>
        /// 可选目标范围。
        /// </summary>
        public PileFlag ranges { get; protected set; } = PileFlag.character | PileFlag.both;

        /// <summary>
        /// 默认实现，存在可以作为目标的卡片。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// 
        /// <returns></returns>
        public override bool checkCondition(CardEngine game, Card card)
        {
            foreach (var player in game.players)
            {
                foreach (var pile in ranges.getPiles(game, card.getOwner()))
                {
                    foreach (var target in pile)
                    {
                        if (checkTarget(game, card, target))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool checkTargets(IGame game, ICard card, object[] vars, object[] targets)
        {
            return checkTargets(game as CardEngine, card as Card, targets);
        }

        public virtual bool checkTargets(CardEngine game, Card card, object[] targets)
        {
            if (targets != null &&
                targets.Length > 0 &&
                targets[0] is Card target &&
                checkTarget(game, card, target))
                return true;
            return false;
        }

        /// <summary>
        /// 默认实现，战吼目标可以为空，但必须触发事件！
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool checkTarget(CardEngine game, Card card, Card target)
        {
            return ranges.getPiles(game, card.getOwner()).Any(p => p.Contains(target));
        }

        public override Task execute(IGame game, ICard card, IBuff buff, IEventArg arg)
        {
            if (!(arg is HearthStoneRule.ActiveEventArg activeEvent))
                return Task.CompletedTask;

            var targets = activeEvent.targets;
            if (targets != null &&
                targets.Length > 0 &&
                targets[0] is Card target)
                return execute(game as CardEngine, card as Card, target);
            else
            {
                return execute(game as CardEngine, card as Card, null);
            }
        }

        public abstract Task execute(CardEngine game, Card card, Card target);
    }

    public delegate bool CheckConditionDelegate(CardEngine game, Card card);

    /// <summary>
    /// 使用lambda表达式的单一目标主动效果
    /// </summary>
    public class LambdaSingleTargetEffect : SingleTargetEffect
    {
        CheckConditionDelegate _onCheckCondition = null;

        public delegate bool CheckTargetDelegate(CardEngine game, Card card, Card target);

        CheckTargetDelegate _onCheckTarget = null;

        public delegate Task ExecuteDelegate(CardEngine game, Card card, Card target);

        public delegate Task ExecuteDelegate_NoTarget(CardEngine game, Card card);

        ExecuteDelegate _onExecute = null;
        ExecuteDelegate_NoTarget _noTargetExecute = null;

        /// <summary>
        /// 使用Lambda来构造一个单一目标效果
        /// </summary>
        /// <param name="onExecute">效果的执行内容</param>
        /// <param name="ranges">效果的目标范围</param>
        /// <param name="onCheckCondition">效果的条件</param>
        /// <param name="onCheckTarget">效果对于目标的条件</param>
        /// <param name="noTargetExecute">target为null时执行的内容，默认为null</param>
        public LambdaSingleTargetEffect(ExecuteDelegate onExecute,
            PileFlag ranges = PileFlag.none,
            CheckConditionDelegate onCheckCondition = null,
            CheckTargetDelegate onCheckTarget = null,
            ExecuteDelegate_NoTarget noTargetExecute = null)
        {
            if (ranges != PileFlag.none)
                this.ranges = ranges;
            _onCheckCondition = onCheckCondition;
            _onCheckTarget = onCheckTarget;
            _onExecute = onExecute;
            _noTargetExecute = noTargetExecute;
        }

        public override bool checkCondition(CardEngine game, Card card)
        {
            return base.checkCondition(game, card) && (_onCheckCondition == null || _onCheckCondition(game, card));
        }

        public override bool checkTarget(CardEngine game, Card card, Card target)
        {
            return base.checkTarget(game, card, target) &&
                   (_onCheckTarget == null || _onCheckTarget(game, card, target));
        }

        public override Task execute(CardEngine game, Card card, Card target)
        {
            if (_onExecute != null)
            {
                if (!card.isSpell())
                {
                    if (target != null)
                    {
                        return _onExecute(game, card, target);
                    }
                    else
                    {
                        return _noTargetExecute(game, card);
                    }
                }

                return _onExecute(game, card, target);
            }

            return Task.CompletedTask;
        }
    }
    /// <summary>
    /// 被动光环，像是巫师学徒的那种
    /// </summary>
    public class Halo : PassiveEffect
    {
        public override string[] piles { get; }
        Func<CardEngine, Card, Pile[]> getRange { get; }
        Func<CardEngine, Card, bool> filter { get; }
        Buff buff { get; }

        private Dictionary<Card, Buff> getBuffDic(CardEngine game, Card card, Buff buff)
        {
            string propName = getPropName(buff, "buffDic");
            Dictionary<Card, Buff> buffDic = card.getProp<Dictionary<Card, Buff>>(game, propName);
            if (buffDic == null)
            {
                buffDic = new Dictionary<Card, Buff>();
                card.setProp(game, propName, buffDic);
            }

            return buffDic;
        }

        private string getPropName(Buff buff, string propName)
        {
            propName = $"{this.buff.id}{nameof(Halo)}.{propName}";
            if (buff != null)
                propName += buff.ToString();
            return propName;
        }

        private Trigger<THHPile.MoveCardEventArg> getOnCardEnterHandTrigger(CardEngine game, Card card, Buff buff)
        {
            return card.getProp<Trigger<THHPile.MoveCardEventArg>>(game, getPropName(buff, "onCardEnterHandTrigger"));
        }

        private void setOnCardEnterHandTrigger(CardEngine game, Card card, Buff buff,
            Trigger<THHPile.MoveCardEventArg> value)
        {
            card.setProp(game, getPropName(buff, "onCardEnterHandTrigger"), value);
        }

        /// <summary>
        /// 简化的构造器
        /// </summary>
        /// <param name="buff">光环添加的buff</param>
        /// <param name="range">光环的作用范围</param>
        /// <param name="pile">光环拥有者必须位于起效范围内，光环才会起效。默认是位于己方的战场上。</param>
        /// <param name="filter">光环的作用条件。</param>
        public Halo(Buff buff, PileFlag range, PileFlag pile = PileFlag.self | PileFlag.field,
            Func<CardEngine, Card, bool> filter = null)
            : this(PILE.getPiles(pile), (game, card) => range.getPiles(game, card.getOwner()), filter, buff)
        {
        }

        public Halo(string[] piles, Func<CardEngine, Card, Pile[]> getRange, Func<CardEngine, Card, bool> filter,
            Buff buff)
        {
            this.piles = piles;
            this.getRange = getRange;
            this.filter = filter;
            this.buff = buff;
        }

        public override async Task onEnable(CardEngine game, Card card, Buff buff)
        {
            Pile[] ranges = getRange != null ? getRange(game, card) : null;
            if (getOnCardEnterHandTrigger(game, card, buff) == null)
            {
                setOnCardEnterHandTrigger(game, card, buff, new Trigger<THHPile.MoveCardEventArg>(action: async arg =>
                {
                    if (ranges == null)
                        return;
                    if (ranges.Contains(arg.to) &&
                        (filter == null || filter(game, arg.card)))
                    {
                        if (!getBuffDic(game, card, buff).ContainsKey(arg.card))
                        {
                            Buff b = this.buff.clone();
                            await arg.card.addBuff(game, b);
                            getBuffDic(game, card, buff).Add(arg.card, b);
                        }
                    }
                    else if (ranges.Contains(arg.from) && (filter == null || filter(game, arg.card)))
                    {
                        if (getBuffDic(game, card, buff).ContainsKey(arg.card))
                        {
                            await arg.card.removeBuff(game, getBuffDic(game, card, buff)[arg.card]);
                            getBuffDic(game, card, buff).Remove(arg.card);
                        }
                    }

                    return;
                }));
                game.triggers.registerAfter(getOnCardEnterHandTrigger(game, card, buff));
            }

            foreach (var range in ranges)
            {
                foreach (var target in range.Where(c => filter == null || filter(game, c)))
                {
                    if (!getBuffDic(game, card, buff).ContainsKey(target))
                    {
                        Buff b = this.buff.clone();
                        await target.addBuff(game, b);
                        getBuffDic(game, card, buff).Add(target, b);
                    }
                }
            }
        }

        public override async Task onDisable(CardEngine game, Card card, Buff buff)
        {
            if (getOnCardEnterHandTrigger(game, card, buff) != null)
            {
                game.triggers.removeAfter(getOnCardEnterHandTrigger(game, card, buff));
                setOnCardEnterHandTrigger(game, card, buff, null);
            }

            foreach (var pair in getBuffDic(game, card, buff))
            {
                await pair.Key.removeBuff(game, pair.Value);
            }

            getBuffDic(game, card, buff).Clear();
        }
    }

    /// <summary>
    /// 扳机触发被动特效
    /// 达到一定条件开启/关闭特效
    /// </summary>
    public class ConditionalPassiveEffect : PassiveEffect
    {
        Func<CardEngine, Card, Buff, bool> condition { get; }
        PassiveEffect effect { get; }

        public ConditionalPassiveEffect(PassiveEffect effect, Func<CardEngine, Card, Buff, bool> condition)
        {
            this.condition = condition;
            this.effect = effect;
        }

        public override string[] piles => effect.piles;

        public override async Task onEnable(CardEngine game, Card card, Buff buff)
        {
            Trigger<HearthStoneRule.TurnStartEventArg> enableTrig = new Trigger<HearthStoneRule.TurnStartEventArg>(action:
                arg =>
                {
                    if (condition == null || condition(game, card, buff))
                        effect.onEnable(game, card, buff);
                    return Task.CompletedTask;
                });
            await card.setProp(game, nameof(ConditionalPassiveEffect) + GetHashCode() + "enableTrig", enableTrig);
            game.triggers.registerAfter(enableTrig);
            Trigger<HearthStoneRule.UseEventArg> disableTrig = new Trigger<HearthStoneRule.UseEventArg>(action: arg =>
            {
                if (condition != null && !condition(game, card, buff))
                    effect.onDisable(game, card, buff);
                return Task.CompletedTask;
            });
            await card.setProp(game, nameof(ConditionalPassiveEffect) + GetHashCode() + "disableTrig", disableTrig);
            game.triggers.registerAfter(disableTrig);
            if (condition == null || condition(game, card, buff))
                await effect.onEnable(game, card, buff);
        }

        public override Task onDisable(CardEngine game, Card card, Buff buff)
        {
            game.triggers.removeAfter(card.getProp<Trigger<HearthStoneRule.TurnStartEventArg>>(game,
                nameof(ConditionalPassiveEffect) + GetHashCode() + "enableTrig"));
            game.triggers.removeAfter(card.getProp<Trigger<HearthStoneRule.UseEventArg>>(game,
                nameof(ConditionalPassiveEffect) + GetHashCode() + "disableTrig"));
            return effect.onDisable(game, card, buff);
        }
    }

    public class AddBuffAfter<T> : THHEffectAfter<T> where T : IEventArg
    {
        public AddBuffAfter(PileFlag pile, CheckConditionDelegate onCheckCondition, Buff originBuff) : base(pile,
            onCheckCondition, (g, c, b, a) => { return c.addBuff(g, originBuff.clone()); })
        {
        }

        public AddBuffAfter(PileFlag pile, CheckConditionNoBuffDelegate onCheckCondition, Buff originBuff) :
            this(pile, (game, card, buff, arg) => onCheckCondition(game, card, arg), originBuff)
        {
        }
    }
    public class RemoveBuffBefore<T> : THHEffectBefore<T> where T : IEventArg
    {
        public RemoveBuffBefore(PileFlag pile, CheckConditionDelegate onCheckCondition, int buffId) : base(pile,
            onCheckCondition,
            (game, card, buff, arg) =>
            {
                return card.removeBuff(game, card.getBuffs().Where(b => b.id == buffId));
            })
        {
        }

        public RemoveBuffBefore(PileFlag pile, int buffId) : this(pile, null, buffId)
        {
        }
    }

    public class RemoveBuffAfter<T> : THHEffectAfter<T> where T : IEventArg
    {
        public RemoveBuffAfter(PileFlag pile, CheckConditionDelegate onCheckCondition, int buffId) : base(pile,
            onCheckCondition,
            (game, card, buff, arg) =>
            {
                return card.removeBuff(game, card.getBuffs().Where(b => b.id == buffId));
            })
        {
        }

        public RemoveBuffAfter(PileFlag pile, int buffId) : this(pile, null, buffId)
        {
        }
    }

    /// <summary>
    /// 在触发一定次数之后删除Buff
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RemoveBuffAfterTimeUp<T> : THHEffectAfter<T> where T : IEventArg
    {
        public RemoveBuffAfterTimeUp(PileFlag pile, int time, CheckConditionDelegate onCheckCondition) :
            base(pile, onCheckCondition, (game, card, buff, arg) =>
            {
                if (!card.containBuff(buff))
                {
                    return Task.CompletedTask;
                }
                var timerName = getTimerName(game, card, buff);
                card.setProp(game, timerName, card.getProp<int>(game, timerName) - 1);
                if (card.getProp<int>(game, timerName) <= 0)
                {
                    return card.removeBuff(game, buff);
                }
                return Task.CompletedTask;
            })
        {
            onRegisterTrigger += (game, card, buff) =>
            {
                card.setProp(game, getTimerName(game, card, buff), time);
            };
        }


        protected override string getEffectName(IGame game, ICard card, IBuff buff, TriggerTime time)
        {
            return nameof(RemoveBuffAfterTimeUp<T>) + time.getEventName(game.triggers);
        }

        public static string getTimerName(IGame game, ICard card, IBuff buff)
        {
            return nameof(RemoveBuffAfterTimeUp<T>) + buff.instanceID;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 亡语的本质是什么？首先亡语本身，是一个静态的效果，和卡片无关的，它具有执行逻辑：
    /// 它具有激活，和禁用两种行为，然后激活行为具有一个时机（入场），但是似乎没有禁用，也不会被重复的激活，是否已经激活是卡片自己应该记录的状态。
    /// 它和被动效果冲突的地方在于，被动效果具有作用域，离开作用域被动效果就会被禁用。但是亡语在时机触发这一点上和战吼非常相似，但是它肯定是一个触发效果而不是使用效果。
    /// 然后还有一个同一个卡怎么记录自己同时注册的N个亡语的问题。亡语不是实例，但是卡片是，BUFF是，那么Buff效果也会是效果吗？
    /// 这他妈是在无限套娃。
    /// 不行了，我放弃，自毁程序启动。
    /// 发现关于这个问题有一个简单的解法，实际上问题在于被动效果在每次移动的时候都要禁用再激活。为什么呢？
    /// </remarks>
    public class DeathRattle : PassiveEffect
    {
        public int id { get; }
        public override string[] piles { get; } = new string[0];
        public CheckConditionDelegate onCheckCondition { get; }

        public delegate Task ExecuteDelegate(CardEngine game, Card card, int position);

        public ExecuteDelegate onExecute { get; }

        public delegate bool CheckConditionBuffedDelegate(CardEngine game, Card card, Buff buff);

        public CheckConditionBuffedDelegate onCheckConditionBuffed { get; } = null;

        public delegate Task ExecuteBuffedDelegate(CardEngine game, Card card, Buff buff, int position);

        public ExecuteBuffedDelegate onExecuteBuffed { get; } = null;

        public DeathRattle(int id, ExecuteDelegate onExecute, CheckConditionDelegate onCheckCondition = null)
        {
            this.id = id;
            this.onExecute = onExecute;
            this.onCheckCondition = onCheckCondition;
        }

        public DeathRattle(int id, ExecuteBuffedDelegate onExecuteBuffed,
            CheckConditionBuffedDelegate onCheckConditionBuffed = null)
        {
            this.id = id;
            this.onExecuteBuffed = onExecuteBuffed;
            this.onCheckConditionBuffed = onCheckConditionBuffed;
        }

        public Task onEnable(IGame game, ICard card, IBuff buff)
        {
            return onEnable(game as CardEngine, card as Card, buff as Buff);
        }

        public override async Task onEnable(CardEngine game, Card card, Buff buff)
        {
            string propName = getPropName(buff);
            game.logger.logTrace("Effect", $"{card}启用亡语{propName}");
            await card.setProp(game, propName, false);
        }

        public Task execute(CardEngine game, Card card, Buff buff, THHCard.DeathEventArg deathEvent)
        {
            if (deathEvent.card != card) // 死亡事件的卡牌必须是该卡牌
                return Task.CompletedTask;
            if (onCheckConditionBuffed != null && !onCheckConditionBuffed(game, card, buff)) // 增益条件必须满足
                return Task.CompletedTask;
            else if (onCheckCondition != null && !onCheckCondition(game, card)) // 触发条件必须满足
                return Task.CompletedTask;
            if (onExecuteBuffed != null) 
                return onExecuteBuffed(game, card, buff, deathEvent.position);
            else if (onExecute != null)
                return onExecute(game, card, deathEvent.position);
            return Task.CompletedTask;
        }

        public Task onDisable(IGame game, ICard card, IBuff buff)
        {
            return onDisable(game as CardEngine, card as Card, buff as Buff);
        }

        public override Task onDisable(CardEngine game, Card card, Buff buff)
        {
            string propName = getPropName(buff);
            game.logger.logTrace("Effect", $"{card}禁用亡语{propName}");
            return card.setProp(game, propName, true);
        }
        public bool isDisabled(IGame game, ICard card, IBuff buff)
        {
            string propName = getPropName(buff);
            return card.getProp<bool>(game, propName);
        }

        private string getPropName(IBuff buff)
        {
            if (buff == null)
                return "DeathRattleDisabled<" + id + ">";
            else
                return "DeathRattleDisabled<" + id + ">(" + buff.instanceID + ")";
        }
    }

    public class BeforeYourTurnEnd : THHEffectBefore<HearthStoneRule.TurnEndEventArg>
    {
        public BeforeYourTurnEnd(PileFlag pile, ExecuteDelegate onExecute) : base(pile,
            (game, card, buff, arg) => { return arg.player == card.getOwner(); }, null, onExecute)
        {
        }

        public BeforeYourTurnEnd(PileFlag pile, ExecuteNoTargetDelegate onExecute) :
            base(pile, (game, card, buff, arg) => { return arg.player == card.getOwner(); }, onExecute)
        {
        }
    }

    public class BeforeTurnStart : THHEffectBefore<HearthStoneRule.TurnStartEventArg>
    {
        public BeforeTurnStart(PileFlag pile, ExecuteDelegate onExecute,
            CheckConditionDelegate onCheckCondition = null) :
            base(pile, onCheckCondition, null, onExecute)
        {
        }

        public BeforeTurnStart(PileFlag pile, ExecuteNoTargetDelegate onExecute,
            CheckConditionDelegate onCheckCondition = null) :
            base(pile, onCheckCondition, onExecute)
        {
        }
    }

    public class AfterTurnStart : THHEffectAfter<HearthStoneRule.TurnStartEventArg>
    {
        public AfterTurnStart(PileFlag pile, ExecuteDelegate onExecute,
            CheckConditionDelegate onCheckCondition = null) :
            base(pile, onCheckCondition, null, onExecute)
        {
        }

        public AfterTurnStart(PileFlag pile, ExecuteNoBuffNoTargetDelegate onExecute,
            CheckConditionNoBuffDelegate onCheckCondition = null) :
            base(pile, onCheckCondition, onExecute)
        {
        }
    }

    [Obsolete("EventTriggeredModifierFixer")]
    /// <summary>
    /// 整数属性修改器，继承PassiveEffect表示它是一个被动效果。
    /// </summary>
    /// <typeparam name="T">T表示属性修正将会在什么事件发生的时候进行更改</typeparam>
    public class IntPropFixer<T> : PassiveEffect where T : IEventArg
    {
        /// <summary>
        /// 被动效果生效的牌堆，比如默认大部分效果是在战场上生效的，你肯定不会希望鱼人领军者在手牌就能给战场上的鱼人提供光环。
        /// </summary>
        public override string[] piles { get; }

        /// <summary>
        /// 修改器修正器
        /// </summary>
        public struct ModifierFixer
        {
            /// <summary>
            /// 整数属性修改器
            /// </summary>
            public IntPropModifier modifier;

            /// <summary>
            /// 用于校准修改器数值的委托
            /// </summary>
            public Func<IGame, Card, Buff, T, int> fixer;

            /// <summary>
            /// 修改器修正器构造函数
            /// </summary>
            /// <param name="modifier">属性修改器</param>
            /// <param name="fixer">当事件发生时，属性修改器的修改值如何发生改变？</param>
            public ModifierFixer(IntPropModifier modifier, Func<IGame, Card, Buff, T, int> fixer)
            {
                this.modifier = modifier;
                this.fixer = fixer;
            }
        }

        ModifierFixer[] modifiers { get; }

        /// <summary>
        /// 整数属性修正器
        /// </summary>
        /// <param name="pile"></param>
        /// <param name="modifiers"></param>
        public IntPropFixer(PileFlag pile, params ModifierFixer[] modifiers)
        {
            piles = pile.getPiles();
            this.modifiers = modifiers;
        }

        /// <summary>
        /// 当被动效果生效的时候被调用
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        public override async Task onEnable(CardEngine game, Card card, Buff buff)
        {
            //首先为卡片添加属性修改器，比如山岭巨人的“费用修改器”，然后设置修改器的初始值。
            foreach (var pair in modifiers)
            {
                IntPropModifier modifier = card.getProp<IntPropModifier>(game, getModifierName(pair.modifier, buff));
                if (modifier == null)
                {
                    modifier = pair.modifier.clone() as IntPropModifier;
                    await card.setProp(game, getModifierName(pair.modifier, buff), modifier);
                }

                if (!card.getModifiers().Contains(modifier))
                {
                    await card.addModifier(game, modifier);
                }

                await modifier.setValue(game, card, pair.fixer(game, card, buff, default));
            }

            //注册当事件发生后的触发器，比如山岭巨人会注册“当卡牌移动”（手牌数量变动也属于卡牌移动的一种）事件，
            //当事件发生之后，调用fixer来修改modifier的修正值。
            //也就是说“每当卡牌发生移动的时候，将山岭巨人的费用修正器的费用修正为等于手牌数量的负数”
            //这里注册触发器就是为了实现这个效果。
            Trigger<T> trigger = card.getProp<Trigger<T>>(game, getTriggerName(buff));
            if (trigger == null)
            {
                trigger = new Trigger<T>(action: async arg =>
                {
                    foreach (var pair in modifiers)
                    {
                        IntPropModifier m = card.getProp<IntPropModifier>(game, getModifierName(pair.modifier, buff));
                        if (m == null)
                        {
                            m = pair.modifier.clone() as IntPropModifier;
                            await card.setProp(game, getModifierName(pair.modifier, buff), m);
                        }

                        if (!card.getModifiers().Contains(m))
                        {
                            await card.addModifier(game, m);
                        }

                        await m.setValue(game, card, pair.fixer(game, card, buff, arg));
                    }
                });
                await card.setProp(game, getTriggerName(buff), trigger);
            }

            game.triggers.registerAfter(trigger);
        }

        /// <summary>
        /// 为触发器生成一个独一无二的属性名。
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        private static string getTriggerName(Buff buff)
        {
            return nameof(IntPropFixer<T>) + "Trigger" + (buff != null ? buff.instanceID.ToString() : "");
        }

        /// <summary>
        /// 为修改器生成一个独一无二的属性名。
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        private string getModifierName(IntPropModifier modifier, Buff buff)
        {
            return nameof(IntPropFixer<T>) + modifier.GetType().Name + (buff != null ? buff.instanceID.ToString() : "");
        }

        /// <summary>
        /// 当效果失效的时候被调用。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        public override async Task onDisable(CardEngine game, Card card, Buff buff)
        {
            //移除所有修改器
            foreach (var pair in modifiers)
            {
                IntPropModifier modifier = card.getProp<IntPropModifier>(game, getModifierName(pair.modifier, buff));
                if (modifier != null)
                    await card.removeModifier(game, modifier);
            }

            //注销所有触发器
            Trigger<T> trigger = card.getProp<Trigger<T>>(game, getTriggerName(buff));
            if (trigger != null)
                game.triggers.removeAfter(trigger);
        }
    }

    public class SecretTriggerBefore<T> : THHEffectBefore<T> where T : IEventArg
    {
        public SecretTriggerBefore(CheckConditionDelegate onCheckCondition, ExecuteNoTargetDelegate onExecute) :
            base(PileFlag.head, onCheckCondition,
                (game, card, buff, arg) =>
                {
                    (card as Card).TriggerSecret(game);
                    return onExecute(game, card, buff, arg);
                })
        {
        }

        public SecretTriggerBefore(CheckConditionNoBuffDelegate onCheckCondition, ExecuteNoBuffNoTargetDelegate onExecute) :
            this((game, card, buff, arg) => onCheckCondition(game, card, arg),
                (game, card, buff, arg) =>
                {
                    (card as Card).TriggerSecret(game);
                    return onExecute(game, card, arg);
                })
        {

        }
    }

    public class SecrettTriggerAfter<T> : THHEffectBefore<T> where T : IEventArg
    {
        public SecrettTriggerAfter(CheckConditionDelegate onCheckCondition, ExecuteNoTargetDelegate onExecute) :
            base(PileFlag.head, onCheckCondition,
                (game, card, buff, arg) =>
            {
                (card as Card).TriggerSecret(game);
                return onExecute(game, card, buff, arg);
            })
        {
        }

        public SecrettTriggerAfter(CheckConditionNoBuffDelegate onCheckCondition, ExecuteNoBuffNoTargetDelegate onExecute) :
            this((game, card, buff, arg) => onCheckCondition(game, card, arg),
                (game, card, buff, arg) =>
                {
                    (card as Card).TriggerSecret(game);
                    return onExecute(game, card, arg);
                })
        {

        }
    }

    public class MissionTrigger<T> : THHEffectAfter<T> where T : IEventArg
    {
        public MissionTrigger(CheckConditionDelegate onCheckCondition, ExecuteNoTargetDelegate onExecute,
            int aimCount) :
            base(PileFlag.head, onCheckCondition,
                async (game, card, buff, arg) =>
                {
                    var counterName = getCounterName(game, card, buff);
                    await card.setProp(game, counterName, card.getProp<int>(game, counterName) + 1);
                    if (card.getProp<int>(game, counterName) == aimCount)
                    {
                        await (card as Card).MissionComplete(game);
                        await onExecute(game, card, buff, arg);
                    }
                })
        {
            onRegisterTrigger += (game, card, buff) =>
            {
                card.setProp(game, getCounterName(game, card, buff), 0);
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onCheckCondition">这个任务的小目标条件</param>
        /// <param name="onExecute">这个任务小目标全部完成时</param>
        /// <param name="aimCount">需要完成多少次小目标</param>
        public MissionTrigger(CheckConditionNoBuffDelegate onCheckCondition, ExecuteNoBuffNoTargetDelegate onExecute,
            int aimCount) :
            this(((game, card, buff, arg) => onCheckCondition(game, card, arg)),
                (game, card, buff, arg) => onExecute(game, card, arg), aimCount)
        {
        }

        protected override string getEffectName(IGame game, ICard card, IBuff buff, TriggerTime time)
        {
            return $"{card}_Mission";
        }

        private static string getCounterName(IGame game, ICard card, IBuff buff)
        {
            return $"{card}_Mission_Counter";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Prop">Modifier对应的属性</typeparam>
    public class EventTriggeredModifierFixer<Prop> : PassiveEffect
    {
        public override string[] piles { get; }
        PropModifier<Prop> modifier { get; }
        IModifierFixer[] fixers { get; }
        private string identifier { get; }

        public interface IModifierFixer : ITriggerEffect
        {
        }

        public class ModifierFixer<T> : THHEffectAfter<T>, IModifierFixer where T : IEventArg
        {
            public delegate Prop ModifierFixerDelegate(CardEngine game, Card card, Buff buff, T arg);

            private string identifier { get; }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="onCheckCondition"></param>
            /// <param name="fixer">该方法返回的值会在事件发生后替换原有Modifier的值</param>
            /// <param name="identifier"></param>
            /// <param name="isUpdatedOnEnable">是否在效果激活时进行修正</param>
            public ModifierFixer(CheckConditionDelegate onCheckCondition, ModifierFixerDelegate fixer, string identifier = null, bool isUpdatedOnEnable = true) :
                base(PileFlag.none, onCheckCondition, (game, card, buff, arg) =>
                {
                    var modifier = card.getProp<PropModifier<Prop>>(game, getModifierName(game, card, buff, identifier));
                    if (modifier != null)
                    {
                        return modifier.setValue(game, card, fixer(game, card, buff, arg));
                    }
                    return Task.CompletedTask;
                })
            {
                this.identifier = identifier;
                if (isUpdatedOnEnable)
                {
                    onRegisterTrigger += (game, card, buff) =>
                    {
                        var modifier = card.getProp<PropModifier<Prop>>(game, getModifierName(game, card, buff, identifier));
                        if (modifier != null)
                        {
                            modifier.setValue(game, card as Card,
                                fixer(game as CardEngine, card as Card, buff as Buff, default));
                        }
                    };
                }
            }

            protected override string getEffectName(IGame game, ICard card, IBuff buff, TriggerTime time)
            {
                return string.IsNullOrEmpty(_name)
                    ? (buff != null ? buff.instanceID.ToString() : string.Empty) +
                      "ModifierFixer" + identifier + time.getEventName(game.triggers)
                    : _name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="piles"></param>
        /// <param name="modifier"></param>
        /// <param name="identifier">对应一张卡牌定义中出现多个该类效果的情况，切记在ModifierFixer构造时也需附上相同的identifier
        /// （见半幽灵
        /// </param>
        /// <param name="fixers">EventTriggeredModifierFixer.ModifierFixer<T> T : IEventArg</param>
        public EventTriggeredModifierFixer(PileFlag piles, PropModifier<Prop> modifier, string identifier, params IModifierFixer[] fixers)
        {
            this.piles = piles.getPiles();
            this.modifier = modifier;
            this.fixers = fixers;
            this.identifier = identifier;
        }

        public override async Task onEnable(CardEngine game, Card card, Buff buff)
        {
            PropModifier<Prop> modifier = card.getProp<PropModifier<Prop>>(game, getModifierName(game, card, buff, identifier));
            if (modifier == null)
            {
                modifier = this.modifier.clone() as PropModifier<Prop>;
                await card.setProp(game, getModifierName(game, card, buff, identifier), modifier);
            }

            if (!card.getModifiers().Contains(modifier))
            {
                await card.addModifier(game, modifier);
            }

            foreach (var fixer in fixers)
            {
                fixer.onEnable(game, card, buff);
            }
        }

        public override async Task onDisable(CardEngine game, Card card, Buff buff)
        {
            card.removeModifier(game, modifier);
            await card.setProp(game, getModifierName(game, card, buff, identifier), null);
            foreach (var fixer in fixers)
            {
                fixer.onDisable(game, card, buff);
            }
        }

        public static string getModifierName(IGame game, ICard card, IBuff buff, string identifier)
        {
            return nameof(EventTriggeredModifierFixer<Prop>) + identifier +
                   (buff != null ? buff.instanceID.ToString() : "");
        }
    }
}