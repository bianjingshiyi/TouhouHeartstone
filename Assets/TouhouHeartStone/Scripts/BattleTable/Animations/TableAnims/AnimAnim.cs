using System;
using System.Collections.Generic;
using UnityEngine;
namespace Game.BattleTable
{
    /// <summary>
    /// 播放Animator动画的动画。
    /// </summary>
    class AnimAnim : TableAnimation
    {
        public AnimAnim(Animator animator, string animName, Func<IEnumerable<TableAnimation>, bool> onBlockAnim = null)
        {
            _animator = animator;
            _animName = animName;
            _onBlockAnim = onBlockAnim;
        }
        public override bool update(BattleTableManager table, float time)
        {
            return AnimHelper.update(_animator, _animName, ref _isPlayed, ref _isFinished);
        }
        public override bool continueNextGroup(IEnumerable<TableAnimation> nextGroup)
        {
            if (_onBlockAnim != null)
                return !_onBlockAnim(nextGroup);
            return base.continueNextGroup(nextGroup);
        }
        Animator _animator;
        string _animName;
        bool _isPlayed = false;
        bool _isFinished = false;
        public bool isFinished
        {
            get => _isFinished;
            private set => _isFinished = value;
        }
        Func<IEnumerable<TableAnimation>, bool> _onBlockAnim;
    }
}
