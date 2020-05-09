using TouhouHeartstone;
namespace UI
{
    class TurnEndAnimation : Animation<THHGame.TurnEndEventArg>
    {
        public TurnEndAnimation(THHGame.TurnEndEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            return true;
        }
    }
}
