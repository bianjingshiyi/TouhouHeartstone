using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace Game.BattleTable
{
    public abstract class EventAnimation : TableAnimation
    {
        #region 公有方法
        public sealed override IEnumerator update(BattleTableManager table)
        {
            do
            {
                _varChanged = false;
                animState = EventAnimState.Started;
                yield return startUpdate(table);
                animState = EventAnimState.StartFinished;
                while (shouldPause())
                {
                    yield return null;
                }
            }
            while (_varChanged);

            if (!eventArg.isCanceled)
            {
                animState = EventAnimState.Logic;
                yield return logicUpdate(table);
            }
            else
            {
                animState = EventAnimState.Canceled;
                yield return canceledUpdate(table);
            }
            animState = EventAnimState.Finished;
        }
        public virtual void postEvent(BattleTableManager table)
        {
        }
        public virtual EventAnimation clone()
        {
            return MemberwiseClone() as EventAnimation;
        }
        public virtual IEnumerator startUpdate(BattleTableManager table)
        {
            yield break;
        }
        public virtual IEnumerator logicUpdate(BattleTableManager table)
        {
            yield break;
        }
        public virtual IEnumerator canceledUpdate(BattleTableManager table)
        {
            yield break;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (animState == EventAnimState.StartFinished)
            {
                if (!eventArg.isCompleted)
                    return true;
                if (shouldPause() && isAnimGroupInsertedAnimOfThis(nextGroup))
                    return true;
            }
            return base.continueNextGroup(nextGroup);
        }
        public void setVar(string name, object value)
        {
            startData.setVar(name, value);
            _varChanged = true;
        }
        public bool isVarChanged()
        {
            return _varChanged;
        }
        #endregion

        #region 私有方法
        protected IEnumerator pauseWhileNotCompleted()
        {
            while (!eventArg.isCompleted)
                yield return null;
            yield break;
        }
        protected IEnumerator stopWhenCanceled()
        {
            if (eventArg.isCanceled)
                finish();
            yield break;
        }
        protected CardSnapshot snapshotCard(BattleTableManager table, Card card)
        {
            return table.game.snapshotCard(card);
        }
        protected CardSnapshot[] snapshotCardArray(BattleTableManager table, IEnumerable<Card> cards)
        {
            return cards.Select(c => table.game.snapshotCard(c)).ToArray();
        }
        private bool shouldPause()
        {
            bool notCompleted = !eventArg.isCompleted;
            bool hasInsertAnim = insertedAnims != null && insertedAnims.Count > 0 && insertedAnims.Any(a => !a.isFinished());
            return (notCompleted || hasInsertAnim) && !_varChanged;
        }
        private bool isAnimGroupInsertedAnimOfThis(AnimationGroup nextGroup)
        {
            if (insertedAnims == null || insertedAnims.Count <= 0)
                return false;
            return nextGroup.Any(a => insertedAnims.Contains(a));
        }
        #endregion

        #region 属性字段
        public IEventArg eventArg { get; set; }
        public List<TableAnimation> insertedAnims { get; } = new List<TableAnimation>();
        public EventAnimStartData startData { get; set; }
        public EventAnimState animState;
        private bool _varChanged;
        #endregion
    }
    public abstract class EventAnimation<T> : EventAnimation where T : IEventArg
    {


        #region 属性字段
        public T tEventArg => eventArg is T t ? t : default;
        #endregion
    }
    public enum EventAnimState
    {
        NotStarted,
        Started,
        StartFinished,
        Logic,
        Canceled,
        Finished
    }
}
