using TouhouHeartstone;
namespace UI
{
    class TurnEndAnimation : UIAnimation<THHGame.TurnEndEventArg>
    {
        public TurnEndAnimation(THHGame.TurnEndEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            table.canControl = false;
            return true;
        }
    }
}
