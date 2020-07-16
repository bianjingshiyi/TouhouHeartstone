using UI;
using UnityEngine;
namespace Game
{
    class AnimAnim : TableAnimation
    {
        Animator _animator;
        string _animName;
        bool _isPlayed = false;
        public AnimAnim(Animator animator, string animName)
        {
            _animator = animator;
            _animName = animName;
        }
        public override bool update(TableManager table)
        {
            if (!_animator.HasState(0, Animator.StringToHash(_animName)))
                return true;
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            if (!_isPlayed)
            {
                _isPlayed = true;
                _animator.Play(_animName);
            }
            if (state.normalizedTime < 1)
                return false;
            return true;
        }
    }
}
