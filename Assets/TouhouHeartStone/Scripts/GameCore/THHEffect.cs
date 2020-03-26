using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class THHEffect : IEffect
    {
        public TriggerTime[] triggers { get; }
        string[] IEffect.events
        {
            get { throw new NotImplementedException(); }
        }
        string[] IEffect.getEvents(ITriggerManager manager)
        {
            return triggers.Select(t => t.getEventName(manager)).ToArray();
        }
        public string[] piles { get; }
        public delegate bool CheckConditionDelegate(THHGame game, THHPlayer player, Card card, object[] vars);
        CheckConditionDelegate onCheckCondition { get; }
        bool IEffect.checkCondition(IGame game, IPlayer player, ICard card, object[] vars)
        {
            if (onCheckCondition != null)
                return onCheckCondition.Invoke(game as THHGame, player as THHPlayer, card as Card, vars);
            else
                return true;
        }
        public delegate bool CheckTargetDelegate(THHGame game, THHPlayer player, Card card, object[] targets);
        CheckTargetDelegate onCheckTarget { get; }
        bool IEffect.checkTarget(IGame game, IPlayer player, ICard card, object[] targets)
        {
            if (onCheckTarget != null)
                return onCheckTarget.Invoke(game as THHGame, player as THHPlayer, card as Card, targets);
            else
                return true;
        }
        public delegate Task ExecuteDelegate(THHGame game, THHPlayer player, Card card, object[] vars, object[] targets);
        ExecuteDelegate onExecute { get; }
        Task IEffect.execute(IGame game, IPlayer player, ICard card, object[] vars, object[] targets)
        {
            if (onExecute != null)
                return onExecute.Invoke(game as THHGame, player as THHPlayer, card as Card, vars, targets);
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
    }
    public class THHEffectBefore<T> : THHEffect where T : IEventArg
    {
        public new delegate bool CheckConditionDelegate(THHGame game, THHPlayer player, Card card, T eventArg);
        public new delegate Task ExecuteDelegate(THHGame game, THHPlayer player, Card card, T eventArg);
        public THHEffectBefore(string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute) :
            base(new Before<T>(), pile, (game, player, card, vars) =>
            {
                if (onCheckCondition != null)
                    return onCheckCondition.Invoke(game, player, card, (T)vars[0]);
                else
                    return true;
            }, onCheckTarget, (game, player, card, vars, targets) =>
            {
                if (onExecute != null)
                    return onExecute.Invoke(game, player, card, (T)vars[0]);
                else
                    return Task.CompletedTask;
            })
        {
        }
    }
    public class THHEffect<T> : THHEffect where T : IEventArg
    {
        public new delegate bool CheckConditionDelegate(THHGame game, THHPlayer player, Card card, T eventArg);
        public new delegate Task ExecuteDelegate(THHGame game, THHPlayer player, Card card, T eventArg, object[] targets);
        public THHEffect(string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute) :
            base(new On<T>(), pile, (game, player, card, vars) =>
            {
                if (onCheckCondition != null)
                    return onCheckCondition.Invoke(game, player, card, (T)vars[0]);
                else
                    return true;
            }, onCheckTarget, (game, player, card, vars, targets) =>
            {
                if (onExecute != null && vars.Length > 0 && vars[0] is T t)
                    return onExecute.Invoke(game, player, card, t, targets);
                else
                    return Task.CompletedTask;
            })
        {
        }
    }
    public class THHEffectAfter<T> : THHEffect where T : IEventArg
    {
        public new delegate bool CheckConditionDelegate(THHGame game, THHPlayer player, Card card, T eventArg);
        public new delegate Task ExecuteDelegate(THHGame game, THHPlayer player, Card card, T eventArg);
        public THHEffectAfter(string pile, CheckConditionDelegate onCheckCondition, CheckTargetDelegate onCheckTarget, ExecuteDelegate onExecute) :
            base(new After<T>(), pile, (game, player, card, vars) =>
            {
                if (onCheckCondition != null)
                    return onCheckCondition.Invoke(game, player, card, (T)vars[0]);
                else
                    return true;
            }, onCheckTarget, (game, player, card, vars, targets) =>
            {
                if (onExecute != null)
                    return onExecute.Invoke(game, player, card, (T)vars[0]);
                else
                    return Task.CompletedTask;
            })
        {
        }
    }
}