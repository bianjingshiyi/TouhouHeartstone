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
        Func<UIAnimation, bool> _onBlockAnim;
        public AnimAnim(Animator animator, string animName, Func<UIAnimation, bool> onBlockAnim = null)
        {
            _animator = animator;
            _animName = animName;
            _onBlockAnim = onBlockAnim;
        }
        public override bool update(BattleTableManager table, float time)
        {
            return AnimHelper.update(_animator, _animName, ref _isPlayed, ref _isFinished);
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (_onBlockAnim != null)
                return _onBlockAnim(nextAnim);
            return base.blockAnim(nextAnim);
        }
    }
}
