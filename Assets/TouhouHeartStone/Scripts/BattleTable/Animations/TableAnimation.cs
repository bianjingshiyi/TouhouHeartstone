using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BattleTable
{
    [Serializable]
    public abstract class TableAnimation : UIAnimation
    {
        public AnimationGroup group { get; set; }
        public List<AnimationGroup> insertedAnims { get; } = new List<AnimationGroup>();
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
                if (_coroutine != null)
                {
                    table.StopCoroutine(_coroutine);
                    _coroutine = null;
                }
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
        public void finish(BattleTableManager table)
        {
            _finished = true;
            if (_coroutine != null)
            {
                table.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
        private IEnumerator updateCoroutine(BattleTableManager table)
        {
            yield return update(table);
            finish(table);
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
}
