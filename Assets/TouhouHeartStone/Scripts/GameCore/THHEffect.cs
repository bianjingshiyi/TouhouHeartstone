using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
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


    public delegate bool CheckConditionDelegate(CardEngine game, Card card);

}