using TouhouHeartstone;
using BJSYGameCore;
namespace UI
{
    class SelectTargetAnimation : Animation
    {
        public THHPlayer.ActiveEventArg eventArg { get; }
        public SelectTargetAnimation(THHPlayer.ActiveEventArg eventArg)
        {
            this.eventArg = eventArg;
        }
        Timer _timer = new Timer() { duration = .8f };
        public override bool update(Table table)
        {
            var target = table.getCharacter(eventArg.targets[0] as TouhouCardEngine.Card);
            if (!_timer.isStarted)
            {
                target.animator.Play("Targeted");
                _timer.start();
            }
            if (!_timer.isExpired())
                return false;
            return true;
        }
    }
}
