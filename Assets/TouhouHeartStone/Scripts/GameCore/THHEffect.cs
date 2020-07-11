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
        Task ITriggerEffect.execute(IGame game, ICard card, object[] vars, object[] targets)
        {
            if (onExecute != null)
                return onExecute.Invoke(game as THHGame, card as Card, vars, targets);
            else
                return Task.CompletedTask;
        }
        public THHEffect(TriggerTime trigger, string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute)
        {
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
        public virtual void onEnable(IGame game, ICard card)
        {
            foreach (TriggerTime time in triggers)
            {
                Trigger trigger = new Trigger(args =>
                {
                    if ((this as ITriggerEffect).checkCondition(game, card, args))
                        return (this as ITriggerEffect).execute(game, card, args, new object[0]);
                    else
                        return Task.CompletedTask;
                });
                string effectName = "Effect" + Array.IndexOf(card.define.effects, this) + time.getEventName(game.triggers);
                card.setProp(effectName, trigger);
                game.logger.log("Effect", card + "注册效果" + effectName);
                game.triggers.register(time.getEventName(game.triggers), trigger);
            }
        }
        public virtual void onDisable(IGame game, ICard card)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = "Effect" + Array.IndexOf(card.define.effects, this) + time.getEventName(game.triggers);
                Trigger trigger = card.getProp<Trigger>(effectName);
                game.logger.log("Effect", card + "注销效果" + effectName);
                game.triggers.remove(trigger);
            }
        }
    }
    public class THHEffectBefore<T> : THHEffect where T : IEventArg
    {
        public new delegate bool CheckConditionDelegate(THHGame game, Card card, T eventArg);
        public new delegate Task ExecuteDelegate(THHGame game, Card card, T eventArg);
        public THHEffectBefore(string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute) :
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
            })
        {
        }
        public override void onEnable(IGame game, ICard card)
        {
            foreach (TriggerTime time in triggers)
            {
                Trigger<T> trigger = new Trigger<T>(arg =>
                {
                    if ((this as ITriggerEffect).checkCondition(game, card, new object[] { arg }))
                        return (this as ITriggerEffect).execute(game, card, new object[] { arg }, new object[0]);
                    else
                        return Task.CompletedTask;
                });
                string effectName = "Effect" + Array.IndexOf(card.define.effects, this) + time.getEventName(game.triggers);
                card.setProp(effectName, trigger);
                game.logger.log("Effect", card + "注册效果" + effectName);
                game.triggers.registerBefore(trigger);
            }
        }
        public override void onDisable(IGame game, ICard card)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = "Effect" + Array.IndexOf(card.define.effects, this) + time.getEventName(game.triggers);
                Trigger<T> trigger = card.getProp<Trigger<T>>(effectName);
                game.logger.log("Effect", card + "注销效果" + effectName);
                game.triggers.removeBefore(trigger);
            }
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
        public new delegate Task ExecuteDelegate(THHGame game, Card card, T eventArg);
        public THHEffectAfter(string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute) :
            base(new After<T>(), pile, (game, card, vars) =>
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
        public override void onEnable(IGame game, ICard card)
        {
            foreach (TriggerTime time in triggers)
            {
                Trigger<T> trigger = new Trigger<T>(arg =>
                {
                    if ((this as ITriggerEffect).checkCondition(game, card, new object[] { arg }))
                        return (this as ITriggerEffect).execute(game, card, new object[] { arg }, new object[0]);
                    else
                        return Task.CompletedTask;
                });
                string effectName = "Effect" + Array.IndexOf(card.define.effects, this) + time.getEventName(game.triggers);
                card.setProp(effectName, trigger);
                game.logger.log("Effect", card + "注册效果" + effectName);
                game.triggers.registerAfter(trigger);
            }
        }
        public override void onDisable(IGame game, ICard card)
        {
            foreach (TriggerTime time in triggers)
            {
                string effectName = "Effect" + Array.IndexOf(card.define.effects, this) + time.getEventName(game.triggers);
                Trigger<T> trigger = card.getProp<Trigger<T>>(effectName);
                game.logger.log("Effect", card + "注销效果" + effectName);
                game.triggers.removeAfter(trigger);
            }
        }
    }
    public abstract class PassiveEffect : IPassiveEffect
    {
        public string[] events => throw new System.NotImplementedException();
        public abstract string[] piles { get; }
        public bool checkCondition(IGame game, ICard card, object[] vars)
        {
            throw new System.NotImplementedException();
        }
        public bool checkTarget(IGame game, ICard card, object[] vars, object[] targets)
        {
            throw new System.NotImplementedException();
        }
        public Task execute(IGame game, ICard card, object[] vars, object[] targets)
        {
            throw new System.NotImplementedException();
        }
        public string[] getEvents(ITriggerManager manager)
        {
            return new string[0];
        }
        void IPassiveEffect.onEnable(IGame game, ICard card)
        {
            onEnable(game as THHGame, card as Card);
        }
        public abstract void onEnable(THHGame game, Card card);
        void IPassiveEffect.onDisable(IGame game, ICard card)
        {
            onDisable(game as THHGame, card as Card);
        }
        public abstract void onDisable(THHGame game, Card card);
    }
    public abstract class ActiveEffect : IActiveEffect
    {
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
    }
    public class NoTargetEffect : ActiveEffect
    {
        CheckConditionDelegate _onCheckCondition;
        public delegate Task ExecuteDelegate(THHGame game, Card card);
        ExecuteDelegate _onExecute;
        public NoTargetEffect(ExecuteDelegate onExecute, CheckConditionDelegate onCheckCondition = null)
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
        public string[] ranges { get; protected set; } = new string[]
        {
            PileName.FIELD,
            PileName.MASTER
        };
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
                foreach (var pileName in ranges)
                {
                    foreach (var target in player.getPile(pileName))
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
        /// 默认实现，目标必须在可选范围内
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool checkTarget(THHGame game, Card card, Card target)
        {
            return ranges.Contains(target.pile.name);
        }
        public override Task execute(IGame game, ICard card, object[] vars, object[] targets)
        {
            if (targets != null &&
                targets.Length > 0 &&
                targets[0] is Card target)
                return execute(game as THHGame, card as Card, target);
            return Task.CompletedTask;
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
        ExecuteDelegate _onExecute = null;
        public LambdaSingleTargetEffect(ExecuteDelegate onExecute,
            PileFlag ranges = PileFlag.none,
            CheckConditionDelegate onCheckCondition = null,
            CheckTargetDelegate onCheckTarget = null)
        {
            if (ranges != PileFlag.none)
                this.ranges = PileName.getPiles(ranges);
            _onCheckCondition = onCheckCondition;
            _onCheckTarget = onCheckTarget;
            _onExecute = onExecute;
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
                return _onExecute(game, card, target);
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
        Dictionary<Card, Buff> buffDic { get; } = new Dictionary<Card, Buff>();
        Trigger<Pile.MoveCardEventArg> onCardEnterHandTrigger { get; set; } = null;
        /// <summary>
        /// 简化的构造器
        /// </summary>
        /// <param name="buff">光环添加的buff</param>
        /// <param name="range"></param>
        /// <param name="pile"></param>
        /// <param name="filter"></param>
        public Halo(Buff buff, PileFlag range, PileFlag pile = PileFlag.self | PileFlag.field, Func<THHGame, Card, bool> filter = null)
        {
            this.buff = buff;
            piles = PileName.getPiles(pile);
            getRange = (game, card) => range.getPiles(game, card.getOwner());
            this.filter = filter;
        }
        public Halo(string[] piles, Func<THHGame, Card, Pile[]> getRange, Func<THHGame, Card, bool> filter, Buff buff)
        {
            this.piles = piles;
            this.getRange = getRange;
            this.filter = filter;
            this.buff = buff;
        }
        public override void onEnable(THHGame game, Card card)
        {
            Pile[] ranges = getRange != null ? getRange(game, card) : null;
            if (onCardEnterHandTrigger == null)
            {
                onCardEnterHandTrigger = new Trigger<Pile.MoveCardEventArg>(arg =>
                {
                    if (ranges == null)
                        return Task.CompletedTask;
                    if (ranges.Contains(arg.to) &&
                       (filter == null || filter(game, arg.card)))
                    {
                        Buff buff = this.buff.clone();
                        arg.card.addBuff(game, buff);
                        buffDic.Add(arg.card, buff);
                    }
                    else if (ranges.Contains(arg.from) &&
                        (filter == null || filter(game, arg.card)))
                    {
                        if (buffDic.ContainsKey(arg.card))
                            arg.card.removeBuff(game, buffDic[arg.card]);
                    }
                    return Task.CompletedTask;
                });
                game.triggers.registerAfter(onCardEnterHandTrigger);
            }
            foreach (var target in card.getOwner().hand.Where(c => filter == null || filter(game, c)))
            {
                Buff buff = this.buff.clone();
                target.addBuff(game, buff);
                buffDic.Add(target, buff);
            }
        }
        public override void onDisable(THHGame game, Card card)
        {
            if (onCardEnterHandTrigger != null)
            {
                game.triggers.removeAfter(onCardEnterHandTrigger);
                onCardEnterHandTrigger = null;
            }
            foreach (var pair in buffDic)
            {
                pair.Key.removeBuff(game, pair.Value);
            }
            buffDic.Clear();
        }
    }
}