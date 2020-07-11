using BJSYGameCore;
using UI;
using BJSYGameCore.UI;
namespace Game
{
    class TargetedAnim : TableAnimation
    {
        UIObject _target;
        Timer _targetingTimer = new Timer() { duration = .8f };
        public TargetedAnim(UIObject target)
        {
            _target = target;
        }
        public override bool update(TableManager table)
        {
            if (!_targetingTimer.isStarted)
            {
                _target.animator.Play("Targeted");
                _targetingTimer.start();
            }
            if (!_targetingTimer.isExpired())
                return false;
            return true;
        }
    }
}
