using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UnityEngine;

namespace Game.BattleTable
{
    [Serializable]
    public abstract class TableAnimation : UIAnimation
    {
        protected float _deltaTime;
        protected float _duration;
        private bool _started = false;
        private bool _finished = false;
        private Coroutine _coroutine;
        /// <summary>
        /// 该动画是否会强制播放之前的动画，以及自身。
        /// 一般用于检视卡牌这种需要等待玩家操作的事件。
        /// </summary>
        public virtual bool forceQueueAnimations => false;
        public virtual void init(BattleTableManager table)
        {
        }
        public virtual bool update(BattleTableManager table, float time)
        {
            _started = true;
            _deltaTime = time;
            _duration += _deltaTime;
            if (_finished)
            {
                table.StopCoroutine(_coroutine);
                _coroutine = null;
                return true;
            }
            else if (_coroutine == null)
            {
                _coroutine = table.StartCoroutine(updateCoroutine(table));
            }
            return false;
        }
        public virtual IEnumerator update(BattleTableManager table)
        {
            yield break;
        }
        public bool isStarted()
        {
            return _started;
        }
        public bool isFinished()
        {
            return _finished;
        }
        public void finish()
        {
            _finished = true;
        }
        private IEnumerator updateCoroutine(BattleTableManager table)
        {
            yield return update(table);
            finish();
        }
        /// <summary>
        /// 是否会阻挡组内的另一个动画？
        /// </summary>
        /// <param name="other">另一个动画</param>
        /// <returns></returns>
        public virtual bool blockAnimInGroup(TableAnimation other)
        {
            return false;
        }
        /// <summary>
        /// 获取可以一组同时播放的动画。
        /// </summary>
        /// <param name="following">剩余的动画</param>
        /// <returns></returns>
        public virtual IEnumerable<TableAnimation> getGroupedAnimations(IEnumerable<TableAnimation> following)
        {
            yield break;
        }
        /// <summary>
        /// 是否会继续下一个动画组？
        /// </summary>
        /// <param name="nextGroup">下一个动画组</param>
        /// <returns></returns>
        public virtual bool continueNextGroup(AnimationGroup nextGroup)
        {
            return false;
        }
        
    }
    public abstract class EventAnimation : TableAnimation
    {
        public IEventArg eventArg { get; set; }
        public TableAnimation[] insertedAnims { get; set; }
        public EventRecord record { get; set; }
        public virtual void postEvent(BattleTableManager table)
        {
        }
        public virtual EventAnimation clone()
        {
            return MemberwiseClone() as EventAnimation;
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (!record.isCompleted)
                return true;
            return base.continueNextGroup(nextGroup);
        }
        protected IEnumerator pauseWhileNotCompleted()
        {
            while (!record.isCompleted)
                yield return null;
            yield break;
        }
        protected IEnumerator stopWhenCanceled()
        {
            if (record.isCanceled)
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
    }
    [Serializable]
    public abstract class EventAnimation<T> : EventAnimation where T : IEventArg
    {
        public T tEventArg => eventArg is T t ? t : default;
        public EventAnimState animState;
        public sealed override void init(BattleTableManager table)
        {
            base.init(table);
            init(table, tEventArg);
        }
        public sealed override void postEvent(BattleTableManager table)
        {
            base.postEvent(table);
            postEvent(table, tEventArg);
        }
        public virtual void init(BattleTableManager table, T eventArg)
        {
        }
        public virtual void postEvent(BattleTableManager table, T eventArg)
        {
        }
        public sealed override IEnumerator update(BattleTableManager table)
        {
            animState = EventAnimState.Started;
            yield return startUpdate(table, record, tEventArg);
            animState = EventAnimState.StartFinished;
            while (!record.isCompleted || (insertedAnims != null && insertedAnims.Length > 0 && !insertedAnims.All(a => a.isFinished())))
            {
                yield return null;
            }
            if (!record.isCanceled)
            {
                animState = EventAnimState.Logic;
                yield return logicUpdate(table, record, tEventArg);
            }
            else
            {
                animState = EventAnimState.Canceled;
                yield return canceledUpdate(table, record, tEventArg);
            }
            animState = EventAnimState.Finished;
        }
        public virtual IEnumerator startUpdate(BattleTableManager table, EventRecord record, T arg)
        {
            yield break;
        }
        public virtual IEnumerator logicUpdate(BattleTableManager table, EventRecord record, T arg)
        {
            yield break;
        }
        public virtual IEnumerator canceledUpdate(BattleTableManager table, EventRecord record, T arg)
        {
            yield break;
        }
        private bool isAnimGroupInsertedAnimOfThis(AnimationGroup nextGroup)
        {
            if (insertedAnims == null || insertedAnims.Length <= 0)
                return false;
            return nextGroup.Any(a => insertedAnims.Contains(a));
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (animState == EventAnimState.StartFinished && isAnimGroupInsertedAnimOfThis(nextGroup))
            {
                return true;
            }
            return base.continueNextGroup(nextGroup);
        }
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
