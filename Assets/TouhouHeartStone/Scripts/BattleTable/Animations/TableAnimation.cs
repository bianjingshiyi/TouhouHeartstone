using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace Game.BattleTable
{
    [Serializable]
    public abstract class TableAnimation : UIAnimation
    {
        protected float _deltaTime;
        protected float _duration;
        private Stack<IEnumerator> _coroutineStack;
        private IEnumerator _coroutine;
        private bool _finished = false;
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
            _deltaTime = time;
            _duration += _deltaTime;
            if (_coroutine == null)
            {
                _coroutineStack = _coroutineStack ?? new Stack<IEnumerator>();
                _finished = false;
                _coroutine = update(table);
            }
            updateCoroutine();
            return _finished;
        }
        public virtual IEnumerator update(BattleTableManager table)
        {
            yield break;
        }
        public bool isFinished()
        {
            return _finished;
        }
        protected void finish()
        {
            _coroutineStack.Clear();
            _finished = true;
        }
        private void updateCoroutine()
        {
            while (_coroutine.MoveNext())
            {
                if (_finished)
                    return;

                var result = _coroutine.Current;
                if (result is IEnumerator enumerator)
                {
                    _coroutineStack.Push(_coroutine);
                    _coroutine = enumerator;
                }
                else
                {
                    return;
                }
            }
            if (_coroutineStack.Count > 0)
            {
                _coroutine = _coroutineStack.Pop();
            }
            else
            {
                finish();
            }
        }
    }
    public abstract class EventAnimation : TableAnimation
    {
        public IEventArg eventArg { get; set; }
        public EventRecord record { get; set; }
        public virtual void postEvent(BattleTableManager table)
        {
        }
        public virtual EventAnimation clone()
        {
            return MemberwiseClone() as EventAnimation;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (!record.isCompleted)
                return false;
            return base.blockAnim(nextAnim);
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
            return update(table, record, tEventArg);
        }
        public virtual IEnumerator update(BattleTableManager table, EventRecord record, T arg)
        {
            yield break;
        }
    }
}
