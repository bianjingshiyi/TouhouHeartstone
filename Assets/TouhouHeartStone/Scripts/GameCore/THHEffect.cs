using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Collections.Generic;
namespace TouhouHeartstone
{
    public class THHEffect : ITriggerEffect
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
        public delegate bool CheckConditionDelegate(THHGame game, Card card, object[] vars);
        CheckConditionDelegate onCheckCondition { get; }
        bool ITriggerEffect.checkCondition(IGame game, ICard card, object[] vars)
        {
            if (onCheckCondition != null)
                return onCheckCondition.Invoke(game as THHGame, card as Card, vars);
            else
                return true;
        }
        public delegate bool CheckTargetDelegate(THHGame game, Card card, object[] targets);
        CheckTargetDelegate onCheckTarget { get; }
        bool ITriggerEffect.checkTargets(IGame game, ICard card, object[] vars, object[] targets)
        {
            if (onCheckTarget != null)
                return onCheckTarget.Invoke(game as THHGame, card as Card, targets);
            else
                return true;
        }
        public delegate Task ExecuteDelegate(THHGame game, Card card, object[] vars, object[] targets);
        ExecuteDelegate onExecute { get; }
        public bool enabled { get; set; }
        Task ITriggerEffect.execute(IGame game, ICard card, object[] vars, object[] targets)
        {
            if (onExecute != null)
                return onExecute.Invoke(game as THHGame, card as Card, vars, targets);
            else
                return Task.CompletedTask;
        }
        public THHEffect(TriggerTime trigger, string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute, string name = null)
        {
            _name = name;
            triggers = new TriggerTime[] { trigger };
            piles = new string[] { pile };
            this.onCheckCondition = onCheckCondition;
            this.onCheckTarget = onCheckTarget;
            this.onExecute = onExecute;
        }
        public THHEffect(TriggerTime[] triggers, string[] piles, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute)
        {
            this.triggers = triggers;
            this.piles = piles;
            this.onCheckCondition = onCheckCondition;
            this.onCheckTarget = onCheckTarget;
            this.onExecute = onExecute;
        }
        public virtual async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger trigger = new Trigger(args =>
                {
                    if ((this as ITriggerEffect).checkCondition(game, card, args))
                        return (this as ITriggerEffect).execute(game, card, args, new object[0]);
                    else
                        return Task.CompletedTask;
                }, name: effectName);
                await card.setProp(game, effectName, trigger);
                game.logger.log("Effect", card + "注册效果" + effectName);
                game.triggers.register(time.getEventName(game.triggers), trigger);
            }
        }

        protected string getEffectName(IGame game, ICard card, IBuff buff, TriggerTime time)
        {
            string name = string.IsNullOrEmpty(_name) ? "Effect" + Array.IndexOf(card.define.effects, this) + time.getEventName(game.triggers) : _name;
            if (buff != null)
                name += buff.instanceID;
            return name;
        }

        public virtual Task onDisable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger trigger = card.getProp<Trigger>(game, effectName);
                game.logger.log("Effect", card + "注销效果" + effectName);
                game.triggers.remove(trigger);
            }
            return Task.CompletedTask;
        }
        public override string ToString()
        {
            return string.IsNullOrEmpty(_name) ? base.ToString() : _name;
        }
    }
    public class THHEffectBefore<T> : THHEffect where T : IEventArg
    {
        public new delegate bool CheckConditionDelegate(THHGame game, Card card, T eventArg);
        public new delegate Task ExecuteDelegate(THHGame game, Card card, T eventArg);
        public THHEffectBefore(string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute, string name = null) :
        base(new Before<T>(), pile, (game, card, vars) =>
        {
            if (onCheckCondition != null && vars != null && vars.Length > 0 && vars[0] is T t)
                return onCheckCondition.Invoke(game, card, t);
            else
                return true;
        }, onCheckTarget, (game, card, vars, targets) =>
        {
            if (onExecute != null && vars != null && vars.Length > 0 && vars[0] is T t)
                return onExecute.Invoke(game, card, t);
            else
                return Task.CompletedTask;
        }, name)
        {
        }
        public THHEffectBefore(PileFlag pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute) :
        base(new TriggerTime[] { new Before<T>() }, pile.getPiles(), (game, card, vars) =>
        {
            if (onCheckCondition != null && vars != null && vars.Length > 0 && vars[0] is T t)
                return onCheckCondition.Invoke(game, card, t);
            else
                return true;
        }, onCheckTarget, (game, card, vars, targets) =>
        {
            if (onExecute != null && vars != null && vars.Length > 0 && vars[0] is T t)
                return onExecute.Invoke(game, card, t);
            else
                return Task.CompletedTask;
        })
        {
        }
        public override async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger<T> trigger = new Trigger<T>(arg =>
                {
                    if ((this as ITriggerEffect).checkCondition(game, card, new object[] { arg }))
                        return (this as ITriggerEffect).execute(game, card, new object[] { arg }, new object[0]);
                    else
                        return Task.CompletedTask;
                }, name: effectName);
                await card.setProp(game, effectName, trigger);
                game.logger.log("Effect", card + "注册效果" + effectName);
                game.triggers.registerBefore(trigger);
            }
        }
        public override Task onDisable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger<T> trigger = card.getProp<Trigger<T>>(game, effectName);
                game.logger.log("Effect", card + "注销效果" + effectName);
                game.triggers.removeBefore(trigger);
            }
            return Task.CompletedTask;
        }
    }
    public class THHEffect<T> : THHEffect where T : IEventArg
    {
        public new delegate bool CheckConditionDelegate(THHGame game, Card card, T eventArg);
        public new delegate Task ExecuteDelegate(THHGame game, Card card, T eventArg, object[] targets);
        public THHEffect(string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute) :
            base(new On<T>(), pile, (game, card, vars) =>
            {
                if (onCheckCondition != null)
                    return onCheckCondition.Invoke(game, card, (T)vars[0]);
                else
                    return true;
            }, onCheckTarget, (game, card, vars, targets) =>
            {
                if (onExecute != null && vars.Length > 0 && vars[0] is T t)
                    return onExecute.Invoke(game, card, t, targets);
                else
                    return Task.CompletedTask;
            })
        {
        }
    }
    public class THHEffectAfter<T> : THHEffect where T : IEventArg
    {
        public new delegate bool CheckConditionDelegate(THHGame game, Card card, T eventArg);
        public delegate bool CheckConditionBuffedDelegate(THHGame game, Card card, Buff buff, T eventArg);
        public new delegate Task ExecuteDelegate(THHGame game, Card card, T eventArg);
        public delegate Task ExecuteBuffedDelegate(THHGame game, Card card, Buff buff, T eventArg);
        public THHEffectAfter(string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute, string name = null) :
        base(new After<T>(), pile, (game, card, vars) =>
        {
            if (onCheckCondition != null && vars != null && vars.Length > 0 && vars.OfType<T>().FirstOrDefault() is T t)
                return onCheckCondition.Invoke(game, card, t);
            else
                return true;
        }, onCheckTarget, (game, card, vars, targets) =>
        {
            if (onExecute != null && vars != null && vars.Length > 0 && vars.OfType<T>().FirstOrDefault() is T t)
                return onExecute.Invoke(game, card, t);
            else
                return Task.CompletedTask;
        }, name)
        {
        }
        public THHEffectAfter(PileFlag pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute) :
        base(new TriggerTime[] { new After<T>() }, pile.getPiles(), (game, card, vars) =>
        {
            if (onCheckCondition != null && vars != null && vars.Length > 0 && vars.OfType<T>().FirstOrDefault() is T t)
                return onCheckCondition.Invoke(game, card, t);
            else
                return true;
        }, onCheckTarget, (game, card, vars, targets) =>
        {
            if (onExecute != null && vars != null && vars.Length > 0 && vars.OfType<T>().FirstOrDefault() is T t)
                return onExecute.Invoke(game, card, t);
            else
                return Task.CompletedTask;
        })
        {
        }
        public THHEffectAfter(PileFlag pile, CheckConditionBuffedDelegate onCheckCondition, ExecuteBuffedDelegate onExecute) :
        base(new TriggerTime[] { new After<T>() }, pile.getPiles(), (game, card, vars) =>
        {
            if (onCheckCondition != null && vars != null && vars.Length > 1 &&
                vars.OfType<Buff>().FirstOrDefault() is Buff buff &&
                vars.OfType<T>().FirstOrDefault() is T t)
                return onCheckCondition(game, card, buff, t);
            else
                return true;
        }, (game, card, vars) => true,
        (game, card, vars, targets) =>
        {
            if (onExecute != null && vars != null && vars.Length > 1 &&
                vars.OfType<Buff>().FirstOrDefault() is Buff buff &&
                vars.OfType<T>().FirstOrDefault() is T t)
                return onExecute(game, card, buff, t);
            else
                return Task.CompletedTask;
        })
        {
        }
        public override async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger<T> trigger = new Trigger<T>(arg =>
                {
                    if ((this as ITriggerEffect).checkCondition(game, card, new object[] { buff, arg }))
                        return (this as ITriggerEffect).execute(game, card, new object[] { buff, arg, }, new object[0]);
                    else
                        return Task.CompletedTask;
                }, name: effectName);
                await card.setProp(game, effectName, trigger);
                game.logger.log("Effect", card + "注册效果" + effectName);
                game.triggers.registerAfter(trigger);
            }
        }
        public override Task onDisable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger<T> trigger = card.getProp<Trigger<T>>(game, effectName);
                game.logger.log("Effect", card + "注销效果" + effectName);
                game.triggers.removeAfter(trigger);
            }
            return Task.CompletedTask;
        }
    }
    public abstract class PassiveEffect : IPassiveEffect
    {
        public string[] events => throw new NotImplementedException();
        public abstract string[] piles { get; }
        public bool enabled { get; set; }
        public bool checkCondition(IGame game, ICard card, object[] vars)
        {
            throw new System.NotImplementedException();
        }
        public bool checkTarget(IGame game, ICard card, object[] vars, object[] targets)
        {
            throw new NotImplementedException();
        }
        public Task execute(IGame game, ICard card, object[] vars, object[] targets)
        {
            throw new NotImplementedException();
        }
        public string[] getEvents(ITriggerManager manager)
        {
            return new string[0];
        }
        Task IPassiveEffect.onEnable(IGame game, ICard card, IBuff buff)
        {
            return onEnable(game as THHGame, card as Card, buff as Buff);
        }
        public abstract Task onEnable(THHGame game, Card card, Buff buff);
        Task IPassiveEffect.onDisable(IGame game, ICard card, IBuff buff)
        {
            return onDisable(game as THHGame, card as Card, buff as Buff);
        }
        public abstract Task onDisable(THHGame game, Card card, Buff buff);
    }
    public abstract class ActiveEffect : IActiveEffect
    {
        string _name;
        public ActiveEffect(string name = null)
        {
            _name = name;
        }
        public string[] piles => throw new NotImplementedException();
        public string[] events => throw new NotImplementedException();
        public string[] getEvents(ITriggerManager manager)
        {
            return new string[] { manager.getName<THHPlayer.ActiveEventArg>() };
        }
        public bool checkCondition(IGame game, ICard card, object[] vars)
        {
            return checkCondition(game as THHGame, card as Card);
        }
        public abstract bool checkCondition(THHGame game, Card card);
        public abstract Task execute(IGame game, ICard card, object[] vars, object[] targets);
        public override string ToString()
        {
            return string.IsNullOrEmpty(_name) ? base.ToString() : _name;
        }
    }
    public delegate Task ExecuteDelegate(THHGame game, Card card);
    public class NoTargetEffect : ActiveEffect
    {
        CheckConditionDelegate _onCheckCondition;
        ExecuteDelegate _onExecute;
        public NoTargetEffect(ExecuteDelegate onExecute, CheckConditionDelegate onCheckCondition = null, string name = null) : base(name)
        {
            _onCheckCondition = onCheckCondition;
            _onExecute = onExecute;
        }
        public override bool checkCondition(THHGame game, Card card)
        {
            return _onCheckCondition == null || _onCheckCondition(game, card);
        }
        public override Task execute(IGame game, ICard card, object[] vars, object[] targets)
        {
            if (_onExecute != null)
                return _onExecute(game as THHGame, card as Card);
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
        public override bool checkCondition(THHGame game, Card card)
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
            return checkTargets(game as THHGame, card as Card, targets);
        }
        public virtual bool checkTargets(THHGame game, Card card, object[] targets)
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
        public virtual bool checkTarget(THHGame game, Card card, Card target)
        {
            return ranges.getPiles(game, card.getOwner()).Any(p => p.Contains(target));
        }
        public override Task execute(IGame game, ICard card, object[] vars, object[] targets)
        {

            if (targets != null &&
                 targets.Length > 0 &&
                 targets[0] is Card target)
                return execute(game as THHGame, card as Card, target);
            else
            {
                return execute(game as THHGame, card as Card, null);
            }
        }
        public abstract Task execute(THHGame game, Card card, Card target);
    }
    public delegate bool CheckConditionDelegate(THHGame game, Card card);
    /// <summary>
    /// 使用lambda表达式的单一目标主动效果
    /// </summary>
    public class LambdaSingleTargetEffect : SingleTargetEffect
    {
        CheckConditionDelegate _onCheckCondition = null;
        public delegate bool CheckTargetDelegate(THHGame game, Card card, Card target);
        CheckTargetDelegate _onCheckTarget = null;
        public delegate Task ExecuteDelegate(THHGame game, Card card, Card target);
        public delegate Task ExecuteDelegate_NoTarget(THHGame game, Card card);
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
        public override bool checkCondition(THHGame game, Card card)
        {
            return base.checkCondition(game, card) && (_onCheckCondition == null || _onCheckCondition(game, card));
        }
        public override bool checkTarget(THHGame game, Card card, Card target)
        {
            return base.checkTarget(game, card, target) && (_onCheckTarget == null || _onCheckTarget(game, card, target));
        }
        public override Task execute(THHGame game, Card card, Card target)
        {
            if (_onExecute != null)
            {
                if (!(card.define is SpellCardDefine))
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
        Func<THHGame, Card, Pile[]> getRange { get; }
        Func<THHGame, Card, bool> filter { get; }
        Buff buff { get; }
        private Dictionary<Card, Buff> getBuffDic(THHGame game, Card card, Buff buff)
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
            propName = this.buff.id + nameof(Halo) + '.' + propName;
            if (buff != null)
                propName += buff.ToString();
            return propName;
        }

        private Trigger<Pile.MoveCardEventArg> getOnCardEnterHandTrigger(THHGame game, Card card, Buff buff)
        {
            return card.getProp<Trigger<Pile.MoveCardEventArg>>(game, getPropName(buff, "onCardEnterHandTrigger"));
        }
        private void setOnCardEnterHandTrigger(THHGame game, Card card, Buff buff, Trigger<Pile.MoveCardEventArg> value)
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
        public Halo(Buff buff, PileFlag range, PileFlag pile = PileFlag.self | PileFlag.field, Func<THHGame, Card, bool> filter = null)
            : this(PILE.getPiles(pile), (game, card) => range.getPiles(game, card.getOwner()), filter, buff)
        {
        }
        public Halo(string[] piles, Func<THHGame, Card, Pile[]> getRange, Func<THHGame, Card, bool> filter, Buff buff)
        {
            this.piles = piles;
            this.getRange = getRange;
            this.filter = filter;
            this.buff = buff;
        }
        public override async Task onEnable(THHGame game, Card card, Buff buff)
        {
            Pile[] ranges = getRange != null ? getRange(game, card) : null;
            if (getOnCardEnterHandTrigger(game, card, buff) == null)
            {
                setOnCardEnterHandTrigger(game, card, buff, new Trigger<Pile.MoveCardEventArg>(async arg =>
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
                            await arg.card.removeBuff(game, getBuffDic(game, card, buff)[arg.card]);
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
        public override async Task onDisable(THHGame game, Card card, Buff buff)
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
        Func<THHGame, Card, Buff, bool> condition { get; }
        PassiveEffect effect { get; }
        public ConditionalPassiveEffect(PassiveEffect effect, Func<THHGame, Card, Buff, bool> condition)
        {
            this.condition = condition;
            this.effect = effect;
        }
        public override string[] piles => effect.piles;
        public override async Task onEnable(THHGame game, Card card, Buff buff)
        {
            Trigger<THHGame.TurnStartEventArg> enableTrig = new Trigger<THHGame.TurnStartEventArg>(arg =>
            {
                if (condition == null || condition(game, card, buff))
                    effect.onEnable(game, card, buff);
                return Task.CompletedTask;
            });
            await card.setProp(game, nameof(ConditionalPassiveEffect) + GetHashCode() + "enableTrig", enableTrig);
            game.triggers.registerAfter(enableTrig);
            Trigger<THHPlayer.UseEventArg> disableTrig = new Trigger<THHPlayer.UseEventArg>(arg =>
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
        public override Task onDisable(THHGame game, Card card, Buff buff)
        {
            game.triggers.removeAfter(card.getProp<Trigger<THHGame.TurnStartEventArg>>(game, nameof(ConditionalPassiveEffect) + GetHashCode() + "enableTrig"));
            game.triggers.removeAfter(card.getProp<Trigger<THHPlayer.UseEventArg>>(game, nameof(ConditionalPassiveEffect) + GetHashCode() + "disableTrig"));
            return effect.onDisable(game, card, buff);
        }
    }
    public class AddBuffAfter<T> : THHEffectAfter<T> where T : IEventArg
    {
        public AddBuffAfter(string pile, CheckConditionDelegate onCheckCondition, Buff originBuff) : base(pile, onCheckCondition, null, (g, c, a) =>
        {
            return c.addBuff(g, originBuff.clone());
        })
        {
        }
        public AddBuffAfter(string pile, CheckConditionDelegate onCheckCondition, ExecuteDelegate onExecute) : base(pile, onCheckCondition, null, onExecute)
        {
        }
    }
    public class RemoveBuffBefore<T> : THHEffectBefore<T> where T : IEventArg
    {
        public RemoveBuffBefore(string pile, int buffId) : base(pile, null, null, (game, card, arg) =>
        {
            return card.removeBuff(game, card.getBuffs().Where(b => b.id == buffId));
        })
        {
        }
        public RemoveBuffBefore(string pile, CheckConditionDelegate onCheckCondition, int buffId) : base(pile, onCheckCondition, null, (game, card, arg) =>
        {
            return card.removeBuff(game, card.getBuffs().Where(b => b.id == buffId));
        })
        {
        }
    }
    public class RemoveBuffAfter<T> : THHEffectAfter<T> where T : IEventArg
    {
        public RemoveBuffAfter(string pile, int buffId) : base(pile, null, null, (game, card, arg) =>
        {
            return card.removeBuff(game, card.getBuffs().Where(b => b.id == buffId));
        }, "移除Buff")
        {
        }
        public RemoveBuffAfter(string pile, CheckConditionDelegate onCheckCondition, int buffId) : base(pile, onCheckCondition, null, (game, card, arg) =>
        {
            return card.removeBuff(game, card.getBuffs().Where(b => b.id == buffId));
        }, "移除Buff")
        {
        }
    }

    public class RemoveBuffAfterTimeUp<T> : THHEffectAfter<T> where T : IEventArg
    {
        public RemoveBuffAfterTimeUp(string pile, int buffId, int time) : base(pile, null, null, (game, card, arg) =>
         {
             if (time == 0)
             {
                 return card.removeBuff(game, card.getBuffs().Where(b => b.id == buffId));
             }
             else
             {
                 time--;
                 return Task.CompletedTask;
             }

         })
        {
        }
        public RemoveBuffAfterTimeUp(string pile, CheckConditionDelegate onCheckCondition, int buffId, int time) : base(pile, onCheckCondition, null, (game, card, arg) =>
         {
             if (time == 0)
             {
                 return card.removeBuff(game, card.getBuffs().Where(b => b.id == buffId));
             }
             else
             {
                 time--;
                 return Task.CompletedTask;
             }

         })
        {
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
    public class DeathRattle : IPassiveEffect
    {
        public int id { get; }
        public string[] piles { get; } = new string[] { PILE.FIELD, PILE.GRAVE };
        public CheckConditionDelegate onCheckCondition { get; }
        public delegate Task ExecuteDelegate(THHGame game, Card card, int position);
        public ExecuteDelegate onExecute { get; }
        public delegate bool CheckConditionBuffedDelegate(THHGame game, Card card, Buff buff);
        public CheckConditionBuffedDelegate onCheckConditionBuffed { get; } = null;
        public delegate Task ExecuteBuffedDelegate(THHGame game, Card card, Buff buff, int position);
        public ExecuteBuffedDelegate onExecuteBuffed { get; } = null;
        public DeathRattle(int id, ExecuteDelegate onExecute, CheckConditionDelegate onCheckCondition = null)
        {
            this.id = id;
            this.onExecute = onExecute;
            this.onCheckCondition = onCheckCondition;
        }
        public DeathRattle(int id, ExecuteBuffedDelegate onExecuteBuffed, CheckConditionBuffedDelegate onCheckConditionBuffed = null)
        {
            this.id = id;
            this.onExecuteBuffed = onExecuteBuffed;
            this.onCheckConditionBuffed = onCheckConditionBuffed;
        }
        public Task onEnable(IGame game, ICard card, IBuff buff)
        {
            return onEnable(game as THHGame, card as Card, buff as Buff);
        }
        public async Task onEnable(THHGame game, Card card, Buff buff)
        {
            string triggerName;
            if (buff == null)
                triggerName = "DeathTrigger<" + id + ">";
            else
                triggerName = "DeathTrigger<" + id + ">(" + buff + ")";
            Trigger<THHCard.DeathEventArg> trigger = card.getProp<Trigger<THHCard.DeathEventArg>>(game, triggerName);
            game.logger.log("Effect", card + "注册亡语" + triggerName);
            trigger = new Trigger<THHCard.DeathEventArg>(arg => execute(game, card, buff, arg));
            await card.setProp(game, triggerName, trigger);
            game.triggers.registerAfter(trigger);
        }
        public Task execute(THHGame game, Card card, Buff buff, THHCard.DeathEventArg deathEvent)
        {
            if (!deathEvent.infoDic.ContainsKey(card))
                return Task.CompletedTask;
            if (onCheckConditionBuffed != null && !onCheckConditionBuffed(game, card, buff))
                return Task.CompletedTask;
            else if (onCheckCondition != null && !onCheckCondition(game, card))
                return Task.CompletedTask;
            if (onExecuteBuffed != null)
                return onExecuteBuffed(game, card, buff, deathEvent.infoDic[card].position);
            else if (onExecute != null)
                return onExecute(game, card, deathEvent.infoDic[card].position);
            return Task.CompletedTask;
        }
        public Task onDisable(IGame game, ICard card, IBuff buff)
        {
            return onDisable(game as THHGame, card as Card, buff as Buff);
        }
        public Task onDisable(THHGame game, Card card, Buff buff)
        {
            string triggerName;
            if (buff == null)
                triggerName = "DeathTrigger<" + id + ">";
            else
                triggerName = "DeathTrigger<" + id + ">(" + buff + ")";
            Trigger<THHCard.DeathEventArg> trigger = card.getProp<Trigger<THHCard.DeathEventArg>>(game, triggerName);
            game.logger.log("Effect", card + "注销亡语" + triggerName);
            game.triggers.removeAfter(trigger);
            return card.setProp<Trigger<THHCard.DamageEventArg>>(game, triggerName, null);
        }
    }

    public class BeforeYourTurnEnd : THHEffectBefore<THHGame.TurnEndEventArg>
    {
        public BeforeYourTurnEnd(ExecuteDelegate onExecute) : base(PILE.FIELD, (game, card, arg) =>
        {
            return arg.player == card.getOwner();
        }, null, async (game, card, arg) =>
        {
            if (onExecute != null)
            {
                await onExecute.Invoke(game, card, arg);
                await game.updateDeath(game);
            }
        })
        {
        }
    }
    public class BeforeTurnStart : THHEffectBefore<THHGame.TurnStartEventArg>
    {
        public BeforeTurnStart(ExecuteDelegate onExecute, CheckConditionDelegate onCheckCondition = null) : base(PILE.FIELD, onCheckCondition, null, async (game, card, arg) =>
         {
             if (onExecute != null)
             {
                 await onExecute.Invoke(game, card, arg);
                 await game.updateDeath(game);
             }
         })
        {
        }
    }
    public class AfterTurnStart : THHEffectAfter<THHGame.TurnStartEventArg>
    {
        public AfterTurnStart(ExecuteDelegate onExecute, CheckConditionDelegate onCheckCondition = null) : base(PILE.FIELD, onCheckCondition, null, async (game, card, arg) =>
        {
            if (onExecute != null)
            {
                await onExecute.Invoke(game, card, arg);
                await game.updateDeath(game);
            }
        })
        {
        }
    }
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
        public override async Task onEnable(THHGame game, Card card, Buff buff)
        {
            //首先为卡片添加属性修改器，比如山岭巨人的“费用修改器”，然后设置修改器的初始值。
            foreach (var pair in modifiers)
            {
                IntPropModifier modifier = card.getProp<IntPropModifier>(game, getModifierName(pair.modifier, buff));
                if (modifier == null)
                {
                    modifier = pair.modifier.clone() as IntPropModifier;
                    await card.setProp(game, getModifierName(pair.modifier, buff), modifier);
                    await card.addModifier(game, modifier);
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
                trigger = new Trigger<T>(async arg =>
                {
                    foreach (var pair in modifiers)
                    {
                        IntPropModifier m = card.getProp<IntPropModifier>(game, getModifierName(pair.modifier, buff));
                        if (m == null)
                        {
                            m = pair.modifier.clone() as IntPropModifier;
                            await card.setProp(game, getModifierName(pair.modifier, buff), m);
                            await card.addModifier(game, m);
                        }
                        else
                        {
                            await card.removeModifier(game, m);
                            m = pair.modifier.clone() as IntPropModifier;
                            await card.setProp(game, getModifierName(pair.modifier, buff), m);
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
        public override async Task onDisable(THHGame game, Card card, Buff buff)
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
        public SecretTriggerBefore(CheckConditionDelegate onCheckCondition, ExecuteDelegate onExecute) : base(PILE.HEAD, onCheckCondition, null, onExecute)
        {
        }
        public override async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger<T> trigger = new Trigger<T>(async arg =>
                {
                    if ((this as ITriggerEffect).checkCondition(game, card, new object[] { arg }))
                    {
                        await (card as Card).TriggerSecret(game);
                        await (this as ITriggerEffect).execute(game, card, new object[] { arg }, new object[0]);
                    }
                }, name: effectName);
                await card.setProp(game, effectName, trigger);
                game.logger.log("Effect", card + "注册效果" + effectName);
                game.triggers.registerBefore(trigger);
            }
        }
        public override Task onDisable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = getEffectName(game, card, buff, time);
                Trigger<T> trigger = card.getProp<Trigger<T>>(game, effectName);
                game.logger.log("Effect", card + "注销效果" + effectName);
                game.triggers.removeBefore(trigger);
            }
            return Task.CompletedTask;
        }
    }
    public class SecrettTriggerAfter<T> : THHEffectBefore<T> where T : IEventArg
    {
        public SecrettTriggerAfter(CheckConditionDelegate onCheckCondition, ExecuteDelegate onExecute) : base(PILE.HEAD, onCheckCondition, null, onExecute)
        {
        }
        public override async Task onEnable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = "Effect" + Array.IndexOf(card.define.effects, this) + time.getEventName(game.triggers);
                Trigger<T> trigger = new Trigger<T>(arg =>
                {
                    if ((this as ITriggerEffect).checkCondition(game, card, new object[] { arg }))
                    {
                        (card as Card).TriggerSecret(game);
                        return (this as ITriggerEffect).execute(game, card, new object[] { arg }, new object[0]);
                    }
                    else
                        return Task.CompletedTask;
                }, name: effectName);
                await card.setProp(game, effectName, trigger);
                game.logger.log("Effect", card + "注册效果" + effectName);
                game.triggers.registerAfter(trigger);
            }
        }
        public override Task onDisable(IGame game, ICard card, IBuff buff)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = "Effect" + Array.IndexOf(card.define.effects, this) + time.getEventName(game.triggers);
                Trigger<T> trigger = card.getProp<Trigger<T>>(game, effectName);
                game.logger.log("Effect", card + "注销效果" + effectName);
                game.triggers.removeAfter(trigger);
            }
            return Task.CompletedTask;
        }
    }


    //public class ChooseOneEffect : SingleTargetEffect
    //{
    //    /// <summary>
    //    /// 选择一项：从以下效果中选择一个触发
    //    /// </summary>
    //    /// <param name="cards">抉择选项，实质是法术衍生牌</param>
    //    /// <param name="targetRange">指向型抉择的指向范围</param>
    //    /// <param name="targetRange">指向型抉择的指向范围</param>
    //    /// <returns></returns>
    //    public ChooseOneEffect()
    //    {

    //    }
    //    public override Task execute(THHGame game, Card card, Card target)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

}