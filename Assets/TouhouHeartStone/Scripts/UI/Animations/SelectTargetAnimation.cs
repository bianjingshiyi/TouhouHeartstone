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
        Timer _timer = new Timer() { duration = .67f };
        public override bool update(Table table)
        {
            var target = table.getCharacter(eventArg.targets[0] as TouhouCardEngine.Card);
            if (!_timer.isStarted)
            {
                UnityEngine.Animation animation = target.GetComponent<UnityEngine.Animation>();
                animation.Play("Servant_Targeted");
            }
            if (!_timer.isExpired())
                return false;
            return true;
        }
    }
}
