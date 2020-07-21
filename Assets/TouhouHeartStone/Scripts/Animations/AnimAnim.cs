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
        public bool isFinished { get; private set; } = false;
        Func<UIAnimation, bool> _onBlockAnim;
        public AnimAnim(Animator animator, string animName, Func<UIAnimation, bool> onBlockAnim = null)
        {
            _animator = animator;
            _animName = animName;
            _onBlockAnim = onBlockAnim;
        }
        public override bool update(TableManager table)
        {
            if (_animator == null)
                return true;
            if (string.IsNullOrEmpty(_animName))
                return true;
            if (isFinished)
                return true;
            if (!hasAnim())
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
            for (int i = 0; i < _animator.layerCount; i++)
            {
                AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(i);
                if (state.IsName(_animName) && state.normalizedTime < 1)
                    return false;
            }
            isFinished = true;
            return true;
        }
        bool hasAnim()
        {
            for (int i = 0; i < _animator.layerCount; i++)
            {
                if (_animator.HasState(i, Animator.StringToHash(_animName)))
                    return true;
            }
            return false;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (_onBlockAnim != null)
                return _onBlockAnim(nextAnim);
            return base.blockAnim(nextAnim);
        }
    }
}
