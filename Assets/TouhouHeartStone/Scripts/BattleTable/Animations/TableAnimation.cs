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
        /// <summary>
        /// 该动画是否会强制播放之前的动画，以及自身。
        /// 一般用于检视卡牌这种需要等待玩家操作的事件。
        /// </summary>
        public virtual bool forceQueueAnimations => false;
        public virtual void init(BattleTableManager table)
        {
        }
        public abstract bool update(BattleTableManager table, float time);
    }
    public abstract class EventAnimation : TableAnimation
    {
        public IEventArg eventArg { get; set; }
        public EventRecord record { get; set; }
        protected Stack<IEnumerator> _coroutineStack;
        protected IEnumerator _coroutine;
        protected float _time;
        protected float _duration;
        protected bool _finished = true;
        public virtual void postEvent(BattleTableManager table)
        {
        }
        public virtual EventAnimation clone()
        {
            return MemberwiseClone() as EventAnimation;
        }
        protected CardSnapshot snapshotCard(BattleTableManager table, Card card)
        {
            return table.game.snapshotCard(card);
        }
        protected CardSnapshot[] snapshotCardArray(BattleTableManager table, IEnumerable<Card> cards)
        {
            return cards.Select(c => table.game.snapshotCard(c)).ToArray();
        }
        protected void finish()
        {
            _finished = true;
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
        public sealed override bool update(BattleTableManager table, float time)
        {
            _time = time;
            _duration += _time;
            if (_coroutine == null)
            {
                _coroutineStack = _coroutineStack ?? new Stack<IEnumerator>();
                _finished = false;
                _coroutine = update(table, record, tEventArg);
            }
            updateCoroutine();
            return _finished;
        }
        public virtual IEnumerator update(BattleTableManager table, EventRecord record, T arg)
        {
            yield break;
        }
        private void updateCoroutine()
        {
            while (_coroutine.MoveNext())
            {
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
}
