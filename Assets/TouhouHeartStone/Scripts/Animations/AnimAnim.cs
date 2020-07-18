using UI;
using UnityEngine;
using System;
namespace Game
{
    class AnimAnim : TableAnimation
    {
        Animator _animator;
        string _animName;
        bool _isPlayed = false;
        Func<UIAnimation, bool> _onBlockAnim;
        public AnimAnim(Animator animator, string animName, Func<UIAnimation, bool> onBlockAnim = null)
        {
            _animator = animator;
            _animName = animName;
            _onBlockAnim = onBlockAnim;
        }
        public override bool update(TableManager table)
        {
            if (!_animator.HasState(0, Animator.StringToHash(_animName)))
            {
                Debug.LogError(_animator + "中不存在动画状态" + _animName, _animator);
                return true;
            }
            if (!_isPlayed)
            {
                _isPlayed = true;
                _animator.Play(_animName);
                return false;
            }
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.normalizedTime < 1)
                return false;
            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (_onBlockAnim != null)
                return _onBlockAnim(nextAnim);
            return base.blockAnim(nextAnim);
        }
    }
}
