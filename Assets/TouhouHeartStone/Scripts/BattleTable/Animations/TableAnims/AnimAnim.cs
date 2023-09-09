using System;
using UnityEngine;
namespace Game.BattleTable
{
    class AnimAnim : TableAnimation
    {
        Animator _animator;
        string _animName;
        bool _isPlayed = false;
        bool _isFinished = false;
        public bool isFinished
        {
            get => _isFinished;
            private set => _isFinished = value;
        }
        Func<AnimationGroup, bool> _onBlockAnim;
        public AnimAnim(Animator animator, string animName, Func<AnimationGroup, bool> onBlockAnim = null)
        {
            _animator = animator;
            _animName = animName;
            _onBlockAnim = onBlockAnim;
        }
        public override bool update(BattleTableManager table, float time)
        {
            return AnimHelper.update(_animator, _animName, ref _isPlayed, ref _isFinished);
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (_onBlockAnim != null)
                return !_onBlockAnim(nextGroup);
            return base.continueNextGroup(nextGroup);
        }
    }
}
