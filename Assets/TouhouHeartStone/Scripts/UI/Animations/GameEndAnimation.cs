using TouhouHeartstone;
namespace UI
{
    class GameEndAnimation : UIAnimation<THHGame.GameEndEventArg>
    {
        public GameEndAnimation(THHGame.GameEndEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            return true;
        }
    }
}
