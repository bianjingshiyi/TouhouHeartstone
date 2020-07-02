using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class THHEffect : IActiveEffect
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
        bool IActiveEffect.checkTarget(IGame game, ICard card, object[] vars, object[] targets)
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
        public virtual void register(IGame game, ICard card)
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
        public virtual void unregister(IGame game, ICard card)
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
        public override void register(IGame game, ICard card)
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
        public override void unregister(IGame game, ICard card)
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
        public override void register(IGame game, ICard card)
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
        public override void unregister(IGame game, ICard card)
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
    public abstract class PassiveEffect : IEffect
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
        public abstract void register(IGame game, ICard card);
        public abstract void unregister(IGame game, ICard card);
    }
}