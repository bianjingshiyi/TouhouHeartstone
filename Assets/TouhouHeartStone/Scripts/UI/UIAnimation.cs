using System;
using UnityEngine;
using TouhouCardEngine.Interfaces;
using Game;
using TouhouCardEngine;
using System.Collections.Generic;
using System.Linq;
using GameCard = TouhouCardEngine.Card;
using System.Collections;

namespace UI
{
    [Serializable]
    public class UIAnimation
    {
        [SerializeField]
        string _name;
        public string name
        {
            get { return _name; }
        }
#if UNITY_EDITOR
        [SerializeField]
        string _content;
#endif
        public UIAnimation()
        {
            _name = GetType().Name;
#if UNITY_EDITOR
            _content = ToString();
#endif
        }
        public UIAnimation(string name)
        {
            _name = name;
#if UNITY_EDITOR
            _content = ToString();
#endif
        }
#if UNITY_EDITOR
        public void updateDebugContent()
        {
            _content = ToString();
        }
#endif
        /// <summary>
        /// 当播放动画时会在每一帧被调用，返回值为真表示动画结束，动画将被从队列中剔除。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        [Obsolete]
        public virtual bool update(Table table)
        {
            return true;
        }
        /// <summary>
        /// 是否会阻挡下一个动画？
        /// </summary>
        /// <param name="nextAnim">下一个动画</param>
        /// <returns></returns>
        public virtual bool blockAnim(UIAnimation nextAnim)
        {
            return true;
        }
    }
    [Serializable]
    public abstract class TableAnimation : UIAnimation
    {
        /// <summary>
        /// 该动画是否会强制播放之前的动画，以及自身。
        /// 一般用于检视卡牌这种需要等待玩家操作的事件。
        /// </summary>
        public virtual bool forceQueueAnimations => false;
        [Obsolete("请使用update(table,time)替代")]
        public sealed override bool update(Table table)
        {
            throw new NotImplementedException();
        }
        public virtual void init(TableManager table)
        {
        }
        public abstract bool update(TableManager table, float time);
    }
    public abstract class EventAnimation : TableAnimation
    {
        public virtual string eventName { get => throw new NotImplementedException(); }//ToString();
        public IEventArg eventArg { get; set; }
        public EventRecord record { get; set; }
        protected Stack<IEnumerator> _coroutineStack;
        protected IEnumerator _coroutine;
        protected float _time;
        protected bool _finished = true;
        public virtual void postEvent(TableManager table)
        {
        }
        public virtual EventAnimation clone()
        {
            return MemberwiseClone() as EventAnimation;
        }
        protected CardSnapshot snapshotCard(TableManager table, GameCard card)
        {
            return table.game.snapshotCard(card);
        }
        protected CardSnapshot[] snapshotCardArray(TableManager table, IEnumerable<GameCard> cards)
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
        public sealed override void init(TableManager table)
        {
            base.init(table);
            init(table, tEventArg);
        }
        public sealed override void postEvent(TableManager table)
        {
            base.postEvent(table);
            postEvent(table, tEventArg);
        }
        public virtual void init(TableManager table, T eventArg)
        {
        }
        public virtual void postEvent(TableManager table, T eventArg)
        {
        }
        public sealed override bool update(TableManager table, float time)
        {
            _time = time;
            if (_coroutine == null)
            {
                _coroutineStack = _coroutineStack ?? new Stack<IEnumerator>();
                _finished = false;
                _coroutine = update(table, record, tEventArg);
            }
            updateCoroutine();
            return update(table, tEventArg, time) && _finished;
        }
        public virtual bool update(TableManager table, T arg, float time)
        {
            return true;
        }
        public virtual IEnumerator update(TableManager table, EventRecord record, T arg)
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
    [Serializable]
    public abstract class UIAnimation<T> : UIAnimation where T : IEventArg
    {
        public T eventArg { get; protected set; }
        public UIAnimation(T eventArg)
        {
            this.eventArg = eventArg;
        }
    }
}
